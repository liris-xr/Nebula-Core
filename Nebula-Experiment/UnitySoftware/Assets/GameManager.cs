using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

[RequireComponent(typeof(PseudoRandomizerExperiment))]
public class GameManager : MonoBehaviour
{
    PseudoRandomizerExperiment randomizerExperiment;
    float timer = 0f;
    float inputTimer = 0f;
    float questionnaireTimer = 0f;
    int expeIndex = 0;
    int tutoIndex = 0;

    public GameObject introCanvas;
    public GameObject tutoIntroCanvas;
    public GameObject expeIntroCanvas;
    public GameObject beReadyCanvas;
    public GameObject diffusingCanvas;
    public GameObject transitionCanvas;
    public GameObject questionnaireCanvas;
    public GameObject quitCanvas;
    public ButtonManager buttonManager;

    public bool useMAO = true;

    InputDevice leftDevice;
    InputDevice rightDevice;
    bool l_triggerTriggered;
    bool r_triggerTriggered;
    float leftTimer = 0.0f;
    float rightTimer = 0.0f;
    bool leftTriggerPressed = false;
    bool rightTriggerPressed = false;
    bool tutorialPhase = false;
    bool buttonPressed = false;

    // Start is called before the first frame update
    void Awake()
    {
        if(useMAO) NebulaStaticCom.InitUSBSerial();

        randomizerExperiment = GetComponent<PseudoRandomizerExperiment>();
        introCanvas.SetActive(true);
        tutoIntroCanvas.SetActive(false);
        expeIntroCanvas.SetActive(false);
        beReadyCanvas.SetActive(false);
        diffusingCanvas.SetActive(false);
        transitionCanvas.SetActive(false);
        questionnaireCanvas.SetActive(false);
        quitCanvas.SetActive(false);
        buttonManager.enabled = false;

        questionnaireCanvas.transform.GetChild(2).gameObject.SetActive(true);
        questionnaireCanvas.transform.GetChild(3).gameObject.SetActive(true);
        questionnaireCanvas.transform.GetChild(4).gameObject.SetActive(false);

        InputDevices.deviceConnected += IdentifyDevice;
        InputDevices.deviceDisconnected += IdentifyDevice;
    }

    // Update is called once per frame
    void Update()
    {
        if (diffusingCanvas.activeSelf)
        {
            GetInput();
            if (!tutorialPhase && (leftTriggerPressed || rightTriggerPressed) && randomizerExperiment.diffusionDetectedTime[expeIndex] == -1)
            {
                randomizerExperiment.diffusionDetectedTime[expeIndex] = inputTimer;
                diffusingCanvas.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (tutorialPhase && (leftTriggerPressed || rightTriggerPressed))
            {
                diffusingCanvas.transform.GetChild(2).gameObject.SetActive(true);
            }
            inputTimer += Time.deltaTime;
        }

        if (questionnaireCanvas.activeSelf)
        {
            if(!tutorialPhase && buttonManager.CheckIfAnswered() && !buttonPressed)
            {
                randomizerExperiment.notationTime[expeIndex] = questionnaireTimer;
                randomizerExperiment.notation[expeIndex] = buttonManager.QuestionNote();
                Debug.Log("Button pressed : " + randomizerExperiment.notation[expeIndex] + "\nTime recordedé : " + randomizerExperiment.notationTime[expeIndex]);
                questionnaireCanvas.transform.GetChild(2).gameObject.SetActive(false);
                questionnaireCanvas.transform.GetChild(3).gameObject.SetActive(false);
                questionnaireCanvas.transform.GetChild(4).gameObject.SetActive(true);
                buttonPressed = true;
            }
            else if (tutorialPhase && buttonManager.CheckIfAnswered() && !buttonPressed)
            {
                Debug.Log("Button pressed : " + buttonManager.QuestionNote() + "\nTime recorded : " + questionnaireTimer);
                questionnaireCanvas.transform.GetChild(2).gameObject.SetActive(false);
                questionnaireCanvas.transform.GetChild(3).gameObject.SetActive(false);
                questionnaireCanvas.transform.GetChild(4).gameObject.SetActive(true);
                buttonPressed = true;
            }

            questionnaireTimer += Time.deltaTime; 
        }

        if (timer > 0f) timer -= Time.deltaTime;
        else timer = 0f;

        if (beReadyCanvas.activeSelf) beReadyCanvas.transform.GetChild(1).GetComponent<Text>().text = "Be ready for the diffusion\n" + (int)Mathf.Ceil(timer) + " s";
        if (transitionCanvas.activeSelf) transitionCanvas.transform.GetChild(1).GetComponent<Text>().text = "Scoring in " + (int)Mathf.Ceil(timer) + " s...";
        if (questionnaireCanvas.activeSelf && !buttonPressed) questionnaireCanvas.transform.GetChild(1).GetComponent<Text>().text = "Time left before next diffusion : " + (int)Mathf.Ceil(timer) + " s";
        else if (questionnaireCanvas.activeSelf && buttonPressed) questionnaireCanvas.transform.GetChild(1).GetComponent<Text>().text = (int)Mathf.Ceil(timer) + " s";

        if (tutorialPhase)
        {
            beReadyCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=orange>TUTORIAL : " + (tutoIndex + 1) + " / 2</color>";
            diffusingCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=orange>TUTORIAL : " + (tutoIndex + 1) + " / 2</color>";
            transitionCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=orange>TUTORIAL : " + (tutoIndex + 1) + " / 2</color>";
            if (!buttonPressed) questionnaireCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=orange>TUTORIAL : " + (tutoIndex + 1) + " / 2</color>";
            else questionnaireCanvas.transform.GetChild(0).GetComponent<Text>().text = "";
        }
        else
        {
            beReadyCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=green>EXPERIMENT : " + (expeIndex + 1) + " / 18</color>";
            diffusingCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=green>EXPERIMENT : " + (expeIndex + 1) + " / 18</color>";
            transitionCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=green>EXPERIMENT : " + (expeIndex + 1) + " / 18</color>";
            if (!buttonPressed) questionnaireCanvas.transform.GetChild(0).GetComponent<Text>().text = "<color=green>EXPERIMENT : " + (expeIndex + 1) + " / 18</color>";
            else questionnaireCanvas.transform.GetChild(0).GetComponent<Text>().text = "";
        }

    }

    public void InitializeIntro()
    {
        introCanvas.SetActive(false);
        tutoIntroCanvas.SetActive(true);
    }

    public void InitializeTuto()
    {
        tutorialPhase = true;
        StartCoroutine(DoTuto());
    }

    public void InitializeExpe()
    {
        tutorialPhase = false;
        StartCoroutine(DoExpe());
    }

    private IEnumerator DoTuto()
    {
        tutoIntroCanvas.SetActive(false);

        for (tutoIndex = 0; tutoIndex < 0; tutoIndex++)
        {
            beReadyCanvas.SetActive(true);

            timer = 3f;
            yield return new WaitForSeconds(timer);
            timer = 0f;

            beReadyCanvas.SetActive(false);
            diffusingCanvas.SetActive(true);
            diffusingCanvas.transform.GetChild(2).gameObject.SetActive(false);


            if (useMAO) NebulaStaticCom.MAOSender(0);
            Debug.Log("Diffusion Level: 0");

            timer = 3f;
            inputTimer = 0f;
            yield return new WaitForSeconds(timer);
            timer = 0f;
            inputTimer = 0f;

            if (useMAO) NebulaStaticCom.MAOSender(-1);

            diffusingCanvas.SetActive(false);
            diffusingCanvas.transform.GetChild(2).gameObject.SetActive(false);
            transitionCanvas.SetActive(true);

            timer = 3f;
            yield return new WaitForSeconds(timer);
            timer = 0f;

            transitionCanvas.SetActive(false);
            buttonPressed = false;
            buttonManager.enabled = true;
            questionnaireCanvas.SetActive(true);

            timer = 30f;
            questionnaireTimer = 0f;
            yield return new WaitForSeconds(timer);
            timer = 0f;

            while (!buttonManager.CheckIfAnswered())
            {
                yield return null;
                if (buttonManager.CheckIfAnswered())
                {
                    timer = 3f;
                    yield return new WaitForSeconds(timer);
                    timer = 0f;
                }
            }

            questionnaireTimer = 0f;
            questionnaireCanvas.SetActive(false);
            buttonManager.enabled = false;
            questionnaireCanvas.transform.GetChild(2).gameObject.SetActive(true);
            questionnaireCanvas.transform.GetChild(3).gameObject.SetActive(true);
            questionnaireCanvas.transform.GetChild(4).gameObject.SetActive(false);
            buttonPressed = false;
        }

        expeIntroCanvas.SetActive(true);
    }

    private IEnumerator DoExpe()
    {
        expeIntroCanvas.SetActive(false);

        for (expeIndex = 0; expeIndex < 18; expeIndex++)
        {
            beReadyCanvas.SetActive(true);

            timer = 3f;
            yield return new WaitForSeconds(timer);
            timer = 0f;

            beReadyCanvas.SetActive(false);
            diffusingCanvas.SetActive(true);
            diffusingCanvas.transform.GetChild(2).gameObject.SetActive(false);


            if(useMAO) NebulaStaticCom.MAOSender(randomizerExperiment.diffusionType[expeIndex]);
            Debug.Log("Diffusion Level: " + randomizerExperiment.diffusionType[expeIndex]);

            timer = 3f;
            inputTimer = 0f;
            yield return new WaitForSeconds(timer);
            timer = 0f;
            inputTimer = 0f;

            if(useMAO) NebulaStaticCom.MAOSender(-1);

            diffusingCanvas.SetActive(false);
            diffusingCanvas.transform.GetChild(2).gameObject.SetActive(false);
            transitionCanvas.SetActive(true);

            timer = 3f;
            yield return new WaitForSeconds(timer);
            timer = 0f;

            transitionCanvas.SetActive(false);
            buttonPressed = false;
            buttonManager.enabled = true;
            questionnaireCanvas.SetActive(true);

            timer = 30f;
            questionnaireTimer = 0f;
            yield return new WaitForSeconds(timer);
            timer = 0f;

            while(!buttonManager.CheckIfAnswered())
            {
                yield return null;
                if(buttonManager.CheckIfAnswered())
                {
                    timer = 3f;
                    yield return new WaitForSeconds(timer);
                    timer = 0f;
                }
            }

            questionnaireTimer = 0f;
            questionnaireCanvas.SetActive(false);
            buttonManager.enabled = false;
            questionnaireCanvas.transform.GetChild(2).gameObject.SetActive(true);
            questionnaireCanvas.transform.GetChild(3).gameObject.SetActive(true);
            questionnaireCanvas.transform.GetChild(4).gameObject.SetActive(false);
            buttonPressed = false;
        }

        randomizerExperiment.SaveCSVFile();
        quitCanvas.SetActive(true);

    }

    void IdentifyDevice(InputDevice device)
    {
        if ((device.characteristics & (InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller)) == (InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller))
        {
            leftDevice = device;
            Debug.Log("Left device associated");
        }
        if ((device.characteristics & (InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller)) == (InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller))
        {
            rightDevice = device;
            Debug.Log("Right device associated");
        }
    }

    void GetInput()
    {
        leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out l_triggerTriggered);
        rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out r_triggerTriggered);

        if (l_triggerTriggered)
        {
            if (leftTimer == 0)
            {
                Debug.Log("Left Grip Pressed");
                leftTriggerPressed = true;
            }
            leftTimer += Time.deltaTime;
        }

        else if (leftTimer > 0)
        {
            Debug.Log("Left Grip Released");
            leftTriggerPressed = false;
            leftTimer = 0;
        }

        if (r_triggerTriggered)
        {
            if (rightTimer == 0)
            {
                Debug.Log("Right Grip Pressed");
                rightTriggerPressed = true;
            }
            rightTimer += Time.deltaTime;
        }

        else if (rightTimer > 0)
        {
            Debug.Log("Right Grip Released");
            rightTriggerPressed = false;
            rightTimer = 0;
        }
    }
}

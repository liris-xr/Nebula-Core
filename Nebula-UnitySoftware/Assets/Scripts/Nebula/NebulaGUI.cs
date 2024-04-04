using System;
using System.Collections;
using UnityEngine;

public class NebulaGUI : MonoBehaviour
{
    public static bool controlFromUI;
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
    int windowOffset = 70;
    float designWidth = 1280.0f;
    float designHeight = 720.0f;
    float singleHeight = 90.0f;
    GameObject[] odorSources;

    private void Start()
    {
        odorSources = GameObject.FindGameObjectsWithTag("Odor Source");
    }

    private void OnGUI()
    {
        if (NebulaManager.nebulaSerial.IsOpen)
        {
            //Calculate change aspects
            float resX = (float)(Screen.width) / designWidth;
            float resY = (float)(Screen.height) / designHeight;
            //Set matrix
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(resX, resY, 1));
            GUI.Box(new Rect(10, 10, 220, singleHeight*odorSources.Length), "Nebula manual activation");
            CreateOdorSourcePanels(odorSources);
        }
    }

    private void CreateOdorSourcePanels(GameObject[] odorSources)
    {
        //Create the same UI template for each odor source in the scene
        //Can currently :           -(de)activate diffusion for each odor source with the configuration filled on the NebulaOdorDiffuser component of the GameObject
        //                          -stop diffusion when diffusion is triggered automatically
        //                          -change the duty cycle sent to the arduino
        //                          -if one odor source is manually activated, it can be activated again automatically again once stopped manually
        //TODO :                    -change min/max dutycycle for each odor source
        //                          -change diffusion mode
        //                          -stop all diffusions at once 

        int offset = 0;
        foreach (GameObject odorSource in odorSources)
        {
            if (!GetOdorSourceAttribute(odorSource, "isDiffusing"))
            {
                if (GUI.Button(new Rect(20, 40 + offset, 200, 20), "Start " + GetOdorSourceAttribute(odorSource,"name") + " atomization"))
                {
                    ChangeOdorSourceAttribute(odorSource, "isDiffusing", true);
                    ChangeOdorSourceAttribute(odorSource, "dutyCycle", GetOdorSourceAttribute(odorSource, "minimumDutyCycle"));
                    NebulaManager.SendCommand(GetOdorSourceAttribute(odorSource,"startDiffusionCommand"));
                    StartCoroutine(ManualDiffusion(odorSource));
                }
            }

            if (GetOdorSourceAttribute(odorSource, "isDiffusing"))
            {
                if (!controlFromUI) GUI.Label(new Rect(25, 60 + offset, 150, 30), "Current " + GetOdorSourceAttribute(odorSource, "name") + " " + GetOdorSourceAttribute(odorSource, "dutyCycle").ToString() + "%");
                else
                {
                    int dutyCycle = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(25, 62 + offset, 200, 30), GetOdorSourceAttribute(odorSource, "dutyCycle"), GetOdorSourceAttribute(odorSource, "minimumDutyCycle"), GetOdorSourceAttribute(odorSource, "maximumDutyCycle")));
                    ChangeOdorSourceAttribute(odorSource, "dutyCycle", dutyCycle);
                    GUI.Label(new Rect(25, 70 + offset, 150, 30), "Manual " + GetOdorSourceAttribute(odorSource, "name") + " " + dutyCycle.ToString() + "%");
                }

                if (GUI.Button(new Rect(20, 40 + offset, 200, 20), "Stop "+ GetOdorSourceAttribute(odorSource,"name") + " atomization"))
                {
                    NebulaManager.SendCommand(GetOdorSourceAttribute(odorSource, "stopDiffusionCommand"));
                    ChangeOdorSourceAttribute(odorSource, "isDiffusing", false);
                }
            }
            offset += windowOffset;
        }
    }

    private IEnumerator ManualDiffusion(GameObject odorSource)
    {
        while (GetOdorSourceAttribute(odorSource, "isDiffusing"))
        {
            yield return new WaitForSeconds(0.1f);
            if ((float)GetOdorSourceAttribute(odorSource, "dutyCycle") != GetOdorSourceAttribute(odorSource, "previousDutyCycle"))
            {
                ChangeOdorSourceAttribute(odorSource, "previousDutyCycle", (int)GetOdorSourceAttribute(odorSource, "dutyCycle"));
                NebulaManager.SendCommand(GetOdorSourceAttribute(odorSource, "changeConfigurationCommand") + GetOdorSourceAttribute(odorSource, "pwmFrequency").ToString() + "; " + GetOdorSourceAttribute(odorSource, "dutyCycle"));
            }
            controlFromUI = true;
        }
        controlFromUI = false;
    }

    private void ChangeOdorSourceAttribute(GameObject objWithOdorSource, string attribute, int value)
    {
        NebulaOdorDiffuser odorSource = objWithOdorSource.GetComponent<NebulaOdorDiffuser>();
        switch (attribute)
        {
            case "dutyCycle":
                odorSource.dutyCycle = value;
                break;
            case "previousDutyCycle":
                odorSource.previousDutyCycle = value;
                break;
            case "minimumDutyCycle":
                odorSource.minimumDutyCycle = value ;
                break;
            case "maximumDutyCycle":
                odorSource.maximumDutyCycle = value;
                break;
            case "pwmFrequency":
                odorSource.pwmFrequency = value;
                break;
            default:
                break;
        }
    }

    private void ChangeOdorSourceAttribute(GameObject objWithOdorSource, string attribute, bool isDiffusing)
    {
        NebulaOdorDiffuser odorSource = objWithOdorSource.GetComponent<NebulaOdorDiffuser>();
        switch (attribute)
        {
            case "isDiffusing":
                odorSource.isDiffusing = isDiffusing;
                break;
        }
    }

    private dynamic GetOdorSourceAttribute(GameObject objWithOdorSource, string attribute)
    {
        NebulaOdorDiffuser odorSource = objWithOdorSource.GetComponent<NebulaOdorDiffuser>();
        switch (attribute)
        {
            case "dutyCycle":
                return odorSource.dutyCycle;
            case "previousDutyCycle":
                return odorSource.previousDutyCycle;
            case "minimumDutyCycle":
                return odorSource.minimumDutyCycle;
            case "maximumDutyCycle":
                return odorSource.maximumDutyCycle;
            case "pwmFrequency":
                return odorSource.pwmFrequency;
            case "isDiffusing":
                return odorSource.isDiffusing;
            case "startDiffusionCommand":
                return odorSource.startDiffusionCommand;
            case "stopDiffusionCommand":
                return odorSource.stopDiffusionCommand;
            case "changeConfigurationCommand":
                return odorSource.changeConfigurationCommand;
            case "name":
                return odorSource.gameObject.name;
            default:
                return 0;
        }
    }
#endif
}

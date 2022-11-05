using System.Collections;
using UnityEngine;
using System.Threading;
using System;
using System.IO;

public class NebulaOdorDiffuser : MonoBehaviour
{
    //Define which odor you are going to diffuse. You can spread different smelling objects in the scene
    public enum AtomizerList { Left, Right};
    public AtomizerList atomizer;
    public enum DiffusionMode { InverseSquare, Linear , Binary};
    public DiffusionMode diffusionMode;

    private string enterString;
    private string exitString;

    private Transform playerHead;

    //Define the distance (in m) between the player head and the object before the start of the diffusion
    private float correctedDistance;
    [HideInInspector]
    private float dutyCycle;
    private float previousDutyCycle;
    //Define min and max duty cycle used for the progressive diffusion
    public int minimumDutyCycle = 1;
    public int maximumDutyCycle = 30;
    public float offsetMaxDutyCycle = 0.1f;
    public float offsetMinDutyCycle = 0.45f;
    //Adjust the PWM frequency of Nebula
    public string pwmFrequency = "100";
    public int booleanDutyCycle = 50;

    Vector3 originalPosition;
    Quaternion originalOrientation;

    private void Awake()
    {
        playerHead = GameObject.Find("NebulaManager").GetComponent<NebulaManager>().playerHead.transform;
        originalOrientation = gameObject.transform.rotation;
        originalPosition = gameObject.transform.position;
        //adjust instruction sent to the Arduino to dissociate left and right atomizer 
        switch (atomizer)
        {
            case AtomizerList.Left:
                enterString = "L\n";
                exitString = "l\n";
                break;
            case AtomizerList.Right:
                enterString = "R\n";
                exitString = "r\n";
                break;
            default:
                Debug.Log("Incorrect atomizer location");
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            StartDiffusion();
        }

        if (other.CompareTag("Floor"))
        {
            ResetGameObject();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            NebulaManager.nebulaIsDiffusing = false;
            dutyCycle = 0;
            NebulaManager.SendData(exitString);
            NebulaManager.currentDutyCycle = dutyCycle;
            StopCoroutine(DiffusionCoroutine());
        }
    }

    private void StartDiffusion()
    {
        if (!NebulaGUI.manualOverride)
        {
            NebulaManager.nebulaIsDiffusing = true;
            NebulaManager.SendData(enterString); //Send the start signal to Nebula 
            dutyCycle = booleanDutyCycle;
            NebulaManager.currentDutyCycle = dutyCycle;
            NebulaManager.SendData("C" + pwmFrequency + ";" + dutyCycle);
            if (diffusionMode != DiffusionMode.Binary) StartCoroutine(DiffusionCoroutine());
        }
    }

    //Coroutine allowing to control diffusion levels in real time
    private IEnumerator DiffusionCoroutine()
    {
        while (!NebulaGUI.manualOverride)
        {
            yield return new WaitForSeconds(0.1f); //Avoid overflowding the Arduino
            switch (diffusionMode)
            {
                case DiffusionMode.InverseSquare:
                    dutyCycle = Mathf.Round((1 / (float)Math.Pow(CalculateCorrectedDistance(), 2))); // Increase duty cycle according to distance
                    break;
                case DiffusionMode.Linear:
                    dutyCycle = Mathf.Round((Mathf.InverseLerp(offsetMinDutyCycle, offsetMaxDutyCycle, Vector3.Distance(playerHead.position, transform.position))) * 100); // Increase duty cycle according to distance
                    break;
            }
            if (dutyCycle <= minimumDutyCycle) dutyCycle = minimumDutyCycle;
            if (dutyCycle >= maximumDutyCycle) dutyCycle = maximumDutyCycle;
            if (dutyCycle != previousDutyCycle)
            {
                NebulaManager.SendData("C" + pwmFrequency + ";" + dutyCycle);
                previousDutyCycle = dutyCycle;
                NebulaManager.currentDutyCycle = dutyCycle;
            }
        }
    }

    //Calculate the distance between the object to smell and the player head and correct it in order to get the max intensity before 
    public float CalculateCorrectedDistance()
    {
        correctedDistance = Vector3.Distance(playerHead.position, transform.position);
        correctedDistance = (float)Math.Pow(correctedDistance, 2);
        if (correctedDistance < 0) correctedDistance = 0;
        return correctedDistance;
    }

    //Simple method to reset teh gameobject when it fall on the ground
    public void ResetGameObject()
    {
        gameObject.transform.SetPositionAndRotation(originalPosition, originalOrientation);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }


    private void OnApplicationQuit()
    {
        StopAllCoroutines();
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        try
        {
            //Send stop value Nebula
            NebulaManager.SendData("S");
            Thread.Sleep(200);
            NebulaManager.thread.Abort();
            NebulaManager.nebulaSerial.Close();
        }
        catch (Exception) {  }

#endif
    }
    }

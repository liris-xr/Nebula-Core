using System.Collections;
using UnityEngine;
using System.Threading;
using System;
using System.IO;

public class NebulaOdorDiffuser : MonoBehaviour
{
    //Define which odor you are going to diffuse. You can spread different smelling objects in the scene
    public enum AtomizerList { Left, Right };
    public enum DiffusionMode { InverseSquare, Linear, Binary };
    public DiffusionMode diffusionMode;
    public AtomizerList atomizer;
    [HideInInspector] public string startDiffusionCommand;
    [HideInInspector] public string stopDiffusionCommand;
    [HideInInspector] public string changeConfigurationCommand;
    [HideInInspector] public float previousDutyCycle;
    [HideInInspector] public float dutyCycle;
    [HideInInspector] public bool isDiffusing;
    //Define min and max duty cycle used for the progressive diffusion
    public int minimumDutyCycle = 1;
    public int maximumDutyCycle = 30;
    //Adjust the PWM frequency of Nebula
    public int pwmFrequency = 100;
    public int binaryDutyCycle = 50;
    public float offsetMaxDutyCycle = 0.1f;
    public float offsetMinDutyCycle = 0.45f;

    private Transform playerHead;

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
                startDiffusionCommand = "L";
                stopDiffusionCommand = "l";
                changeConfigurationCommand = "C";
                break;
            case AtomizerList.Right:
                startDiffusionCommand = "R";
                stopDiffusionCommand = "r";
                changeConfigurationCommand = "D";
                break;
            default:
                Debug.Log("Incorrect atomizer");
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            Debug.Log(NebulaGUI.controlFromUI);
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
            isDiffusing = false;
            dutyCycle = 0;
            NebulaManager.SendCommand(stopDiffusionCommand);
            StopCoroutine(DiffusionCoroutine());
        }
    }

    private void StartDiffusion()
    {
        if (!NebulaGUI.controlFromUI)
        {
            isDiffusing = true;
            NebulaManager.SendCommand(startDiffusionCommand); //Send the start signal to Nebula 
            if (diffusionMode == DiffusionMode.Binary) dutyCycle = binaryDutyCycle;
            NebulaManager.SendCommand(changeConfigurationCommand + pwmFrequency + ";" + dutyCycle);
            if (diffusionMode != DiffusionMode.Binary) StartCoroutine(DiffusionCoroutine());
        }
    }

    //Coroutine allowing to control diffusion levels in real time
    private IEnumerator DiffusionCoroutine()
    {
        while (!NebulaGUI.controlFromUI)
        {
            yield return new WaitForSeconds(0.1f); //Avoid overflowding the Arduino
            switch (diffusionMode)
            {
                case DiffusionMode.InverseSquare:
                    dutyCycle = Mathf.Round((1 / (float)Math.Pow(Vector3.Distance(playerHead.position, transform.position), 2))); // Increase duty cycle according to distance
                    break;
                case DiffusionMode.Linear:
                    dutyCycle = Mathf.Round((Mathf.InverseLerp(offsetMinDutyCycle, offsetMaxDutyCycle, Vector3.Distance(playerHead.position, transform.position))) * 100); // Increase duty cycle according to distance
                    break;
            }
            if (dutyCycle <= minimumDutyCycle) dutyCycle = minimumDutyCycle;
            else dutyCycle = maximumDutyCycle;
            if (dutyCycle != previousDutyCycle)
            {
                NebulaManager.SendCommand(changeConfigurationCommand + pwmFrequency + ";" + dutyCycle);
                previousDutyCycle = dutyCycle;
            }
        }
    }

    //Simple method to reset the game object when it fall on the ground
    public void ResetGameObject()
    {
        gameObject.transform.SetPositionAndRotation(originalPosition, originalOrientation);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}

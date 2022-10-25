using System.Collections;
using UnityEngine;
using System.Threading;
using System;

public class NebulaOdorDiffuser : MonoBehaviour
{
    //Define which odor you are going to diffuse. You can spread different smelling objects in the scene
    public enum AtomizerList { Left, Right, Other };
    public AtomizerList atomizer;
    public enum DiffusionMode { Linear, Boolean, Other };
    public DiffusionMode diffusionMode;

    private string enterString;
    private string exitString;

    private Transform playerHead;

    //Define the distance (in m) between the player head and the object before the start of the diffusion
    private float distanceBetweenGameObjectAndPlayer;
    //Define the setpoints used (min and max) in order to adjust odor strength (can me modified in real-time with the provided GUI)
    [HideInInspector]
    private float dutyCycle;
    private float previousSetpoint;
    public int minimumDutyCycle = 1;
    public int maximumDutyCycle = 30;
    [HideInInspector]
    public string propName;
    //Adjust the PWM frequency of the diffusion
    public string pwmFrequency = "100";
    public int booleanDutyCycle = 50;

    Vector3 originalPosition;
    Quaternion originalOrientation;

    private void Awake()
    {
        playerHead = GameObject.Find("NebulaManager").GetComponent<NebulaManager>().playerHead.transform;
        originalOrientation = this.gameObject.transform.rotation;
        originalPosition = this.gameObject.transform.position;
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
            case AtomizerList.Other:
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
            if (diffusionMode == DiffusionMode.Linear)
            {
                LinearDiffusionMode();
            }
            else if (diffusionMode == DiffusionMode.Boolean)
            {
                BooleanDiffusionMode();
            }
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
            if (diffusionMode == DiffusionMode.Linear)
            {
                NebulaManager.nebulaIsDiffusing = false;
                NebulaManager.nebulaSender(exitString);
                StopCoroutine(OdorDiffusionLinearMode());
            }
            else if (diffusionMode == DiffusionMode.Boolean)
            {
                dutyCycle = 0;
                NebulaManager.currentDutyCycle = dutyCycle;
                NebulaManager.nebulaSender(exitString);
                NebulaManager.nebulaIsDiffusing = false;
            }
        }
    }

    private void LinearDiffusionMode()
    {
        if (!NebulaGUI.manualOverride)
        {
            NebulaManager.nebulaIsDiffusing = true;
            NebulaManager.nebulaSender(enterString); //Send the start signal to Nebula 
            StartCoroutine(OdorDiffusionLinearMode()); //Start a coroutine in order to adjust odor strength in real time
        }
    }

    public float updateDistance()
    {
        //Calculate in real-time the distance between the object to smell and the player head
        distanceBetweenGameObjectAndPlayer = (Vector3.Distance(playerHead.position, transform.position)-0.35f);
        return distanceBetweenGameObjectAndPlayer;
    }

    private IEnumerator OdorDiffusionLinearMode()
    {
        while (!NebulaGUI.manualOverride)
        {
            yield return new WaitForSeconds(0.1f); //Avoid overflowding the Arduino
            dutyCycle = Mathf.Round(1-((updateDistance()/0.3f) * maximumDutyCycle));
            //dutyCycle = Mathf.Round(maximumDutyCycle/(updateDistance()+0.2f));
            if (dutyCycle <= minimumDutyCycle) dutyCycle = minimumDutyCycle;
            if (dutyCycle >= maximumDutyCycle) dutyCycle = maximumDutyCycle;
            if (dutyCycle != previousSetpoint)
            {
                NebulaManager.nebulaSender("C" + pwmFrequency + ";" + dutyCycle);
                previousSetpoint = dutyCycle;
                NebulaManager.currentDutyCycle = dutyCycle;
            }
        }
    }

    private void BooleanDiffusionMode()
    {
        dutyCycle = booleanDutyCycle;
        NebulaManager.currentDutyCycle = dutyCycle;
        NebulaManager.nebulaSender(enterString);
        NebulaManager.nebulaSender("C" + pwmFrequency + ";" + dutyCycle);
        NebulaManager.nebulaIsDiffusing = true;
    }

    public void ResetGameObject()
    {
        this.gameObject.transform.position = originalPosition;
        this.gameObject.transform.rotation = originalOrientation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }


    private void OnApplicationQuit()
    {
        StopAllCoroutines();
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        NebulaManager.nebulaSender("S");
        try
        {
            //Send stop value Nebula
            Thread.Sleep(200);
            NebulaManager.thread.Abort();
            NebulaManager.nebulaSerial.Close();
        }
        catch (Exception) {  }
#endif
    }
}

using System.Collections;
using UnityEngine;
using System.Threading;

public class Nebula_MULTIPLATFORM_WIN_ANDROID : MonoBehaviour
{
    //Define which odor you are going to diffuse. You spread different smelling objects in your scene
    public enum AtomizerList { Left, Right, Other };
    public AtomizerList atomizer;

    private string enterString;
    private string exitString;

    private Transform playerHead;

    //Define the distance (in m) between the player head and the object before the start of the diffusion
    public float smellRadius = 0.6f;
    private float distanceFromObject;

    //Define the setpoints used (min and max) in order to adjust odor strength
    [HideInInspector]
    public float dutyCycle;
    private float previousSetpoint;
    public int minimumDutyCycle = 1;
    public int maximumDutyCycle = 30;
    //Adjust the PWM frequency of the diffusion
    public string pwmFrequency = "100";

    public static bool isDiffusing;
    static bool COMPortInitialized;

    AndroidJavaObject _pluginInstance;
#if (UNITY_ANDROID)
    [HideInInspector]
#endif
    public bool useNebulaGUI = true;

    private void Awake()
    {
        playerHead = GameObject.Find("Main Camera").transform; //Get the transform of the Main Camera, mandatory to measure the distance the player head and the object
        
        if (!COMPortInitialized) //Adjust the manner to open COM port depending of the platform used
        {
#if (UNITY_ANDROID)
            COMPortInitialized = InitializePlugin("fr.enise.unitymaoplugin.MAOPlugin"); //We use an ANdroid ARchive (AAR) to handle the communication between a Quest and the MAO
#elif (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            COMPortInitialized = Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.InitUSBSerial(); //We use an another script when on Unity Editor and player to handle the communication for better reading
        if (useNebulaGUI) this.gameObject.AddComponent<Nebula_GUI>();
#endif
        }

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
                Debug.Log("Incorrect atomizer location, check AtomizerList");
                break;
        }
    }

    void Update()
    {
        //Calculate in real-time the distance between the object to smell and the player head
        distanceFromObject = (Vector3.Distance(playerHead.position, transform.position) - 0.2f);

        //Activate the diffusion when the player enter in the smell radius
        if (distanceFromObject < smellRadius && !isDiffusing && !Nebula_GUI.manualOverride)
        {
            isDiffusing = true;
            usbSend(enterString); //Start the diffusion 
            StartCoroutine(OdorDiffusion()); //Start a coroutine in order to adjust odor strength in real time
        }
    }

#if (UNITY_ANDROID)
    //Plugin initializer used when scene is built on Quest. Mandatory to use the java android library
    bool InitializePlugin(string pluginName)
    {
        var pluginClass = new AndroidJavaClass("fr.enise.unitymaoplugin.MAOPlugin");

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

        _pluginInstance = pluginClass.CallStatic<AndroidJavaObject>("InitializePlugin", context, new MAOPluginCallback());
        Debug.Log("Plugin initialized !");

        Thread.Sleep(2000);

        return true;
    }
#endif

    public void usbSend(string data)
    {
#if (UNITY_ANDROID)
        if (_pluginInstance != null) _pluginInstance.Call("SendMessage", data + "\n");

#elif (UNITY_EDITOR || UNITY_STANDALONE_WIN)

        Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.serial.Write(data + "\n");
#endif
    }

    private IEnumerator OdorDiffusion()
    {
        while (distanceFromObject < smellRadius && !Nebula_GUI.manualOverride)
        {
            yield return new WaitForSeconds(0.1f); //Avoid overflowding the Arduino
            dutyCycle = Mathf.Round((1 - (distanceFromObject / smellRadius)) * maximumDutyCycle);
            if (dutyCycle <= minimumDutyCycle) dutyCycle = minimumDutyCycle;
            if (dutyCycle >= maximumDutyCycle) dutyCycle = maximumDutyCycle;
            //Pre-format the consigna sent to the arduino
            if (dutyCycle != previousSetpoint)
            {
                usbSend("C" + pwmFrequency + ";" + dutyCycle);
                previousSetpoint = dutyCycle;
            }
        }
        isDiffusing = false;
        usbSend(exitString);
        StopCoroutine(OdorDiffusion());
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN && !UNITY_ANDROID)
        Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.thread.Abort();
        Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.serial.Close();
#endif
    }
}

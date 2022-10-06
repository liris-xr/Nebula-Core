using System.Collections;
using UnityEngine;
using System.Threading;

public class Nebula_MULTIPLATFORM_WIN_ANDROID : MonoBehaviour
{
    //Define which odor you are going to diffuse. You can spread different smelling objects in the scene
    public enum AtomizerList { Left, Right, Other };
    public AtomizerList atomizer;

    private string enterString;
    private string exitString;

    private Transform playerHead;

    //Define the distance (in m) between the player head and the object before the start of the diffusion
    public float smellRadius = 0.6f;
    private float distanceBetweenPropAndPlayer;

    //Define the setpoints used (min and max) in order to adjust odor strength (can me modified in real-time with the provided GUI)
    [HideInInspector]
    public float dutyCycle;
    private float previousSetpoint;
    public int minimumDutyCycle = 1;
    public int maximumDutyCycle = 30;
    [HideInInspector]
    public string propName;
    //Adjust the PWM frequency of the diffusion
    public string pwmFrequency = "100";

    public static bool isDiffusing;
    static bool COMPortInitialized;

    Vector3 originalPosition;
    Quaternion originalOrientation;

    static AndroidJavaObject _pluginInstance;
#if (UNITY_ANDROID)
    [HideInInspector]
#endif
    public bool useNebulaGUI = true;
#if (UNITY_ANDROID)
    [HideInInspector]
#endif
    public static bool useListener = false;

    private void Awake()
    {
        playerHead = GameObject.Find("Main Camera").transform; //Get the transform of the Main Camera, mandatory to measure the distance the player head and the object
        originalOrientation = this.gameObject.transform.rotation;
        originalPosition = this.gameObject.transform.position;
        
        if (!COMPortInitialized) //Adjust the manner to open COM port depending of the platform used
        {
#if (UNITY_ANDROID)
            COMPortInitialized = InitializePlugin("fr.enise.unitymaoplugin.MAOPlugin"); //We use an ANdroid ARchive (AAR) to handle the communication between a Quest and the MAO
#elif (UNITY_EDITOR || UNITY_STANDALONE_WIN)
            COMPortInitialized = Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.InitUSBSerial(); //We use an another script when on Unity Editor and player to handle the communication for better reading
            if (useNebulaGUI) InitializeGUI();
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
        getDistanceBetweenPropAndPlayer();
        //Activate the diffusion when the player enter in the smell radius
        if (distanceBetweenPropAndPlayer < smellRadius && !isDiffusing && !Nebula_GUI.manualOverride)
        {
            isDiffusing = true;
            nebulaSender(enterString); //Send the start signal to Nebula 
            StartCoroutine(OdorDiffusion()); //Start a coroutine in order to adjust odor strength in real time
        }
    }

#if (UNITY_ANDROID)
    //Plugin initializer used when scene is built on Quest. Mandatory to use the java android library
    static bool InitializePlugin(string pluginName)
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

    public void InitializeGUI()
    {
        this.gameObject.AddComponent<Nebula_GUI>();
    }

    public void ResetGameObject()
    {
        this.gameObject.transform.position = originalPosition;
        this.gameObject.transform.rotation = originalOrientation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public float getDistanceBetweenPropAndPlayer()
    {
        //Calculate in real-time the distance between the object to smell and the player head
        distanceBetweenPropAndPlayer = (Vector3.Distance(playerHead.position, transform.position) - 0.2f);
        return distanceBetweenPropAndPlayer; 
    }

    public void nebulaSender(string data)
    {
#if (UNITY_ANDROID)
        if (_pluginInstance != null) _pluginInstance.Call("SendMessage", data + "\n");

#elif (UNITY_EDITOR || UNITY_STANDALONE_WIN)

        Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.serial.Write(data + "\n");
#endif
    }

    private IEnumerator OdorDiffusion()
    {
        while (distanceBetweenPropAndPlayer < smellRadius && !Nebula_GUI.manualOverride)
        {
            yield return new WaitForSeconds(0.1f); //Avoid overflowding the Arduino
            dutyCycle = Mathf.Round((1 - (distanceBetweenPropAndPlayer / smellRadius)) * maximumDutyCycle);
            if (dutyCycle <= minimumDutyCycle) dutyCycle = minimumDutyCycle;
            if (dutyCycle >= maximumDutyCycle) dutyCycle = maximumDutyCycle;
            //Pre-format the consigna sent to the arduino
            if (dutyCycle != previousSetpoint)
            {
                nebulaSender("C" + pwmFrequency + ";" + dutyCycle);
                previousSetpoint = dutyCycle;
            }
        }
        isDiffusing = false;
        nebulaSender(exitString);
        StopCoroutine(OdorDiffusion());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
            ResetGameObject();
        }
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        nebulaSender("S");                                             //Send stop value Nebula
        Thread.Sleep(200);
        Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.thread.Abort();
        Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.serial.Close();
#endif
    }
}

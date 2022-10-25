using System;
using UnityEngine;
using System.ComponentModel;
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
using System.IO.Ports;
#endif
using System.Threading;

//Class allowing to use nebulaSerial communication while using Unity Editor or Unity built project

public class NebulaManager : MonoBehaviour
{
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
    //Initialize the nebulaSerial port.
    public static SerialPort nebulaSerial;
#endif
    [SerializeField]
    private int baudRate = 115200;
    //By default the correct port is defined automaticaly by using a single handshake (FindPort method)
    //You can manually provide the correct port using the bool and the string bellow 
    [SerializeField]
    private bool defineManuallyCOMPort = false;
    [SerializeField]
    private string NebulaPort = "";
    public GameObject playerHead;
    //Thread used to read and print on the console everything that your Nebula is writing on it
    public static Thread thread;
    public bool useListener;
    [HideInInspector]
    public static bool nebulaIsDiffusing;
    public bool useNebulaGUI = true;
    public static float currentDutyCycle;
#if (UNITY_ANDROID)
    static AndroidJavaObject _pluginInstance;
    [HideInInspector]
#endif


    //#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)

    private void Awake()
    {
#if (UNITY_ANDROID)
            InitializePlugin("fr.enise.unitymaoplugin.MAOPlugin"); //We use an Android ARchive (AAR) to handle the communication between a Quest and the MAO
#elif (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        if (useNebulaGUI) InitializeGUI();

        if (!defineManuallyCOMPort) NebulaPort = FindPort("Nebula");
        try
        {
            nebulaSerial = new SerialPort(NebulaPort, baudRate);
            nebulaSerial.Parity = Parity.None;
            nebulaSerial.StopBits = StopBits.One;
            nebulaSerial.DataBits = 8;
            nebulaSerial.DtrEnable = true;
            nebulaSerial.Open();
            thread = new Thread(ThreadLoop);
            thread.Start();
        }
        catch (Exception)
        {
            Debug.LogError("Nebula not detected : check COM ports");
        }
#endif
        SphereCollider HeadCollider = playerHead.AddComponent<SphereCollider>();
        HeadCollider.radius = 0.3f;
        HeadCollider.isTrigger = true;
    }
    //#endif

    //Method looking for the MAO, given the necessary handshake => here "Nebula"
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
    private string FindPort(string handShake)
    {
        string[] portList = SerialPort.GetPortNames();
        foreach (string port in portList)
        {
            if (port != "COM1")
            {
                try
                {
                    SerialPort currentPort = new SerialPort(port, baudRate);
                    currentPort.Parity = Parity.None;
                    currentPort.StopBits = StopBits.One;
                    currentPort.DataBits = 8;
                    currentPort.DtrEnable = true;
                    if (!currentPort.IsOpen)
                    {
                        currentPort.Open();
                        currentPort.WriteLine(handShake);
                        string received = currentPort.ReadLine();
                        currentPort.Close();
                        if (received.Equals(handShake))
                        {
                            Debug.Log("Nebula found on " + port);
                            return port;
                        }
                    }
                }
                catch (Exception) {   }
            }

        }
        return null;
    }

    //Thread dedicated to listen the nebulaSerial port nad read datas sent from Nebula
    private void ThreadLoop()
    {
        while (true)
        {
            if (nebulaSerial.BytesToRead > 0)
            {
                try
                {
                    string data = nebulaSerial.ReadTo("\n"); //gathering working return from Nebula
                    if (useListener) Debug.Log(data);
                }
                catch (Exception) { }
            }
        }
    }

        public void InitializeGUI()
    {
        this.gameObject.AddComponent<NebulaGUI>();
    }
#endif
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

    public static void nebulaSender(string data)
    {
#if (UNITY_ANDROID)
        if (_pluginInstance != null)
        {
            _pluginInstance.Call("SendMessage", data + "\n");
            Debug.Log("Sending " + data);
        }

#elif (UNITY_EDITOR || UNITY_STANDALONE_WIN)
        try
        {
            nebulaSerial.Write(data + "\n");
        }
        catch (Exception) { }
#endif
    }


}

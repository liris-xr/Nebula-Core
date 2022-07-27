using System;
using UnityEngine;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN && !UNITY_ANDROID)
using System.IO.Ports;
#endif
using System.Threading;

//Class allowing to use serial communication while using Unity Editor or Unity built project

public static class Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER
{
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN && !UNITY_ANDROID)
    //Initialize the serial port.
    public static SerialPort serial;
    public static int baudRate = 115200;
    //By default the correct port is defined automaticaly by using a single handshake (FindPort method)
    //You can manually providethe correct port using the bool and the string bellow 
    public static bool defineManuallyCOMPort = false;
    public static string NebulaPort = "COM8";

    //Thread used to read and print on the console everything trhat your Nebula is writing on it
    public static Thread thread;

    public static bool InitUSBSerial()
    {
        if (!defineManuallyCOMPort) NebulaPort = FindPort("Nebula");
        serial = new SerialPort(NebulaPort, baudRate);
        serial.Parity = Parity.None;
        serial.StopBits = StopBits.One;
        serial.DataBits = 8;
        serial.DtrEnable = true;
        serial.Open();
        thread = new Thread (ThreadLoop);
        thread.Start();
        return true;
    }

    //Method looking for the MAO, given the handshake necessary => here "Nebula"
    private static string FindPort(string handShake)
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
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

        }
        return null;
    }

    //Thread dedicated to listen the serial port nad read datas sent from Nebula
    public static void ThreadLoop()
    {
        while (true)
        {
            if (serial.BytesToRead > 0)
            {
                string data = serial.ReadTo("\n"); //gathering working return from Nebula
                Debug.Log(data);
            }
        }
    }
#endif
}

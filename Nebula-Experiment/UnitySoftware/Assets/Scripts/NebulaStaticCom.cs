using System;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

//Class allowing to use serial communication while using Unity Editor or Unity built project

public static class NebulaStaticCom
{
    //Initialize the serial port.
    public static SerialPort serial;
    public static int baudRate = 115200;
    //By default the correct port is defined automaticaly by using a single handshake (FindPort method)
    //You can manually provide the correct port using the bool and the string bellow 
    public static bool defineManuallyCOMPort = false;
    public static string nebulaPort = "COM3";

    //Thread used to read and print on the console everything that Nebula is writing on it
    public static Thread thread;

    public static bool InitUSBSerial()
    {
        if (!defineManuallyCOMPort) nebulaPort = FindPort("Nebula");
        serial = new SerialPort(nebulaPort, baudRate);
        serial.Parity = Parity.None;
        serial.StopBits = StopBits.One;
        serial.DataBits = 8;
        serial.DtrEnable = true;
        serial.Open();
        thread = new Thread(ThreadLoop);
        thread.Start();
        return true;
    }

    //Method looking for the Nebula, given the handshake necessary 
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

    public static void MAOSender(int diffusionType)
    {
        switch (diffusionType)
        {
            case 0:
                serial.WriteLine("L");
                serial.WriteLine("C100;0");
                break;
            case 1:
                serial.WriteLine("L");
                serial.WriteLine("C100;10");
                break;
            case 2:
                serial.WriteLine("L");
                serial.WriteLine("C100;50");
                break;
            case -1: //end diffusion value
                serial.WriteLine("l");
                serial.WriteLine("r");
                break;
        }
    }

    public static void ThreadLoop()
    {
        while (true)
        {

            if (serial.BytesToRead > 0)
            {
                string data = serial.ReadTo("\n"); 
            }

        }
    }

}
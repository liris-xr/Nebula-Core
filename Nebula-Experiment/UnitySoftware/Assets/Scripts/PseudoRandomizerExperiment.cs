using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO.Ports;
using System.IO;
using System;


public class PseudoRandomizerExperiment : MonoBehaviour
{
    public List<int> diffusionNumber = new List<int>();
    public List<int> diffusionType;
    public List<float> diffusionDetectedTime = new List<float>();
    public List<int> notation = new List<int>();
    public List<float> notationTime = new List<float>();
    public string experimentname;
    string pathCSV;
    StreamWriter sw;

    void Awake()
    {
        diffusionType = CreateList(2, 1, 0, 2, 0, 2, 1, 0, 1, 0, 2, 1, 2, 0, 2, 1, 0, 1);
      
        for (int i = 0; i < 20; i++)
        {
            diffusionNumber.Add(i + 1);
            diffusionDetectedTime.Add(-1);
            notation.Add(-1);
            notationTime.Add(-1);
        }
        if (experimentname == "")
        {
            Debug.Log("CAUTION : experiment name empty");
        }

    }

    public void SaveCSVFile()
    {

        pathCSV = @"CSV\" + experimentname + "_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm") + ".csv";

        if (!File.Exists(pathCSV))
        {
            using (sw = File.CreateText(pathCSV))
            {
                sw.WriteLine("Diffusion_Number;Diffusion_Type;Odor_Detected;Odor_Notation;Notation_Time");
            }
        }
        using (sw = File.AppendText(pathCSV))
        {
            for (int k = 0; k < 20; k++)
            {
                sw.WriteLine(diffusionNumber[k] + ";" + diffusionType[k] + ";" + diffusionDetectedTime[k] + ";" + notation[k] + ";" + notationTime[k]);
            }
        }
    }

    List<T> CreateList<T>(params T[] values)
    {
        return new List<T>(values);
    }

}


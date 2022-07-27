using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nebula_GUI : MonoBehaviour
{
    public static bool manualOverride = false;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN && !UNITY_ANDROID)
    static int pwmFrequency = 100;
    [HideInInspector]
    float leftDutyCyclef;
    [HideInInspector]
    int leftDutyCycle;
    [HideInInspector]
    int leftpreviousDutyCyle;

    void OnGUI()
    {
        if (!manualOverride) leftDutyCyclef = this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().dutyCycle;
        GUI.Box(new Rect(10, 10, 250, 150), "Nebula manual activation");
        GUI.Box(new Rect(275, 10, 250, 150), "Nebula global settings");
        GUI.Label(new Rect(280, 30, 150, 30), "Minimal duty cycle");
        GUI.Label(new Rect(280, 80, 150, 30), "Maximal duty cyle");
        GUI.Label(new Rect(320, 50, 100, 30), this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle.ToString());
        GUI.Label(new Rect(320, 100, 100, 30), this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle.ToString());

        
        if (GUI.Button(new Rect(340, 50, 30, 30), "+"))
        {
            if (this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle <= 100) this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle++;
            Debug.Log("Min duty cycle has been set to = " + this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle + " %");
        }
        if (GUI.Button(new Rect(280, 50, 30, 30), "-"))
        {
            if (this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle > 1) this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle--;
            Debug.Log("Min duty cycle has been set to = " + this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle + " %");
        }
        if (GUI.Button(new Rect(340, 100, 30, 30), "+"))
        {
            if (this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle < 100) this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle++;
            Debug.Log("Max duty cycle has been set to = " + this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle + " %");
        }
        if (GUI.Button(new Rect(280, 100, 30, 30), "-"))
        {
            if (this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle > 1) this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle--;
            Debug.Log("Max duty cycle has been set to = " + this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle + " %");
        }

        if (!Nebula_MULTIPLATFORM_WIN_ANDROID.isDiffusing)
        {
            if (GUI.Button(new Rect(20, 40, 200, 20), "Start left atomization"))
            {
                manualOverride = true;
                Nebula_MULTIPLATFORM_WIN_ANDROID.isDiffusing = true;
                Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.serial.Write("L" + "\n");
                StartCoroutine(manualDiffusion()); 
            }
        }
        else
        {
            if (!manualOverride) GUI.Label(new Rect(25, 90, 150, 30), "Current duty cyle : " + leftDutyCycle.ToString() + "%");
            else GUI.Label(new Rect(25, 90, 150, 30), "Duty cyle : " + leftDutyCycle.ToString() +"%");
            leftDutyCyclef = GUI.HorizontalSlider(new Rect(25, 110, 100, 30), leftDutyCyclef, this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().minimumDutyCycle, this.GetComponent<Nebula_MULTIPLATFORM_WIN_ANDROID>().maximumDutyCycle);
            leftDutyCycle = (int)Mathf.Round(this.leftDutyCyclef);

            if (GUI.Button(new Rect(20, 40, 200, 20), "Stop Atomization"))
            {
                if (Nebula_MULTIPLATFORM_WIN_ANDROID.isDiffusing) manualOverride = true;
                else manualOverride = !manualOverride;
                Nebula_MULTIPLATFORM_WIN_ANDROID.isDiffusing = false;
            }
        }
    }

    private IEnumerator manualDiffusion()
    {
        while (Nebula_MULTIPLATFORM_WIN_ANDROID.isDiffusing)
        {
            yield return new WaitForSeconds(0.1f); 
            if (leftDutyCycle != leftpreviousDutyCyle)
            {
                leftpreviousDutyCyle = leftDutyCycle;
                Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.serial.Write("C" + pwmFrequency.ToString() + ";" + leftDutyCycle + "\n");
            }
        }
        Nebula_MULTIPLATFORM_WIN_ANDROID.isDiffusing = false;
        Nebula_WINSTANDALONE_UNITYEDITOR_INITIALIZER.serial.Write("l" + "\n");
        StopCoroutine(manualDiffusion());
        Debug.Log("Stopped left diffusion");
    }
#endif
}

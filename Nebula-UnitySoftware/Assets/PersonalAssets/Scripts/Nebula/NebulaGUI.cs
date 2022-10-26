using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NebulaGUI : MonoBehaviour
{
    public static bool manualOverride = false;
#if (!UNITY_ANDROID && UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
    static int pwmFrequency = 100;
    [HideInInspector]
    float dutyCyclef;
    [HideInInspector]
    int manualDC;
    [HideInInspector]
    int previousDutyCycle;
    public float DesignWidth = 1280.0f;
    public float DesignHeight = 720.0f;
    [SerializeField]
    private int minManualDutyCycle = 1;
    [SerializeField]
    private int maxManualDutyCycle = 100;
    [SerializeField]
    private int manualPWMfreq = 100;

    void OnGUI()
    {
        //Calculate change aspects
        float resX = (float)(Screen.width) / DesignWidth;
        float resY = (float)(Screen.height) / DesignHeight;
        //Set matrix
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(resX, resY, 1));
        if (!manualOverride) dutyCyclef = NebulaManager.currentDutyCycle;
        GUI.Box(new Rect(10, 10, 250, 150), "Nebula manual activation");

        if (!NebulaManager.nebulaIsDiffusing)
        {
            if (GUI.Button(new Rect(20, 40, 200, 20), "Start left atomization"))
            {
                manualOverride = true;
                NebulaManager.nebulaIsDiffusing = true;
                NebulaManager.NebulaSender("L");
                NebulaManager.NebulaSender("C" + manualPWMfreq.ToString() + "; " + manualDC);
                StartCoroutine(ManualDiffusion()); 
            }
        }
        else
        {
            if (!manualOverride) GUI.Label(new Rect(25, 90, 150, 30), "Current duty cyle : " + NebulaManager.currentDutyCycle.ToString() + "%");
            else GUI.Label(new Rect(25, 90, 150, 30), "Duty cyle : " + NebulaManager.currentDutyCycle.ToString() +"%");
            dutyCyclef = GUI.HorizontalSlider(new Rect(25, 110, 100, 30), dutyCyclef, minManualDutyCycle, maxManualDutyCycle);
            manualDC = (int)Mathf.Round(this.dutyCyclef);

            if (GUI.Button(new Rect(20, 40, 200, 20), "Stop Atomization"))
            {
                if (NebulaManager.nebulaIsDiffusing) manualOverride = true;
                else manualOverride = !manualOverride;
                NebulaManager.nebulaIsDiffusing = false;
            }
        }
    }

    private IEnumerator ManualDiffusion()
    {
        while (NebulaManager.nebulaIsDiffusing)
        {
            yield return new WaitForSeconds(0.1f); 
            if (manualDC != previousDutyCycle)
            {
                previousDutyCycle = manualDC;
                NebulaManager.currentDutyCycle = manualDC;
                NebulaManager.NebulaSender("C" + pwmFrequency.ToString() + "; " + manualDC);
            }
        }
        NebulaManager.nebulaIsDiffusing = false;
        NebulaManager.NebulaSender("l");
        StopCoroutine(ManualDiffusion());
        Debug.Log("Stopped left diffusion");
    }
#endif
}

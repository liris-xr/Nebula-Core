using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSlider : MonoBehaviour
{

    public float testSliderValue;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        testSliderValue = GUI.HorizontalSlider(new Rect(0, 0, 100, 20), testSliderValue, 0, 100);
    }
}

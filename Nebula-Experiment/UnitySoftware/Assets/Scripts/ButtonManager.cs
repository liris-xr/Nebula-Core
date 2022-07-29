using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ButtonManager : MonoBehaviour
{
    public Button[] listButtons;

    public bool CheckIfAnswered()
    {
        foreach (Button button in listButtons)
        {
            if (!button.interactable)
            {
                return true;
            }
        }
        return false;
    }

    public int QuestionNote()
    {
        foreach (Button button in listButtons)
        {
            if (!button.interactable)
            {
                return int.Parse(button.gameObject.name);
            }
        }
        return -1;
    }

    private void OnEnable()
    {
        SetAllButtonsInteractable();
    }

    // Update is called once per frame

    public void SetAllButtonsInteractable()
    {
        foreach (Button button in listButtons)
        {
            button.interactable = true;
        }
    }

    public void OnButtonClicked(Button clickedButton)
    {
        int buttonIndex = System.Array.IndexOf(listButtons, clickedButton);

        if (buttonIndex == -1)
            return;

        SetAllButtonsInteractable();

        clickedButton.interactable = false;
    }

    private void OnDisable()
    {
        foreach (Button button in listButtons)
        {
            button.interactable = false;
        }
    }
}

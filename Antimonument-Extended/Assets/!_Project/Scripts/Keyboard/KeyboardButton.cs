using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardButton : MonoBehaviour
{
    //Class to show the appropiate Name on Keyboard
    Keyboard keyboard;
    TextMeshProUGUI buttonText;

    void Start()
    {
        keyboard = GetComponentInParent<Keyboard>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText.text.Length == 1)
        {
            NameToButtonText();
            //GetComponentInChildren<ButtonVRS>().onRelease.AddListener(delegate { keyboard.InsertChar(buttonText.text); });
        }

    }

    public void NameToButtonText()
    {
        buttonText.text = gameObject.name;
    }

    
}

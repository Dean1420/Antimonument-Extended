using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Keyboard : MonoBehaviour
{
    public TextMeshPro inputField;  //Main Inputfield to display the text
    public TextMeshPro inputField2;

    [Header("Debug - live sichtbares Textfeld, unabhängig von der RenderTexture-Pipeline")]
    public TextMeshProUGUI debugText;  // Im Inspector auf ein simples, garantiert sichtbares Canvas-Text-Element ziehen

    GameObject normalButtons;   //normal letter buttons
    GameObject capsButtons;      //capital letter buttons
    public GameObject KanjiNormal;
    public GameObject KanjiCaps;
    public GameObject KatakanaNormal;
    public GameObject KatakanaCaps;
    public GameObject deleteButtonMove;
    
    private bool Katakana = false;
    private bool caps;  //Flag to indicate if caps lock is active
    public bool isPressed;  //Flag to indicate if the keyboard is currently pressed
    public static GameObject textOnRunTime;
    public TextMeshProUGUI LanguageText;
    public TextMeshProUGUI CapsText;
    public TextMeshProUGUI DeleteText;
    public TextMeshProUGUI CreateText;
    string kata = "カタカナ";
    string kanji = "Romanji";
    string CapsRomanji = "Caps";
    string CapsKata = "大・小";
    string DeleteRomanji = "Delete";
    string DeleteKata = "削除";
    string CreateRomanji = "Create";
    string CreateKata = "終了";
    void Start()
    {
        textOnRunTime = this.gameObject;
        isPressed = false;
        caps = false;
        normalButtons = KanjiNormal;
        capsButtons = KanjiCaps;
        normalButtons.SetActive(true);
        capsButtons.SetActive(false);

        Debug.Log("Keyboard Start(): inputField ist " + (inputField == null ? "NULL!" : "zugewiesen (" + inputField.name + ")"));
    }
    private void Update()
    {
        
    }
    //Method to insert a character into the input field
    public void InsertChar(string c)
    {
        Debug.Log("InsertChar aufgerufen mit: '" + c + "'");

        if (inputField == null)
        {
            Debug.LogError("inputField ist NULL - im Inspector am Keyboard-Script zuweisen!");
        }
        else
        {
            inputField.text += c;
        }

        UpdateDebugText();
    }
    //Method to delete the last character from the input field
    public void DeleteChar()
    {
        if (inputField != null && inputField.text.Length > 0)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
        UpdateDebugText();
    }
    //Method to insert a space character into the input field
    public void InsertSpace()
    {
        if (inputField != null) inputField.text += " ";
        UpdateDebugText();
    }

    void UpdateDebugText()
    {
        if (debugText != null && inputField != null)
        {
            debugText.text = "[DEBUG] " + inputField.text;
        }
    }
    //Method for Capslock
    public void CapsPressed()
    {
        if (!caps)
        {
            normalButtons.SetActive(false);
            capsButtons.SetActive(true);
            caps = true;
        }
        else
        {
            capsButtons.SetActive(false);
            normalButtons.SetActive(true);
            caps = false;
        }
    }
    public void LanguageSwitch()
    {
        if (Katakana == false)
        {
            capsButtons.SetActive(false);
            normalButtons.SetActive(false);
            Katakana = true;
            normalButtons = KatakanaNormal;
            capsButtons = KatakanaCaps;
            LanguageText.text =  kanji;
            DeleteText.text = DeleteKata;
            CapsText.text = CapsKata;
            CreateText.text = CreateKata;
            capsButtons.SetActive(false);
            normalButtons.SetActive(true);
            caps = false;
            deleteButtonMove.transform.localPosition += new Vector3(0.06f,0,0);
        }
        else
        {
            capsButtons.SetActive(false);
            normalButtons.SetActive(false);
            Katakana = false;
            normalButtons = KanjiNormal;
            capsButtons = KanjiCaps;
            LanguageText.text =  kata;
            
            DeleteText.text = DeleteRomanji;
            CapsText.text = CapsRomanji;
            CreateText.text = CreateRomanji;
            capsButtons.SetActive(false);
            normalButtons.SetActive(true);
            caps = false;
            deleteButtonMove.transform.localPosition -= new Vector3(0.06f,0,0);
        }
    }
}
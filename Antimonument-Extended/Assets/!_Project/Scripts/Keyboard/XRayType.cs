using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System;

public class XRayType : MonoBehaviour
{
    string s;
    string sName;
    char c;

    [SerializeField] Keyboard keyboard;

    GameObject ColliderHit;

    Vector3 buttonPosition;  //Orginal position
    AudioSource sound;

    private LayerMask LayerBoard;   //Layermask for Keyboard

    public GameObject newProtestShild;   //Prefab
    public GameObject ProtestTransform;  //transform reference

    public InputActionReference triggerAction;

    public RenderTexture rTex;    //rendertexture for texture conversion
    public Texture2D tex2;    //temptexture

    void OnEnable()
    {
        triggerAction.action.Enable();
    }

    void OnDisable()
    {
        triggerAction.action.Disable();
    }

    void Start()
    {
        LayerBoard = LayerMask.NameToLayer("Keyboard");
        Debug.Log("Start() abgeschlossen, LayerBoard = " + LayerBoard);
    }

    void Update()
    {
        RayCastKeyboard();
    }

    void RayCastKeyboard()
    {
        var ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (triggerAction.action.WasPressedThisFrame())
            {
                Debug.Log("TRIGGER GEDRÜCKT erkannt!");
            }

            if (triggerAction.action.WasPressedThisFrame() && keyboard.isPressed == false && hit.collider.tag != "Keyboard")
            {
                if (hit.transform.gameObject.layer == LayerBoard)
                {
                    Debug.Log("Taste getroffen: " + hit.transform.gameObject.name);

                    ColliderHit = hit.transform.gameObject;
                    buttonPosition = ColliderHit.transform.parent.GetChild(1).localPosition;

                    s = ColliderHit.transform.parent.gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
                    sName = ColliderHit.transform.parent.gameObject.name;
                    Debug.Log(sName);

                    if (s.Length == 1)
                    {
                        keyboard.InsertChar(s);
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Delete Button")
                    {
                        keyboard.DeleteChar();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Caps Button")
                    {
                        keyboard.CapsPressed();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Create Button")
                    {
                        NewShield();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Space Button")
                    {
                        keyboard.InsertSpace();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else
                    {
                        keyboard.LanguageSwitch();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }

                    keyboard.isPressed = true;
                }
            }
            if (triggerAction.action.WasReleasedThisFrame() && keyboard.isPressed == true && ColliderHit.transform.parent.localPosition != buttonPosition)
            {
                ColliderHit.transform.parent.GetChild(1).localPosition = buttonPosition;
                keyboard.isPressed = false;
            }
        }
    }

    //Create a new protest shield by instantiating the prefab and applying the texture
    void NewShield()
    {
        GameObject tempShield = Instantiate(newProtestShild, ProtestTransform.transform.position, Quaternion.Euler(0, -70, 0));
        tempShield.name = "NewProtestShield";
        GameObject quad = tempShield.transform.Find("Wood").Find("Sign").Find("Quad").gameObject;
        Material newShield = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        // Kopiere die Eigenschaften des aktuellen Materials in das neue Material
        newShield.CopyPropertiesFromMaterial(quad.GetComponent<MeshRenderer>().material);
        StartCoroutine(ConvertToTextureCoroutine(quad, newShield));
    }

    //Converts the quad texture to a RenderTexture and then to a Texture 2D
    private IEnumerator ConvertToTextureCoroutine(GameObject quad, Material standard)
    {
        // Einen Frame warten, damit die CanvasCamera garantiert schon gerendert hat,
        // bevor wir per ReadPixels aus der RenderTexture auslesen
        yield return new WaitForEndOfFrame();

        RenderTexture.active = rTex;
        tex2 = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
        tex2.name = "ProtestShieldtexture";
        // ReadPixels looks at the active RenderTexture.
        tex2.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex2.Apply();
        quad.GetComponent<MeshRenderer>().material = standard;
        quad.GetComponent<MeshRenderer>().material.mainTexture = tex2;
        RenderTexture.active = null;
    }
}
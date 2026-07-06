using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;
using System;
using UnityEngine.InputSystem;

public class XRayType1 : MonoBehaviour
{
    public LineRenderer laserLineRenderer;    //Raycastlinerenderer
    public float laserMaxLength = 6f;

    public InputActionReference triggerAction;
    
    string s;
    string sName;
    char c;

    [SerializeField ]Keyboard keyboard;

    GameObject ColliderHit;

    Vector3 buttonPosition;  //Orginal position
    AudioSource sound;

    public Material matLine;   //material for Laser

    private LayerMask LayerBoard;   //Layermask for Keyboard

    public GameObject newProtestShild;   //Prefab

    public GameObject ProtestTransform;  //transform reference

    [SerializeField] SoundManager soundManager;

    GameObject laserObject;      //Gameobject for the laser
    public RenderTexture rTex;    //rendertexture for texture conversion
    public Texture2D tex2;    //temptexture


    // Start is called before the first frame update
    void Start()
    {
       //Initialize the laser line renderer
        matLine.color = Color.green;
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        laserLineRenderer.SetPositions(initLaserPositions);
        laserLineRenderer.startWidth = 0.01f;            //Start Width for line
        laserLineRenderer.endWidth = 0.001f;            //end Widtj for the Line

        LayerBoard = LayerMask.NameToLayer("Keyboard");
    }

    // Update is called once per frame
    void Update()
    {
        RayCastKeyboard();   //Perfrom Raycast from they Controller
    }

    //Shoots a laser from a target position in a given direction with a specified lenght
    void ShootLaserFromTargetPosition(Vector3 targetPosition, Vector3 direction, float length)
    {
        Ray ray = new Ray(targetPosition, direction);
        RaycastHit raycastHit;
        Vector3 endPosition = targetPosition + (length * direction);

        if (Physics.Raycast(ray, out raycastHit, length))
        {
            endPosition = raycastHit.point;
        }

        laserLineRenderer.SetPosition(0, targetPosition);
        laserLineRenderer.SetPosition(1, endPosition);
    }

    //Raycast to Keyboard
    void RayCastKeyboard()
    {
        var ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            //Check if the hit object belongs to the LayerBoard (Keyboard layer)
            if (hit.transform.gameObject.layer == LayerBoard )
            {
                matLine.color = Color.green;
                ShootLaserFromTargetPosition(transform.position, this.transform.forward, laserMaxLength);
                laserLineRenderer.enabled = true;
            }     
            else if (hit.collider.tag == "Keyboard")
            {
                matLine.color = Color.red;
                ShootLaserFromTargetPosition(transform.position, this.transform.forward, laserMaxLength);
            }
            else
            {
                
                laserLineRenderer.enabled = false;
            }
            //check if trigger button is pressed and interact with keyboard
            if (triggerAction.action.WasPressedThisFrame() && keyboard.isPressed == false && hit.collider.tag != "Keyboard")
            {
                if (hit.transform.gameObject.layer == LayerBoard)
                {
                    
                    ColliderHit = hit.transform.gameObject;                  
                    buttonPosition = ColliderHit.transform.parent.GetChild(1).localPosition;
                    soundManager.PlayTypewriterSound(ColliderHit.GetComponent<AudioSource>());
                    //Get the character from the pressed button
                    s = ColliderHit.transform.parent.gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
                    sName = ColliderHit.transform.parent.gameObject.name;
                    Debug.Log(sName);
                    if (s.Length == 1)   //Insert Char
                    {
                        keyboard.InsertChar(s);
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Delete Button")  //Backspace
                    {
                        keyboard.DeleteChar();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Caps Button")   //Caps
                    {
                        keyboard.CapsPressed();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Create Button")  //Create
                    {
                        NewShield();    //Create new Protestshild
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else if (sName == "Space Button")
                    {
                        keyboard.InsertSpace();   //Space
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }
                    else
                    {
                        keyboard.LanguageSwitch();
                        ColliderHit.transform.parent.GetChild(1).localPosition -= new Vector3(0, 0.01f, 0);
                    }


                    keyboard.isPressed = true;
                    
                    //ColliderHit = hit.collider.gameObject;
                    //buttonVRS.EnterHit(ColliderHit);
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
        GameObject tempShield = Instantiate(newProtestShild, ProtestTransform.transform.position, Quaternion.Euler(0, -60, 0));
        tempShield.name = "NewProtestShield";
        GameObject quad = tempShield.transform.Find("Wood").Find("Sign").Find("Quad").gameObject;
        Material newShield = new Material(Shader.Find("Standard"));
        // Kopiere die Eigenschaften des aktuellen Materials in das neue Material
        newShield.CopyPropertiesFromMaterial(quad.GetComponent<MeshRenderer>().material);
        ConvertToTexture(quad, newShield);
        
    }
    //Converts the quad texture to a RenderTexture and then to a Texture 2D
    private void ConvertToTexture(GameObject quad, Material standard)
    {
        RenderTexture.active = rTex;
        tex2 = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
        tex2.name = "ProtestShieldtexture";
		// ReadPixels looks at the active RenderTexture.
		tex2.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex2.Apply();
        quad.GetComponent<MeshRenderer>().material = standard;
        quad.GetComponent<MeshRenderer>().material.mainTexture = tex2;
    }
    void OnEnable()
{
    triggerAction.action.Enable();
}

void OnDisable()
{
    triggerAction.action.Disable();
}
}

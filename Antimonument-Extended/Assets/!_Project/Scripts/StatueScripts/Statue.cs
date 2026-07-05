using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Statue : MonoBehaviour
{
    //basically the identifier script
    public StatueValues StatueValues;
    public bool IsGrabbable;

    void Start()
    {
        if (GetComponent<XRGrabInteractable>() != null)
            IsGrabbable = false;
    }
}

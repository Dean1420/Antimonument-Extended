using System;
using UnityEngine;
using UnityEngine.InputSystem; // ADDED


[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public Transform playerCenter; // Center of player circle
    public Transform statueCenter; // Center of statue body
    public float positionScaleX = 2f;
    public float positionScaleY = 2f;
    public float positionScaleZ = 2f;

    public void Map()
    {
        Vector3 localPos = vrTarget.TransformPoint(trackingPositionOffset);

        // Get offset from player center
        Vector3 offsetFromPlayer = localPos - playerCenter.position;

        // Scale per axis
        offsetFromPlayer.x *= positionScaleX;
        offsetFromPlayer.y *= positionScaleY;
        offsetFromPlayer.z *= -positionScaleZ;

        ikTarget.position = statueCenter.position + offsetFromPlayer;

        // Flip rotation 180 degrees around Y axis
        Quaternion baseRotation = Quaternion.Euler(0, 180, 0) * vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
        Vector3 euler = baseRotation.eulerAngles;
        euler.y = -euler.y; // Negate Y-axis rotation
        ikTarget.rotation = Quaternion.Euler(euler);
    }
}

public class IKTargetFollowVRRig : MonoBehaviour
{
    public VRMap leftHand;
    public VRMap rightHand;
    public InputActionReference rightTriggerAction;
    public Transform controllerModel; // drag controller here
    private bool isTriggerHeld = false;



    // ADDED
    void OnTriggerEnter(Collider other)
    {
        if (other.transform == controllerModel)
        {
            rightTriggerAction.action.started += OnTriggerStarted;
            rightTriggerAction.action.canceled += OnTriggerCanceled;
        }
    }

    // ADDED
    void OnTriggerExit(Collider other)
    {
        if (other.transform == controllerModel)
        {
            rightTriggerAction.action.started -= OnTriggerStarted;
            rightTriggerAction.action.canceled -= OnTriggerCanceled;
            isTriggerHeld = false;
        }
    }

    private void OnTriggerStarted(InputAction.CallbackContext context)
    {
        isTriggerHeld = true;
    }

    // added to detect release
    private void OnTriggerCanceled(InputAction.CallbackContext context)
    {
        isTriggerHeld = false;
    }

    // update to continuously call while held
    void Update()
    {
        if (isTriggerHeld)
        {
            UpdatePostion();
            Debug.Log("MOVED STATUE ARMS");
        }
    }

    public void UpdatePostion()
    {
        leftHand.Map();
        rightHand.Map();
    }
}
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class AnnotationToggler : MonoBehaviour
{
    public ControllerAnnotation[] allAnnotations;

    private InputDevice leftController;
    private bool lastButtonState = false;

    void Update()
    {
        // Controller suchen falls noch nicht gefunden
        if (!leftController.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
                devices
            );
            if (devices.Count > 0)
                leftController = devices[0];
        }

        // X Button Linker Controller
        if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed))
        {
            if (pressed && !lastButtonState)
            {
                foreach (var annotation in allAnnotations)
                    annotation.ToggleVisibility();
            }
            lastButtonState = pressed;
        }
    }
}
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using System.Collections.Generic;

public class AnnotationDebugMenu : MonoBehaviour
{
    public Transform labelAnchor;
    public Transform buttonAnchor;
    public float moveSpeed = 0.05f;
    public Camera headCamera;

    private TextMeshProUGUI debugText;
    private GameObject debugCanvas;
    private InputDevice rightController;
    private InputDevice leftController;
    private bool lastMenuState = false;
    private bool menuVisible = true;

    void Start()
    {
        debugCanvas = new GameObject("DebugCanvas");
        Canvas canvas = debugCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        debugCanvas.transform.localScale = Vector3.one * 0.002f;
        debugCanvas.transform.SetParent(headCamera.transform);
        debugCanvas.transform.localPosition = new Vector3(0, 0, 0.5f);
        debugCanvas.transform.localRotation = Quaternion.identity;

        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugCanvas.transform);
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 24;
        debugText.color = Color.white;
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 300);
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void Update()
    {
        // Controller suchen
        if (!rightController.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
            if (devices.Count > 0) rightController = devices[0];
        }
        if (!leftController.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, devices);
            if (devices.Count > 0) leftController = devices[0];
        }

        // Menu Button togglet Debug Menu
        if (leftController.TryGetFeatureValue(CommonUsages.menuButton, out bool menuPressed))
        {
            if (menuPressed && !lastMenuState)
                menuVisible = !menuVisible;
            lastMenuState = menuPressed;
        }

        debugCanvas.SetActive(menuVisible);
        if (!menuVisible) return;

        // Rechter Thumbstick = labelAnchor X/Y
        if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStick))
        {
            labelAnchor.localPosition += new Vector3(
                rightStick.x * moveSpeed * Time.deltaTime,
                rightStick.y * moveSpeed * Time.deltaTime,
                0
            );
        }

        // A = labelAnchor Z+, B = labelAnchor Z-
        if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool aPressed) && aPressed)
            labelAnchor.localPosition += new Vector3(0, 0, moveSpeed * Time.deltaTime);
        if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bPressed) && bPressed)
            labelAnchor.localPosition -= new Vector3(0, 0, moveSpeed * Time.deltaTime);

        // Linker Thumbstick = buttonAnchor X/Y
        if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStick))
        {
            buttonAnchor.localPosition += new Vector3(
                leftStick.x * moveSpeed * Time.deltaTime,
                leftStick.y * moveSpeed * Time.deltaTime,
                0
            );
        }

        // X = buttonAnchor Z+, Y = buttonAnchor Z-
        if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool xPressed) && xPressed)
            buttonAnchor.localPosition += new Vector3(0, 0, moveSpeed * Time.deltaTime);
        if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yPressed) && yPressed)
            buttonAnchor.localPosition -= new Vector3(0, 0, moveSpeed * Time.deltaTime);

        // Positionen anzeigen
        debugText.text = $"LabelAnchor:\n" +
                        $"X: {labelAnchor.localPosition.x:F3}\n" +
                        $"Y: {labelAnchor.localPosition.y:F3}\n" +
                        $"Z: {labelAnchor.localPosition.z:F3}\n\n" +
                        $"ButtonAnchor:\n" +
                        $"X: {buttonAnchor.localPosition.x:F3}\n" +
                        $"Y: {buttonAnchor.localPosition.y:F3}\n" +
                        $"Z: {buttonAnchor.localPosition.z:F3}\n\n" +
                        $"MenuBtn = Debug an/aus\n" +
                        $"R.Stick = Label X/Y\n" +
                        $"A/B = Label Z\n" +
                        $"L.Stick = Button X/Y\n" +
                        $"X/Y = Button Z";
    }
}
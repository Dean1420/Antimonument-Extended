using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class ControllerAnnotation : MonoBehaviour
{
    [Header("Referenzen")]
    public Transform buttonAnchor;
    public Transform labelTarget;
    public TextMeshProUGUI labelText;
    public LineRenderer lineRenderer;


    [Header("Einstellungen")]
    public string actionName = "Teleport";
    public bool isContextSensitive = false;
    private bool isVisible = true;

    public static bool AnnotationsGloballyEnabled = true;
    void Start()
    {
        if (isContextSensitive)
    {
        isVisible = false;
        labelText.gameObject.SetActive(false);
        lineRenderer.enabled = false;
    }
        labelText.text = actionName;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (!isVisible) return;

        lineRenderer.SetPosition(0, buttonAnchor.position);
        lineRenderer.SetPosition(1, labelTarget.position);
        // Label immer zur Kamera drehen
        labelTarget.LookAt(Camera.main.transform);
        labelTarget.Rotate(0, 180f, 0);
    }

public void ShowAnnotation()
{
    if (!AnnotationsGloballyEnabled) return;
    isVisible = true;
    lineRenderer.enabled = true;
    labelText.gameObject.SetActive(true);
}

public void HideAnnotation()
{
    isVisible = false;
    lineRenderer.enabled = false;
    labelText.gameObject.SetActive(false);
}

}

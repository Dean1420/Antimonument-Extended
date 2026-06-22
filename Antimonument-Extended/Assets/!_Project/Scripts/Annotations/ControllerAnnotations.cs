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

    private bool isVisible = true;

    void Start()
    {
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

    public void ToggleVisibility()
    {
        isVisible = !isVisible;
        lineRenderer.enabled = isVisible;
        labelText.gameObject.SetActive(isVisible);
    }
}

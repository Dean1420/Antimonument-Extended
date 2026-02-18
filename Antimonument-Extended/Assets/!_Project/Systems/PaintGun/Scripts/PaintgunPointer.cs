using UnityEngine;

public class PaintgunPointer : MonoBehaviour
{
    public Transform pointer;
    public Transform pointerDirection;
    public float maxDistance = 100f;
    public float pointerSize = 1f;
    public float minimizedPointerSize = 1f;
    public Transform[] minimizePointerForObjects;


    private bool pointerIsActive = false;

    void Start()
    {
        pointer.localScale = Vector3.one * pointerSize;
    }

    void Update()
    {
        if (pointerIsActive)
        {
            RepositionPointer();
        }
    }

    private void RepositionPointer()
    {
        Vector3 origin = pointerDirection.position;
        Vector3 direction = pointerDirection.forward;
        Ray ray = new Ray(origin, direction);


        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            AdjustPointerSize(hit);
            MovePointer(hit);
        }
    }

    private void MovePointer(RaycastHit hit)
    {
        Vector3 offset = hit.normal * 0.01f;
        pointer.position = hit.point + offset;
        pointer.rotation = Quaternion.LookRotation(hit.normal);
    }

    private void AdjustPointerSize(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        foreach (Transform minimizeObject in minimizePointerForObjects)
        {
            if (minimizeObject.gameObject == hitObject)
            {
                pointer.localScale = Vector3.one * minimizedPointerSize;
                return;
            }
        }

        pointer.localScale = Vector3.one * pointerSize;
    }

    public void TogglePointer()
    {
        pointerIsActive = !pointerIsActive;
        pointer.gameObject.SetActive(pointerIsActive);
    }
}

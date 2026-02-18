using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    public Transform target;

    public void Toggle()
    {
        GameObject targetObject =target.transform.gameObject;
        bool state = targetObject.activeSelf;
        targetObject.SetActive(!state);
    }
}

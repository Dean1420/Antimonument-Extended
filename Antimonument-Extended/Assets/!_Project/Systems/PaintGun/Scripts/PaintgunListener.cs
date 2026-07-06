using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PaintgunListener : MonoBehaviour
{
    public XRGrabInteractable sprayGunInteractable;  // Im Inspector die Spraydose reinziehen
    public GameObject colorWheelVisual;              // Das sichtbare Farbrad-Objekt

    void OnEnable()
    {
        sprayGunInteractable.selectEntered.AddListener(OnGunGrabbed);
        sprayGunInteractable.selectExited.AddListener(OnGunReleased);
    }

    void OnDisable()
    {
        sprayGunInteractable.selectEntered.RemoveListener(OnGunGrabbed);
        sprayGunInteractable.selectExited.RemoveListener(OnGunReleased);
    }

    void Start()
    {
        colorWheelVisual.SetActive(false);
    }

    private void OnGunGrabbed(SelectEnterEventArgs args)
    {
        colorWheelVisual.SetActive(true);
    }

    private void OnGunReleased(SelectExitEventArgs args)
    {
        colorWheelVisual.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.Events;

public class CallExternalFunction : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private bool useTrigger = false;
    [SerializeField] private LayerMask allowedLayers;
    
    [Header("Function to Call")]
    [SerializeField] private UnityEvent onCollisionEnter;
    
    //[Header("Trigger with GameObject")]
    //[SerializeField] private PhotoHandler photoHandler;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"COLLISION >>> Hit by: {collision.gameObject.name} on layer: {collision.gameObject.layer}");
        if (!useTrigger && ((1 << collision.gameObject.layer) & allowedLayers) != 0)
        {
            onCollisionEnter?.Invoke();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER Enter >>> Hit by: {other.gameObject.name} on layer: {other.gameObject.layer}");
        if (useTrigger && ((1 << other.gameObject.layer) & allowedLayers) != 0)
        {
            //if (photoHandler != null)
            //{
            //    //photoHandler.OnObjectEnterBoundingBox(other.gameObject);
            //}
            //else
            //{
            //    Debug.LogError("TRIGGER >>> PhotoHandler not assigned!");
            //}
        }
        if (useTrigger && ((1 << other.gameObject.layer) & allowedLayers) != 0)
        {
            onCollisionEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"TRIGGER Exit >>> Hit by: {other.gameObject.name} on layer: {other.gameObject.layer}");
        if (useTrigger && ((1 << other.gameObject.layer) & allowedLayers) != 0)
        {
            //if (photoHandler != null)
            //{
            //    //photoHandler.OnObjectExitBoundingBox(other.gameObject);
            //}
            //else
            //{
            //    Debug.LogError("TRIGGER >>> PhotoHandler not assigned!");
            //}
        }
    }
}
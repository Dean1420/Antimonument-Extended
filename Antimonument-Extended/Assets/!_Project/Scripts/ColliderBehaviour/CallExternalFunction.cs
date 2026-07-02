using UnityEngine;
using UnityEngine.Events;

public class CallExternalFunction : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private bool useTrigger = false; // false = collision, true = trigger
    [SerializeField] private LayerMask allowedLayers; 
    
    [Header("Function to Call")]
    [SerializeField] private UnityEvent onCollisionEnter;
    
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
       Debug.Log($"TRIGGER >>> Hit by: {other.gameObject.name} on layer: {other.gameObject.layer}");
       if (useTrigger && ((1 << other.gameObject.layer) & allowedLayers) != 0)
    {
        onCollisionEnter?.Invoke();
    }
    }
}
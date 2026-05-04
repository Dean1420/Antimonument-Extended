using UnityEngine;
using UnityEngine.Events;

public class CallExternalFunction : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private bool useTrigger = false; // false = collision, true = trigger
    
    [Header("Function to Call")]
    [SerializeField] private UnityEvent onCollisionEnter;
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"test");

        if (!useTrigger)
        {
            onCollisionEnter?.Invoke();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"test");

        if (useTrigger)
        {
            onCollisionEnter?.Invoke();
        }
    }
}
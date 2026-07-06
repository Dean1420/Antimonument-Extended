using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SliceObject : MonoBehaviour
{
    [Header("Slicing Setup")]
    [SerializeField] private Transform startSlicePoint;
    [SerializeField] private Transform endSlicePoint;
    [SerializeField] private LayerMask sliceableLayer;
    [SerializeField] private VelocityEstimator velocityEstimator;

    [Header("Slicing Parameters")]
    [SerializeField] private Material crossSectionMaterial;
    [SerializeField] public float cutForce = 2000;

    [Header("Effects")]
    [SerializeField] private AudioClip sliceSound;

    [Header("Paintable Setup")]
    [SerializeField] private string paintableTag = "Paintable";

    // Lists to track objects
    [SerializeField] private List<GameObject> originalObjects = new List<GameObject>();
    private List<GameObject> slicedObjects = new List<GameObject>();

    private float sliceCooldown = 0f;
    private const float SLICE_COOLDOWN_TIME = 0.1f;


    // Store the actual layer number (not the mask)
    private int sliceableLayerNumber;
    private Paint paintGunReference;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Extract the layer number from the LayerMask
        sliceableLayerNumber = LayerMaskToLayer(sliceableLayer);
        CollectSliceableObjects();
        paintGunReference = FindObjectOfType<Paint>();

        // START VELOCITY ESTIMATION!
        if (velocityEstimator != null)
        {
            velocityEstimator.BeginEstimatingVelocity();
        }
        else
        {
            Debug.LogError("SLICE >>> VelocityEstimator not assigned!");
        }
    }



// Convert LayerMask to actual layer number
private int LayerMaskToLayer(LayerMask layerMask)
{
    int layerNumber = 0;
    int layer = layerMask.value;
    
    while (layer > 1)
    {
        layer = layer >> 1;
        layerNumber++;
    }
    
    return layerNumber;
}



    private void CollectSliceableObjects()
    {
        // Find all objects on the sliceable layer at start
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Check if object is on the sliceable layer
            if (IsInLayerMask(obj, sliceableLayer))
            {
                originalObjects.Add(obj);
                Debug.Log($"SLICE >>> Added sliceable object: {obj.name}");
            }
        }
        Debug.Log($"SLICE >>> Total sliceable objects found: {originalObjects.Count}");
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << obj.layer));
    }



    void FixedUpdate()
    {

        //Debug.Log("SLICE >>> Linecast found "+ Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hitted, sliceableLayer));
        sliceCooldown -= Time.fixedDeltaTime;

        Vector3 direction = (endSlicePoint.position - startSlicePoint.position).normalized;
        float distance = Vector3.Distance(startSlicePoint.position, endSlicePoint.position);
        
        bool hasHit = Physics.Raycast(
            startSlicePoint.position, 
            direction, 
            out RaycastHit hit, 
            distance, 
            sliceableLayer
        );

        //Debug.Log("SLICE >>> RayCast found " + hasHit);
        if (hasHit && sliceCooldown <= 0f)
        {
            GameObject target = hit.transform.gameObject;
            Debug.Log($"SLICE >>> Hit object: {target.name}");
            Slice(target);
            sliceCooldown = SLICE_COOLDOWN_TIME; // Prevent multiple slices per frame
        }
    }

    public void Slice(GameObject target)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();

        // Debug: Check velocity
        if (velocity.magnitude < 0.1f)
        {
            Debug.LogWarning($"SLICE >>> Velocity too low: {velocity.magnitude}. Slice might not work properly.");
        }

        Vector3 direction = (endSlicePoint.position - startSlicePoint.position).normalized;
        Vector3 planeNormal = Vector3.Cross(direction, velocity).normalized;

        // Fallback if cross product is zero
        if (planeNormal.magnitude < 0.01f)
        {
            planeNormal = transform.right; // Use a default normal
            planeNormal = transform.right;
            Debug.LogWarning("SLICE >>> Velocity was zero, using default plane normal");
        }
    

    Debug.Log($"SLICE >>> Slicing {target.name} with velocity {velocity.magnitude} m/s and normal {planeNormal}");
    
    // Get paint data BEFORE slicing
    Paint paintData = target.GetComponent<Paint>();
    
    SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
    
    if (hull != null)
    {
        Debug.Log($"SLICE >>> Successfully created hull for {target.name}");
        
        GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
        SetupSlicedComponent(upperHull, sliceableLayerNumber, paintData);
        slicedObjects.Add(upperHull);
        
        GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
        SetupSlicedComponent(lowerHull, sliceableLayerNumber, paintData);
        slicedObjects.Add(lowerHull);
        
        HandleObjectCleanup(target);

        if (sliceSound != null)
        {
            AudioSource.PlayClipAtPoint(sliceSound, transform.position);
        }
    }
    else
    {
        Debug.LogError($"SLICE >>> Failed to slice {target.name} - hull is null");
    }
}

    private void HandleObjectCleanup(GameObject target)
    {
        // If it's an original object, disable it instead of destroying
        if (originalObjects.Contains(target))
        {
            originalObjects.Remove(target); // REMOVE from list!
            target.SetActive(false);
            Debug.Log($"SLICE >>> Disabled original object: {target.name}");
        }
        else if (slicedObjects.Contains(target))
        {
            // If it's a sliced object being re-sliced, remove from list and destroy
            slicedObjects.Remove(target);
            Destroy(target);
            Debug.Log($"SLICE >>> Destroyed sliced object: {target.name}");
        }
    }

    public void SetupSlicedComponent(GameObject slicedObject, int layer, Paint originalPaintData = null)
    {
        slicedObject.layer = layer;
        slicedObject.tag = paintableTag;

        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);

        // Add XR Grab Interactable
        XRGrabInteractable grabInteractable = slicedObject.AddComponent<XRGrabInteractable>();
        grabInteractable.useDynamicAttach = true;

        // Copy paint data if available
        if (originalPaintData != null)
    {
        Paint newPaint = slicedObject.AddComponent<Paint>();
        // Copy paint-relevant data (du musst ggf. anpassen, je nachdem wie Paint strukturiert ist)
        Debug.Log($"SLICE >>> Paint data copied to {slicedObject.name}");
    }
}

    public void ResetCuts()
    {
        // Destroy all sliced objects
        foreach (GameObject slicedObj in slicedObjects)
        {
            if (slicedObj != null)
            {
                Destroy(slicedObj);
            }
        }
        slicedObjects.Clear();

        // Re-enable all original objects
        foreach (GameObject originalObj in originalObjects)
        {
            if (originalObj != null)
            {
                originalObj.SetActive(true);
            }
        }
    }

public void MakeSlicesCutable()
{
    foreach (GameObject slicedObject in slicedObjects)
    {
        slicedObject.layer = sliceableLayerNumber;     
    }
}

public void MakeSlicesGrabbable()
{
    foreach (GameObject slicedObject in slicedObjects)
    {
        // set to "Default" layer (layer 0)
        slicedObject.layer = 0;         
    }
}

    public void addOriginalStatue(Statue statue)
    {
        if (statue == null)
        {
            return;
        }
        if (!originalObjects.Contains(statue.gameObject))
        {
            originalObjects.Clear();
            originalObjects.Add(statue.gameObject);
            Debug.Log($"SLICE >>> Added original statue: {statue.name}");
        }
        else
        {
            originalObjects.Remove(statue.gameObject);
        }
    }
    public void removeOriginalStatue(Statue statue)
    {
        if (originalObjects.Contains(statue.gameObject))
        {
            originalObjects.Remove(statue.gameObject);
            Debug.Log($"SLICE >>> Removed original statue: {statue.name}");
        }
    }
}
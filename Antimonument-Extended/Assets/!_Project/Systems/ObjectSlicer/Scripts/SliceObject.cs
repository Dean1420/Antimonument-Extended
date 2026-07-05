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
    private List<GameObject> originalObjects = new List<GameObject>();
    private List<GameObject> slicedObjects = new List<GameObject>();


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
            }
        }
    }



    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << obj.layer));
    }



    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);

        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;
            Slice(target);
        }
    }



    public void Slice(GameObject target)
{
    Vector3 velocity = velocityEstimator.GetVelocityEstimate();
    Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
    planeNormal.Normalize();
    
    SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
    
    if(hull != null)
    {
        GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
        SetupSlicedComponent(upperHull, sliceableLayerNumber);
        slicedObjects.Add(upperHull);
        
        GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
        SetupSlicedComponent(lowerHull, sliceableLayerNumber);
        slicedObjects.Add(lowerHull);
        
        HandleObjectCleanup(target);

        if(sliceSound != null){AudioSource.PlayClipAtPoint(sliceSound, transform.position);}

    }
}



    private void HandleObjectCleanup(GameObject target)
    {
        // If it's an original object, disable it instead of destroying
            if (originalObjects.Contains(target))
            {
                target.SetActive(false);
            }
            else if (slicedObjects.Contains(target))
            {
                // If it's a sliced object being re-sliced, remove from list and destroy
                slicedObjects.Remove(target);
                Destroy(target);
            }
    }



public void SetupSlicedComponent(GameObject slicedObject, int layer)
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
}
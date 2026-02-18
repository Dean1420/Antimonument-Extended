using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;

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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
        SetupSlicedComponent(upperHull, target.layer);
        
        GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
        SetupSlicedComponent(lowerHull, target.layer);
        
        Destroy(target);
    }
}

public void SetupSlicedComponent(GameObject slicedObject, int layer)
{
    slicedObject.layer = layer;
    
    Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
    MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
    collider.convex = true;
    rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
}
}

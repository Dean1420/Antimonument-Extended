using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DecalGun : MonoBehaviour
{
    [SerializeField] private GameObject decalPrefab;
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private float decalOffset = 0.01f;
    [SerializeField] private float previewExtraOffset = 0.005f;
    
    [Header("Decal Materials")]
    [SerializeField] private List<Material> decalMaterials = new List<Material>();
    
    [Header("Input")]
    [SerializeField] private InputActionReference cycleInputAction;

    private GameObject previewDecal;
    private DecalProjector previewProjector;
    private int currentMaterialIndex = 0;
    private int lastAppliedMaterialIndex = -1; // Track what's actually applied

    void Start()
    {
        // Create preview decal
        if (decalPrefab != null)
        {
            previewDecal = Instantiate(decalPrefab);
            previewDecal.name = "DecalPreview";
            previewProjector = previewDecal.GetComponent<DecalProjector>();
        }
    }

    void Update()
    {
        UpdatePreview();
    }

    void UpdatePreview()
    {
        if (previewDecal == null) return;

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {
            // Show and position preview with EXTRA offset
            previewDecal.SetActive(true);
            previewDecal.transform.position = hit.point + hit.normal * (decalOffset + previewExtraOffset);
            previewDecal.transform.rotation = Quaternion.LookRotation(-hit.normal);
            
            // Update material if it changed
            if (lastAppliedMaterialIndex != currentMaterialIndex)
            {
                UpdatePreviewMaterial();
            }
        }
        else
        {
            // Hide preview when not hitting anything
            previewDecal.SetActive(false);
        }
    }

    public void EnableCycleInput()
    {
        if (cycleInputAction != null && cycleInputAction.action != null)
        {
            cycleInputAction.action.performed += OnCyclePerformed;
            cycleInputAction.action.Enable();
        }
    }

    public void DisableCycleInput()
    {
        if (cycleInputAction != null && cycleInputAction.action != null)
        {
            cycleInputAction.action.performed -= OnCyclePerformed;
            cycleInputAction.action.Disable();
        }
    }

    private void OnCyclePerformed(InputAction.CallbackContext context)
    {
        CycleDecalMaterial();
    }

    public void CycleDecalMaterial()
{
    if (decalMaterials.Count == 0) return;

    // Move to next material
    currentMaterialIndex = (currentMaterialIndex + 1) % decalMaterials.Count;
    
    // Update material IMMEDIATELY
    UpdatePreviewMaterial();
}

private void UpdatePreviewMaterial()
{
    if (previewProjector != null && decalMaterials.Count > 0)
    {
        previewProjector.material = decalMaterials[currentMaterialIndex];
        lastAppliedMaterialIndex = currentMaterialIndex;
    }
}

    public void ShootDecal()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {
            // Spawn decal slightly above surface
            Vector3 spawnPosition = hit.point + hit.normal * decalOffset;
            GameObject decal = Instantiate(decalPrefab, spawnPosition, Quaternion.identity);

            // Rotate decal to face surface
            decal.transform.rotation = Quaternion.LookRotation(-hit.normal);

            // Copy material from preview (guaranteed to match)
            DecalProjector projector = decal.GetComponent<DecalProjector>();
            if (projector != null && previewProjector != null)
            {
                projector.material = previewProjector.material;
            }

            // Optional: Parent to hit object
            decal.transform.parent = hit.transform;
        }
    }

    void OnDestroy()
    {
        // Clean up input subscription
        DisableCycleInput();
        
        // Clean up preview when gun is destroyed
        if (previewDecal != null)
        {
            Destroy(previewDecal);
        }
    }
}
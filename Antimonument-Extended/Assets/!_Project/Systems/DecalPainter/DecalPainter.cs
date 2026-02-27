using UnityEngine;

public class DecalPainter : MonoBehaviour
{
    [Header("Decal Settings")]
    [SerializeField] private Sprite decalSprite;
    [SerializeField] private Color decalColor = Color.red;
    [SerializeField] private float decalSize = 0.5f;
    [SerializeField] private float decalOffset = 0.01f;
    
    [Header("Paint Setup")]
    [SerializeField] private Transform paintGun;
    [SerializeField] private float paintDistance = 100f;
    [SerializeField] private Transform[] paintableObjects;
    [SerializeField] private Transform decalContainer;

    // Call this single function to paint
    public void Paint()
    {
        if (paintGun == null) return;

        Ray ray = new Ray(paintGun.position, paintGun.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, paintDistance))
            return;

        if (!IsPaintable(hit.collider.gameObject))
            return;

        if (decalSprite == null) return;

        // Create decal
        GameObject decalObj = new GameObject("Decal");
        SpriteRenderer sr = decalObj.AddComponent<SpriteRenderer>();
        sr.sprite = decalSprite;
        sr.color = decalColor;
        
        decalObj.transform.position = hit.point + hit.normal * decalOffset;
        decalObj.transform.rotation = Quaternion.LookRotation(-hit.normal);
        decalObj.transform.localScale = Vector3.one * decalSize;
        
        if (decalContainer != null)
            decalObj.transform.parent = decalContainer;
        else
            decalObj.transform.parent = hit.transform;
    }

    private bool IsPaintable(GameObject obj)
    {
        foreach (Transform paintable in paintableObjects)
        {
            if (paintable != null && paintable.gameObject == obj)
                return true;
        }
        return false;
    }

    public void ClearAll()
    {
        if (decalContainer != null)
        {
            foreach (Transform child in decalContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;



public class Paint : MonoBehaviour
{   // data required upfront in editor
    [Header("Paint Properties")]
    [SerializeField] private Color color;
    [SerializeField] private float radius = 10;
    [SerializeField, Range(0f, 1f)] private float opacity = 1f;
    [SerializeField, Range(0.01f, 1f)] private float falloffRange = 1f;

    [Header("Paint Control")]
    // the particle system dictates the direction of the raycast
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float paintDistance = 100;
    [SerializeField] private Transform[] paintableObjects;
    
    [Header("Colour Picker")]
    [SerializeField] private Transform colourpicker;


    // caching of copied textures for resets and efficiency
    private Dictionary<GameObject, Texture2D> cachedTextures = new Dictionary<GameObject, Texture2D>();

    // data acquired while runtime for painting
    private Vector2 pixelUV;
    private Texture2D copyTexture;
    private Renderer targetRenderer;
    private RaycastHit raycastHit;

    private bool continuePainting = false;

    void Start()
    {
        particles.Stop();
    }

    void Update()
    {
        if (continuePainting && (Time.frameCount % 15 == 0))
        {
            PaintOnRenderTexture();
        }

    }



    public void StartPainting()
    {
        EnableEffects();
        PaintOnRenderTexture();
        continuePainting = true;
    }

    private void PaintOnRenderTexture()
    {
        ShootRaycast();

        if (isPaintable())
        {
            ApplyPaint();
        }
        else if (raycastHit.collider.gameObject.name == colourpicker.name)
        {
            ChangePaintColour();
        }
    }

    private void ChangePaintColour()
    {
        Renderer renderer = raycastHit.collider.GetComponent<Renderer>();
        if (renderer != null && renderer.material.mainTexture != null)
        {
            Texture2D texture = renderer.material.mainTexture as Texture2D;

            Vector2 pixelUV = raycastHit.textureCoord;

            int x = (int)(pixelUV.x * texture.width);
            int y = (int)(pixelUV.y * texture.height);

            color = texture.GetPixel(x, y);

            Debug.Log($"Paint_Gun >>> colour changed to {color} at ({x}, {y})");

            ParticleSystem.MainModule main = particles.main;
            main.startColor = color;
        }

    }

    private void ApplyPaint()
    {
        GetTextureCopy();

        if (copyTexture == null)
        {
            Debug.Log($"PAINT_GUN >>> no texture found for: {raycastHit.transform.name}");
            return;
        }

        pixelUV = raycastHit.textureCoord;
        pixelUV.x *= copyTexture.width;
        pixelUV.y *= copyTexture.height;
        Debug.Log($"PAINT_GUN >>> painted at: ({pixelUV.x}, {pixelUV.y}) with color: {color}");

        PaintCircleOnTexture(pixelUV.x, pixelUV.y, radius, color);

        // apply the changes and update the material
        copyTexture.Apply();
        // older version may use "_MainTex"
        targetRenderer.material.SetTexture("_BaseMap", copyTexture);

    }

    private void GetTextureCopy()
    {
        targetRenderer = raycastHit.transform.GetComponent<Renderer>();

        // check if texture copy already cached 
        GameObject hitObject = raycastHit.transform.gameObject;
        if (cachedTextures.ContainsKey(hitObject))
        {
            copyTexture = cachedTextures[hitObject];
            return;
        }

        // copy and cache texture
        Texture mainTexture = targetRenderer.material.mainTexture;

        Debug.Log($"PAINT_GUN >>> Hit object: {raycastHit.transform.name}");

        if (mainTexture == null)
        {
            mainTexture = targetRenderer.material.GetTexture("_BaseMap");
        }

        if (mainTexture == null)
        {
            Debug.LogError($"PAINT_GUN >>> No texture found");
            copyTexture = null;
            return;
        }

        if (mainTexture is RenderTexture renderTexture)
        {
            RenderTexture.active = renderTexture;
            copyTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            copyTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            copyTexture.Apply();
            RenderTexture.active = null;
        }
        else if (mainTexture is Texture2D texture2D) // Testing if this works
        {
            // blit into a RenderTexture first to handle any compressed/unsupported format
            RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0, RenderTextureFormat.ARGB32);

            Graphics.Blit(texture2D, rt);

            RenderTexture.active = rt;
            copyTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, false);
            copyTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            copyTexture.Apply();
            RenderTexture.active = null;

            RenderTexture.ReleaseTemporary(rt);
        }
        else
        {
            Debug.LogError($"PAINT_GUN >>> Unsupported texture type: {mainTexture.GetType().Name}");

        }

        cachedTextures.Add(hitObject, copyTexture);
        Debug.Log($"PAINT_GUN >>> Cache Texture of object: {hitObject.name}");
    }

    private void ShootRaycast()
    {
        Vector3 origin = particles.transform.position;
        Vector3 direction = particles.transform.forward;
        Ray cameraRay = new Ray(origin, direction);

        if (!Physics.Raycast(cameraRay, out raycastHit, paintDistance))
        {
            Debug.Log("PAINT_GUN >>> hit: null");
            return;
        }
        Debug.Log($"PAINT_GUN >>> hit: {raycastHit.collider.gameObject.name}");
    }

    private void EnableEffects()
    {
        if (!particles.isPlaying)
        {
            particles.Play();
        }
    }


    private bool isPaintable()
    {
        if (raycastHit.collider == null)
        {
            return false;
        }

        GameObject hitObject = raycastHit.collider.gameObject;
        if (hitObject == null)
        {
            return false;
        }

        foreach (Transform paintableObject in paintableObjects)
        {
            if (paintableObject.gameObject == hitObject)
            {
                return true;
            }
        }

        return false;
    }


    private void PaintCircleOnTexture(float x, float y, float radius, Color c)
    {
        float area = radius * radius;

        for (int width = 0; width < copyTexture.width; width++)
        {
            for (int height = 0; height < copyTexture.height; height++)
            {
                // check if out of bounds
                if (x < 0 || x >= copyTexture.width || y < 0 || y >= copyTexture.height)
                {
                    continue;
                }
                //using circle equation to check if pixel is stil inside the circle to be painted
                float dx = x - width;
                float dy = y - height;
                float distanceSquared = dx * dx + dy * dy;

                bool isOnCircle = distanceSquared < area;
                if (isOnCircle)
                {
                    Color existingColor = copyTexture.GetPixel(width, height);
                    
                    // 0 = center, 1 = edge
                    float distance = Mathf.Sqrt(distanceSquared) / radius; 
                    float falloff = 1f - Mathf.Clamp01(distance / falloffRange);
                    copyTexture.SetPixel(width, height, Color.Lerp(existingColor, c, opacity * falloff));
                }
            }
        }
    }

    public void StopPainting()
    {
        particles.Stop();
        continuePainting = false;
    }
}

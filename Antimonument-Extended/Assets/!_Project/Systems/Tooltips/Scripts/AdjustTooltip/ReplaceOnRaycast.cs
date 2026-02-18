using UnityEngine;
using UnityEngine.UI;

public class ReplaceOnRaycast : MonoBehaviour
{
    [SerializeField] private Transform tooltip;
    [SerializeField] private Sprite replacementSprite;
    [SerializeField] private bool replacementState;
    [SerializeField] private Transform[] targetObjects;
    [SerializeField] private Transform rayDirection;
    [SerializeField] private float maxDistance = 100f;

    private Sprite originalSprite;
    private bool originalState;
    private Image tooltipImage;
    private bool active = false;
    private bool replacedInPreviousIteration = false;



    void Update()
    {
        if (active)
        {
            UpdateTooltip();
        }
    }



    private void UpdateTooltip()
    {
        Vector3 origin = rayDirection.position;
        Vector3 direction = rayDirection.forward;
        Ray ray = new Ray(origin, direction);


        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            // something was hit
            ReplaceTooltipOrDefault(hit);
        }
        else
        {
            // nothing was hit
            Reset();
            replacedInPreviousIteration = false;
        }
    }



    private void ReplaceTooltipOrDefault(RaycastHit hit)
    {
        bool replaceTooltip = HitReplaceTrigger(hit);

        // if already the same state skip this block
        if (replacedInPreviousIteration != replaceTooltip)
        {
            AdjustTooltip(replaceTooltip);
            replacedInPreviousIteration = replaceTooltip;
        }
    }



    private bool HitReplaceTrigger(RaycastHit hit)
    {
        // checks if raycast hit  a target
        foreach (Transform target in targetObjects)
        {
            if (target.transform.gameObject == hit.collider.gameObject)
            {
                return true;
            }
        }

        return false;
    }


    
    private void AdjustTooltip(bool replaceTooltip)
    {
        if (replaceTooltip)
        {
            Replace();
        }
        else
        {
            Reset();
        }
    }



    void Start()
    {
        tooltipImage = tooltip.GetComponentInChildren<Image>();
        originalSprite = tooltipImage.sprite;
    }



    public void Replace()
    {
        tooltipImage.sprite = replacementSprite;
        tooltip.gameObject.SetActive(replacementState);
    }



    public void Reset()
    {
        tooltipImage.sprite = originalSprite;
        tooltip.gameObject.SetActive(originalState);
    }



    public void ToggleRaycast()
    {
        originalState = tooltip.gameObject.activeSelf;
        active = true;
    }
}

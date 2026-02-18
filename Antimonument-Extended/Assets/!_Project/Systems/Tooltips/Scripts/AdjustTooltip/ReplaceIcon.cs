using UnityEngine;
using UnityEngine.UI;

public class ReplaceIcon : MonoBehaviour
{
    [SerializeField] private Transform tooltip;
    [SerializeField] private Sprite replacementSprite;
    [SerializeField] private bool replacementState;

    private Sprite originalSprite;
    private bool originalState;
    private Image tooltipImage;

    void Start()
    {
        tooltipImage = tooltip.GetComponentInChildren<Image>();
        originalSprite = tooltipImage.sprite;
        originalState = tooltip.gameObject.activeSelf;
    }

    public void Replace()
    {
        tooltipImage.sprite = replacementSprite;
        tooltip.gameObject.SetActive(replacementState);
        Debug.Log("TOOLTIP >>> " + originalSprite.name + " changed to " + replacementSprite.name);
    }

    public void Reset()
    {
        tooltipImage.sprite = originalSprite;
        tooltip.gameObject.SetActive(originalState);
        Debug.Log("TOOLTIP >>> " + replacementSprite.name + " changed to " + originalSprite.name);
    }
}

using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class ReplaceText : MonoBehaviour
{
    [SerializeField] private Transform tooltip;
    [SerializeField] private string replacementText;
    [SerializeField] private bool replacementState;

    private string originalText;
    private bool originalState;
    private TextMeshProUGUI tooltipTextMesh;



    void Start()
    {
        tooltipTextMesh = tooltip.GetComponentInChildren<TextMeshProUGUI>();
        originalText = tooltipTextMesh.text;
        originalState = tooltip.gameObject.activeSelf;
    }



    public void Replace()
    {
        tooltipTextMesh.text = replacementText;
        tooltip.gameObject.SetActive(replacementState);
        Debug.Log("TOOLTIP >>> " + originalText + "changed to " + replacementText);
    }



    public void Reset()
    {
        tooltipTextMesh.text = originalText;
        tooltip.gameObject.SetActive(originalState);
        Debug.Log("TOOLTIP >>> " + replacementText + " changed to " + originalText);
    }
}

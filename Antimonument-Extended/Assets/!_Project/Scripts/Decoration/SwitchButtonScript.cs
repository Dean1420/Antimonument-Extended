using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SwitchButtonScript : MonoBehaviour
{
    [SerializeField] private float pressDistance = 0.05f;
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float buttonCooltime = 0.6f;

    private Vector3 startLocalPosition;
    private Vector3 targetLocalPosition;
    private bool isPressed = false;
    private bool isCooltime = false;

    void Start()
    {
        startLocalPosition = transform.localPosition;
        targetLocalPosition = startLocalPosition + new Vector3(0, -pressDistance, 0);
    }

    void Update()
    {
        Vector3 targetPos = isPressed ? targetLocalPosition : startLocalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * returnSpeed);
    }

    public void ButtonPressed()
    {
        if (isPressed || isCooltime) return;
        isPressed = true;

        Invoke("ResetCooltime", buttonCooltime);
    }

    public void ButtonReleased()
    {
        isPressed = false;
    }

    private void ResetCooltime()
    {
        isCooltime = false;
    }
}

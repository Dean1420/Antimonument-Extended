using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandSpecificAnnotationTrigger : MonoBehaviour
{
    [Header("Referenzen")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    [System.Serializable]
    public class HandLink
    {
        public string label; // nur zur Übersicht im Inspector, z.B. "Links" oder "Rechts"
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;     // der Interactor dieser Hand (z.B. Left/Right Controller Interactor)
        public ControllerAnnotation annotation; // die zugehörige Annotation für diese Hand
    }

    [Header("Hand-Zuordnung (2 Einträge: Links und Rechts)")]
    public HandLink[] hands;

    void OnEnable()
    {
        if (grabInteractable == null)
        {
            Debug.LogWarning("[HandSpecificAnnotationTrigger] Kein XRGrabInteractable zugewiesen.", this);
            return;
        }

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        if (grabInteractable == null) return;

        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var annotation = GetAnnotationForInteractor(args.interactorObject);
        if (annotation != null)
            annotation.ShowAnnotation();
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        var annotation = GetAnnotationForInteractor(args.interactorObject);
        if (annotation != null)
            annotation.HideAnnotation();
    }

    private ControllerAnnotation GetAnnotationForInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactorObject)
    {
        foreach (var hand in hands)
        {
            if (hand.interactor != null && (object)hand.interactor == (object)interactorObject)
                return hand.annotation;
        }
        return null;
    }
}
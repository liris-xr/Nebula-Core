using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRCustomGrabInteractable : XRGrabInteractable
{
    private Vector3 interactorPosition = Vector3.zero;
    private Quaternion interactorRotation = Quaternion.identity;

    /*protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);

        if (args.interactorObject is XRRayInteractor)
        {
            interactorPosition = args.interactorObject.GetAttachTransform(this).localPosition;
            interactorRotation = args.interactorObject.GetAttachTransform(this).localRotation;


            bool hasAttach = attachTransform != null;
            args.interactorObject.GetAttachTransform(this).position = hasAttach ? attachTransform.position : transform.position;
            args.interactorObject.GetAttachTransform(this).rotation = hasAttach ? attachTransform.rotation : transform.rotation;
        }
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);

        if (args.interactorObject is XRRayInteractor)
        {
            interactorPosition = Vector3.zero;
            interactorRotation = Quaternion.identity;
        }
    }*/

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        StoreInteractor(args.interactor);
        MatchAttachmentPoints(args.interactor);
    }

    private void StoreInteractor(XRBaseInteractor interactor)
    {
        interactorPosition = interactor.attachTransform.localPosition;
        interactorRotation = interactor.attachTransform.localRotation;
    }

    private void MatchAttachmentPoints(XRBaseInteractor interactor)
    {
        bool hasAttach = attachTransform != null;
        interactor.attachTransform.position = hasAttach ? attachTransform.position : transform.position;
        interactor.attachTransform.rotation = hasAttach ? attachTransform.rotation : transform.rotation;
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        ResetAttachmentPoint(args.interactor);
        ClearInteractor(args.interactor);
    }

    private void ResetAttachmentPoint(XRBaseInteractor interactor)
    {
        interactor.attachTransform.localPosition = interactorPosition;
        interactor.attachTransform.localRotation = interactorRotation;
    }

    private void ClearInteractor(XRBaseInteractor interactor)
    {
        interactorPosition = Vector3.zero;
        interactorRotation = Quaternion.identity;
    }

}
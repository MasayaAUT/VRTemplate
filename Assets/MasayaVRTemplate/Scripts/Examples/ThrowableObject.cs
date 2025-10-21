using UnityEngine;

public class ThrowableObject : MonoBehaviour, IGrabbable
{

    Rigidbody rb;
    VRControllerGrab currentController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void GrabStart(VRControllerGrab controller)
    {
        if (currentController != null)
        {
            if (currentController != controller)
            {
                currentController.GrabEnd();
                currentController = controller;
                ParentObject();
            }
        }
        else
        {
            currentController = controller;
            ParentObject();
        }
    }

    void ParentObject()
    {
        rb.isKinematic = true;
        transform.parent = currentController.transform;
    }

    public void GrabEnd()
    {
        transform.parent = null;
        currentController.GrabGone(true, transform);
        ApplyThrow();
        currentController = null;
    }

    void ApplyThrow()
    {
        rb.isKinematic = true;
    }
}

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
        GetComponent<Collider>().isTrigger = true;
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
        GetComponent<Collider>().isTrigger = false;
        rb.isKinematic = false;
        ThrowValues throwValues = currentController.GetThrowValues();

        rb.linearVelocity = throwValues.direction * throwValues.force * 2;
    }
}

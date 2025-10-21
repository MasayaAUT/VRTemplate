using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static VRController;

public class VRControllerGrab : MonoBehaviour
{
    VRController controller;
    List<Transform> grabbableList = new List<Transform>();
    public IGrabbable currentHeld { get; private set; }

    BoxCollider bc;

    Vector3 throwDirection;
    float throwForce;

    List<PhysicsSample> samples = new List<PhysicsSample>(32);
    struct PhysicsSample
    {
        public float t;
        public Vector3 pos;
    }

    private void Start()
    {
        controller = GetComponent<VRController>();
        bc = GetComponent<BoxCollider>();
        controller.onGrabStart += GrabStart;
        controller.onGrabEnd += GrabEnd;
    }

    private void LateUpdate()
    {
        CalculateForce();
    }

    private void OnDisable()
    {
        controller.onGrabStart -= GrabStart;
        controller.onGrabEnd -= GrabEnd;
    }

    [ContextMenu("Grab Start")]
    public void GrabStart()
    {
        if(grabbableList.Count > 0)
        {
            currentHeld = grabbableList[0].GetComponent<IGrabbable>();
            currentHeld.GrabStart(this);
        }
    }

    [ContextMenu("Grab End")]
    public void GrabEnd()
    {
        if(currentHeld != null)
        {
            currentHeld.GrabEnd();
        }

        ColliderCheck();
    }

    void ColliderCheck()
    {
        Vector3 colPos = bc.center;
        Vector3 colSize = bc.size;
        Collider[] hits = Physics.OverlapBox(transform.position + colPos, colSize / 2, transform.rotation);
        if(hits.Length > 0)
        {
            foreach(Collider col in hits)
            {
                if (col.gameObject.CompareTag("Grabbable") && !grabbableList.Contains(col.transform))
                {
                    grabbableList.Add(col.transform);
                }
            }
        }
    }

    public void GrabGone(bool removeFromList, Transform obj)
    {
        if (removeFromList)
        {
            grabbableList.Remove(obj);
        }
        currentHeld = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            grabbableList.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            grabbableList.Remove(other.transform);
            if (grabbableList.Count == 0)
                grabbableList.Clear();
        }
    }

    void CalculateForce()
    {
        samples.Add(new PhysicsSample
        {
            t = Time.time,
            pos = transform.localPosition,
        });
    }
}

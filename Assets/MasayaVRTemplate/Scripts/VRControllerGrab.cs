using System.Collections.Generic;
using Unity.VisualScripting;
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

    List<PhysicsSample> samples = new List<PhysicsSample>();
    struct PhysicsSample
    {
        public float time;
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
        float now = Time.time;

        samples.Add(new PhysicsSample
        {
            time = now,
            pos = transform.localPosition,
        });

        if(samples.Count >= 30)
        {
            samples.RemoveAt(0);
        }
    }

    public ThrowValues GetThrowValues()
    {
        ThrowValues returnValue = new ThrowValues()
        {
            force = 0,
            direction = Vector3.zero
        };

        if (samples == null || samples.Count < 2)
            return returnValue;

        Vector3 velocitySum = Vector3.zero;
        float totalTime = 0f;

        for (int i = 1; i < samples.Count; i++)
        {
            float dt = samples[i].time - samples[i - 1].time;
            if (dt <= 0f) continue;

            Vector3 v = (samples[i].pos - samples[i - 1].pos) / dt;
            velocitySum += v * dt;
            totalTime += dt;
        }

        Vector3 avgVel = velocitySum / Mathf.Max(1e-6f, totalTime);
        float force = avgVel.magnitude;
        Vector3 dir = avgVel.normalized;

        returnValue.force = force;
        returnValue.direction = dir;

        return returnValue;
    }
}

public class ThrowValues
{
    public float force;
    public Vector3 direction;
}

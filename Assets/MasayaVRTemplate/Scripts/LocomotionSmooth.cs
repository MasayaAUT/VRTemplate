using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LocomotionSmooth : MonoBehaviour
{

    VRRig vrRig;
    VRController controller;
    List<Transform> objDirections = new List<Transform>();
    [SerializeField] float speed;
    Rigidbody rb;
    public ForwardDirection forwardDirection;
    Vector2 analogInput;

    public enum ForwardDirection
    {
        head,
        leftController,
        rightController
    }

    private void Start()
    {
        vrRig = GetComponent<VRRig>();
        rb = GetComponent<Rigidbody>();
        controller = vrRig.GetController(VRRig.ControllerHand.Left);
        objDirections.Add(vrRig.head);
        objDirections.Add(vrRig.GetController(VRRig.ControllerHand.Left).transform);
        objDirections.Add(vrRig.GetController(VRRig.ControllerHand.Right).transform);
    }

    private void Update()
    {
        GetInputs();
    }
    void GetInputs()
    {
        analogInput = controller.controllerValues.analogValue;
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

   void ApplyMovement()
    {
        Vector3 forward = GetDirection(0) * analogInput.y;
        Vector3 right = GetDirection(1) * analogInput.x;

        Vector3 moveDirection = (forward + right).normalized;
        moveDirection *= Time.deltaTime * speed *100;

        rb.linearVelocity = moveDirection;
    }


    /// <summary>
    /// ctx = 0; return forward
    /// ctx = 1; return right
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="ctx"></param>
    /// <returns></returns>
    Vector3 GetDirection(int ctx)
    {
        Transform obj = objDirections[(int)forwardDirection];

        Vector3 returnValue = ctx == 0 ? obj.forward : obj.right;
        returnValue.y = 0;
        returnValue.Normalize();

        return returnValue;
    }
}

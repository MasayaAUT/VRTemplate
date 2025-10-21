using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.XR;

public class VRController : MonoBehaviour
{
    bool initialised;
    [SerializeField] ControllerHand controllerHand;
    [field: SerializeField] public Transform pointer { get; private set; }
    public VRController otherController { get; private set; }
    public VRControllerInteraction interaction { get; private set; }
    public VRControllerGrab grab { get; private set; }

    VRRig vrRig;
    VRActions inputs;
    XRController controller;
    public ControllerValues controllerValues { get; private set; }

    //Delegate Events
    public delegate void OnTeleportStart();
    public OnTeleportStart onTeleportStart;
    public delegate void OnTeleportEnd();
    public OnTeleportEnd onTeleportEnd;

    public delegate void OnGrabStart();
    public OnGrabStart onGrabStart;
    public delegate void OnGrabEnd();
    public OnGrabEnd onGrabEnd;

    public delegate void OnInteractStart();
    public OnInteractStart onInteractStart;
    public delegate void OnInteractEnd();
    public OnInteractEnd onInteractEnd;

    public enum ControllerHand
    {
        left,
        right
    }

    public ControllerHand GetHand()
    {
        return controllerHand;
    }

    public void Initialise(VRRig rig)
    {
        vrRig = rig;
        if(controllerHand == ControllerHand.left)
        {
            otherController = vrRig.GetController(VRRig.ControllerHand.Right);
        }
        else
        {
            otherController = vrRig.GetController(VRRig.ControllerHand.Left);
        }
        interaction = GetComponent<VRControllerInteraction>();
        grab = GetComponent<VRControllerGrab>();

        if (initialised)
        {
            EnableController();
        }
        else
        {
            StartCoroutine(LinkingController());
        }
    }

    IEnumerator LinkingController()
    {
        controllerValues = new ControllerValues();
        while(controller == null){
            switch (controllerHand)
            {
                case ControllerHand.left:
                    controller = InputSystem.GetDevice<XRController>(UnityEngine.InputSystem.CommonUsages.LeftHand);
                    break;
                case ControllerHand.right:
                    controller = InputSystem.GetDevice<XRController>(UnityEngine.InputSystem.CommonUsages.RightHand);
                    break;
            }

            yield return null;
        }
        inputs = new VRActions();
        InputUser inputUser = InputUser.PerformPairingWithDevice(controller.device);
        inputUser.AssociateActionsWithUser(inputs);

        EnableController();
        initialised = true;
    }

    void EnableController()
    {
        // Trigger
        inputs.VRInputs.TriggerPressed.performed += OnTriggerStarted;
        inputs.VRInputs.TriggerPressed.canceled += OnTriggerCanceled;

        // Grip
        inputs.VRInputs.GripPressed.performed += OnGripStarted;
        inputs.VRInputs.GripPressed.canceled += OnGripCanceled;

        // Analog (thumbstick click?)
        inputs.VRInputs.AnalogPressed.performed += OnAnalogStarted;
        inputs.VRInputs.AnalogPressed.canceled += OnAnalogCanceled;

        // Primary
        inputs.VRInputs.PrimaryButtonPressed.performed += OnPrimaryStarted;
        inputs.VRInputs.PrimaryButtonPressed.canceled += OnPrimaryCanceled;

        // Secondary
        inputs.VRInputs.SecondaryButtonPressed.performed += OnSecondaryStarted;
        inputs.VRInputs.SecondaryButtonPressed.canceled += OnSecondaryCanceled;

        //Teleport
        inputs.VRInputs.Teleport.performed += OnTeleportStarted;
        inputs.VRInputs.Teleport.canceled += OnTeleportCanceled;

        //Grab
        inputs.VRInputs.Grab.performed += OnGrabStarted;
        inputs.VRInputs.Grab.canceled += OnGrabCanceled;

        //Interact
        inputs.VRInputs.Interact.performed += OnInteractStarted;
        inputs.VRInputs.Interact.canceled += OnInteractCanceled;

        inputs.Enable();
    }

    private void OnDisable()
    {
        DisableController();
    }

    void DisableController()
    {

        // Trigger
        inputs.VRInputs.TriggerPressed.performed -= OnTriggerStarted;
        inputs.VRInputs.TriggerPressed.canceled -= OnTriggerCanceled;

        // Grip
        inputs.VRInputs.GripPressed.performed -= OnGripStarted;
        inputs.VRInputs.GripPressed.canceled -= OnGripCanceled;

        // Analog (thumbstick click?)
        inputs.VRInputs.AnalogPressed.performed -= OnAnalogStarted;
        inputs.VRInputs.AnalogPressed.canceled -= OnAnalogCanceled;

        // Primary
        inputs.VRInputs.PrimaryButtonPressed.performed -= OnPrimaryStarted;
        inputs.VRInputs.PrimaryButtonPressed.canceled -= OnPrimaryCanceled;

        // Secondary
        inputs.VRInputs.SecondaryButtonPressed.performed -= OnSecondaryStarted;
        inputs.VRInputs.SecondaryButtonPressed.canceled -= OnSecondaryCanceled;

        //Teleport
        inputs.VRInputs.Teleport.performed -= OnTeleportStarted;
        inputs.VRInputs.Teleport.canceled -= OnTeleportCanceled;

        //Grab
        inputs.VRInputs.Grab.performed -= OnGrabStarted;
        inputs.VRInputs.Grab.canceled -= OnGrabCanceled;

        //Interact
        inputs.VRInputs.Interact.performed -= OnInteractStarted;
        inputs.VRInputs.Interact.canceled -= OnInteractCanceled;

        inputs.Disable();
    }


    private void Update()
    {
        if (!initialised)
            return;

        controllerValues.triggerValue = inputs.VRInputs.TriggerValue.ReadValue<float>();
        controllerValues.gripValue = inputs.VRInputs.GripValue.ReadValue<float>();
        controllerValues.analogValue = inputs.VRInputs.AnalogValue.ReadValue<Vector2>();
    }

    #region handlers
    void OnTriggerStarted(InputAction.CallbackContext ctx) { controllerValues.triggerPressed = true; OnTriggerPressed(ctx); }
    void OnTriggerCanceled(InputAction.CallbackContext ctx) { controllerValues.triggerPressed = false; OnTriggerReleased(ctx); }

    void OnGripStarted(InputAction.CallbackContext ctx) { controllerValues.gripPressed = true; OnGripPressed(ctx); }
    void OnGripCanceled(InputAction.CallbackContext ctx) { controllerValues.gripPressed = false; OnGripReleased(ctx); }

    void OnAnalogStarted(InputAction.CallbackContext ctx) { controllerValues.analogPressed = true; OnAnalogPressed(ctx); }
    void OnAnalogCanceled(InputAction.CallbackContext ctx) { controllerValues.analogPressed = false; OnAnalogReleased(ctx); }

    void OnPrimaryStarted(InputAction.CallbackContext ctx) { controllerValues.primaryButtonPressed = true; OnPrimaryPressed(ctx); }
    void OnPrimaryCanceled(InputAction.CallbackContext ctx) { controllerValues.primaryButtonPressed = false; OnPrimaryReleased(ctx); }

    void OnSecondaryStarted(InputAction.CallbackContext ctx) { controllerValues.secondaryButtonPressed = true; OnSecondaryPressed(ctx); }
    void OnSecondaryCanceled(InputAction.CallbackContext ctx) { controllerValues.secondaryButtonPressed = false; OnSecondaryReleased(ctx); }

    void OnTeleportStarted(InputAction.CallbackContext ctx) { onTeleportStart?.Invoke(); }
    void OnTeleportCanceled(InputAction.CallbackContext ctx) { onTeleportEnd?.Invoke(); }

    void OnGrabStarted(InputAction.CallbackContext ctx) { onGrabStart?.Invoke(); }
    void OnGrabCanceled(InputAction.CallbackContext ctx) { onGrabEnd?.Invoke(); }

    void OnInteractStarted(InputAction.CallbackContext ctx) { onInteractStart?.Invoke(); }
    void OnInteractCanceled(InputAction.CallbackContext ctx) { onInteractEnd?.Invoke(); }
    #endregion

    #region Individual Input Checks -> If you need more control over buttons
    private void OnTriggerPressed(InputAction.CallbackContext ctx)
    {

    }
    public void OnTriggerReleased(InputAction.CallbackContext ctx)
    {

    }
    public void OnGripPressed(InputAction.CallbackContext ctx)
    {

    }
    public void OnGripReleased(InputAction.CallbackContext ctx)
    {

    }
    private void OnPrimaryPressed(InputAction.CallbackContext ctx)
    {

    }
    private void OnPrimaryReleased(InputAction.CallbackContext ctx)
    {

    }
    private void OnSecondaryPressed(InputAction.CallbackContext ctx)
    {

    }
    private void OnSecondaryReleased(InputAction.CallbackContext ctx)
    {

    }
    public void OnAnalogPressed(InputAction.CallbackContext ctx)
    {

    }
    public void OnAnalogReleased(InputAction.CallbackContext ctx)
    {

    }
    #endregion
}

[System.Serializable]
public class ControllerValues
{
    public bool triggerPressed;
    public float triggerValue;
    public bool gripPressed;
    public float gripValue;
    public bool primaryButtonPressed;
    public bool secondaryButtonPressed;
    public bool analogPressed;
    public Vector2 analogValue;
}

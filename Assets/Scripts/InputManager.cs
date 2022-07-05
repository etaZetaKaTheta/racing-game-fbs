using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class InputManager : NetworkBehaviour
{
    public Controls controls;

    public static event Action ShiftUp;
    public static event Action ShiftDown;

    public float accelerationInput;
    public float steeringInput;

    private void Awake()
    {
        controls = new Controls();
        controls.Enable();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Main.GearShiftingUp.performed += OnShiftUp;
        controls.Main.GearShiftingDown.performed += OnShiftDown;
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.Main.GearShiftingUp.performed -= OnShiftUp;
        controls.Main.GearShiftingDown.performed -= OnShiftDown;
    }

    private void Update()
    {
        accelerationInput = controls.Main.CarMovement.ReadValue<Vector2>().y;
        steeringInput = controls.Main.CarMovement.ReadValue<Vector2>().x;
    }

    private void OnShiftUp(InputAction.CallbackContext ctx)
    {
        ShiftUp?.Invoke();
    }

    private void OnShiftDown(InputAction.CallbackContext ctx)
    {
        ShiftDown?.Invoke();
    }
}

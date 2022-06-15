using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 10.0f;
    [SerializeField] private float jumpHeight = 10.0f;


    private Rigidbody rb;
    private InputManager inputManager;

    private Vector2 moveInput;

    /*private void Awake()
    {
        if(!IsLocalPlayer)
        {
            enabled = false;
        }
    }*/

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out inputManager);
    }

    void Start()
    {
        if (!IsLocalPlayer)
        {
            enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputManager.controls.Main.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        inputManager.controls.Main.Jump.performed -= Jump;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        moveInput = inputManager.controls.Main.Walk.ReadValue<Vector2>();

        rb.AddForce(new Vector3(moveInput.x, 0.0f, moveInput.y) * movementSpeed, ForceMode.Acceleration);
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
    }
}

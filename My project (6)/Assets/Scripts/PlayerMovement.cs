using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Camera playerCamera;

    private float walkSpeed = 5.0f;
    private float runSpeed = 7.0f;
    private float crouchSpeed = 1.5f;
    private float gravity = -12.81f;
    private float jumpForce = 2.0f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2.0f;
    public float upDownRange = 90.0f;

    [Header("Advanced Settings")]
    public float bobFrequency = 5.0f;
    public float bobIntensity = 0.1f;

    private bool isRunning;
    private bool isCrouching;

    private float rotationX = 0;

    private Vector3 playerVelocity;
    private Vector3 originalCameraPosition;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalCameraPosition = playerCamera.transform.localPosition;
    }

    private void Update()
    {
        HandleMovementInput();
        HandleMouseLook();
        HandleCrouch();
        HandleJump();

        ApplyFinalMovements();
        ApplyBobbing();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.forward * vertical) + (transform.right * horizontal);
        moveDirection.Normalize();

        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        float speed = isRunning ? runSpeed : isCrouching ? crouchSpeed : walkSpeed;

        if (controller.isGrounded)
        {
            playerVelocity.y = -0.5f; // Reset vertical velocity when grounded

            if (Input.GetButtonDown("Jump"))
            {
                playerVelocity.y = Mathf.Sqrt(jumpForce * -2.0f * gravity);
            }
        }
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        moveDirection *= speed;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX += mouseY;
        rotationX = Mathf.Clamp(rotationX, -upDownRange, upDownRange);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseX, 0);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                controller.height = 0.5f;
                originalCameraPosition = playerCamera.transform.localPosition;
                playerCamera.transform.localPosition = new Vector3(0, 0.5f, 0);
            }
            else
            {
                controller.height = 2.0f;
                playerCamera.transform.localPosition = originalCameraPosition;
            }
        }
    }

    private void HandleJump()
    {
        if (controller.isGrounded && Mathf.Abs(controller.velocity.y) < 0.01f)
        {
            playerVelocity.y = -0.5f;

            if (Input.GetButtonDown("Jump"))
            {
                playerVelocity.y = Mathf.Sqrt(jumpForce * -2.0f * gravity);
            }
        }
    }

    private void ApplyFinalMovements()
    {
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void ApplyBobbing()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 targetBobPosition = originalCameraPosition;

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            float bobAmount = Mathf.Sin(Time.time * bobFrequency) * bobIntensity;
            targetBobPosition.y += bobAmount;
        }

        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetBobPosition, Time.deltaTime * 10f);
    }
}

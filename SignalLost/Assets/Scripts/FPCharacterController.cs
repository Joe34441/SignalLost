using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCharacterController : MonoBehaviour
{

    [SerializeField] private Transform playerCamera = null;
    [SerializeField] private float gravityStrength = 9.81f;
    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float cameraYMaxRotation = 75.0f;
    [SerializeField] private float moveSmoothTime = 0.1f;
    [SerializeField] private float mouseSmoothTime = 0.03f;

    private CharacterController controller = null;

    private Vector2 currentDirection = Vector2.zero;
    private Vector2 currentDirectionVelocity = Vector2.zero;
    private  Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;

    private bool lockCursor = true;
    private float velocityY = 0.0f;
    private float cameraYRotation = 0.0f;

    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        UpdateMouseLook();
        UpdateMovement();
    }

    private void UpdateMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraYRotation -= currentMouseDelta.y * mouseSensitivity;
        cameraYRotation = Mathf.Clamp(cameraYRotation, -cameraYMaxRotation, cameraYMaxRotation);

        playerCamera.localEulerAngles = Vector3.right * cameraYRotation;

        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }

    private void UpdateMovement()
    {
        Vector2 targetDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDirection.Normalize();

        currentDirection = Vector2.SmoothDamp(currentDirection, targetDirection, ref currentDirectionVelocity, moveSmoothTime);

        if (controller.isGrounded)
        {
            velocityY = -controller.stepOffset / Time.deltaTime;
        }
        else
        {
            velocityY = -gravityStrength * Time.deltaTime * 50;
        }

        Vector3 velocity = (transform.forward * currentDirection.y + transform.right * currentDirection.x) * walkSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);
    }

}
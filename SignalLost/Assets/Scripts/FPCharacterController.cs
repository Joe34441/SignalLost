using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCharacterController : MonoBehaviour
{
    [SerializeField] private MoveArm moveArm;

    [SerializeField] private float gravityStrength = 9.81f;

    [SerializeField] private Transform playerCamera = null;

    [SerializeField] private float cameraYMaxRotation = 75.0f;
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float mouseSmoothTime = 0.03f;

    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float moveSmoothTime = 0.1f;

    [SerializeField] private float maxDistance = 10.0f;

    private PlayerBodyParts BodyParts = null;
    private CharacterController controller = null;

    private Vector2 currentDirection = Vector2.zero;
    private Vector2 currentDirectionVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;

    private bool lockCursor = true;
    private float velocityY = 0.0f;
    private float cameraYRotation = 0.0f;

    private bool LeftMouseButtonPressed = false;
    private bool LeftMouseButtonReleased = false;
    private bool RightMouseButtonPressed = false;
    private bool RightMouseButtonReleased = false;


    private GameObject fakeRightHand = null;
    private GameObject fakeLeftHand = null;

    public GameObject GetFakeRightHand() { return fakeRightHand; }
    public GameObject GetFakeLeftHand() { return fakeLeftHand; }


    private void Start()
    {
        BodyParts = gameObject.GetComponent<PlayerBodyParts>();
        controller = gameObject.GetComponent<CharacterController>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        ResetMouseButtonFlags();

        UpdateMouseLook();
        UpdateMovement();
        UpdateMouseButtonInputs();

        ProcessInputs();
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

    private void UpdateMouseButtonInputs()
    {
        //Left Mouse Button
        if (Input.GetKeyDown(KeyCode.Mouse0)) LeftMouseButtonPressed = true;
        if (Input.GetKeyUp(KeyCode.Mouse0)) LeftMouseButtonReleased = true;
        //Right Mouse Button
        if (Input.GetKeyDown(KeyCode.Mouse1)) RightMouseButtonPressed = true;
        if (Input.GetKeyUp(KeyCode.Mouse1)) RightMouseButtonReleased = true;

    }

    private void ProcessInputs()
    {
        if (LeftMouseButtonPressed)
        {
            RaycastHit outHit;
            Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out outHit))
            {
                GameObject objectHit = outHit.transform.gameObject;

                Vector3 hitPoint = outHit.point;

                //Debug.Log("Hit: " + objectHit.name);

                float distanceFromLeft = Vector3.Distance(BodyParts.GetLeftHand().transform.position, hitPoint);
                float distanceFromRight = Vector3.Distance(BodyParts.GetRightHand().transform.position, hitPoint);

                if (distanceFromLeft < distanceFromRight)
                {
                    //Debug.Log("Left hand is closer! left: " + distanceFromLeft + ", right: " + distanceFromRight);
                }
                else
                {
                    //Debug.Log("Right hand is closer! left: " + distanceFromLeft + ", right: " + distanceFromRight);
                }


                Destroy(fakeLeftHand);

                GameObject hand = BodyParts.GetLeftHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                fakeLeftHand = Instantiate(hand);
                hand.GetComponent<MeshRenderer>().enabled = false;
                BodyParts.SetLeftHand(hand);

                fakeLeftHand.transform.position = hitPoint;

                moveArm.SetWidth(0.1f);
                moveArm.DrawLeftArm(true);
            }

        }

        if (LeftMouseButtonReleased)
        {
            Destroy(fakeLeftHand);

            GameObject hand = BodyParts.GetLeftHand();
            hand.GetComponent<MeshRenderer>().enabled = true;
            BodyParts.SetLeftHand(hand);

            moveArm.DrawLeftArm(false);
        }




        if (RightMouseButtonPressed)
        {
            RaycastHit outHit;
            Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out outHit))
            {
                GameObject objectHit = outHit.transform.gameObject;

                Vector3 hitPoint = outHit.point;

                //Debug.Log("Hit: " + objectHit.name);

                float distanceFromLeft = Vector3.Distance(BodyParts.GetLeftHand().transform.position, hitPoint);
                float distanceFromRight = Vector3.Distance(BodyParts.GetRightHand().transform.position, hitPoint);

                if (distanceFromLeft < distanceFromRight)
                {
                    //Debug.Log("Left hand is closer! left: " + distanceFromLeft + ", right: " + distanceFromRight);
                }
                else
                {
                    //Debug.Log("Right hand is closer! left: " + distanceFromLeft + ", right: " + distanceFromRight);
                }


                Destroy(fakeRightHand);

                GameObject hand = BodyParts.GetRightHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                fakeRightHand = Instantiate(hand);
                hand.GetComponent<MeshRenderer>().enabled = false;
                BodyParts.SetRightHand(hand);

                fakeRightHand.transform.position = hitPoint;

                moveArm.SetWidth(0.1f);
                moveArm.DrawRightArm(true);

            }

        }

        if (RightMouseButtonReleased)
        {
            Destroy(fakeRightHand);

            GameObject hand = BodyParts.GetRightHand();
            hand.GetComponent<MeshRenderer>().enabled = true;
            BodyParts.SetRightHand(hand);

            moveArm.DrawRightArm(false);
        }



        //if (LeftMouseButtonPressed) Debug.Log("LMB Press");
        //if (LeftMouseButtonReleased) Debug.Log("LMB Release");

        //if (RightMouseButtonPressed) Debug.Log("RMB Press");
        //if (RightMouseButtonReleased) Debug.Log("RMB Release");


    }

    private void ResetMouseButtonFlags()
    {
        LeftMouseButtonPressed = false;
        LeftMouseButtonReleased = false;
        RightMouseButtonPressed = false;
        RightMouseButtonReleased = false;
    }
}

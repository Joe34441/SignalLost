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

    private Vector3 fakeRightHandDestination;
    private Vector3 fakeLeftHandDestination;

    private Vector3 fakeRightHandStartPosition;
    private Vector3 fakeLeftHandStartPosition;

    private bool moveRightHandAway;
    private bool moveRightHandBack;
    private bool moveLeftHandAway;
    private bool moveLeftHandBack;

    float speed = 15.0f;
    float rightHandStartTime = 0.0f;
    float leftHandStartTime = 0.0f;
    float rightHandJourneyLength = 0.0f;
    float leftHandJourneyLength = 0.0f;

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

        UpdateHandPositions();
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
        if (LeftMouseButtonPressed && !moveLeftHandBack)
        {
            RaycastHit outHit;
            Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out outHit))
            {
                GameObject objectHit = outHit.transform.gameObject;

                Vector3 hitPoint = outHit.point;

                Destroy(fakeLeftHand);

                GameObject hand = BodyParts.GetLeftHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                fakeLeftHand = Instantiate(hand);
                hand.GetComponent<MeshRenderer>().enabled = false;
                BodyParts.SetLeftHand(hand);

                fakeLeftHandDestination = hitPoint;
                fakeLeftHandStartPosition = BodyParts.GetLeftHand().transform.position;
                moveLeftHandAway = true;
                leftHandStartTime = 0.0f;

                moveArm.SetWidth(0.1f);
                moveArm.DrawLeftArm(true);
            }
        }

        if (LeftMouseButtonReleased && !moveLeftHandBack && fakeLeftHand)
        {
            fakeLeftHandDestination = BodyParts.GetLeftHand().transform.position;
            fakeLeftHandStartPosition = fakeLeftHand.transform.position;

            moveLeftHandAway = false;
            moveLeftHandBack = true;
            leftHandStartTime = 0.0f;
        }


        if (RightMouseButtonPressed && !moveRightHandBack)
        {
            RaycastHit outHit;
            Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out outHit))
            {
                GameObject objectHit = outHit.transform.gameObject;

                Vector3 hitPoint = outHit.point;

                Destroy(fakeRightHand);

                GameObject hand = BodyParts.GetRightHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                fakeRightHand = Instantiate(hand);
                hand.GetComponent<MeshRenderer>().enabled = false;
                BodyParts.SetRightHand(hand);

                fakeRightHandDestination = hitPoint;
                fakeRightHandStartPosition = BodyParts.GetRightHand().transform.position;
                moveRightHandAway = true;
                rightHandStartTime = 0.0f;

                moveArm.SetWidth(0.1f);
                moveArm.DrawRightArm(true);
            }
        }

        if (RightMouseButtonReleased && !moveRightHandBack && fakeRightHand)
        {
            fakeRightHandDestination = BodyParts.GetRightHand().transform.position;
            fakeRightHandStartPosition = fakeRightHand.transform.position;

            moveRightHandAway = false;
            moveRightHandBack = true;
            rightHandStartTime = 0.0f;
        }
    }

    private void ResetMouseButtonFlags()
    {
        LeftMouseButtonPressed = false;
        LeftMouseButtonReleased = false;
        RightMouseButtonPressed = false;
        RightMouseButtonReleased = false;
    }

    private void UpdateHandPositions()
    {
        if (moveRightHandAway)
        {
            if (rightHandStartTime == 0)
            {
                rightHandStartTime = Time.time;
                rightHandJourneyLength = Vector3.Distance(fakeRightHandStartPosition, fakeRightHandDestination);
            }

            float distanceCovered = (Time.time - rightHandStartTime) * speed;
            float fractionOfJourney = distanceCovered / rightHandJourneyLength;

            fakeRightHand.transform.position = Vector3.Lerp(fakeRightHandStartPosition, fakeRightHandDestination, fractionOfJourney);

            float distanceFromHand = Vector3.Distance(BodyParts.GetRightHand().transform.position, fakeRightHand.transform.position);
            if (distanceFromHand > maxDistance)
            {
                fakeRightHandDestination = BodyParts.GetRightHand().transform.position;
                fakeRightHandStartPosition = fakeRightHand.transform.position;
                moveRightHandAway = false;
                moveRightHandBack = true;
                rightHandStartTime = 0.0f;
            }
        }
        else if (moveRightHandBack)
        {
            if (rightHandStartTime == 0)
            {
                rightHandStartTime = Time.time;
                rightHandJourneyLength = Vector3.Distance(fakeRightHandStartPosition, fakeRightHandDestination);
            }

            float distanceCovered = (Time.time - rightHandStartTime) * speed;
            float fractionOfJourney = distanceCovered / rightHandJourneyLength;

            fakeRightHand.transform.position = Vector3.Lerp(fakeRightHandStartPosition, BodyParts.GetRightHand().transform.position, fractionOfJourney);

            if (fakeRightHand.transform.position == BodyParts.GetRightHand().transform.position)
            {
                Destroy(fakeRightHand);

                GameObject hand = BodyParts.GetRightHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                BodyParts.SetRightHand(hand);
                moveArm.DrawRightArm(false);
                moveRightHandBack = false;
            }
        }

        if (moveLeftHandAway)
        {
            if (leftHandStartTime == 0)
            {
                leftHandStartTime = Time.time;
                leftHandJourneyLength = Vector3.Distance(fakeLeftHandStartPosition, fakeLeftHandDestination);
            }

            float distanceCovered = (Time.time - leftHandStartTime) * speed;
            float fractionOfJourney = distanceCovered / leftHandJourneyLength;

            fakeLeftHand.transform.position = Vector3.Lerp(fakeLeftHandStartPosition, fakeLeftHandDestination, fractionOfJourney);

            float distanceFromHand = Vector3.Distance(BodyParts.GetLeftHand().transform.position, fakeLeftHand.transform.position);
            if (distanceFromHand > maxDistance)
            {
                fakeLeftHandDestination = BodyParts.GetLeftHand().transform.position;
                fakeLeftHandStartPosition = fakeLeftHand.transform.position;
                moveLeftHandAway = false;
                moveLeftHandBack = true;
                leftHandStartTime = 0.0f;
            }
        }
        else if (moveLeftHandBack)
        {
            if (leftHandStartTime == 0)
            {
                leftHandStartTime = Time.time;
                leftHandJourneyLength = Vector3.Distance(fakeLeftHandStartPosition, fakeLeftHandDestination);
            }

            float distanceCovered = (Time.time - leftHandStartTime) * speed;
            float fractionOfJourney = distanceCovered / leftHandJourneyLength;

            fakeLeftHand.transform.position = Vector3.Lerp(fakeLeftHandStartPosition, BodyParts.GetLeftHand().transform.position, fractionOfJourney);

            if (fakeLeftHand.transform.position == BodyParts.GetLeftHand().transform.position)
            {
                Destroy(fakeLeftHand);

                GameObject hand = BodyParts.GetLeftHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                BodyParts.SetLeftHand(hand);
                moveArm.DrawLeftArm(false);
                moveLeftHandBack = false;
            }
        }
    }
}

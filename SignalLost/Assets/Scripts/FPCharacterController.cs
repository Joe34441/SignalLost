using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCharacterController : MonoBehaviour
{
    [SerializeField] private GameStateManager GameManager;

    [SerializeField] private MoveArm Arm;

    [SerializeField] private float gravityStrength = 9.81f;

    [SerializeField] private Transform playerCamera = null;

    [SerializeField] private float cameraYMaxRotation = 75.0f;
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float mouseSmoothTime = 0.03f;

    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float moveSmoothTime = 0.1f;

    [SerializeField] private float maxDistance = 10.0f;

    [SerializeField] private Material mainMaterial;
    [SerializeField] private float firstWarningDistance = 5.0f;
    [SerializeField] private Material firstWarningMaterial;
    [SerializeField] private float lastWarningDistance = 8.0f;
    [SerializeField] private Material lastWarningMaterial;

    [SerializeField] private string portTag = "Port";
    [SerializeField] private string dockTag = "Dock";
    [SerializeField] private string floorSignalTag = "SignalFloor";
    [SerializeField] private string centreCircuitBoardTag = "Board";
    [SerializeField] private string fakeHandTag = "FakeHand";
    [SerializeField] private string checkpointTriggerTag = "Checkpoint";

    [SerializeField] private Material shoulderNoSignalMaterial;
    [SerializeField] private Material shoulderHasSignalMaterial;

    [SerializeField] private GameObject leftCircuitBoard;
    [SerializeField] private GameObject rightCircuitBoard;
    [SerializeField] private GameObject centreCircuitBoard;

    [SerializeField] private GameObject thinSmokeEffect;
    [SerializeField] private GameObject thickSmokeEffect;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject fireEffect;

    [SerializeField] private List<GameObject> checkpoints = new List<GameObject>();

    private List<GameObject> checkpointTriggers = new List<GameObject>();
    private int currentCheckpointIndex = 0;

    private bool hasSignal;
    private int floorSignalCount = 0;

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

    float handMoveSpeed = 15.0f;
    float rightHandStartTime = 0.0f;
    float leftHandStartTime = 0.0f;
    float rightHandJourneyLength = 0.0f;
    float leftHandJourneyLength = 0.0f;

    private bool rightHandMovingToDock = false;
    private bool leftHandMovingToDock = false;
    private bool rightHandDocked = false;
    private bool leftHandDocked = false;

    private bool playerKilled = false;
    bool isPlayerResetReady = true;
    bool forceResetPlayer = false;
    private bool playDeathParticles = true;

    private bool playingParticles = false;

    private float deathParticlesTimer = 0.0f;
    private float deathParticleTotalTime = 8.0f;

    private GameObject deathParticleEffect1Left;
    private GameObject deathParticleEffect1Right;
    private GameObject deathParticleEffect2Left;
    private GameObject deathParticleEffect2Right;
    private GameObject deathParticleEffect3Left;
    private GameObject deathParticleEffect3Right;
    private GameObject deathParticleEffect4Left;
    private GameObject deathParticleEffect4Right;


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
        ProcessDockedState();

        if (CheckHasSignal() && !playerKilled)
        {
            UpdateMouseLook();
            UpdateMovement();

            ResetMouseButtonFlags();
            UpdateMouseButtonInputs();
            ProcessInputs();

            isPlayerResetReady = true;
            playDeathParticles = false;
        }
        else
        {
            if (isPlayerResetReady)
            {
                KillPlayer();
                GameManager.AddDeath();
            }
        }

        UpdateHandPositions();
        UpdateArmColour();

        ManagePlayerReset();

        ManageParticles();
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
                Vector3 hitPoint = outHit.point;
                leftHandMovingToDock = false;

                GameObject objectHit = outHit.transform.gameObject;
                if (objectHit.CompareTag(portTag))
                {
                    foreach (Transform transform in objectHit.transform)
                    {
                        if (transform.CompareTag(dockTag))
                        {
                            hitPoint = transform.position;
                            leftHandMovingToDock = true;
                        }
                    }
                }

                Destroy(fakeLeftHand);

                GameObject hand = BodyParts.GetLeftHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                fakeLeftHand = Instantiate(hand);
                hand.GetComponent<MeshRenderer>().enabled = false;
                BodyParts.SetLeftHand(hand);

                SphereCollider myCollider = fakeLeftHand.AddComponent<SphereCollider>();
                myCollider.isTrigger = true;
                fakeLeftHand.tag = fakeHandTag;
                fakeLeftHandDestination = hitPoint;
                fakeLeftHandStartPosition = BodyParts.GetLeftHand().transform.position;
                moveLeftHandAway = true;
                leftHandStartTime = 0.0f;

                Arm.SetWidth(0.1f);
                Arm.DrawLeftArm(true);

                if (objectHit.CompareTag(centreCircuitBoardTag))
                {
                    playerKilled = true;
                }
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
                Vector3 hitPoint = outHit.point;
                rightHandMovingToDock = false;

                GameObject objectHit = outHit.transform.gameObject;
                if (objectHit.CompareTag(portTag))
                {
                    foreach (Transform transform in objectHit.transform)
                    {
                        if (transform.CompareTag(dockTag))
                        {
                            hitPoint = transform.position;
                            rightHandMovingToDock = true;
                        }
                    }
                }

                Destroy(fakeRightHand);

                GameObject hand = BodyParts.GetRightHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                fakeRightHand = Instantiate(hand);
                hand.GetComponent<MeshRenderer>().enabled = false;
                BodyParts.SetRightHand(hand);

                SphereCollider myCollider = fakeRightHand.AddComponent<SphereCollider>();
                myCollider.isTrigger = true;
                fakeRightHand.tag = fakeHandTag;
                fakeRightHandDestination = hitPoint;
                fakeRightHandStartPosition = BodyParts.GetRightHand().transform.position;
                moveRightHandAway = true;
                rightHandStartTime = 0.0f;

                Arm.SetWidth(0.1f);
                Arm.DrawRightArm(true);

                if (objectHit.CompareTag(centreCircuitBoardTag))
                {
                    playerKilled = true;
                }
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

            float distanceCovered = (Time.time - rightHandStartTime) * handMoveSpeed;
            float fractionOfJourney = distanceCovered / rightHandJourneyLength;

            fakeRightHand.transform.position = Vector3.Lerp(fakeRightHandStartPosition, fakeRightHandDestination, fractionOfJourney);

            if (fakeRightHand.transform.position == fakeRightHandDestination && rightHandMovingToDock)
            {
                rightHandDocked = true;
            }

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
            rightHandDocked = false;

            if (rightHandStartTime == 0)
            {
                rightHandStartTime = Time.time;
                rightHandJourneyLength = Vector3.Distance(fakeRightHandStartPosition, fakeRightHandDestination);
            }

            float distanceCovered = (Time.time - rightHandStartTime) * handMoveSpeed;
            float fractionOfJourney = distanceCovered / rightHandJourneyLength;

            fakeRightHand.transform.position = Vector3.Lerp(fakeRightHandStartPosition, BodyParts.GetRightHand().transform.position, fractionOfJourney);

            if (fakeRightHand.transform.position == BodyParts.GetRightHand().transform.position)
            {
                Destroy(fakeRightHand);

                GameObject hand = BodyParts.GetRightHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                BodyParts.SetRightHand(hand);
                Arm.DrawRightArm(false);
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

            float distanceCovered = (Time.time - leftHandStartTime) * handMoveSpeed;
            float fractionOfJourney = distanceCovered / leftHandJourneyLength;

            fakeLeftHand.transform.position = Vector3.Lerp(fakeLeftHandStartPosition, fakeLeftHandDestination, fractionOfJourney);

            if (fakeLeftHand.transform.position == fakeLeftHandDestination && leftHandMovingToDock)
            {
                leftHandDocked = true;
            }

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
            leftHandDocked = false;

            if (leftHandStartTime == 0)
            {
                leftHandStartTime = Time.time;
                leftHandJourneyLength = Vector3.Distance(fakeLeftHandStartPosition, fakeLeftHandDestination);
            }

            float distanceCovered = (Time.time - leftHandStartTime) * handMoveSpeed;
            float fractionOfJourney = distanceCovered / leftHandJourneyLength;

            fakeLeftHand.transform.position = Vector3.Lerp(fakeLeftHandStartPosition, BodyParts.GetLeftHand().transform.position, fractionOfJourney);

            if (fakeLeftHand.transform.position == BodyParts.GetLeftHand().transform.position)
            {
                Destroy(fakeLeftHand);

                GameObject hand = BodyParts.GetLeftHand();
                hand.GetComponent<MeshRenderer>().enabled = true;
                BodyParts.SetLeftHand(hand);
                Arm.DrawLeftArm(false);
                moveLeftHandBack = false;
            }
        }
    }

    private void UpdateArmColour()
    {
        if (fakeLeftHand && !moveLeftHandBack)
        {
            float distanceFromHand = Vector3.Distance(BodyParts.GetLeftHand().transform.position, fakeLeftHand.transform.position);

            if (distanceFromHand >= lastWarningDistance) Arm.ChangeColour(0, lastWarningMaterial);
            else if (distanceFromHand >= firstWarningDistance) Arm.ChangeColour(0, firstWarningMaterial);
            else Arm.ChangeColour(0, mainMaterial);
        }

        if (fakeRightHand && !moveRightHandBack)
        {
            float distanceFromHand = Vector3.Distance(BodyParts.GetRightHand().transform.position, fakeRightHand.transform.position);

            if (distanceFromHand >= lastWarningDistance) Arm.ChangeColour(1, lastWarningMaterial);
            else if (distanceFromHand >= firstWarningDistance) Arm.ChangeColour(1, firstWarningMaterial);
            else Arm.ChangeColour(1, mainMaterial);
        }
    }

    private void ProcessDockedState()
    {
        if (rightHandDocked) BodyParts.GetRightShoulder().GetComponent<MeshRenderer>().material = shoulderHasSignalMaterial;
        else BodyParts.GetRightShoulder().GetComponent<MeshRenderer>().material = shoulderNoSignalMaterial;

        if (leftHandDocked)  BodyParts.GetLeftShoulder().GetComponent<MeshRenderer>().material = shoulderHasSignalMaterial;
        else BodyParts.GetLeftShoulder().GetComponent<MeshRenderer>().material = shoulderNoSignalMaterial;
        
        if (!leftHandDocked && !rightHandDocked) hasSignal = false;
        else hasSignal = true;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(floorSignalTag))
        {
            floorSignalCount++;
        }

        if (other.gameObject.CompareTag(checkpointTriggerTag))
        {
            if (checkpointTriggers.Count > 0)
            {
                bool contains = false;
                foreach (GameObject go in checkpointTriggers)
                {
                    if (go == other.gameObject) contains = true;
                }

                if (!contains)
                {
                    checkpointTriggers.Add(other.gameObject);
                    currentCheckpointIndex++;
                }
            }
            else
            {
                checkpointTriggers.Add(other.gameObject);
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(floorSignalTag))
        {
            floorSignalCount--;
        }
    }

    private bool CheckHasSignal()
    {
        if (floorSignalCount > 0 || hasSignal) return true;

        return false;
    }

    private void KillPlayer()
    {
        isPlayerResetReady = false;
        playerKilled = true;
        playDeathParticles = true;

        if (moveLeftHandAway)
        {
            moveLeftHandBack = true;
            moveLeftHandAway = false;
        }
        if (moveRightHandAway)
        {
            moveRightHandBack = true;
            moveRightHandAway = false;
        }

        Invoke("ResetPlayer", deathParticleTotalTime);
    }

    private void ResetPlayer()
    {
        gameObject.transform.position = checkpoints[0].transform.position;
        playerKilled = false;
        forceResetPlayer = true;
    }

    private void TurnOffForceReset()
    {
        forceResetPlayer = false;
    }

    private void PlayDeathParticles()
    {
        playingParticles = true;

        deathParticlesTimer += Time.deltaTime;

        if (deathParticlesTimer <= 2.0f)
        {
            if (!deathParticleEffect1Left) deathParticleEffect1Left = Instantiate(thinSmokeEffect, leftCircuitBoard.transform.position, Quaternion.identity);
            if (!deathParticleEffect1Right) deathParticleEffect1Right = Instantiate(thinSmokeEffect, rightCircuitBoard.transform.position, Quaternion.identity);
        }
        else if (deathParticlesTimer <= 4.0f)
        {
            if (!deathParticleEffect2Left) deathParticleEffect2Left = Instantiate(thickSmokeEffect, leftCircuitBoard.transform.position, Quaternion.identity);
            if (!deathParticleEffect2Right) deathParticleEffect2Right = Instantiate(thickSmokeEffect, rightCircuitBoard.transform.position, Quaternion.identity);
        }
        else if (deathParticlesTimer <= 8.0f)
        {
            if (deathParticleEffect1Left) Destroy(deathParticleEffect1Left);
            if (deathParticleEffect1Right) Destroy(deathParticleEffect1Right);

            if (deathParticleEffect2Left) Destroy(deathParticleEffect2Left);
            if (deathParticleEffect2Right) Destroy(deathParticleEffect2Right);


            if (!deathParticleEffect3Left) deathParticleEffect3Left = Instantiate(explosionEffect, leftCircuitBoard.transform.position, Quaternion.identity);
            if (!deathParticleEffect3Right) deathParticleEffect3Right = Instantiate(explosionEffect, rightCircuitBoard.transform.position, Quaternion.identity);
            if (!deathParticleEffect4Left) deathParticleEffect4Left = Instantiate(fireEffect, leftCircuitBoard.transform.position, Quaternion.identity);
            if (!deathParticleEffect4Right) deathParticleEffect4Right = Instantiate(fireEffect, rightCircuitBoard.transform.position, Quaternion.identity);
        }
    }

    private void StopDeathParticles()
    {
        if (deathParticleEffect1Left) Destroy(deathParticleEffect1Left);
        if (deathParticleEffect1Right) Destroy(deathParticleEffect1Right);
        if (deathParticleEffect2Left) Destroy(deathParticleEffect2Left);
        if (deathParticleEffect2Right) Destroy(deathParticleEffect2Right);
        if (deathParticleEffect3Left) Destroy(deathParticleEffect3Left);
        if (deathParticleEffect3Right) Destroy(deathParticleEffect3Right);
        if (deathParticleEffect4Left) Destroy(deathParticleEffect4Left);
        if (deathParticleEffect4Right) Destroy(deathParticleEffect4Right);

        deathParticlesTimer = 0.0f;
        playDeathParticles = false;
        playingParticles = false;
    }

    private void ManagePlayerReset()
    {
        if (forceResetPlayer)
        {
            gameObject.transform.position = checkpoints[currentCheckpointIndex].transform.position;
            gameObject.transform.rotation = checkpoints[currentCheckpointIndex].transform.rotation;
            Invoke("TurnOffForceReset", 0.5f);
        }
    }

    private void ManageParticles()
    {
        if (playDeathParticles)
        {
            if (!playingParticles) Invoke("StopDeathParticles", deathParticleTotalTime + 0.1f);
            PlayDeathParticles();
        }
    }
}

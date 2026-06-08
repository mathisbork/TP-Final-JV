using UnityEngine;
using UnityEngine.InputSystem; // Indispensable pour le nouveau système

[RequireComponent(typeof(CharacterController))]
public class CharacterController3D : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Caméras Enfants")]
    public Camera fpCamera;
    public TPSCameraOrbit tpCameraScript;

    [Header("Inputs (Nouveau Système)")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference jumpAction;
    public InputActionReference switchCameraAction;

    [Header("Mouvement")]
    public float playerSpeed = 7.0f;
    public float gravityValue = -9.81f;
    public float rotationSpeed = 15.0f;

    [Header("Caméra FPS (Sensibilité)")]
    public float mouseSensitivity = 0.1f; // Réduit pour le nouveau système
    private float xRotation = 0f;

    [Header("Saut & Double Saut")]
    public float jumpHeight = 2.0f;
    private bool canDoubleJump;
    public bool hasDoubleJumpAbility = true;

    [Header("Grappin")]
    [HideInInspector] public bool isGrappling = false;
    [HideInInspector] public Vector3 grappleTarget;
    private float grappleSpeed;
    [HideInInspector] public bool isFirstPersonMode = true;

    private bool _isCurrentPlayer = false;
    public bool isCurrentPlayer
    {
        get { return _isCurrentPlayer; }
        set
        {
            _isCurrentPlayer = value;
            if (_isCurrentPlayer) ActivateCurrentMode();
            else TurnOffAllCameras();
        }
    }

    void Awake() { controller = GetComponent<CharacterController>(); }

    // On allume les inputs quand le script s'active
    void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        jumpAction?.action.Enable();
        switchCameraAction?.action.Enable();
    }

    void Update()
    {
        if (!_isCurrentPlayer)
        {
            ApplyGravityOnly();
            return;
        }

        // BASCULE CAMÉRA
        if (switchCameraAction.action.WasPressedThisFrame())
        {
            isFirstPersonMode = !isFirstPersonMode;
            ActivateCurrentMode();
        }

        // Si on est tiré par le grappin, on court-circuite le mouvement normal !
        if (isGrappling)
        {
            HandleRotations(); // On peut toujours regarder autour
            Vector3 pullDirection = (grappleTarget - transform.position).normalized;
            controller.Move(pullDirection * grappleSpeed * Time.deltaTime);
            return;
        }

        HandleRotations();
        HandleMovement();
        HandleJump();
    }

    public void ActivateCurrentMode()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (isFirstPersonMode)
        {
            if (tpCameraScript != null) { tpCameraScript.isActive = false; tpCameraScript.gameObject.SetActive(false); }
            if (fpCamera != null) fpCamera.gameObject.SetActive(true);
        }
        else
        {
            if (fpCamera != null) fpCamera.gameObject.SetActive(false);
            if (tpCameraScript != null) { tpCameraScript.gameObject.SetActive(true); tpCameraScript.isActive = true; }
        }
    }

    public void TurnOffAllCameras()
    {
        if (fpCamera != null) fpCamera.gameObject.SetActive(false);
        if (tpCameraScript != null) { tpCameraScript.isActive = false; tpCameraScript.gameObject.SetActive(false); }
    }



    void HandleRotations()
    {
        // On n'applique la rotation du corps à la souris QUE si on est en FPS !
        if (isFirstPersonMode)
        {
            // Lecture du mouvement de la souris via l'Input System
            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;

            // Fait pivoter le corps de gauche à droite
            transform.Rotate(Vector3.up * mouseX);

            // Fait pivoter la tête (caméra FPS) de haut en bas
            if (fpCamera != null)
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                fpCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }
    }

    void HandleMovement()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            canDoubleJump = true;
        }

        // Lecture du mouvement (ZQSD/Joystick) via l'Input System
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 inputMove = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        if (inputMove.magnitude >= 0.1f)
        {
            Vector3 moveDirection = Vector3.zero;

            if (isFirstPersonMode && fpCamera != null)
            {
                moveDirection = fpCamera.transform.right * inputMove.x + fpCamera.transform.forward * inputMove.z;
            }
            else if (!isFirstPersonMode && tpCameraScript != null)
            {
                Vector3 camForward = tpCameraScript.transform.forward;
                Vector3 camRight = tpCameraScript.transform.right;
                camForward.y = 0f; camRight.y = 0f;

                moveDirection = camRight * inputMove.x + camForward * inputMove.z;

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            moveDirection.y = 0f;
            controller.Move(moveDirection.normalized * Time.deltaTime * playerSpeed);
        }
    }

    void HandleJump()
    {
        if (jumpAction.action.WasPressedThisFrame())
        {
            if (groundedPlayer) Jump();
            else if (canDoubleJump && hasDoubleJumpAbility) { Jump(); canDoubleJump = false; }
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void Jump() { playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue); }

    void ApplyGravityOnly()
    {
        if (!controller.isGrounded)
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
        else playerVelocity.y = 0f;
    }
    public void StartGrapplePull(Vector3 target, float speed)
    {
        isGrappling = true;
        grappleTarget = target;
        grappleSpeed = speed;
        playerVelocity.y = 0f; // On annule la gravité
    }

    public void StopGrapple()
    {
        isGrappling = false;
        // Petit saut vertical à l'arrivée pour bien atterrir sur le rebord
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -1.0f * gravityValue);
    }
}
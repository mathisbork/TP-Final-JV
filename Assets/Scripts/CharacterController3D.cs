using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterController3D : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Caméras Enfants")]
    public Camera fpCamera;
    public TPSCameraOrbit tpCameraScript;

    // On garde le FPS par défaut
    private bool isFirstPersonMode = true;

    [Header("Mouvement")]
    public float playerSpeed = 7.0f;
    public float gravityValue = -9.81f;
    public float rotationSpeed = 15.0f;

    [Header("Caméra FPS (Sensibilité)")]
    public float mouseSensitivity = 2.0f;
    private float xRotation = 0f;

    [Header("Saut & Double Saut")]
    public float jumpHeight = 2.0f;
    private bool canDoubleJump;
    public bool hasDoubleJumpAbility = true;

    // LE SECRET EST ICI : C'est une "Property". Dès que le PlayerSwitcher 
    // change cette valeur, le code dans le "set" s'exécute INSTANTANÉMENT.
    private bool _isCurrentPlayer = false;
    public bool isCurrentPlayer
    {
        get { return _isCurrentPlayer; }
        set
        {
            _isCurrentPlayer = value;
            if (_isCurrentPlayer)
            {
                ActivateCurrentMode(); // Allume LA bonne caméra
            }
            else
            {
                TurnOffAllCameras();   // Éteint TOUT pour ce perso
            }
        }
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Si le perso n'est pas joué, on lui applique juste la gravité
        if (!_isCurrentPlayer)
        {
            ApplyGravityOnly();
            return;
        }

        // --- BASCULE FPS / TPS (F4) ---
        if (Input.GetKeyDown(KeyCode.F4))
        {
            isFirstPersonMode = !isFirstPersonMode;
            ActivateCurrentMode(); // Met à jour les caméras instantanément
        }

        // --- GESTION DES ROTATIONS ---
        HandleRotations();

        // --- GESTION DES DÉPLACEMENTS ---
        HandleMovement();

        // --- GESTION DU SAUT ---
        HandleJump();
    }

    // --- FONCTIONS DE SÉCURITÉ POUR LES CAMÉRAS ---

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

    // --- FONCTIONS DE GAMEPLAY ---

    void HandleRotations()
    {
        if (isFirstPersonMode)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotation du corps de gauche à droite
            transform.Rotate(Vector3.up * mouseX);

            // Rotation de la TÊTE de haut en bas (c'est ce qu'il manquait !)
            if (fpCamera != null)
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Empêche de se faire un torticolis
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

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 inputMove = new Vector3(moveX, 0, moveZ).normalized;

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
                camForward.y = 0f;
                camRight.y = 0f;

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
        if (Input.GetButtonDown("Jump"))
        {
            if (groundedPlayer) Jump();
            else if (canDoubleJump && hasDoubleJumpAbility) { Jump(); canDoubleJump = false; }
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void Jump()
    {
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
    }

    void ApplyGravityOnly()
    {
        if (!controller.isGrounded)
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
        else playerVelocity.y = 0f;
    }
}
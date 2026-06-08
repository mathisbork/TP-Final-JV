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

    private bool isFirstPersonMode = true;

    [Header("Mouvement")]
    public float playerSpeed = 7.0f;
    public float gravityValue = -9.81f;
    public float rotationSpeed = 15.0f;

    [Header("Saut & Double Saut")]
    public float jumpHeight = 2.0f;
    private bool canDoubleJump;
    public bool hasDoubleJumpAbility = true;

    [HideInInspector] public bool isCurrentPlayer = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (isCurrentPlayer)
        {
            UpdateCameraSystem();
        }
    }

    void Update()
    {
        if (!isCurrentPlayer)
        {
            if (fpCamera.gameObject.activeSelf) fpCamera.gameObject.SetActive(false);
            if (tpCameraScript != null && tpCameraScript.gameObject.activeSelf)
            {
                tpCameraScript.isActive = false;
                tpCameraScript.gameObject.SetActive(false);
            }
            ApplyGravityOnly();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            isFirstPersonMode = !isFirstPersonMode;
        }

        UpdateCameraSystem();

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

        if (Input.GetButtonDown("Jump"))
        {
            if (groundedPlayer)
            {
                Jump();
            }
            else if (canDoubleJump && hasDoubleJumpAbility)
            {
                Jump();
                canDoubleJump = false;
            }
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void UpdateCameraSystem()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (isFirstPersonMode)
        {
            if (fpCamera != null && !fpCamera.gameObject.activeSelf) fpCamera.gameObject.SetActive(true);
            if (tpCameraScript != null && tpCameraScript.gameObject.activeSelf)
            {
                tpCameraScript.isActive = false;
                tpCameraScript.gameObject.SetActive(false);
            }

            float mouseX = Input.GetAxis("Mouse X") * 2f;
            transform.Rotate(Vector3.up * mouseX);
        }
        else
        {
            if (fpCamera != null && fpCamera.gameObject.activeSelf) fpCamera.gameObject.SetActive(false);
            if (tpCameraScript != null && !tpCameraScript.gameObject.activeSelf) tpCameraScript.gameObject.SetActive(true);

            if (tpCameraScript != null) tpCameraScript.isActive = true;
        }
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
        else
        {
            playerVelocity.y = 0f;
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class TPSCameraOrbit : MonoBehaviour
{
    public Transform target;

    [Header("Input (Nouveau Système)")]
    public InputActionReference lookAction;

    [Header("Réglages de la Sphère")]
    public float distance = 3.0f;
    public float targetHeight = 1.6f;

    [Header("Sensibilité")]
    public float xSpeed = 30.0f;
    public float ySpeed = 30.0f;

    [Header("Limites Verticales")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    [HideInInspector] public bool isActive = false;

    void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
    }

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (!isActive) return;

        if (target == null)
        {
            Debug.LogWarning("ATTENTION : La Target n'est pas assignée sur " + gameObject.name);
            return;
        }

        if (lookAction == null)
        {
            Debug.LogWarning("ATTENTION : La Look Action n'est pas assignée sur " + gameObject.name);
            return;
        }

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        x += lookInput.x * xSpeed * 0.02f;
        y -= lookInput.y * ySpeed * 0.02f;

        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + targetPosition;

        transform.rotation = rotation;
        transform.position = position;
    }
}
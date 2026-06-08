using UnityEngine;

public class TPSCameraOrbit : MonoBehaviour
{
    public Transform target;

    [Header("Réglages de la Sphère")]
    public float distance = 2.3f;
    public float targetHeight = 2f;

    [Header("Sensibilité")]
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    [Header("Limites Verticales")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    [HideInInspector] public bool isActive = false;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (!isActive || target == null) return;

        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);

        Vector3 targetPosition = target.position + Vector3.up * targetHeight;

        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + targetPosition;

        transform.rotation = rotation;
        transform.position = position;
    }
}
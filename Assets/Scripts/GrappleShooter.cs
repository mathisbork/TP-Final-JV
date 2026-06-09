using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class GrappleShooter : MonoBehaviour
{
    public CharacterController3D characterController;
    public InputActionReference fireAction;

    [Header("Configuration")]
    public GameObject hookPrefab;
    public Transform shootPoint;

    [Header("Forces & Vitesses")]
    public float throwForce = 25f;
    public float amaruPullSpeed = 25f;
    public float raftRetractSpeed = 15f;

    private GrappleHook currentHook;
    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
    }

    void OnEnable() { fireAction?.action.Enable(); }

    void Update()
    {
        if (!characterController.isCurrentPlayer) return;

        if (fireAction.action.WasPressedThisFrame() && currentHook == null)
        {
            Shoot();
        }

        if (currentHook != null)
        {
            lr.enabled = true;
            lr.SetPosition(0, shootPoint.position);
            lr.SetPosition(1, currentHook.transform.position);

            if (currentHook.isAttached)
            {
                if (currentHook.isLedge)
                {
                    characterController.StartGrapplePull(currentHook.transform.position, amaruPullSpeed);
                }
                else
                {
                    if (fireAction.action.IsPressed())
                    {
                        currentHook.Retract(raftRetractSpeed);
                    }
                }
            }
        }

        if (characterController.isGrappling)
        {
            if (Vector3.Distance(transform.position, characterController.grappleTarget) < 1.5f)
            {
                DestroyHook();
            }
        }
    }

    void Shoot()
    {
        Transform camTransform = characterController.isFirstPersonMode ? characterController.fpCamera.transform : characterController.tpCameraScript.transform;

        Vector3 targetPoint;
        Ray ray = new Ray(camTransform.position, camTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;

        GameObject hookObj = Instantiate(hookPrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
        currentHook = hookObj.GetComponent<GrappleHook>();
        currentHook.playerTransform = transform;
        currentHook.shooter = this;

        Rigidbody rb = hookObj.GetComponent<Rigidbody>();
        rb.AddForce(shootDirection * throwForce, ForceMode.Impulse);
    }

    public void HookHit(GrappleHook hook, bool isLedge)
    {
        if (isLedge) Debug.Log("Arête ! Amaru Mode activé.");
        else Debug.Log("Mur plat. Raft Mode activé.");
    }

    public void DestroyHook()
    {
        if (currentHook != null) Destroy(currentHook.gameObject);
        currentHook = null;
        lr.enabled = false;
        characterController.StopGrapple();
    }
}
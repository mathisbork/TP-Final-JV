using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class GrappleShooter : MonoBehaviour
{
    public CharacterController3D characterController;
    public InputActionReference fireAction;

    [Header("Configuration")]
    public GameObject hookPrefab;
    public Transform shootPoint; // Là d'où part le grappin (ex: la main)

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
        // Règle la taille de la corde
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
    }

    void OnEnable() { fireAction?.action.Enable(); }

    void Update()
    {
        // On ne peut tirer que si c'est notre tour de jouer
        if (!characterController.isCurrentPlayer) return;

        // 1. TIRER LE GRAPPIN
        if (fireAction.action.WasPressedThisFrame() && currentHook == null)
        {
            Shoot();
        }

        // 2. GESTION DU GRAPPIN EN JEU
        if (currentHook != null)
        {
            // Dessiner la corde
            lr.enabled = true;
            lr.SetPosition(0, shootPoint.position);
            lr.SetPosition(1, currentHook.transform.position);

            if (currentHook.isAttached)
            {
                if (currentHook.isLedge)
                {
                    // MODE AMARU : On dit au perso de s'envoler vers l'arête
                    characterController.StartGrapplePull(currentHook.transform.position, amaruPullSpeed);
                }
                else
                {
                    // MODE RAFT : Si on maintient le clic gauche, le grappin revient
                    if (fireAction.action.IsPressed())
                    {
                        currentHook.Retract(raftRetractSpeed);
                    }
                }
            }
        }

        // 3. ARRÊT DU MODE AMARU
        if (characterController.isGrappling)
        {
            // Si on est arrivé en haut du mur, on coupe tout
            if (Vector3.Distance(transform.position, characterController.grappleTarget) < 1.5f)
            {
                characterController.StopGrapple();
                DestroyHook();
            }
        }
    }

    void Shoot()
    {
        // 1. On récupère la bonne caméra (FPS ou TPS)
        Transform camTransform = characterController.isFirstPersonMode ? characterController.fpCamera.transform : characterController.tpCameraScript.transform;

        // 2. On trouve le point exact visé par le crosshair (centre de l'écran)
        Vector3 targetPoint;
        Ray ray = new Ray(camTransform.position, camTransform.forward);

        // On lance un laser invisible depuis la caméra. S'il touche un mur à moins de 100m...
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point; // ... la cible est ce point sur le mur.
        }
        else
        {
            // Sinon (on vise le ciel), on prend un point dans le vide très loin devant.
            targetPoint = ray.GetPoint(100f);
        }

        // 3. On calcule la VRAIE direction : de la main vers la cible
        Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;

        // 4. On crée le grappin orienté vers cette cible
        GameObject hookObj = Instantiate(hookPrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
        currentHook = hookObj.GetComponent<GrappleHook>();
        currentHook.playerTransform = transform;
        currentHook.shooter = this;

        // 5. On propulse le grappin dans cette nouvelle direction
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
        characterController.StopGrapple(); // Sécurité
    }
}
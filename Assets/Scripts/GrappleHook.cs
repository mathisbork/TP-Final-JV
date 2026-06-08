using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrappleHook : MonoBehaviour
{
    private Rigidbody rb;
    public bool isAttached = false;
    public bool isLedge = false;

    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public GrappleShooter shooter;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isAttached) return;

        rb.isKinematic = true;
        isAttached = true;

        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        if (Mathf.Abs(normal.y) < 0.5f)
        {
            Vector3 topCheckStart = contact.point + (Vector3.up * 0.5f) - (normal * 0.2f);
            
            if (Physics.Raycast(topCheckStart, Vector3.down, out RaycastHit hit, 1f))
            {
                if (hit.normal.y > 0.7f) 
                {
                    isLedge = true;
                }
            }
        }

        shooter.HookHit(this, isLedge);
    }

    public void Retract(float speed)
    {
        if (playerTransform == null) return;
        
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);


        if (Vector3.Distance(transform.position, playerTransform.position) < 1.5f)
        {
            shooter.DestroyHook();
        }
    }
}
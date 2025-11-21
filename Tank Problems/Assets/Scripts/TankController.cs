using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Movement")]
    public float driveSpeed = 6f;
    public float turnSpeed = 180f; // degrees per second

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 12f;
    public float fireCooldown = 0.5f;
    public int bulletBounces = 3;

    [Header("Identity")]
    public int playerId = 1; // 1 or 2

    Rigidbody2D rb;
    float fireTimer = 0f;

    // Keys (customizable per instance)
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode shootKey = KeyCode.LeftControl;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        // Shooting (use Update for input)
        if (Input.GetKeyDown(shootKey) && fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireCooldown;
        }
    }

    void FixedUpdate()
    {
        float forward = 0f;
        if (Input.GetKey(forwardKey)) forward = 1f;
        if (Input.GetKey(backKey)) forward = -0.6f; // slower reverse

        float turn = 0f;
        if (Input.GetKey(leftKey)) turn = 1f;
        if (Input.GetKey(rightKey)) turn = -1f;

        // Movement
        Vector2 move = transform.up * forward * driveSpeed;
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

        // Rotation
        float rot = turn * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + rot);
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
        if (brb != null)
        {
            brb.velocity = transform.up * bulletSpeed;
        }

        Bullet bulletScript = b.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.ownerId = playerId;
            bulletScript.remainingBounces = bulletBounces;
        }
    }

    // Optional: call this to reset for respawn
    public void ResetForRespawn(Vector2 position, float rotation)
    {
        rb.position = position;
        rb.rotation = rotation;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
}

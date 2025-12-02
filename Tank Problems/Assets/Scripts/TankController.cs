using UnityEngine;
using System.Collections;

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

    [Header("Shotgun Power-Up")]
    public bool shotgunActive = false;
    public int shotgunPellets = 3;          // how many bullets per shot
    public float shotgunSpreadAngle = 20f;  // total cone angle in degrees
    public float shotgunDuration = 5f;      // default duration for power-up
    Coroutine shotgunCoroutine;

    // Only used to push pellets forward while shotgun is active
    private bool useShotgunOffset = false;
    private float shotgunSpawnOffset = 0.25f; // tweak if pellets still collide

    // Input keys (can be customized per instance in inspector)
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode shootKey = KeyCode.LeftControl;

    Rigidbody2D rb;
    float fireTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        // Shooting (input handled in Update)
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

    // ==========================================================
    //  SHOOTING LOGIC
    // ==========================================================
    void Fire()
    {
        if (!shotgunActive)
            FireSingle();
        else
            FireShotgun();
    }

    void FireSingle()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        Rigidbody2D brb = b.GetComponent<Rigidbody2D>();

        if (brb != null)
            brb.velocity = transform.up * bulletSpeed;

        Bullet bulletScript = b.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.ownerId = playerId;
            bulletScript.remainingBounces = bulletBounces;
        }
    }

    void FireShotgun()
    {
        if (bulletPrefab == null || firePoint == null || shotgunPellets <= 0) return;

        float halfSpread = shotgunSpreadAngle / 2f;

        for (int i = 0; i < shotgunPellets; i++)
        {
            // t ranges 0..1 across the pellets so they spread evenly
            float t = (shotgunPellets == 1) ? 0.5f : (float)i / (shotgunPellets - 1);
            float angle = Mathf.Lerp(-halfSpread, halfSpread, t);

            // Compute pellet rotation relative to the tank
            Quaternion pelletRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);

            // Spawn position: apply small forward offset only when shotgun offset is active
            Vector3 spawnPos = firePoint.position;
            if (useShotgunOffset)
            {
                // firePoint.up is the forward direction (consistent with movement)
                spawnPos += firePoint.up * shotgunSpawnOffset;
            }

            GameObject b = Instantiate(bulletPrefab, spawnPos, pelletRotation);
            Rigidbody2D brb = b.GetComponent<Rigidbody2D>();

            if (brb != null)
            {
                // Bullet should travel along the pellet's forward (up) direction
                Vector2 dir = pelletRotation * Vector2.up;
                brb.velocity = dir.normalized * bulletSpeed;
            }

            Bullet bulletScript = b.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.ownerId = playerId;
                bulletScript.remainingBounces = bulletBounces;
            }
        }
    }

    // ==========================================================
    //  SHOTGUN POWER-UP ACTIVATION
    // ==========================================================
    /// <summary>
    /// Call to grant shotgun mode for a duration. This also enables a small forward spawn offset
    /// to keep pellets from colliding with the firing tank.
    /// </summary>
    public void ApplyShotgun(int pellets, float spreadAngle, float duration)
    {
        shotgunPellets = Mathf.Max(1, pellets);
        shotgunSpreadAngle = Mathf.Max(0f, spreadAngle);

        // ensure previous coroutine is stopped so duration resets
        if (shotgunCoroutine != null)
            StopCoroutine(shotgunCoroutine);

        // enable offset while shotgun is active
        useShotgunOffset = true;

        shotgunCoroutine = StartCoroutine(ShotgunTimer(duration));
    }

    IEnumerator ShotgunTimer(float duration)
    {
        shotgunActive = true;
        float start = Time.time;

        // Simple timer loop so other systems can still run
        while (Time.time - start < duration)
        {
            yield return null;
        }

        shotgunActive = false;
        useShotgunOffset = false;
        shotgunCoroutine = null;
    }

    // ==========================================================
    //  RESPAWN SUPPORT
    // ==========================================================
    // Optional: call this to reset for respawn (keeps Rigidbody consistent)
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
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

    // Input keys
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

        // Shooting
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
        if (Input.GetKey(backKey)) forward = -0.6f;

        float turn = 0f;
        if (Input.GetKey(leftKey)) turn = 1f;
        if (Input.GetKey(rightKey)) turn = -1f;

        Vector2 move = transform.up * forward * driveSpeed;
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

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
        if (bulletPrefab == null || firePoint == null) return;

        float halfSpread = shotgunSpreadAngle / 2f;

        for (int i = 0; i < shotgunPellets; i++)
        {
            float t = (shotgunPellets == 1) ? 0.5f : (float)i / (shotgunPellets - 1);
            float angle = Mathf.Lerp(-halfSpread, halfSpread, t);

            Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, angle);

            GameObject b = Instantiate(bulletPrefab, firePoint.position, rot);
            Rigidbody2D brb = b.GetComponent<Rigidbody2D>();

            if (brb != null)
                brb.velocity = b.transform.up * bulletSpeed; // fire from rotated direction

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
    public void ApplyShotgun(int pellets, float spreadAngle, float duration)
    {
        shotgunPellets = pellets;
        shotgunSpreadAngle = spreadAngle;

        if (shotgunCoroutine != null)
            StopCoroutine(shotgunCoroutine);

        shotgunCoroutine = StartCoroutine(ShotgunTimer(duration));
    }

    IEnumerator ShotgunTimer(float duration)
    {
        shotgunActive = true;
        yield return new WaitForSeconds(duration);
        shotgunActive = false;
        shotgunCoroutine = null;
    }

    // ==========================================================
    //  RESPAWN SUPPORT
    // ==========================================================
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

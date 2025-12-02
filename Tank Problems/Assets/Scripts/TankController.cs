using UnityEngine;
using System.Collections;

public class TankController : MonoBehaviour
{
    [Header("Movement")]
    public float driveSpeed = 6f;
    public float turnSpeed = 180f; // degrees per second

    [Header("Shooting")]
    public GameObject bulletPrefab;         // normal bullet prefab
    public GameObject sniperBulletPrefab;   // sniper bullet prefab (pierces walls)
    public Transform firePoint;
    public float bulletSpeed = 12f;
    public float sniperBulletSpeed = 28f; // faster for sniper feel
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
    private float shotgunSpawnOffset = 0.5f; // tweak to avoid overlaps

    [Header("Sniper Power-Up")]
    // Timed sniper mode (optional) — kept for flexibility
    public bool sniperActive = false;
    public float sniperDuration = 6f;
    Coroutine sniperCoroutine;

    // Single-shot charges (preferred for balance)
    public int sniperCharges = 0; // number of single sniper shots available

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
    //  SHOOTING LOGIC (priority: sniper charge -> timed sniper -> shotgun -> single)
    // ==========================================================
    void Fire()
    {
        // Consume sniper single-shot charges first
        if (sniperCharges > 0)
        {
            sniperCharges--;
            FireSniper();
            return;
        }

        // If timed sniper mode active, use sniper
        if (sniperActive)
        {
            FireSniper();
            return;
        }

        // Shotgun (if active)
        if (shotgunActive)
        {
            FireShotgun();
            return;
        }

        // Default single shot
        FireSingle();
    }

    void FireSingle()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // spawn at firePoint (no offset for normal shot)
        GameObject b = Instantiate(bulletPrefab, firePoint.position, transform.rotation);

        // Immediately set owner and bounce count
        Bullet bulletScript = b.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.ownerId = playerId;
            bulletScript.remainingBounces = bulletBounces;
        }

        // Ignore collisions between this bullet and the firing tank
        IgnoreBulletOwnerCollision(b);

        // Set velocity
        Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
        if (brb != null)
            brb.velocity = transform.up * bulletSpeed;
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
                spawnPos += firePoint.up * shotgunSpawnOffset;
            }

            GameObject b = Instantiate(bulletPrefab, spawnPos, pelletRotation);

            // Immediately set owner and bounce count
            Bullet bulletScript = b.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.ownerId = playerId;
                bulletScript.remainingBounces = bulletBounces;
            }

            // Ignore collisions between this bullet and the firing tank
            IgnoreBulletOwnerCollision(b);

            // Set velocity along the pellet's forward direction (up)
            Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
            if (brb != null)
            {
                Vector2 dir = pelletRotation * Vector2.up;
                brb.velocity = dir.normalized * bulletSpeed;
            }
        }
    }

    // Sniper shot: spawns a specialized sniper bullet prefab that passes through walls
    void FireSniper()
    {
        if (sniperBulletPrefab == null || firePoint == null) return;

        // spawn slightly forward so it doesn't overlap the tank
        Vector3 spawnPos = firePoint.position + firePoint.up * 0.6f;
        Quaternion rot = transform.rotation;

        GameObject b = Instantiate(sniperBulletPrefab, spawnPos, rot);

        // set owner if SniperBullet script exists
        var sb = b.GetComponent<SniperBullet>();
        if (sb != null)
            sb.ownerId = playerId;

        // also ignore collisions between sniper bullet collider and tank colliders just to be safe
        IgnoreBulletOwnerCollision(b);

        Rigidbody2D brb = b.GetComponent<Rigidbody2D>();
        if (brb != null)
            brb.velocity = transform.up * sniperBulletSpeed;
    }

    // ==========================================================
    //  HELPER: Ignore collision between the newly spawned bullet and this tank
    // ==========================================================
    void IgnoreBulletOwnerCollision(GameObject bullet)
    {
        if (bullet == null) return;

        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol == null) return;

        // ignore collision for all colliders on this tank (root + children)
        Collider2D[] ownerCols = GetComponentsInChildren<Collider2D>();
        if (ownerCols == null || ownerCols.Length == 0) return;

        foreach (var oc in ownerCols)
        {
            if (oc == null) continue;
            if (oc == bulletCol) continue;
            Physics2D.IgnoreCollision(bulletCol, oc, true);
        }
    }

    // ==========================================================
    //  SHOTGUN POWER-UP ACTIVATION
    // ==========================================================
    public void ApplyShotgun(int pellets, float spreadAngle, float duration)
    {
        shotgunPellets = Mathf.Max(1, pellets);
        shotgunSpreadAngle = Mathf.Max(0f, spreadAngle);

        if (shotgunCoroutine != null)
            StopCoroutine(shotgunCoroutine);

        useShotgunOffset = true;
        shotgunCoroutine = StartCoroutine(ShotgunTimer(duration));
    }

    IEnumerator ShotgunTimer(float duration)
    {
        shotgunActive = true;
        float start = Time.time;

        while (Time.time - start < duration)
            yield return null;

        shotgunActive = false;
        useShotgunOffset = false;
        shotgunCoroutine = null;
    }

    // ==========================================================
    //  SNIPER POWER-UP (timed mode; optional)
    // ==========================================================
    public void ApplySniperTimed(float duration)
    {
        if (sniperCoroutine != null)
            StopCoroutine(sniperCoroutine);

        sniperCoroutine = StartCoroutine(SniperTimer(duration));
    }

    IEnumerator SniperTimer(float duration)
    {
        sniperActive = true;
        float start = Time.time;

        while (Time.time - start < duration)
            yield return null;

        sniperActive = false;
        sniperCoroutine = null;
    }

    // ==========================================================
    //  SNIPER SINGLE-SHOT (preferred)
    // ==========================================================
    /// <summary>
    /// Grants one or more one-shot sniper charges. Each charge will be consumed on the next Fire() call.
    /// </summary>
    public void ApplySniperSingleShot(int count = 1)
    {
        sniperCharges += Mathf.Max(1, count);
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

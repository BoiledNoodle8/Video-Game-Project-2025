using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SniperBullet : MonoBehaviour
{
    public int ownerId = 0;
    public float lifeTime = 6f;
    public bool destroyOnHit = true;   // destroy when hitting a tank
    public int pierceTanks = 1;        // how many tanks it can damage before destruction (-1 = infinite)
    public GameObject hitEffectPrefab;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    // This uses trigger events — ensure the Collider2D on the prefab is set to "Is Trigger"
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // Hit a tank?
        if (other.gameObject.CompareTag("Player1") || other.gameObject.CompareTag("Player2"))
        {
            Health h = other.gameObject.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(1, ownerId);
            }

            // spawn small hit effect at impact
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            if (pierceTanks > 0)
            {
                pierceTanks--;
            }

            if (destroyOnHit || pierceTanks == 0)
            {
                Destroy(gameObject);
            }
            return;
        }

        // If it hits other triggers (powerups, pickups) ignore.
        // Note: walls should normally have non-trigger colliders, so triggers pass through them.
    }
}

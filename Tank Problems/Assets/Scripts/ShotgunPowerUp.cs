using UnityEngine;

/// <summary>
/// Shotgun power-up pickup. When a tank touches this (trigger), it grants a temporary shotgun
/// to the tank's TankController and then destroys itself. Optionally plays a pickup effect and sound.
/// </summary>
public class ShotgunPowerUp : MonoBehaviour
{
    [Header("Shotgun settings")]
    public int pellets = 3;
    public float spreadAngle = 20f;
    public float duration = 6f;

    [Header("Optional feedback")]
    public GameObject pickupEffectPrefab;   // particle or visual to spawn on pickup
    public AudioClip pickupSound;           // sound to play on pickup
    [Range(0f, 1f)]
    public float pickupSoundVolume = 0.8f;

    // If you want this pickup to respawn after some time instead of being destroyed,
    // you can add respawn logic here later.

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // Try to find a TankController on the collider or its parent
        TankController tc = other.GetComponent<TankController>();
        if (tc == null)
            tc = other.GetComponentInParent<TankController>();

        if (tc != null)
        {
            // Apply the shotgun power-up
            tc.ApplyShotgun(pellets, spreadAngle, duration);

            // Spawn optional pickup effect
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Play optional sound (uses world-space position)
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
            }

            // Destroy the pickup object
            Destroy(gameObject);
        }
    }

    // Optional: draw gizmo in editor to see pickup position
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
#endif
}

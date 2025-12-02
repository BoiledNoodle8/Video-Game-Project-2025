using UnityEngine;

/// <summary>
/// Grants shield charges to any tank that touches this pickup.
/// If the tank does not already have a Shield component, one will be added automatically.
/// </summary>
public class ShieldPowerUp : MonoBehaviour
{
    public int chargesGranted = 1;

    [Header("Optional feedback")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.9f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // Find the Health or TankController to identify the tank root
        Health health = other.GetComponent<Health>();
        if (health == null)
            health = other.GetComponentInParent<Health>();

        if (health != null)
        {
            GameObject tankRoot = health.gameObject;

            Shield shield = tankRoot.GetComponent<Shield>();
            if (shield == null)
            {
                shield = tankRoot.AddComponent<Shield>();
                // Note: you can then set shield.visual prefab in inspector on the tank prefab,
                // or assign shieldVisualPrefab programmatically here if you want.
            }

            shield.AddCharges(chargesGranted);

            if (pickupEffectPrefab != null)
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);

            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
#endif
}

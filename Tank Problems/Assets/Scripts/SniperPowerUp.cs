using UnityEngine;

public class SniperPowerUp : MonoBehaviour
{
    [Header("Single-shot settings")]
    public int sniperCharges = 1; // how many single shots this pickup grants

    [Header("Optional feedback")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        TankController tc = other.GetComponent<TankController>();
        if (tc == null)
            tc = other.GetComponentInParent<TankController>();

        if (tc != null)
        {
            // Grant single-shot sniper charges (consumed on next shot)
            tc.ApplySniperSingleShot(sniperCharges);

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
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
    }
#endif
}

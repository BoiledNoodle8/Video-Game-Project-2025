using UnityEngine;

/// <summary>
/// Simple shield component that can absorb a number of incoming hits.
/// Keeps an optional visual prefab enabled while charges > 0.
/// </summary>
public class Shield : MonoBehaviour
{
    [Header("Shield settings")]
    public int charges = 0;

    [Header("Visuals (optional)")]
    public GameObject shieldVisualPrefab; // optional: a child prefab (e.g. ring) to show when shield active
    GameObject visualInstance;

    [Header("Feedback")]
    public GameObject blockEffectPrefab;  // optional VFX when shield blocks a shot
    public AudioClip blockSound;
    [Range(0f, 1f)] public float blockSoundVolume = 0.9f;

    void Start()
    {
        // If a prefab is assigned, instantiate as a child and keep it off when no charges
        if (shieldVisualPrefab != null && visualInstance == null)
        {
            visualInstance = Instantiate(shieldVisualPrefab, transform);
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
            visualInstance.SetActive(charges > 0);
        }
    }

    /// <summary>
    /// Add shield charges (call when pickup is collected)
    /// </summary>
    public void AddCharges(int amount)
    {
        charges += Mathf.Max(0, amount);
        UpdateVisual();
    }

    /// <summary>
    /// Try to absorb a single incoming hit. Returns true if the shield consumed the hit.
    /// </summary>
    public bool TryAbsorb()
    {
        if (charges <= 0) return false;

        charges--;
        // play block effect/sound
        if (blockEffectPrefab != null)
        {
            Instantiate(blockEffectPrefab, transform.position, Quaternion.identity);
        }
        if (blockSound != null)
        {
            AudioSource.PlayClipAtPoint(blockSound, transform.position, blockSoundVolume);
        }

        UpdateVisual();
        return true;
    }

    void UpdateVisual()
    {
        if (visualInstance != null)
            visualInstance.SetActive(charges > 0);
    }
}


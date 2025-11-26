using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    private int hp;


public GameManager gameManager;
    public int playerId;
    public GameObject deathEffectPrefab;

    void Awake()
    {
        hp = maxHP;
    }

    public void TakeDamage(int amount, int attackerId)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Die(attackerId);
        }
    }

    void Die(int attackerId)
    {
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        if (gameManager != null)
            gameManager.OnTankDestroyed(playerId, attackerId);

        // Disable root object
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        hp = maxHP;

        // Activate root first
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Enable all SpriteRenderers on self and children
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in renderers)
        {
            sr.enabled = true;
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        // Enable Animator if present
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = true;

        // Reset physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset transform scale (in case it was zero)
        if (transform.localScale == Vector3.zero)
            transform.localScale = Vector3.one;

        Debug.Log("[Health] Respawned playerId=" + playerId + " at " + transform.position);
    }


}

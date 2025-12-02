using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    int hp;
    public GameManager gameManager;
    public int playerId;

    // Optional: death effect
    public GameObject deathEffectPrefab;
    public float deathEffectLifetime = 3f;

    void Awake()
    {
        hp = maxHP;
    }

    /// <summary>
    /// amount = damage amount, attackerId = playerId of who fired the bullet
    /// </summary>
    public void TakeDamage(int amount, int attackerId)
    {
        // Check for a shield component on this GameObject
        Shield shield = GetComponent<Shield>();
        if (shield == null)
            shield = GetComponentInChildren<Shield>(); // in case shield is on a child

        if (shield != null)
        {
            // Try absorb one hit per call (you can tune to reduce amount to <=1)
            bool absorbed = shield.TryAbsorb();
            if (absorbed)
            {
                // Shield blocked the incoming damage; do not reduce HP.
                Debug.Log($"[Health] Player {playerId} shield absorbed damage from player {attackerId}");
                return;
            }
        }

        // No shield / shield didn't absorb — apply damage
        hp -= amount;
        if (hp <= 0)
        {
            Die(attackerId);
        }
    }

    void Die(int attackerId)
    {
        // Spawn death effect only on real tank death
        if (deathEffectPrefab != null)
        {
            GameObject fx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.parent = null;
            Destroy(fx, deathEffectLifetime);
        }

        // Notify GameManager for scoring / respawn
        if (gameManager != null)
            gameManager.OnTankDestroyed(playerId, attackerId);

        // disable tank visuals/logic until respawn
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        hp = maxHP;
        gameObject.SetActive(true);
    }
}

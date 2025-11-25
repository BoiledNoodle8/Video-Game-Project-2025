using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    int hp;

    public GameManager gameManager;
    public int playerId;

    // ? NEW: Drop your hit/death effect here in the Inspector
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
        // Spawn the death effect ONLY when tank actually dies
        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        // Send score info to GameManager
        if (gameManager != null)
            gameManager.OnTankDestroyed(playerId, attackerId);

        // Disable tank until respawn
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        hp = maxHP;
        gameObject.SetActive(true);
    }
}

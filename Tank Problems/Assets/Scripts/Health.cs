using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    int hp;
    public GameManager gameManager;
    public int playerId;

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
        // Tell GameManager for scoring/respawn
        if (gameManager != null)
            gameManager.OnTankDestroyed(playerId, attackerId);

        // Optionally disable visuals until respawn; GameManager will handle respawn
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        hp = maxHP;
        gameObject.SetActive(true);
    }
}
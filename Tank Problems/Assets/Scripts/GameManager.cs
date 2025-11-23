using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Tanks")]
    public List<Health> tanks = new List<Health>();
    public float respawnDelay = 2f;

    [Header("Score Settings")]
    public int scoreToWin = 5;
    public Text player1ScoreText;
    public Text player2ScoreText;

    private Dictionary<int, int> playerScores = new Dictionary<int, int>();

    void Start()
    {
        // Initialize scores
        foreach (var t in tanks)
        {
            playerScores[t.playerId] = 0;
            t.gameManager = this; // assign GameManager to each tank automatically
        }

        UpdateScoreUI();
    }

    public void OnTankDestroyed(int destroyedPlayerId, int attackerPlayerId)
    {
        Debug.Log($"Player {destroyedPlayerId} destroyed by {attackerPlayerId}");

        // Increment score
        if (attackerPlayerId != destroyedPlayerId && playerScores.ContainsKey(attackerPlayerId))
        {
            playerScores[attackerPlayerId]++;
            UpdateScoreUI();

            // Check for win
            if (playerScores[attackerPlayerId] >= scoreToWin)
            {
                Debug.Log($"Player {attackerPlayerId} wins!");
                // Optional: show a UI message or load a win scene
            }
        }

        // Respawn tank
        Health destroyedTank = tanks.Find(t => t.playerId == destroyedPlayerId);
        if (destroyedTank != null)
            StartCoroutine(RespawnTank(destroyedTank, respawnDelay));
    }

    IEnumerator RespawnTank(Health tank, float delay)
    {
        yield return new WaitForSeconds(delay);
        tank.Respawn();
        tank.transform.position = GetSpawnPosition(tank.playerId);
        tank.transform.rotation = Quaternion.identity;

        Rigidbody2D rb = tank.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    Vector3 GetSpawnPosition(int playerId)
    {
        switch (playerId)
        {
            case 1: return new Vector3(-4, -4, 0);
            case 2: return new Vector3(4, 4, 0);
            default: return Vector3.zero;
        }
    }

    void UpdateScoreUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = "Player 1: " + playerScores[1];
        if (player2ScoreText != null)
            player2ScoreText.text = "Player 2: " + playerScores[2];
    }
}
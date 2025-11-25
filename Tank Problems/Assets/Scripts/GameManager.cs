using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Tanks & UI (assign in Inspector)")]
    public List<Health> tanks = new List<Health>();
    public Text player1ScoreText;
    public Text player2ScoreText;

    [Header("Win settings")]
    public int winningScore = 5;
    public GameObject winCanvas;   // The whole win UI Canvas (disabled at start)
    public Text winText;           // "Player X Wins!"
    public GameObject gameplay;    // Parent object that contains all gameplay objects

    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private bool gameOver = false;

    void Start()
    {
        // Initialize scores and give tanks a reference to this GameManager
        playerScores.Clear();
        foreach (var t in tanks)
        {
            if (t == null) continue;
            playerScores[t.playerId] = 0;
            t.gameManager = this;
        }

        // Ensure win UI is hidden at start
        if (winCanvas != null) winCanvas.SetActive(false);

        UpdateScoreUI();
    }

    /// <summary>
    /// Called by Health when a tank dies.
    /// </summary>
    public void OnTankDestroyed(int destroyedPlayerId, int attackerPlayerId)
    {
        if (gameOver) return;

        // Award points to attacker if not a self-kill
        if (attackerPlayerId != destroyedPlayerId)
        {
            if (playerScores.ContainsKey(attackerPlayerId))
                playerScores[attackerPlayerId]++;
        }

        UpdateScoreUI();

        // Check for win
        if (playerScores.ContainsKey(1) && playerScores[1] >= winningScore)
        {
            ShowWinScreen(1);
            return;
        }
        if (playerScores.ContainsKey(2) && playerScores[2] >= winningScore)
        {
            ShowWinScreen(2);
            return;
        }

        // If game still running, respawn the destroyed tank
        StartCoroutine(RespawnTankCoroutine(destroyedPlayerId, 2f));
    }

    void UpdateScoreUI()
    {
        if (player1ScoreText != null && playerScores.ContainsKey(1))
            player1ScoreText.text = playerScores[1].ToString();
        if (player2ScoreText != null && playerScores.ContainsKey(2))
            player2ScoreText.text = playerScores[2].ToString();
    }

    IEnumerator RespawnTankCoroutine(int playerId, float delay)
    {
        yield return new WaitForSeconds(delay);
        // find the Health component for this player and call Respawn
        Health h = tanks.Find(t => t != null && t.playerId == playerId);
        if (h != null)
        {
            h.Respawn();
            // Optional: reset position & rotation if you have a spawn system
            h.transform.position = GetSpawnPosition(playerId);
            h.transform.rotation = Quaternion.identity;
            Rigidbody2D rb = h.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }

    Vector3 GetSpawnPosition(int playerId)
    {
        // Quick default spawn points — change to your own spawn logic
        if (playerId == 1) return new Vector3(-4f, -4f, 0f);
        if (playerId == 2) return new Vector3(4f, 4f, 0f);
        return Vector3.zero;
    }

    void ShowWinScreen(int winningPlayerId)
    {
        gameOver = true;

        // Stop gameplay objects so nothing moves/interacts
        if (gameplay != null) gameplay.SetActive(false);

        // Show win UI
        if (winCanvas != null) winCanvas.SetActive(true);
        if (winText != null) winText.text = "Player " + winningPlayerId + " Wins!";
    }

    // UI button: restart current scene
    public void RestartGame()
    {
        // Optional: enable gameplay again immediately on restart
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // UI button: go back to main menu (replace with your menu scene name)
    public void BackToMenu(string menuSceneName = "MainMenu")
    {
        SceneManager.LoadScene(menuSceneName);
    }
}

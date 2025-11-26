using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Tank References (Assign in Inspector)")]
    public List<Health> tanks;

    [Header("UI")]
    public Text scoreTextP1;
    public Text scoreTextP2;

    [Header("Win Screen UI")]
    public GameObject winCanvas;
    public Text winText;

    [Header("Gameplay Settings")]
    public int scoreToWin = 5;
    public float respawnDelay = 1.5f;

    [Header("Spawn Settings")]
    public List<Transform> spawnPoints = new List<Transform>();
    public LayerMask spawnBlockMask;
    public float spawnCheckRadius = 0.6f;
    public int maxSpawnAttempts = 12;

    // The Z-depth to force on respawn so visuals render correctly
    public float respawnZ = 631f;

    private int scoreP1 = 0;
    private int scoreP2 = 0;

    void Start()
    {
        UpdateScoreUI();
        if (winCanvas != null)
            winCanvas.SetActive(false);
    }

    // Called by Health when a tank dies
    public void OnTankDestroyed(int deadPlayerId, int attackerId)
    {
        Debug.Log("OnTankDestroyed: Dead=" + deadPlayerId + ", Attacker=" + attackerId);

        if (attackerId == 1) scoreP1++;
        if (attackerId == 2) scoreP2++;

        UpdateScoreUI();
        CheckWinCondition();

        StartCoroutine(RespawnTankCoroutine(deadPlayerId, respawnDelay));
    }

    void UpdateScoreUI()
    {
        if (scoreTextP1 != null) scoreTextP1.text = "Player 1: " + scoreP1;
        if (scoreTextP2 != null) scoreTextP2.text = "Player 2: " + scoreP2;
    }

    void CheckWinCondition()
    {
        if (scoreP1 >= scoreToWin) ShowWinScreen(1);
        else if (scoreP2 >= scoreToWin) ShowWinScreen(2);
    }

    void ShowWinScreen(int winningPlayerId)
    {
        Time.timeScale = 0f;
        if (winCanvas != null) winCanvas.SetActive(true);
        if (winText != null) winText.text = "Player " + winningPlayerId + " Wins!";
        Debug.Log("ShowWinScreen: Player " + winningPlayerId + " Wins!");
    }

    IEnumerator RespawnTankCoroutine(int playerId, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Find the Health object for this playerId (safe loop to avoid LINQ/lambda issues)
        Health tank = null;
        if (tanks != null)
        {
            for (int i = 0; i < tanks.Count; i++)
            {
                if (tanks[i] != null && tanks[i].playerId == playerId)
                {
                    tank = tanks[i];
                    break;
                }
            }
        }

        if (tank == null)
        {
            Debug.LogWarning("Respawn failed — tank not found for playerId: " + playerId);
            yield break;
        }

        Vector3 spawnPos = Vector3.zero;
        bool foundSpot = false;

        // Try random spawn points and ensure no overlap with spawnBlockMask
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            if (spawnPoints == null || spawnPoints.Count == 0) break;

            Transform candidate = spawnPoints[Random.Range(0, spawnPoints.Count)];
            if (candidate == null) continue;

            Vector3 candidatePos = candidate.position;

            // Check 2D overlap; adjust Z later
            Collider2D[] hits = Physics2D.OverlapCircleAll(candidatePos, spawnCheckRadius, spawnBlockMask);
            if (hits == null || hits.Length == 0)
            {
                spawnPos = candidatePos;
                foundSpot = true;
                break;
            }
        }

        // Fallback if nothing found
        if (!foundSpot)
        {
            spawnPos = GetFallbackSpawn(playerId);
            Debug.LogWarning("No valid random spawn — using fallback for player " + playerId);
        }

        // Force correct Z depth for visuals
        spawnPos.z = respawnZ;

        // Place tank at spawn position before enabling visuals/respawn logic
        tank.transform.position = spawnPos;
        tank.transform.rotation = Quaternion.identity;

        // Reset physics to avoid carryover motion
        Rigidbody2D rb = tank.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Ensure root object is active before calling Respawn
        if (!tank.gameObject.activeSelf)
            tank.gameObject.SetActive(true);

        // Call Respawn on Health to reset HP and re-enable visuals (Health.Respawn should re-enable renderers/animators)
        tank.Respawn();

        Debug.Log("[GameManager] Respawned Player " + playerId + " at " + spawnPos);
    }

    Vector3 GetFallbackSpawn(int playerId)
    {
        // keep fallback Z at respawnZ
        Vector3 fallback = Vector3.zero;
        if (playerId == 1) fallback = new Vector3(-4f, -4f, respawnZ);
        else if (playerId == 2) fallback = new Vector3(4f, 4f, respawnZ);
        else fallback = new Vector3(0f, 0f, respawnZ);

        return fallback;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    // Optional: draw spawn-check radius gizmos in editor for convenience
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.yellow;
        foreach (var sp in spawnPoints)
        {
            if (sp == null) continue;
            Vector3 p = sp.position;
            p.z = respawnZ;
            Gizmos.DrawWireSphere(p, spawnCheckRadius);
        }
    }
#endif
}

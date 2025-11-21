using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TankController player1;
    public TankController player2;
    public Health p1Health;
    public Health p2Health;

    public Transform[] spawnPoints; // list of spawn transforms
    public float respawnDelay = 2f;

    public Text scoreTextP1;
    public Text scoreTextP2;

    int scoreP1 = 0;
    int scoreP2 = 0;

    void Start()
    {
        UpdateScores();
    }

    public void OnTankDestroyed(int victimId, int attackerId)
    {
        if (attackerId == 1) scoreP1++;
        if (attackerId == 2) scoreP2++;
        UpdateScores();

        // Start respawn coroutine for victim
        if (victimId == 1)
            StartCoroutine(RespawnPlayer(player1.gameObject, p1Health, 1));
        else if (victimId == 2)
            StartCoroutine(RespawnPlayer(player2.gameObject, p2Health, 2));
    }

    IEnumerator RespawnPlayer(GameObject tankObj, Health health, int playerId)
    {
        // find a spawn point (simple round-robin or random)
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        yield return new WaitForSeconds(respawnDelay);

        // Set position and reactivate
        tankObj.transform.position = spawn.position;
        tankObj.transform.rotation = Quaternion.identity;
        health.playerId = playerId;
        health.Respawn();

        TankController tc = tankObj.GetComponent<TankController>();
        if (tc != null)
        {
            tc.ResetForRespawn(spawn.position, 0f);
        }
    }

    void UpdateScores()
    {
        if (scoreTextP1) scoreTextP1.text = scoreP1.ToString();
        if (scoreTextP2) scoreTextP2.text = scoreP2.ToString();
    }
}
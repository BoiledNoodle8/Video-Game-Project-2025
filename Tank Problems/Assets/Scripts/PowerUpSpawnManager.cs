using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns random powerup prefabs at random spawn points over time.
/// Designed to be future-proof: drop more powerup prefabs into the list.
/// </summary>
public class PowerUpSpawnManager : MonoBehaviour
{
    [Header("Spawn points (assign transforms in scene)")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Powerup prefabs (prefab list to choose from)")]
    public List<GameObject> powerupPrefabs = new List<GameObject>();

    [Header("Timing")]
    public float minSpawnInterval = 8f;
    public float maxSpawnInterval = 18f;
    public int maxConcurrentPickups = 2; // how many powerups can exist at once

    [Header("Spawn safety")]
    public LayerMask spawnBlockMask;      // layers that block spawning (e.g. Walls, Tanks)
    public float spawnCheckRadius = 0.5f; // radius used to check overlaps before spawning
    public int maxSpawnAttempts = 12;     // how many random tries before giving up this cycle

    [Header("Behavior")]
    public bool useUnscaledTime = true;   // spawn while game is paused? (usually keep true if you want spawns during pause)
    public bool startOnAwake = true;

    // bookkeeping
    private HashSet<GameObject> activePickups = new HashSet<GameObject>();
    private Coroutine spawnRoutine;

    void Awake()
    {
        if (startOnAwake)
            StartSpawning();
    }

    /// <summary>
    /// Start the spawn loop (call from menu manager or inspector).
    /// </summary>
    public void StartSpawning()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Stop the spawn loop.
    /// </summary>
    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // wait a random interval
            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(wait);
            else
                yield return new WaitForSeconds(wait);

            // don't spawn if we already have many pickups
            if (activePickups.Count >= maxConcurrentPickups)
                continue;

            // pick a random powerup prefab
            if (powerupPrefabs == null || powerupPrefabs.Count == 0)
                continue;

            GameObject prefab = powerupPrefabs[Random.Range(0, powerupPrefabs.Count)];
            if (prefab == null) continue;

            // try to find a valid spawn point
            Vector3 spawnPos = Vector3.zero;
            bool found = false;

            if (spawnPoints != null && spawnPoints.Count > 0)
            {
                for (int i = 0; i < maxSpawnAttempts; i++)
                {
                    Transform cand = spawnPoints[Random.Range(0, spawnPoints.Count)];
                    if (cand == null) continue;

                    Vector3 pos2d = cand.position;
                    // check overlap in 2D
                    Collider2D[] hits = Physics2D.OverlapCircleAll(pos2d, spawnCheckRadius, spawnBlockMask);
                    if (hits == null || hits.Length == 0)
                    {
                        spawnPos = pos2d;
                        found = true;
                        break;
                    }
                }
            }

            // if none found, skip this spawn (keeps logic safe)
            if (!found)
            {
                Debug.LogWarning("[PowerUpSpawnManager] Could not find a free spawn point this cycle.");
                continue;
            }

            // instantiate and register
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            activePickups.Add(instance);

            // auto-cleanup callback: if prefab destroyed by pickup, remove from active set
            // We'll try to detect when the instance is destroyed using a helper component:
            PickupTracker tracker = instance.GetComponent<PickupTracker>();
            if (tracker == null)
                tracker = instance.AddComponent<PickupTracker>();
            tracker.manager = this;
        }
    }

    /// <summary>
    /// Called by PickupTracker when a pickup is removed/destroyed so we know we can spawn more.
    /// </summary>
    public void NotifyPickupRemoved(GameObject pickup)
    {
        if (activePickups.Contains(pickup))
            activePickups.Remove(pickup);
    }

    // Draw simple gizmos for spawn points
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var sp in spawnPoints)
        {
            if (sp == null) continue;
            Gizmos.DrawWireSphere(sp.position, spawnCheckRadius);
        }
    }
#endif
}

/// <summary>
/// Small helper attached to spawned pickup instances so they notify the manager when destroyed.
/// You can also call manager.NotifyPickupRemoved(...) manually from your pickup logic.
/// </summary>
public class PickupTracker : MonoBehaviour
{
    [HideInInspector] public PowerUpSpawnManager manager;

    void OnDestroy()
    {
        if (manager != null)
            manager.NotifyPickupRemoved(this.gameObject);
    }
}

using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager I;

    private Spawnpoint[] spawnpoints;

    private void Awake()
    {
        I = this;
        spawnpoints = GetComponentsInChildren<Spawnpoint>();
    }

    public Transform GetSpawnpoint()
    {
        return spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
    }
}
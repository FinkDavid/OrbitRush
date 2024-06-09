using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] public GameObject[] asteroidPrefabs;
    [SerializeField] public float count = 100;
    [SerializeField] public int worldSize = 10000;
    [SerializeField] public int offset = 100;

    // Start is called before the first frame update
    void Start()
    {
        // Spawn asteroids on random location within the world size
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize));
            GameObject asteroid = Instantiate(asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)], spawnPosition, Quaternion.identity);
            asteroid.transform.parent = transform;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            Vector3 spawnPosition = new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize));
            GameObject asteroid = Instantiate(asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)], spawnPosition, Quaternion.identity);
            asteroid.transform.parent = transform;
        }
    }
}
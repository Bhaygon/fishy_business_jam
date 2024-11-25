using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    public float timeBetweenSpawns;
    float timer;
    public float distance;
    public int enemiesCount;

    private void Start()
    {
        timer = timeBetweenSpawns;
    }

    private void Update()
    {
        if (enemiesCount <= 0)
        {
            gameObject.SetActive(false);
            return;
        }
        
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = timeBetweenSpawns;
            StartCoroutine(Spawn());
        }
    }

    private IEnumerator Spawn()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemiesCount <= 0) yield return null;
            Instantiate(enemy, transform.position + new Vector3(Random.Range(-distance, distance), Random.Range(-distance, distance)), Quaternion.identity);
            enemiesCount -= 1;
            yield return new WaitForSeconds(0.5f);
        }
    }
}

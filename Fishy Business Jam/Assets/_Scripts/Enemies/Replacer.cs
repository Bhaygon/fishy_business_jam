using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Replacer : MonoBehaviour
{
    public GameObject EnemyPrefab;
    private GameObject _enemy;
    public float timeBetweenSpawns;
    float timer;

    private void Start()
    {
        timer = timeBetweenSpawns;
        _enemy = Instantiate(EnemyPrefab, transform.position, quaternion.identity);
    }

    private void Update()
    {
        if (_enemy.gameObject.activeSelf == false)
        {
            if (timer < 0)
            {
                timer = timeBetweenSpawns;
                _enemy = Instantiate(EnemyPrefab, transform.position, quaternion.identity);
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
    }
}

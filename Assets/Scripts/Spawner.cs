using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;

    private void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        Enemy enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.OnDeath.AddListener(SpawnEnemy);
    }
}

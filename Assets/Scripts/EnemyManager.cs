using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyType
{
    OneHand, 
    TwoHand, 
    Archer
}

public enum PatrolType
{
    Linear,
    Random,
    Loop
}

public class EnemyManager : Singleton<EnemyManager>
{

    public Transform[] spawnPoints;         //The spawn point for our enemies to spawn at
    public GameObject[] enemyTypes;         //Contains all the different enemy types in our game
    public List<GameObject> enemies;        //A list containing all the enemies in our scene
    public string[] enemyNames;

    public int spawnCount = 10;
    public string killCondition = "Two";
    public float SpawnDelay = 2f;


    private void Start()
    {
        StartCoroutine(SpawnDelayed());
        ShuffleList(enemies);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            SpawnEnemy();
        }
        if(Input.GetKeyDown(KeyCode.K))
        {
            KillAllEnemies();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            KillSpecificEnemy(killCondition);
        }
    }

    /// <summary>
    /// Spawns an enemu with a delay until enemy count is reached
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnDelayed()
    {
        yield return new WaitForSeconds(SpawnDelay);
        if (_GM.gameState == GameState.Playing)
        {
            SpawnEnemy();
        }

        if (enemies.Count <= spawnCount)
        {
            StartCoroutine(SpawnDelayed());
        }
    }

    //<summary>
    //Spawns a random enemy at a random spawn
    //</summary>

    void SpawnEnemy()
    {
        int enemyNumber = Random.Range(0, enemyTypes.Length);
        int spawnPoint = Random.Range(0, spawnPoints.Length);
        GameObject enemy = Instantiate(enemyTypes[enemyNumber], spawnPoints[spawnPoint].position, spawnPoints[spawnPoint].rotation, transform);
        enemies.Add(enemy);
    }

    //This wi;; spawn an enemy at each spawn point suquentally
    
    void SpawnEnemies()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject enemy = Instantiate(enemyTypes[Random.Range(0, enemyTypes.Length)], spawnPoints[i].position, spawnPoints[i].rotation, transform);
            enemies.Add(enemy);
        }
    }

    /// <summary>
    /// Kills a specific enemy in our game
    /// </summary>
    /// <param name="_enemy"></param>
    public void KillEnemy(GameObject _enemy)
    {
        if(enemies.Count == 0)
            return;

        Destroy(_enemy);
        enemies.Remove(_enemy);
    }

    /// <summary>
    /// Kills an enemy of the specific condition
    /// </summary>
    /// <param name="_condition">The condition of the enemy we want to kill</param>


    void KillSpecificEnemy(string _condition)
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].name.Contains(_condition))
                KillEnemy(enemies[i]);
        }
    }

    void KillAllEnemies()
    {
        if (enemies.Count == 0)
            return;

        for(int i = 0; i < enemies.Count; i++)
        {
            Destroy(enemies[i]);
        }
        enemies.Clear();
    }


    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDie += KillEnemy;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDie -= KillEnemy;
    }

}

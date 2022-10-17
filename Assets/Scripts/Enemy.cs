using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType myType;
    
    public float myHealth = 100f;
    public float mySpeed = 2f;
    
    EnemyManager _EM;

    [Header("AI")]
    public PatrolType myPatrol;
    int patrolPoint = 0;        //Needed for linear patrol movement
    bool reverse = false;       //Needed for repeat patrol movement
    Transform startPos;         //Needed for repeat patrol movement
    Transform endPos;           //Needed for repeat patrol movement
    Transform moveToPos;

    // Start is called before the first frame update
    void Start()
    {
        _EM = FindObjectOfType<EnemyManager>();
        Setup();
        SetupAI();
        StartCoroutine(Move());
    }

    void Setup()
    {
        switch(myType)
        {
            case EnemyType.OneHand:
                myHealth = 100f;
                mySpeed = 2f;
                myPatrol = PatrolType.Linear;
                break;

            case EnemyType.TwoHand:
                myHealth = 200f;
                mySpeed = 1f;
                myPatrol = PatrolType.Loop;
                break;

            case EnemyType.Archer:
                myHealth = 60f;
                mySpeed = 5f;
                myPatrol = PatrolType.Random;
                break;
        }
    }

    void SetupAI()
    {
        startPos = Instantiate(new GameObject(), transform.position, transform.rotation).transform;
        endPos = _EM.GetRandomSpawnPoint();
        moveToPos = endPos;
    }

    IEnumerator Move()
    {
        switch(myPatrol)
        {
            case PatrolType.Linear:
                moveToPos = _EM.spawnPoints[patrolPoint];
                patrolPoint = patrolPoint != _EM.spawnPoints.Length ? patrolPoint + 1 : 0;
                break;

            case PatrolType.Random:
                moveToPos = _EM.GetRandomSpawnPoint();
                break;
            
            case PatrolType.Loop:
                moveToPos = reverse ? startPos : endPos;
                reverse = !reverse;
                break;
        }

        transform.LookAt(moveToPos);
        while (Vector3.Distance(transform.position, moveToPos.position) > 0.3f)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveToPos.position, Time.deltaTime * mySpeed);
            yield return null;
        }
        
        yield return new WaitForSeconds(1);

        StartCoroutine(Move());
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : GameBehaviour
{
    Animator anim;
    
    public EnemyType myType;
    public float mySpeed;
    public float myHealth;
    public float myMaxHealth;
    public float attackRadius = 5;

    [Header("AI")]
    public PatrolType myPatrol;
    //public int patrolPoint = 0;            //Needed for linear patrol movement
    //public bool reverse = false;           //Needed for repeat patrol movement
    //public Transform startPos;             //Needed for repeat patrol movement
    //public Transform endPos;               //Needed for repeat patrol movement
    //public Transform moveToPos;
    NavMeshAgent agent;
    int currentWaypoint;
    float detectDistance = 10f;
    float detectTime = 5f;
    float attackDistance = 2f;

    [Header("Health Bar")]
    public Slider healthBarSlider;
    public TMP_Text healthBarText;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        Setup();
        SetupAI();
        //StartCoroutine(Move());
        transform.SetPositionAndRotation(transform.position, transform.rotation);
    }

    void Setup()
    {
        float healthModifier = 1;
        float speedModifier = 1;
        switch(_GM.difficulty)
        {
            case Difficulty.Easy:
                healthModifier = 1f;
                speedModifier = 1f;
                break;
            case Difficulty.Medium:
                healthModifier = 2f;
                speedModifier = 1.2f;
                break;
            case Difficulty.Hard:
                healthModifier = 3f;
                speedModifier = 1.5f;
                break;
            default:
                healthModifier = 1f;
                speedModifier = 1f;
                break;
        }

        switch(myType)
        {
            case EnemyType.OneHand:
                myHealth = 100f * healthModifier;
                mySpeed = 2f * speedModifier;
                myPatrol = PatrolType.Patrol;
                break;
            case EnemyType.TwoHand:
                myHealth = 200f * healthModifier;
                mySpeed = 1f * speedModifier;
                myPatrol = PatrolType.Patrol;
                break;
            case EnemyType.Archer:
                myHealth = 60f * healthModifier;
                mySpeed = 5f * speedModifier;
                myPatrol = PatrolType.Patrol;
                break;
        }
        myMaxHealth = myHealth;
        healthBarSlider.maxValue = myHealth;
        
    }

    void SetupAI()
    {
        //startPos = Instantiate(new GameObject(), transform.position, transform.rotation).transform;
        //endPos = _EM.GetRandomSpawnPoint();
        //moveToPos = endPos;

        currentWaypoint = UnityEngine.Random.Range(0, _EM.spawnPoints.Length);
        agent.SetDestination(_EM.spawnPoints[currentWaypoint].position);
        ChangeSpeed(mySpeed);
    }

    void ChangeSpeed(float _speed)
    {
        agent.speed = _speed;
    }

    /*
    IEnumerator Move()
    {
        switch(myPatrol)
        {
            case PatrolType.Patrol:
                moveToPos = _EM.spawnPoints[patrolPoint];
                patrolPoint = patrolPoint != _EM.spawnPoints.Length ? patrolPoint + 1 : 0;
                break;
            case PatrolType.Patrol:
                moveToPos = _EM.GetRandomSpawnPoint();
                break;
            case PatrolType.Patrol:
                moveToPos = reverse ? startPos : endPos;
                reverse = !reverse;
                break;
        }

        transform.LookAt(moveToPos);

        while (Vector3.Distance(transform.position, moveToPos.position) > 0.3f)
        {
            if(Vector3.Distance(_P.transform.position, transform.position) < attackRadius)
            {
                StopAllCoroutines();
                StartCoroutine(Attack());
                yield break;
            }
            transform.position = Vector3.MoveTowards(transform.position, moveToPos.position, Time.deltaTime * mySpeed);
            yield return null;
        }

        yield return new WaitForSeconds(1);

        StartCoroutine(Move());
    }
    */

    IEnumerator Attack()
    {
        myPatrol = PatrolType.Attack;
        ChangeSpeed(0);
        PlayAnimation("Attack");
        yield return new WaitForSeconds(1.5f);
        ChangeSpeed(mySpeed);
        myPatrol = PatrolType.Chase;
    }

    private void Update()
    {
        //get the distance between the player & agent
        float distToPlayer = Vector3.Distance(transform.position, _P.transform.position);

        if(distToPlayer <= detectDistance && myPatrol != PatrolType.Attack)
        {
            if (myPatrol != PatrolType.Chase)
                myPatrol = PatrolType.Detect;
        }

        anim.SetFloat("Speed", mySpeed);

        switch(myPatrol)
        {
            case PatrolType.Patrol:
                //get the distance between agent and current waypoint
                float disToWaypoint = Vector3.Distance(transform.position, _EM.spawnPoints[currentWaypoint].position);
                //if the distance is close enough, get a new waypoint
                if (disToWaypoint < 1)
                    SetupAI();
                break;

            case PatrolType.Detect:
                agent.SetDestination(transform.position);   //Set the destination to ourself, stopping us
                ChangeSpeed(0);                             //Stops our movement speed
                detectTime -= Time.deltaTime;               //Decrement our detect time
                if(distToPlayer <= detectDistance)
                {
                    myPatrol = PatrolType.Chase;
                    detectTime = 5;
                }
                if(detectTime <=0)
                {
                    myPatrol = PatrolType.Patrol;
                    SetupAI();
                }
                break;

            case PatrolType.Chase:
                agent.SetDestination(_P.transform.position);    //set desitination to the player
                ChangeSpeed(mySpeed * 2);                       //Increase the speed of which to chase yhe player
                if (distToPlayer > detectDistance)              //if the player outside detect range, go back to detect
                    myPatrol = PatrolType.Detect;
                if (distToPlayer <= attackDistance)
                    StartCoroutine(Attack());
                break;
        }
    }

    void UpdateHealthBar()
    {
        healthBarSlider.value = myHealth;
        healthBarText.text = myHealth + "/" + myMaxHealth;
    }

    public void Hit(int _damage)
    {
        myHealth -= _damage;
        healthBarSlider.value = myHealth;
        if (myHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayAnimation("Hit");
            GameEvents.ReportEnemyHit(this.gameObject);
        }
    }

    void Die()
    {
        GetComponent<Collider>().enabled = false;
        PlayAnimation("Die");
        ChangeSpeed(0);
        myPatrol = PatrolType.Die;
        StopAllCoroutines();
        GameEvents.ReportEnemyDie(this.gameObject);
    }

    void PlayAnimation(string _animation)
    {
        int randAnim = UnityEngine.Random.Range(1, 4);
        anim.SetTrigger(_animation + randAnim.ToString());
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : GameBehaviour
{
    public EnemyType myType;
    public float mySpeed;
    public float myHealth;
    public float myMaxHealth;

    [Header("AI")]
    public PatrolType myPatrol;
    public int patrolPoint = 0;            //Needed for linear patrol movement
    public bool reverse = false;           //Needed for repeat patrol movement
    public Transform startPos;             //Needed for repeat patrol movement
    public Transform endPos;               //Needed for repeat patrol movement
    public Transform moveToPos;

    [Header("Health Bar")]
    public Slider healthBarSlider;
    public TMP_Text healthBarText;

    void Start()
    {
        Setup();
        SetupAI();
        StartCoroutine(Move());
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
                myPatrol = PatrolType.Linear;
                break;
            case EnemyType.TwoHand:
                myHealth = 200f * healthModifier;
                mySpeed = 1f * speedModifier;
                myPatrol = PatrolType.Loop;
                break;
            case EnemyType.Archer:
                myHealth = 60f * healthModifier;
                mySpeed = 5f * speedModifier;
                myPatrol = PatrolType.Random;
                break;
        }
        myMaxHealth = myHealth;
        healthBarSlider.maxValue = myHealth;
        
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            Hit(10);
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
            GameEvents.ReportEnemyHit(this.gameObject);
        }
    }

    void Die()
    {
        StopAllCoroutines();
        GameEvents.ReportEnemyDie(this.gameObject);
    }
}

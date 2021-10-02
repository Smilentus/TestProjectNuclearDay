using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class EnemyController : MonoBehaviour
{
    SkeletonAnimation skeletonAnimation;
    Spine.AnimationState animationState;
    Spine.TrackEntry trackEntry;

    private Transform playerObject;
    private HealthModule healthModule;
    private Rigidbody2D rb;

    private AudioSource audioSource;
    public GameObject deathEffect;

    private float damage = 12.5f;
    private float moveSpeed = 4f;
    private bool canAttack = true;
    private bool isPlayerAround = false;

    /*
     Состояния бота:
        Стоим на месте
        Стоим на месте и атакуем игрока
        Следуем за игроком
        Следуем за игроком и атакуем его
        Умираем

    =====

    - Стоим на месте
    State - Idle

        Проигрываем анимацию Inactivity

    - Стоим на месте и атакуем игрока
    State - IdleAttacking

        Триггеримся тогда, когда подходит игрок в зону видимости
        Проигрываем анимацию удара, снимаем хп
        Повторяем если игрок в зоне видимости

    - Следуем за игроком
    State - Following

        Переключаемся в это состояние, когда здоровье игрока <= 50%
        Включаем анимацию ходьбы до позиции игрока

    - Следуем за игроком, затем атакуем как приближаемся к нему
    State - FollowingAndAttacking

        Переключаемся сюда как только бот догонит игрока 
        Проигрываем анимацию удара, снимаем хп
        Если игрок в зоне видимости - повторяем, иначе 

    - Умираем
    State - Died

        Триггерим, когда здоровье <= 0
        Включаем анимацию смерти

    =====

    Переключения между состояниями:

        Выключаем действия предыдущего состояния
        Добавляем в очередь следующее состояние
        Дожидаемся действия следующего состояния

        [!] Если очередь состояний пустая - возвращаемся к последнему состоянию, либо впадаем в Idle
    
     */

    private enum EnemyState { Idle, IdleAttacking, Following, FollowingAndAttacking, Died };

    private EnemyState currentEnemyState;
    private EnemyState lastEnemyState;
    private bool isFollowingPlayer = false;
    private bool canSwitchAIState = true;

    private Queue<EnemyState> enemyStates = new Queue<EnemyState>();

    private void Awake()
    {
        healthModule = GetComponent<HealthModule>();
        healthModule.OnHealthBelowZeroHandler += EnemyDeathAction;
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        playerObject = GameObject.FindGameObjectWithTag("Player").transform;

        skeletonAnimation = transform.GetChild(2).GetComponent<SkeletonAnimation>();
        animationState = skeletonAnimation.AnimationState;
        animationState.Complete += OnSpineAnimationComplete;

        trackEntry = animationState.SetAnimation(0, "inactivity", true);
        trackEntry.Complete += OnSpineAnimationComplete;

        AddAIState(EnemyState.Idle);
    }

    private void LateUpdate()
    {
        if (currentEnemyState != EnemyState.Died)
            LookAtPlayer();
    }

    private void FixedUpdate()
    {
        if (currentEnemyState == EnemyState.Following)
        {
            if (Vector3.Distance(rb.position, playerObject.position) > 1f)
                rb.MovePosition(rb.position + new Vector2(PlayerDirection(), 0) * moveSpeed * Time.fixedDeltaTime);
            else
                AddAIState(EnemyState.FollowingAndAttacking);
        }
    }

    private void AddAIState(EnemyState stateToChange)
    {
        if (currentEnemyState == EnemyState.Died)
            return;

        enemyStates.Enqueue(stateToChange);
        ChangeAIState();
    }

    private void ChangeAIState()
    {
        lastEnemyState = currentEnemyState;

        if (enemyStates.Count == 0)
            currentEnemyState = EnemyState.Idle;
        else
            currentEnemyState = enemyStates.Dequeue();

        audioSource.Stop();

        switch (currentEnemyState)
        {
            case EnemyState.Idle:
                trackEntry = animationState.SetAnimation(0, "inactivity", true);
                break;
            case EnemyState.IdleAttacking:
                StartCoroutine(AttackHandler());                
                break;
            case EnemyState.Following:
                audioSource.Play();
                trackEntry = animationState.SetAnimation(0, "walking", true);
                break;
            case EnemyState.FollowingAndAttacking:
                StartCoroutine(AttackHandler());
                break;
            case EnemyState.Died:
                audioSource.Stop();

                FindObjectOfType<OvenAction>().canActivateOven = true;

                deathEffect.SetActive(true);

                trackEntry = animationState.SetAnimation(0, "pick up", true);

                StartCoroutine(DeathAnimations());
                break;
            default:
                break;
        }
    }

    public void OnSpineAnimationComplete(TrackEntry trackEntry)
    {
        switch (currentEnemyState)
        {
            case EnemyState.IdleAttacking:
                if (isPlayerAround)
                    StartCoroutine(AttackHandler());
                else
                    AddAIState(EnemyState.Idle);
                break;

            case EnemyState.FollowingAndAttacking:
                if (isPlayerAround)
                    StartCoroutine(AttackHandler());
                else
                    AddAIState(EnemyState.Following);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerAround = true;
            
            if (currentEnemyState == EnemyState.Idle)
                AddAIState(EnemyState.IdleAttacking);

            if (currentEnemyState == EnemyState.Following)
                AddAIState(EnemyState.FollowingAndAttacking);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerAround = false;

            if (currentEnemyState == EnemyState.IdleAttacking)
                AddAIState(EnemyState.Idle);

            if (currentEnemyState == EnemyState.FollowingAndAttacking)
                AddAIState(EnemyState.Following);
        }
    }

    private float PlayerDirection()
    {
        if (this.transform.position.x - playerObject.position.x > 0)
            return -1;
        else
            return 1;
    }

    private void LookAtPlayer()
    {
        skeletonAnimation.skeleton.ScaleX = PlayerDirection();
    }

    private IEnumerator AttackHandler()
    {
        trackEntry = animationState.SetAnimation(0, "hit", true);

        yield return new WaitForSpineAnimationComplete(trackEntry);

        playerObject.GetComponent<HealthModule>().TakeHealth(damage);

        trackEntry = animationState.SetAnimation(0, "inactivity", true);

        if (!isFollowingPlayer && playerObject.GetComponent<HealthModule>().health <= 50)
        {
            isFollowingPlayer = true;
            AddAIState(EnemyState.Following);
        }
    }

    private void EnemyDeathAction(object sender, EventArgs e)
    {
        if (currentEnemyState == EnemyState.Died)
            return;

        AddAIState(EnemyState.Died);
    }

    private IEnumerator DeathAnimations()
    {
        yield return new WaitForSpineAnimationComplete(trackEntry);
        Destroy(this.gameObject);
    }
}

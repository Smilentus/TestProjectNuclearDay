using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using System;

public class PlayerController : MonoBehaviour
{
    private JoystickController controller;
    private CameraController cameraController;
    private HealthModule healthModule;
    private Rigidbody2D rb;

    [Header("Урон игрока")]
    public float damage = 12.5f;
    [Header("Скорость передвижения")]
    public float moveSpeed = 5f;

    SkeletonAnimation skeletonAnimation;
    Spine.AnimationState animationState;
    public Spine.TrackEntry trackEntry;

    private AudioSource audioSource;

    // Доступ к управлению игроком
    public bool canControlPlayer = true;

    // Текущее состояние игрока
    private enum PlayerState
    {
        Idle,       
        Running,        
        Walking,        
        Attacking,      
        Interacting    
    }

    private PlayerState currentPlayerState;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        cameraController = FindObjectOfType<CameraController>();
        controller = FindObjectOfType<JoystickController>();
        healthModule = GetComponent<HealthModule>();

        rb = GetComponent<Rigidbody2D>();

        skeletonAnimation = transform.GetChild(1).GetComponent<SkeletonAnimation>();
        animationState = skeletonAnimation.AnimationState;

        currentPlayerState = PlayerState.Idle;
        trackEntry = animationState.SetAnimation(0, "inactivity", true);

        healthModule.OnHealthBelowZeroHandler += PlayerDeathAction;
    }

    private void LateUpdate()
    {
        if (canControlPlayer == false)
            return;

        if (currentPlayerState == PlayerState.Idle)
        {
            if (controller.Horizontal() != 0)
            {
                trackEntry = animationState.SetAnimation(0, "walking", true);
                currentPlayerState = PlayerState.Walking;
                audioSource.Play();
            }
        }

        if (currentPlayerState == PlayerState.Walking)
        {
            if (controller.Horizontal() == 0)
            {
                trackEntry = animationState.SetAnimation(0, "inactivity", true);
                currentPlayerState = PlayerState.Idle;
                audioSource.Stop();
            }
        }

        if (controller.Horizontal() < 0)
        {
            cameraController.cameraOffset.x = -10;
            skeletonAnimation.skeleton.ScaleX = -1;
        }
        else if (controller.Horizontal() > 0)
        {
            cameraController.cameraOffset.x = 10;
            skeletonAnimation.skeleton.ScaleX = 1;
        }
    }

    private void FixedUpdate()
    {
        if (currentPlayerState == PlayerState.Attacking || canControlPlayer == false)
            return;

        rb.MovePosition(rb.position + controller.InputVector() * moveSpeed * Time.fixedDeltaTime);
    }

    private void PlayerDeathAction(object sender, EventArgs e)
    {
        canControlPlayer = false;
        audioSource.Stop();
        FindObjectOfType<Fader>().StartFadeIn();
    }

    public void AttackButtonAction()
    {
        if (currentPlayerState != PlayerState.Attacking && canControlPlayer)
            StartCoroutine(AttackCooldown());
    }

    public void InteractWithObject()
    {
        canControlPlayer = false;
        currentPlayerState = PlayerState.Interacting;
        trackEntry = animationState.SetAnimation(0, "interaction", true);
        StartCoroutine(InteractCooldown());
    }

    private IEnumerator InteractCooldown()
    {
        yield return new WaitForSpineAnimationComplete(trackEntry);
        canControlPlayer = true;
        currentPlayerState = PlayerState.Idle;
        trackEntry = animationState.SetAnimation(0, "inactivity", true);
    }

    private IEnumerator AttackCooldown()
    {
        currentPlayerState = PlayerState.Attacking;

        trackEntry = animationState.SetAnimation(0, "hit", true);

        yield return new WaitForSpineAnimationComplete(trackEntry);

        Collider2D[] enemies = Physics2D.OverlapCircleAll(this.transform.position + new Vector3(0, 5, 0), 3.5f);

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                var hp = enemy.gameObject.GetComponent<HealthModule>();
                if (hp != null)
                {
                    hp.TakeHealth(damage);
                }
            }

            trackEntry = animationState.SetAnimation(0, "inactivity", true);

            currentPlayerState = PlayerState.Idle;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 8f;
    [SerializeField] int difficultTerrainReduction = 5;
    [SerializeField] Vector2 deathKick = new Vector2(0f, 10f);
    [SerializeField] float deathTimer = 2f;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;
    [SerializeField] AudioClip bulletSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip crunchSound;
    [SerializeField] AudioClip bounceSound;
    AudioSource audioSource;

    float timer = 0f;
    bool isAlive = true;
    Vector2 currentVelocity;
    float maxVelocity = 20f;
    float startingGravity;
    Vector2 moveInput;
    Rigidbody2D myRigidBody2D;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    Transform myTransform;

    void Start()
    {
        myRigidBody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        myTransform = GetComponent<Transform>();
        audioSource = FindObjectOfType<AudioSource>();
        startingGravity = myRigidBody2D.gravityScale;
    }

    void Update()
    {
        ResetAnimator();
        if (isAlive)
        {
            Move();
            FlipSprite();
            ClimbLadder();
            currentVelocity = myRigidBody2D.velocity;
        }
        else if (timer >= deathTimer)
        {
            myRigidBody2D.velocity = new Vector2(0f, myRigidBody2D.velocity.y);
        }
        else
        {
            timer += Time.deltaTime;
        }

        //Debug.Log($"Is moving Y: {IsMovingVertically()}. Moving at: {currentVelocity.y}. Timer: {timer}");
    }

    private void LateUpdate()
    {
        if (currentVelocity.y >= maxVelocity)
        {
            myRigidBody2D.velocity = new Vector2(myRigidBody2D.velocity.x, maxVelocity);
        }
    }

    public AnimationClip FindAnimation(Animator animator, string name)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }

        return null;
    }

    void Die()
    {
        if (isAlive)
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myRigidBody2D.gravityScale = startingGravity;
            myRigidBody2D.velocity = deathKick;
            audioSource.PlayOneShot(deathSound, 0.6f);
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }

    void OnEscape()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnMove(InputValue inputValue)
    {
        if (!isAlive)
        {
            return;
        }
        moveInput = inputValue.Get<Vector2>();
    }

    void OnJump(InputValue inputValue)
    {
        if (!isAlive)
        {
            return;
        }
        if (inputValue.isPressed && IsTouchingGround())
        {
            myRigidBody2D.velocity += new Vector2(0f, jumpSpeed);
            myAnimator.SetBool("IsJumping", true);
        }
    }

    void OnFire(InputValue inputValue)
    {
        if (!isAlive || IsTouchingLadder())
            return;

        Instantiate(bullet, gun.position, transform.rotation * Quaternion.Euler(0f, 0f, -90f));
        audioSource.PlayOneShot(bulletSound, 0.5f);
    }

    private void ClimbLadder()
    {
        if (IsTouchingLadder())
        {
            myRigidBody2D.gravityScale = 0f;
            Vector2 playerVelocity = new Vector2(myRigidBody2D.velocity.x, moveInput.y * climbSpeed);

            myRigidBody2D.velocity = playerVelocity;

            ResetAnimator();

            myAnimator.SetBool("IsClimbing", true);
            if (IsMovingVertically())
            {
                myAnimator.speed = 1f;
            }
            else
            {
                myAnimator.speed = 0f;
            }
        }
        else
        {
            myRigidBody2D.gravityScale = startingGravity;
        }
    }

    private void Move()
    {
        float calculatedMoveSpeed;

        if (IsTouchingLadder())
        {
            calculatedMoveSpeed = movementSpeed - difficultTerrainReduction;
        }
        else
        {
            calculatedMoveSpeed = movementSpeed;
        }

        Vector2 playerVelocity = new Vector2(moveInput.x * calculatedMoveSpeed, myRigidBody2D.velocity.y);

        myRigidBody2D.velocity = playerVelocity;

        if (IsTouchingGround())
        {
            myAnimator.SetBool("IsRunning", IsMovingHorizontally() && !IsJumping());
        }
        else if (!IsTouchingGround() && !IsJumping() && !IsTouchingLadder())
        {
            myAnimator.SetBool("IsJumping", true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ladder")
        {
            ResetAnimator();
        }

        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Water")) || myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Hazards")))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            ResetAnimator();
        }

        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Bouncing")))
        {
            audioSource.PlayOneShot(bounceSound, 0.8f);
        }

        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Enemies")) && isAlive)
        {
            Kill(collision.gameObject);
        }
        else if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies")) || myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Hazards")))
        {
            Die();
        }
    }

    private void Kill(GameObject gameObject)
    {
        audioSource.PlayOneShot(crunchSound, 0.8f);
        Destroy(gameObject);
        myRigidBody2D.velocity += deathKick;
    }

    private void ResetAnimator()
    {
        myAnimator.speed = 1f;
        myAnimator.SetBool("IsJumping", false);
        myAnimator.SetBool("IsRunning", false);
        myAnimator.SetBool("IsClimbing", false);
    }

    private bool IsMovingHorizontally()
    {
        //check player is moving left or right
        return Mathf.Abs(myRigidBody2D.velocity.x) > Mathf.Epsilon;
    }

    private bool IsMovingVertically()
    {
        //check player is moving up or down
        return Mathf.Abs(myRigidBody2D.velocity.y) > Mathf.Epsilon;
    }

    private bool IsTouchingGround()
    {
        return myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    private bool IsTouchingLadder()
    {
        return myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Ladder"));
    }

    private bool IsJumping()
    {
        return myAnimator.GetBool("IsJumping");
    }

    private void FlipSprite()
    {
        if (IsMovingHorizontally())
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody2D.velocity.x), 1f);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] AudioClip killSound;
    AudioSource audioSource;
    Rigidbody2D myRigidBody2D;
    PlayerMovement player;
    float bulletDirection;

    void Start()
    {
        myRigidBody2D = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerMovement>();
        audioSource = FindObjectOfType<AudioSource>();  
        bulletDirection = player.transform.localScale.x * bulletSpeed;
    }

    void Update()
    {
        myRigidBody2D.velocity = new Vector2(bulletDirection, 0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Destroy();
        }

        if (collision.gameObject.tag == "Enemy")
        {
            Kill(collision);
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }

    private void Kill(Collision2D collision)
    {
        audioSource.PlayOneShot(killSound, 0.8f);
        Destroy(collision.gameObject);
    }
}

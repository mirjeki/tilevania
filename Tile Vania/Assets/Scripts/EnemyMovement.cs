using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    Rigidbody2D myRigidBody2D;

    void Start()
    {
        myRigidBody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        myRigidBody2D.velocity = new Vector2(moveSpeed, 0f);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        transform.localScale = new Vector2(Mathf.Sign(-myRigidBody2D.velocity.x), 1f);
        moveSpeed = -moveSpeed;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] AudioClip coinPickupSound;
    AudioSource audioSource;
    bool collected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !collected)
        {
            audioSource = FindObjectOfType<AudioSource>();
            collected = true;
            audioSource.PlayOneShot(coinPickupSound, 0.5f);
            FindObjectOfType<GameSession>().CollectCoin();
            Destroy(this.gameObject);
        }
    }
}

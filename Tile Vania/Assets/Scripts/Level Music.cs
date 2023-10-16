using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    [SerializeField] AudioClip musicClip;
    [SerializeField] float musicVolume;
  
    void Start()
    {
        var gameSession = FindObjectOfType<GameSession>();
        if (gameSession != null)
        {
            gameSession.PlayMusic(musicClip, musicVolume);
        }
    }
}

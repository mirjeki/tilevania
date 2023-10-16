using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [SerializeField] int playerLives = 3;
    [SerializeField] int coinsRequiredForNewLife = 25;
    [SerializeField] float levelLoadDelay = 1f;
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI coinsText;
    [SerializeField] TextMeshProUGUI lifeGainedText;

    [SerializeField] int coins = 0;
    [SerializeField] int coinsToNewLife = 0;

    bool lifeGainedRecently = false;
    AudioSource audioSource;
    void Awake()
    {
        int numGameSessions = FindObjectsOfType<GameSession>().Length;

        if (numGameSessions > 1 )
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        SetUIText();
        lifeGainedText.color = new Color(lifeGainedText.color.r, lifeGainedText.color.g, lifeGainedText.color.b, 0f);
    }

    private void Update()
    {
        if (lifeGainedRecently)
        {
            StartCoroutine(FadeLifeGainedText());
        }
    }
    IEnumerator FadeLifeGainedText()
    {
        yield return new WaitForSecondsRealtime(2f);

        lifeGainedText.color = new Color(lifeGainedText.color.r, lifeGainedText.color.g, lifeGainedText.color.b, 0f);
        lifeGainedRecently = false;
    }

    private void SetUIText()
    {
        livesText.text = $"Lives: {playerLives}";
        coinsText.text = $"Coins: {coins}";
    }

    public void CollectCoin()
    {
        coins++;
        coinsToNewLife++;
        if (coinsToNewLife >= coinsRequiredForNewLife)
        {
            playerLives++;
            lifeGainedRecently = true;
            lifeGainedText.color = new Color(lifeGainedText.color.r, lifeGainedText.color.g, lifeGainedText.color.b, 1f);
            coinsToNewLife = 0;
        }
        SetUIText();
    }

    public void ProcessPlayerDeath()
    {
        if (playerLives > 1)
        {
            TakeLife();
            StartCoroutine(ReloadLevel());
        }
        else
        {
            StartCoroutine(ResetGameSession());
        }
    }

    private void TakeLife()
    {
        playerLives--;
        SetUIText();
    }

    public void PlayMusic(AudioClip clip, float volume)
    {
        audioSource = FindObjectOfType<AudioSource>();
        audioSource.volume = volume;
        audioSource.clip = clip;
        audioSource.PlayDelayed(1f);
    }

    public IEnumerator ResetGameSession()
    {
        yield return new WaitForSecondsRealtime(levelLoadDelay);

        FindObjectOfType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    IEnumerator ReloadLevel()
    {
        yield return new WaitForSecondsRealtime(levelLoadDelay);

        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentSceneIndex);
    }
}

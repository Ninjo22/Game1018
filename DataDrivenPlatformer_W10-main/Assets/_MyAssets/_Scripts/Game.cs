using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class Game : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text triesText;
    [SerializeField] float maxTime = 90f;
    public int Score { get; private set; }
    private float startTime;
    private int tries;
    public static Game Instance { get; private set; } // Static object of the class.
    public SoundManager SOMA;

    private void Awake() // Ensure there is only one instance.
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Will persist between scenes.
            Initialize();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances.
        }
    }

    private void Initialize()
    {
        SOMA = new SoundManager();
        SOMA.Initialize(gameObject);
        SOMA.AddSound("Jump", Resources.Load<AudioClip>("jump"), SoundManager.SoundType.SOUND_SFX);
        SOMA.AddSound("Pickup", Resources.Load<AudioClip>("pickup"), SoundManager.SoundType.SOUND_SFX);
        SOMA.AddSound("Checkpoint", Resources.Load<AudioClip>("checkpoint"), SoundManager.SoundType.SOUND_SFX);
        SOMA.AddSound("Respawn", Resources.Load<AudioClip>("respawn"), SoundManager.SoundType.SOUND_SFX);
        SOMA.AddSound("Victory", Resources.Load<AudioClip>("Victory"), SoundManager.SoundType.SOUND_MUSIC);
        SOMA.AddSound("LavaChicken", Resources.Load<AudioClip>("LavaChicken"), SoundManager.SoundType.SOUND_MUSIC);
        SOMA.AddSound("MissionFailed", Resources.Load<AudioClip>("MissionFailed"), SoundManager.SoundType.SOUND_MUSIC);
        SOMA.PlayMusic("LavaChicken");

        Score = 0;
        tries = 0;
        startTime = Time.time;
        StartCoroutine("UpdateTimer");
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            float elapsedTime = Time.time - startTime;
            float displayedTime = maxTime - elapsedTime;
            if (displayedTime < 0f) { displayedTime = 0f; }
            timerText.text = "Time: " + displayedTime.ToString("F2") + "s";
            if (displayedTime == 0f)
            {
                LoadFailedScene();
            }
            yield return null;
        }
    }

    public void SetVolume(float volume)
    {
        SOMA.SetMasterVolume(volume);
    }

    public void AddScore(int s = 1)
    {
        Score += s;
        Score = Mathf.Clamp(Score, 0, 6767);
        scoreText.text = "Rings: " + Score.ToString();
    }

    public void AddTry(int n = 1)
    {
        Instance.SOMA.PlaySound("Respawn");
        tries += n;
        tries = Mathf.Clamp(tries, 0, 67);
        triesText.text = "Tries: " + tries.ToString();
    }

    public void LoadVictoryScene()
    {
        SceneCleanup();
        Instance.SOMA.PlayMusic("Victory", false);
        SceneManager.LoadScene(2);
    }

    public void LoadFailedScene()
    {
        SceneCleanup();
        Instance.SOMA.PlayMusic("MissionFailed", false);
        SceneManager.LoadScene(3);
    }

    private void SceneCleanup()
    {
        Instance.StopCoroutine("UpdateTimer");
        Instance.SOMA.StopMusic();
        GameObject cam = GameObject.Find("Main Camera");
        cam.GetComponent<CameraFollow>().enabled = false;
        cam.transform.GetChild(0).gameObject.SetActive(false);
    }
}

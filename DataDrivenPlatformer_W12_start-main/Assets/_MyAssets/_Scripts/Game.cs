using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text triesText;
    [SerializeField] float maxTime = 90f;

    [Header("Leaderboard")]
    [SerializeField] private LeaderboardManager leaderboardManager;

    public int Score { get; private set; }
    private float startTime;
    private float elapsedTime;
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

        InitGameState();
        StartCoroutine("UpdateTimer");
    }

    private void InitGameState()
    {
        Score = 0;
        tries = 1;
        startTime = Time.time;
        elapsedTime = 0f;

        if (scoreText != null) { scoreText.text = "Rings: " + Score; }
        if (triesText != null) { triesText.text = "Tries: " + tries; }
        if (timerText != null) { timerText.text = "Time: " + maxTime.ToString("F2") + "s"; }
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            elapsedTime = Time.time - startTime;
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
        tries = Mathf.Clamp(tries, 1, 67);
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
        SubmitCurrentScore();
    }

    public void SubmitCurrentScore()
    {
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogWarning("LeaderboardManager instance not found.");
            return;
        }

        LeaderboardManager.Instance.SubmitCurrentScore(Score, elapsedTime, tries);
    }
}

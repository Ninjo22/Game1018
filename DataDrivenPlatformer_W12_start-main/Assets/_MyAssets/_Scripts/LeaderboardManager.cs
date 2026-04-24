using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class HighScoreEntry
{
    public int score;
    public float et;
    public int tries;
}

[Serializable]
public class SubmitScorePayload
{
    public int score;
    public float et;
    public int tries;
}

[Serializable]
public class HighScoreResponse
{
    public HighScoreEntry[] highScores;
    public bool updated;
    public string error;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Server")]
    [SerializeField] private string leaderboardUrl = "https://your-domain.example/server/highscore.php";

    [Header("UI")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private TMP_Text leaderboardText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    public void ToggleLeaderboard()
    {
        if (leaderboardPanel == null) return;

        bool newState = !leaderboardPanel.activeSelf;
        leaderboardPanel.SetActive(newState);

        if (newState)
        {
            StartCoroutine(LoadLeaderboard());
        }
    }

    public void SubmitCurrentScore(int score, float elapsedTime, int tries)
    {
        StartCoroutine(SubmitScore(score, elapsedTime, tries));
    }

    public IEnumerator LoadLeaderboard()
    {
        LogStatus("Loading leaderboard...");

        using UnityWebRequest req = UnityWebRequest.Get(leaderboardUrl);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            LogStatus("Leaderboard load failed: " + req.error);
            yield break;
        }

        HighScoreResponse response = ParseResponse(req.downloadHandler.text);
        if (response == null)
        {
            LogStatus("Leaderboard parse failed.");
            yield break;
        }

        UpdateLeaderboardUI(response.highScores);
        LogStatus("Leaderboard loaded.");
    }

    public IEnumerator SubmitScore(int score, float elapsedTime, int tries)
    {
        LogStatus("Submitting score...");

        SubmitScorePayload payload = new SubmitScorePayload
        {
            score = score,
            et = elapsedTime,
            tries = tries
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest req = new UnityWebRequest(leaderboardUrl, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            LogStatus("Score submission failed: " + req.error);
            yield break;
        }

        HighScoreResponse response = ParseResponse(req.downloadHandler.text);
        if (response == null)
        {
            LogStatus("Response parse failed.");
            yield break;
        }

        if (!string.IsNullOrEmpty(response.error))
        {
            LogStatus("Server error: " + response.error);
            yield break;
        }

        UpdateLeaderboardUI(response.highScores);

        if (response.updated)
            LogStatus("New top 10 score posted!");
        else
            LogStatus("Run finished. Not in top 10.");
    }

    private HighScoreResponse ParseResponse(string json)
    {
        try
        {
            return JsonUtility.FromJson<HighScoreResponse>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[Leaderboard] JSON parse error: " + e.Message);
            Debug.LogError(json);
            return null;
        }
    }

    private void UpdateLeaderboardUI(HighScoreEntry[] scores)
    {
        if (leaderboardText == null) return;

        if (scores == null || scores.Length == 0)
        {
            leaderboardText.text = "TOP 10\nNo scores yet.";
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("TOP 10");

        for (int i = 0; i < scores.Length; i++)
        {
            HighScoreEntry e = scores[i];
            sb.AppendLine($"{i + 1}. Score: {e.score} | Time: {e.et:F2}s | Tries: {e.tries}");
        }

        leaderboardText.text = sb.ToString();
    }

    private void LogStatus(string message)
    {
        Debug.Log("[Leaderboard] " + message);
    }
}
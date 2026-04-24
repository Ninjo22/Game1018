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
    public int tires;
}

[Serializable]
public class SubmitScorePayload
{
    public int score;
    public float et;
    public int tries;
}

[Serializable]
public class HighscoreResponse
{
    public HighScoreEntry[] highScores;
    public bool updated;
    public string error;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Server Address")]
    [SerializeField] private string leaderboardUrl = "";

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
        
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void ToggleLeaderboard()
    { 
        if(leaderboardPanel == null) return;

        bool newState = !leaderboardPanel.activeSelf;

        if (newState) 
        {
            StartCoroutine(LoadLeaderboard());
        }
    }

    public IEnumerator LoadLeaderboard()
    {
        LogStatus("Loading Leaderboard...");

        using UnityWebRequest req = UnityWebRequest.Get(leaderboardUrl);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success) 
        { 
            LogStatus("Leaderboard log failed: " + req.error);
        }

        HighscoreResponse response = ParseResponse(req.downloadHandler.text);
        if (response == null) yield break;

        UpdateLeaderboardUI(response.highScores);
        LogStatus("Loaded successfuly");
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
            sb.AppendLine($"{i + 1}. Score: {e.score} | Time: {e.et:F2}s | Tries: {e.tires}");
        }
        leaderboardText.text = sb.ToString();
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
        }

        HighscoreResponse response = ParseResponse(req.downloadHandler.text);
        if (response == null) yield break;

        if (!string.IsNullOrEmpty(response.error))
        {
            LogStatus("Server error " + response.error);
            yield break;
        }

        UpdateLeaderboardUI(response.highScores);

        if (response.updated)
            LogStatus("New top 10 score posted!");
        else
            LogStatus("Run complete but not in the top 10.");
    }

    public void SubmitCurrentScore(int score, float et, int tries)
    {
        StartCoroutine(SubmitScore(score, et, tries));
    }

    private HighscoreResponse ParseResponse(string json)
    {
        try
        {
            return JsonUtility.FromJson<HighscoreResponse>(json);
        }
        catch (Exception e) 
        {
            LogStatus("JSON parse error: " + e.Message);
            return null;
        }
    }


    private void LogStatus(string msg)
    {
        Debug.Log("[Leaderboard" + msg);
    }
}
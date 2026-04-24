using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // This will be called by your Toggles
    public void SetLevelFile(string fileName)
    {
        LevelSettings.SelectedLevel = fileName;
        Debug.Log("Level set to: " + fileName);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("YourGameSceneName");
    }
}
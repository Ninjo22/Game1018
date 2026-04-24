using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void ChangeSceneTo(int sceneToLoad)
    {
        if (sceneToLoad == 0)
        {
            Destroy(GameObject.Find("Grid"));
            Destroy(GameObject.Find("GameManager"));
        }
        Destroy(GameObject.Find("Main Camera"));
        SceneManager.LoadScene(sceneToLoad);
    }
}

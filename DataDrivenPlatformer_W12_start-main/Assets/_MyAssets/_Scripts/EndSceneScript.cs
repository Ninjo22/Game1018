using UnityEngine;
using TMPro;

public class EndSceneScript : MonoBehaviour
{
    [SerializeField] TMP_Text ringText;
    [SerializeField] TMP_Text Tries;
    [SerializeField] private Color cameraColor;
    private Camera cam;

    void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = cameraColor;
        ringText.text = "Final Ring Count: " + Game.Instance.Score;
        Tries.text = "Total Tries: " + Game.Instance.tries;

    }
}

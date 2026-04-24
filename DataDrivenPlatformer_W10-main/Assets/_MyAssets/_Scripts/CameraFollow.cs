using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Target is player's position.
    [SerializeField] private float screenDivision; // 3f to 4f should be good.
    [SerializeField] private SpriteRenderer[] backgrounds;
    [SerializeField] private float[] scrollSpeeds;
    [SerializeField][Range(0f, 1f)] private float scrollSpeedScale;

    private float leftOffset;
    private float rightOffset;
    private Vector2[] scrolls;
    private float lastCamX;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        float cameraWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;
        float boundaryOffset = cameraWidth * (screenDivision - 2f) / (2f * screenDivision);
        leftOffset = -boundaryOffset;
        rightOffset = boundaryOffset;

        backgrounds = GetComponentsInChildren<SpriteRenderer>();

        scrolls = new Vector2[backgrounds.Length];
        for (int i = 0; i < scrolls.Length; i++)
        {
            scrolls[i] = Vector2.zero;
        }

        lastCamX = transform.position.x;
    }

    void LateUpdate()
    {
        // Get player and camera positions.
        Vector3 playerPosition = target.position;
        Vector3 camPosition = transform.position;

        // Calculate new bounds/thresholds.
        float newLeftBound = camPosition.x + leftOffset;
        float newRightBound = camPosition.x + rightOffset;

        // Draw the debug lines for the bounds.
        Debug.DrawLine(new Vector3(newLeftBound,0f,0f), new Vector3(newLeftBound, -24f,0f), Color.red);
        Debug.DrawLine(new Vector3(newRightBound, 0f,0f), new Vector3(newRightBound, -24f,0f), Color.red);

        float targetCamX = camPosition.x;

        if (playerPosition.x <= newLeftBound)
        {
            targetCamX = playerPosition.x - leftOffset;
        }
        else if (playerPosition.x >= newRightBound)
        {
            targetCamX = playerPosition.x - rightOffset;
        }

        transform.position = new Vector3(targetCamX, camPosition.y, camPosition.z);

        float camDeltaX = transform.position.x - lastCamX;

        if (Mathf.Abs(camDeltaX) > 0.0001f) // If the camera has moved, scroll the backgrounds.
        {
            for (int i = 1; i < backgrounds.Length; i++)
            {
                scrolls[i].x += camDeltaX * scrollSpeeds[i-1] * scrollSpeedScale;
                backgrounds[i].material.mainTextureOffset = scrolls[i];
            }
        }

        lastCamX = transform.position.x;
    }
}

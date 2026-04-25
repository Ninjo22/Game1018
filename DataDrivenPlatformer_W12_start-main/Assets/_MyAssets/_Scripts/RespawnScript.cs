using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnScript : MonoBehaviour
{
    [SerializeField] float killZ;
    private Vector3 respawnPosition;
    private bool finishTriggered = false;
    
    void Start()
    {
        respawnPosition = transform.position;
    }

    private void Update()
    {
        if (this.transform.position.y <= killZ)
        {
            Game.Instance.AddTry();
            this.transform.position = respawnPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // 2D.
    {
        if (other.gameObject.tag == "Checkpoint")
        {
            Game.Instance.SOMA.PlaySound("Checkpoint");
            respawnPosition = other.transform.position;

            // Search the parent and its children for the SpriteRenderer
            SpriteRenderer sr = other.GetComponentInChildren<SpriteRenderer>();

            if (sr != null)
            {
                sr.color = Color.green;
            }
            else
            {
                Debug.LogWarning("SpriteRenderer not found on " + other.gameObject.name + " or its children!");
            }
        }
        if (other.gameObject.tag == "Finish")
        {
            if (!finishTriggered) 
            {
                finishTriggered = true;
                respawnPosition = other.transform.position; // Checkpoint's position.
                Game.Instance.LoadVictoryScene();
            }
        }
    }

    
}

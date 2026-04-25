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

            // Specifically find the child named "Checkpoint (2)"
            Transform flagTransform = other.transform.Find("Checkpoint (2)");

            if (flagTransform != null)
            {
                SpriteRenderer sr = flagTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.green;
                }
            }
            else
            {
                Debug.LogWarning("Could not find a child named 'Checkpoint (2)' under " + other.gameObject.name);
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

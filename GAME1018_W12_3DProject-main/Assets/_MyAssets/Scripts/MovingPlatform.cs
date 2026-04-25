using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 3.0f;
    public float waitTime = 1.5f;

    private Vector3 _targetPosition;
    private bool _isWaiting = false;

    void Start()
    {
        transform.position = startPoint.position;
        _targetPosition = endPoint.position;
    }

    void Update()
    {
        if (_isWaiting) return;

        // Move the platform
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

        // Check if we reached the target
        if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    IEnumerator WaitAtPoint()
    {
        _isWaiting = true;

        // Toggle the target position
        _targetPosition = _targetPosition == startPoint.position ? endPoint.position : startPoint.position;

        // Wait for the specified time
        yield return new WaitForSeconds(waitTime);

        _isWaiting = false;
    }

    // Parenting logic remains the same
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
using UnityEngine;
using System.Collections;

// This component provides a simple way to smoothly move a GameObject from its current position to a target position over a set duration.
public class SmoothLerp : MonoBehaviour
{
    // This holds a reference to the currently active movement coroutine, so we can stop it if a new move starts.
    private Coroutine _activeMoveCoroutine;

    // Call this method to initiate a smooth move.
    // If a move is already in progress, it will be stopped, and a new one will begin.
    public void StartMove(Vector3 targetPosition, float duration)
    {
        // If there's an existing move coroutine running, we stop it to prevent conflicts.
        if (_activeMoveCoroutine != null)
        {
            StopCoroutine(_activeMoveCoroutine);
        }
        _activeMoveCoroutine = StartCoroutine(MoveCoroutine(targetPosition, duration));
    }

    // This coroutine performs the actual smooth movement over the specified duration.
    private IEnumerator MoveCoroutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        // The loop continues until the elapsed time reaches the desired duration.
        while (elapsedTime < duration)
        {
            // 't' is the interpolation factor, ranging from 0 to 1.
            float t = elapsedTime / duration;
            // We use an ease-out function (cubic) to make the movement start fast and slow down smoothly as it reaches the target.
            t = 1 - Mathf.Pow(1 - t, 3);

            // Vector3.Lerp linearly interpolates between the start and target positions based on 't'.
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // After the loop, we snap the object to the exact target position to ensure precision.
        transform.position = targetPosition;
        _activeMoveCoroutine = null;
    }
}

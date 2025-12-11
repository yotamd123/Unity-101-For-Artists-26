using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PushbackTrigger : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("Assign the Player root transform.")]
    [SerializeField] private Transform playerRoot;

    // ----------------- NEW: Required Component Reference -----------------
    [Header("Required Player Component (Teaching Example)")]
    [Tooltip("The player must have a BoxCollider assigned or the pushback will not work.")]
    [SerializeField] private BoxCollider playerBoxCollider;
    // ----

    [Header("Pushback Settings")]
    [Tooltip("Distance (in meters) the player will be pushed back.")]
    [SerializeField] private float pushDistance = 2f;

    [Tooltip("Fixed pushback speed (locked).")]
    [SerializeField] private float pushSpeed = 4f;

    [Header("Color Pulse")]
    [Tooltip("Color the player flashes to.")]
    [SerializeField] private Color pulseColor = Color.red;

    private Renderer playerRenderer;
    private Color originalPlayerColor;

    private Coroutine pushRoutine;
    private Coroutine colorRoutine;

    private Collider myCollider;

    private void Start()
    {
        if (playerRoot == null)
        {
            Debug.LogWarning($"{name}: No player assigned to PushbackTrigger!");
            return;
        }

        playerRenderer = playerRoot.GetComponent<Renderer>();

        if (playerRenderer == null)
        {
            Debug.LogWarning($"{name}: Player has no Renderer component.");
            return;
        }

        myCollider = GetComponent<Collider>();
        originalPlayerColor = playerRenderer.material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ----------------- NEW: Required reference check -----------------
        if (playerBoxCollider == null)
        {
            Debug.LogWarning($"{name}: Missing BoxCollider reference! Add a BoxCollider to the player and assign it.");
            return; // Prevent ALL pushback logic
        }
        // -----------------------------------------------------------------

        if (other.transform != playerRoot) return;

        // STEP 1 — Find the surface point of the object closest to the player
        Vector3 playerPos = playerRoot.position;
        Vector3 surfacePoint = myCollider.ClosestPoint(playerPos);

        // STEP 2 — Push direction = away from the surface hit point
        Vector3 pushDirection = (playerPos - surfacePoint).normalized;

        // Prevent pushing into the object if somehow too close
        if (pushDirection.sqrMagnitude < 0.001f)
            pushDirection = (playerPos - transform.position).normalized;

        // STEP 3 — Start push + color pulse animations
        if (pushRoutine != null) StopCoroutine(pushRoutine);
        if (colorRoutine != null) StopCoroutine(colorRoutine);

        pushRoutine = StartCoroutine(PushPlayer(pushDirection));
        colorRoutine = StartCoroutine(PlayerColorPulse());
    }

    private System.Collections.IEnumerator PushPlayer(Vector3 direction)
    {
        float moved = 0f;
        Vector3 moveDir = direction.normalized;

        while (moved < pushDistance)
        {
            float step = pushSpeed * Time.deltaTime;
            playerRoot.position += moveDir * step;
            moved += step;
            yield return null;
        }
    }

    private System.Collections.IEnumerator PlayerColorPulse()
    {
        const float duration = 0.12f;

        for (int i = 0; i < 1; i++)
        {
            yield return LerpColor(originalPlayerColor, pulseColor, duration);
            yield return LerpColor(pulseColor, originalPlayerColor, duration);
        }
    }

    private System.Collections.IEnumerator LerpColor(Color from, Color to, float duration)
    {
        float t = 0f;
        Material mat = playerRenderer.material;

        while (t < duration)
        {
            t += Time.deltaTime;
            mat.color = Color.Lerp(from, to, t / duration);
            yield return null;
        }

        mat.color = to;
    }
}

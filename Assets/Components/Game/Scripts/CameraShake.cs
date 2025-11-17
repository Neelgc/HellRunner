using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("Force maximale de la vibration.")]
    public float shakeMagnitude = 0.2f;

    [Tooltip("Durée totale de la vibration (secondes).")]
    public float shakeDuration = 0.3f;

    [Tooltip("Atténuation progressive de l'intensité.")]
    public float dampingSpeed = 1.5f;

    private Vector3 initialPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    /// <summary>
    /// Démarre une vibration de la caméra.
    /// </summary>
    /// <param name="duration">Durée de la vibration.</param>
    /// <param name="magnitude">Intensité de la vibration.</param>
    public void Shake(float duration, float magnitude)
    {
        // Si un shake est déjà en cours, on le redémarre
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Position aléatoire autour du point initial
            Vector2 randomPoint = Random.insideUnitCircle * magnitude;
            transform.localPosition = initialPosition + new Vector3(randomPoint.x, randomPoint.y, 0f);

            // On réduit progressivement l’intensité
            magnitude = Mathf.MoveTowards(magnitude, 0f, dampingSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Retour à la position d’origine
        transform.localPosition = initialPosition;
        shakeRoutine = null;
    }

    /// <summary>
    /// Réinitialise la position de la caméra manuellement.
    /// </summary>
    public void ResetPosition()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }
        transform.localPosition = initialPosition;
    }
}

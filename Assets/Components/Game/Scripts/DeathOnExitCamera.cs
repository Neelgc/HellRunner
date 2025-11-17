using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathOnExitCamera : MonoBehaviour
{
    private Camera mainCamera;
    private Health playerHealth;
    public float startDelay = 2f;
    private float resetDelay = 0f;

    private void Start()
    {
        mainCamera = Camera.main;
        playerHealth = GetComponent<Health>();

        if (mainCamera == null)
            Debug.LogError("[DeathOnExitCamera] No Main Camera found!");

        if (playerHealth == null)
            Debug.LogError("[DeathOnExitCamera] No Health component found on player!");

    }

    private void Update()
    {
        if (mainCamera == null || playerHealth == null)
            return;

        if(startDelay > 0f)
        {
            startDelay -= Time.deltaTime;
            return;
        }

        if (resetDelay > 0f)
        {
            resetDelay -= Time.deltaTime;
            return;
        }

        // Convertit la position du joueur en coordonnées d'écran
        Vector3 screenPos = mainCamera.WorldToViewportPoint(transform.position);

        // Si le joueur est complètement hors de l'écran (X ou Y)
        bool isOffScreen = screenPos.x < 0f || screenPos.x > 1f ||
                           screenPos.y < 0f || screenPos.y > 1f;

        if (isOffScreen)
        {
            // Applique 100% de dégâts -> mort instantanée
            playerHealth.TakeDamage(playerHealth.maxHealth);

            // Tu peux aussi ajouter un message de debug
            Debug.Log("[DeathOnExitCamera] Player went off-screen -> instant death.");

            // Reset delay to avoid multiple deaths
            resetDelay = 2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}

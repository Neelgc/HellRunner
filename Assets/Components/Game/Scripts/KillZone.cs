using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private int damageAmount = 999;
    [SerializeField] private bool killInstantly = true;

    private void Start()
    {
        Debug.Log($"KillZone créée à la position {transform.position}");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Debug.Log($"KillZone a un collider, IsTrigger = {col.isTrigger}");
        }
        else
        {
            Debug.LogError("KillZone n'a PAS de collider !");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"KillZone touchée par : {collision.gameObject.name} avec tag : {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            Debug.Log("C'est le Player !");

            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log("Health trouvée, application des dégâts...");

                if (killInstantly)
                {
                    playerHealth.TakeDamage(playerHealth.maxHealth);
                }
                else
                {
                    playerHealth.TakeDamage(damageAmount);
                }
            }
            else
            {
                Debug.LogError("Pas de composant Health sur le Player !");
            }
        }
    }
}
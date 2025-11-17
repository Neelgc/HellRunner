using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D myCollider;

    private float speed = 20f;
    private int damage = 1;
    private int penetration = 1;

    // On stocke les GameObjects déjà touchés pour ne pas les compter deux fois
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    // Option : délai avant la destruction pour laisser la trail visible (0 = immédiat)
    [SerializeField] private float destroyDelay = 0f;

    public void Initialize(GunData gun)
    {
        speed = gun.bulletSpeed;
        damage = gun.damage;
        penetration = gun.penetration;
        rb.gravityScale = gun.useGravity ? 1f : 0f;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        // évite que la balle soit affectée par triggers/physique du joueur directement si besoin
        // ex: Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), gameObject.layer, true); // si utile
    }

    private void Start()
    {
        rb.AddForce(transform.right * speed, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore collision avec le player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            return;

        GameObject otherGO = collision.gameObject;

        // Si on a déjà géré ce GameObject (par ex. plusieurs colliders), on ignore
        if (hitObjects.Contains(otherGO))
            return;

        // Si c'est un ennemi (ou a un composant Health)
        Health enemyHealth = otherGO.GetComponent<Health>();
        if (enemyHealth != null)
        {
            // On marque cet objet comme touché (pour éviter double comptage)
            hitObjects.Add(otherGO);

            // Infliger les dégâts
            enemyHealth.TakeDamage(damage);

            AudioManager.Instance.PlaySound("Hit");

            // Décrémenter la pénétration (1 == meurt après ce hit)
            penetration--;

            // Ignorer tous les colliders de l'ennemi pour ne plus détecter ce GameObject
            Collider2D[] enemyColliders = otherGO.GetComponents<Collider2D>();
            foreach (var col in enemyColliders)
            {
                if (col == null) continue;
                Physics2D.IgnoreCollision(myCollider, col, true);
            }

            if (penetration <= 0)
            {
                if (destroyDelay > 0f)
                    Destroy(gameObject, destroyDelay);
                else
                    Destroy(gameObject);
            }
            // else la balle continue
        }
        else
        {
            // Si c'est un mur/obstacle -> détruire immédiatement
            if (destroyDelay > 0f)
                Destroy(gameObject, destroyDelay);
            else
                Destroy(gameObject);
        }
    }
}

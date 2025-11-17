using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Revival Settings")]
    [SerializeField][Range(0,100)] private int lossHPerSecond = 2;
    private float revivalTimer = 0f;
    private bool isRevivalActive = false;

    public bool isInvincible = false;

    public int maxHealth = 100;
    public int currentHealth;

    public bool SetFullHealthOnStart = true;

    [SerializeField] private DeathType deathType = DeathType.Destroy;

    public HealthBar healthBar;

    [HideInInspector] public Action actionOnDeath;

    private void Start()
    {
        if (SetFullHealthOnStart)
            currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Update()
    {
        if(isInvincible)
            return;


        if (isRevivalActive)
        {
            revivalTimer += Time.deltaTime;

            if(revivalTimer < 1f)
                return;

            currentHealth -= lossHPerSecond;

            AudioManager.Instance.PlaySound("Fire");

            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }

            if (currentHealth <= 0)
            {
                DefinitiveDeath();
            }

            revivalTimer -= 1f;
        }
    }

    public void TakeDamage(int damage)
    {

        if (isInvincible)
            return;

        currentHealth -= damage;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        actionOnDeath?.Invoke();

        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null && pm.isAlive)
        {
            AudioManager.Instance.PlaySound("Electronic");

            CameraShake camShake = Camera.main.GetComponent<CameraShake>();
            camShake.Shake(0.5f, 0.7f);

            PlayerRespawn respawn = GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                pm.isAlive = false;
                pm.isReviving = true;
                respawn.RespawnInUnderworld();
                StartRevival();
                return;
            }
        }

        DefinitiveDeath();
    }

    public void StartRevival()
    {
        currentHealth = maxHealth;
        revivalTimer = 0f;
        isRevivalActive = true;
        isInvincible = true; 

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        StartCoroutine(InvincibilityDuration(1f));
        Debug.Log("Revival mode started!");
    }

    private System.Collections.IEnumerator InvincibilityDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }


    public void CompleteRevival()
    {
        isRevivalActive = false;
        revivalTimer = 0f;
        currentHealth = maxHealth;

        AudioManager.Instance.PlaySound("Heal");

        GetComponent<PlayerMovement>().isReviving = false;
        GetComponent<PlayerMovement>().isAlive = true;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        Debug.Log("Revival completed, player is alive again.");
    }

    private void DefinitiveDeath()
    {
        isRevivalActive = false;

        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.isAlive = false;
            pm.isReviving = false;
        }

        //AudioManager.Instance.PlaySound("Electronic");

        //CameraShake camShake = Camera.main.GetComponent<CameraShake>();
        //camShake.Shake(0.5f, 0.7f);

        if (deathType == DeathType.Destroy)
        {
            Destroy(gameObject);
        }
        else if (deathType == DeathType.Disable)
        {
            gameObject.SetActive(false);
        }
        else if (deathType == DeathType.MainMenu)
        {
            //AudioManager.Instance.PlaySound("Electronic");
            AudioManager.Instance.PlaySound("Death");

            // Save the score or any necessary data here
            SaveManager.PlayerData lastData = new();
            lastData.score = ScoreManager.Instance.GetCurrentScore();
            SaveManager.Save(lastData, "LastScore");

            // Load the death menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
        }

        Debug.Log($"{gameObject.name} has died definitively.");
    }

    enum DeathType
    {
        None,
        Destroy,
        Disable,
        MainMenu

    }
}

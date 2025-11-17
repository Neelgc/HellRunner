using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score Values")]
    [SerializeField] private int zombiePoints = 100;
    [SerializeField] private int archerPoints = 150;
    [SerializeField] private int bossPoints = 500;
    [SerializeField] private int revivalPoints = 1000;
    [SerializeField] private float pointsPerSecond = 10f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    private int currentScore = 0;
    private float timeAccumulator = 0f;

    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreDisplay();
    }

    private void Update()
    {
        if (playerMovement == null) return;

        if (playerMovement.isAlive && !playerMovement.isReviving)
        {
            timeAccumulator += Time.deltaTime;

            if (timeAccumulator >= 1f)
            {
                int pointsToAdd = Mathf.FloorToInt(pointsPerSecond * timeAccumulator);
                AddScore(pointsToAdd);
                timeAccumulator = 0f;
            }
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    public void OnZombieKilled()
    {
        AddScore(zombiePoints);
        Debug.Log($"+{zombiePoints} points (Enemy Normal)");
    }

    public void OnArcherKilled()
    {
        AddScore(archerPoints);
        Debug.Log($"+{archerPoints} points (Archer)");
    }

    public void OnBossKilled()
    {
        AddScore(bossPoints);
        Debug.Log($"+{bossPoints} points (BOSS)");
    }

    public void OnPlayerRevived()
    {
        AddScore(revivalPoints);
        Debug.Log($"+{revivalPoints} points (Revival!)");
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        timeAccumulator = 0f;
        UpdateScoreDisplay();
    }
}
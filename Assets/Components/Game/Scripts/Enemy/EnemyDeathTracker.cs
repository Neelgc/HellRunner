using UnityEngine;

public class EnemyDeathTracker : MonoBehaviour
{
    [HideInInspector] public EnemySpawner spawner;

    public enum EnemyType
    {
        Normal,
        Archer,
        Boss
    }

    [HideInInspector] public EnemyType enemyType = EnemyType.Normal;

    private void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }

        if (ScoreManager.Instance != null)
        {
            switch (enemyType)
            {
                case EnemyType.Normal:
                    ScoreManager.Instance.OnZombieKilled();
                    break;
                case EnemyType.Archer:
                    ScoreManager.Instance.OnArcherKilled();
                    break;
                case EnemyType.Boss:
                    ScoreManager.Instance.OnBossKilled();
                    break;
            }
        }
    }
}
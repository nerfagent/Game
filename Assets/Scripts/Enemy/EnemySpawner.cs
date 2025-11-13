using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    [Header("生成設定")]
    [Tooltip("要生成的敵人 Prefab")]
    [SerializeField] private BaseEnemy enemyPrefab;
    [Tooltip("此 Spawner 生成的敵人的全遊戲唯一ID。必須手動設置!")]
    [SerializeField] private string enemyUniqueID;

    private BaseEnemy spawnedEnemy;
    private Vector3 spawnPosition;

    // 公開屬性供 EnemyManager 查詢
    public string UniqueID => enemyUniqueID;
    public bool IsNormalEnemy => enemyPrefab != null && enemyPrefab.ShouldRespawn;
    public string SceneName => gameObject.scene.name;

    private void Awake()
    {
        if (string.IsNullOrEmpty(enemyUniqueID))
        {
            Debug.LogError($"位於 {gameObject.scene.name} 的 EnemySpawner ({gameObject.name}) 沒有設置唯一的 enemyUniqueID!", this);
            enabled = false;
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawner ({enemyUniqueID}) 未指定敵人 Prefab!", this);
            enabled = false;
            return;
        }

        spawnPosition = transform.position;
    }

    private void Start()
    {
        // 向管理器註冊自己
        EnemyManager.Instance.RegisterSpawner(this);
    }

    /// <summary>
    /// 由 EnemyManager 呼叫來執行生成。
    /// </summary>
    public void SpawnEnemy()
    {
        // 如果之前生成過且還在場景中，則重置它
        if (spawnedEnemy != null && spawnedEnemy.gameObject != null)
        {
            spawnedEnemy.ResetEnemy();
            spawnedEnemy.transform.position = spawnPosition;
            return;
        }

        spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemy.name = $"Enemy_{enemyUniqueID}"; // 方便在層級視圖中識別
        spawnedEnemy.AssignUniqueID(enemyUniqueID); // 分配ID

        SceneManager.MoveGameObjectToScene(spawnedEnemy.gameObject, gameObject.scene);

        Debug.Log($"已在 {spawnPosition} 生成 {spawnedEnemy.name}");
    }
    
    private void OnDestroy()
    {
        // 從管理器中反註冊
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterSpawner(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"ID: {enemyUniqueID}");
        #endif
    }
}
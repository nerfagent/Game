using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // 按場景名稱存儲所有 Spawner
    private Dictionary<string, List<EnemySpawner>> spawnersByScene = new Dictionary<string, List<EnemySpawner>>();
    
    // 記錄當前 Session 普通敵人死亡的列表
    private HashSet<string> sessionDeadEnemies = new HashSet<string>();

    // 僅用於存檔的 Boss 列表
    private List<string> defeatedBosses = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 訂閱事件
        SaveLoadManager.onLevelLoaded += OnLevelLoaded;
        Checkpoint.onCheckpointRest += OnCheckpointRest;
    }

    private void OnDestroy()
    {
        // 取消訂閱事件
        SaveLoadManager.onLevelLoaded -= OnLevelLoaded;
        Checkpoint.onCheckpointRest -= OnCheckpointRest;
    }

    /// <summary>
    /// 從 EnemySpawner 註冊。
    /// </summary>
    public void RegisterSpawner(EnemySpawner spawner)
    {
        string sceneName = spawner.SceneName;
        if (!spawnersByScene.ContainsKey(sceneName))
        {
            spawnersByScene[sceneName] = new List<EnemySpawner>();
        }
        spawnersByScene[sceneName].Add(spawner);
    }

    /// <summary>
    /// 從 EnemySpawner 反註冊。
    /// </summary>
    public void UnregisterSpawner(EnemySpawner spawner)
    {
        string sceneName = spawner.SceneName;
        if (spawnersByScene.ContainsKey(sceneName))
        {
            spawnersByScene[sceneName].Remove(spawner);
        }
    }

    /// <summary>
    /// 關卡加載完成後的回調。
    /// </summary>
    private void OnLevelLoaded()
    {
        string currentScene = LevelManager.Instance.GetCurrentLevelName();
        ProcessSpawnsForLevel(currentScene);
    }
    
    /// <summary>
    /// 處理當前關卡的所有敵人生成。
    /// </summary>
    public void ProcessSpawnsForLevel(string sceneName)
    {
        if (!spawnersByScene.ContainsKey(sceneName))
        {
            Debug.Log($"場景 {sceneName} 沒有註冊的 EnemySpawner。");
            return;
        }

        Debug.Log($"處理場景 {sceneName} 的敵人生成...");
        foreach (EnemySpawner spawner in spawnersByScene[sceneName])
        {
            bool shouldSpawn = true;
            
            // *** 核心修改：根據敵人類型檢查不同的死亡記錄 ***
            if (spawner.IsNormalEnemy)
            {
                // 普通敵人：檢查 Session 死亡列表
                if (sessionDeadEnemies.Contains(spawner.UniqueID))
                {
                    shouldSpawn = false;
                }
            }
            else // 是 Boss
            {
                // Boss：檢查持久化狀態
                string persistentKey = "BossDefeated_" + spawner.UniqueID;
                if (PersistentStateManager.Instance.GetBoolState(persistentKey, false))
                {
                    shouldSpawn = false;
                }
            }

            if (shouldSpawn)
            {
                spawner.SpawnEnemy();
            }
            else
            {
                Debug.Log($"敵人 {spawner.UniqueID} 已被擊敗或在本會話中死亡，此次不生成。");
            }
        }
    }

    /// <summary>
    /// 玩家在檢查點休息時的回調。
    /// </summary>
    private void OnCheckpointRest()
    {
        string currentScene = LevelManager.Instance.GetCurrentLevelName();
        if (!spawnersByScene.ContainsKey(currentScene)) return;

        Debug.Log($"檢查點休息：重生場景 {currentScene} 的普通敵人。");

        // 使用 ToList() 創建副本以安全修改 HashSet
        foreach (EnemySpawner spawner in spawnersByScene[currentScene].ToList())
        {
            if (spawner.IsNormalEnemy)
            {
                // 如果這個敵人在會話死亡列表中，將其移除並重生
                if (sessionDeadEnemies.Remove(spawner.UniqueID))
                {
                    Debug.Log($"從會話死亡列表中移除 {spawner.UniqueID} 並重生。");
                    spawner.SpawnEnemy();
                }
            }
        }
    }

    /// <summary>
    /// 記錄一個普通敵人在當前會話中死亡。
    /// </summary>
    public void RecordSessionEnemyDeath(string enemyID)
    {
        if (!sessionDeadEnemies.Contains(enemyID))
        {
            sessionDeadEnemies.Add(enemyID);
        }
    }

    /// <summary>
    /// 清空所有普通敵人的會話死亡記錄。在讀檔時呼叫。
    /// </summary>
    public void ClearSessionEnemyDeaths()
    {
        sessionDeadEnemies.Clear();
        Debug.Log("已清空當前會話的敵人死亡記錄。");
    }

    /// <summary>
    /// 記錄 Boss 被擊敗（用於存檔）。
    /// </summary>
    public void RecordBossDefeated(string bossID)
    {
        if (!defeatedBosses.Contains(bossID))
        {
            defeatedBosses.Add(bossID);
            Debug.Log($"Boss {bossID} 已被擊敗並記錄到存檔列表。");
        }
    }

    /// <summary>
    /// 從存檔加載已擊敗的 Boss 列表，並更新持久化狀態。
    /// </summary>
    public void LoadDefeatedBosses(List<string> bossList)
    {
        defeatedBosses = new List<string>(bossList ?? new List<string>());
        
        foreach (string bossID in defeatedBosses)
        {
            // 將 Boss 的擊敗狀態寫入當前遊戲會話的持久化管理器中
            string persistentKey = "BossDefeated_" + bossID;
            PersistentStateManager.Instance.SetBoolState(persistentKey, true);
        }
        Debug.Log($"已從存檔加載 {defeatedBosses.Count} 個已擊敗的Boss狀態。");
    }

    /// <summary>
    /// 獲取已擊敗的 Boss 列表（用於存檔）。
    /// </summary>
    public List<string> GetDefeatedBossList()
    {
        return new List<string>(defeatedBosses);
    }
}

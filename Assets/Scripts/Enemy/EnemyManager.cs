using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // 按場景名稱存儲所有 Spawner
    private Dictionary<string, List<EnemySpawner>> spawnersByScene = new Dictionary<string, List<EnemySpawner>>();
    
    // 僅用於存檔目的
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
        EventManager.StartListening("OnLevelLoaded", OnLevelLoaded);
        EventManager.StartListening("OnCheckpointRest", OnCheckpointRest);
    }

    private void OnDestroy()
    {
        // 取消訂閱事件
        EventManager.StopListening("OnLevelLoaded", OnLevelLoaded);
        EventManager.StopListening("OnCheckpointRest", OnCheckpointRest);
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
            // 檢查持久化狀態，決定是否生成
            string persistentKey = (spawner.IsNormalEnemy ? "EnemyDead_" : "BossDefeated_") + spawner.UniqueID;
            bool isDefeated = PersistentStateManager.Instance.GetBoolState(persistentKey, false);

            if (!isDefeated)
            {
                spawner.SpawnEnemy();
            }
            else
            {
                Debug.Log($"敵人 {spawner.UniqueID} 已被擊敗，此次不生成。");
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
        foreach (EnemySpawner spawner in spawnersByScene[currentScene])
        {
            // 只重生普通敵人
            if (spawner.IsNormalEnemy)
            {
                string persistentKey = "EnemyDead_" + spawner.UniqueID;
                
                // 檢查是否處於已死亡狀態
                bool wasDead = PersistentStateManager.Instance.GetBoolState(persistentKey, false);
                
                if(wasDead)
                {
                    // 在持久化狀態中將其標記為“未死亡”
                    PersistentStateManager.Instance.SetBoolState(persistentKey, false);
                    
                    // 指示 Spawner 立即重生該敵人
                    spawner.SpawnEnemy();
                }
            }
        }
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

// Assets/Scripts/Enemy/EnemyManager.cs
using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // 所有活躍敵人列表
    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();

    // Boss 註冊表：以名稱為鍵，便於查詢特定 Boss（shouldRespawn = false 的敵人）
    private Dictionary<string, BaseEnemy> bossRegistry = new Dictionary<string, BaseEnemy>();
    private List<string> defeatedBosses = new List<string>();

    private void Awake()
    {
        // 單例模式實現
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
        // 啟動時掃描場景中的所有敵人
        RefreshEnemyList();
    }

    /// <summary>
    /// 刷新活躍敵人列表。掃描場景中所有 BaseEnemy 並分類。
    /// 區分普通敵人與 Boss（根據 shouldRespawn 屬性）。
    /// </summary>
    public void RefreshEnemyList()
    {
        activeEnemies.Clear();
        bossRegistry.Clear();

        BaseEnemy[] enemies = FindObjectsOfType<BaseEnemy>();

        foreach (BaseEnemy enemy in enemies)
        {
            // 僅統計場景中啟用的敵人
            if (enemy.gameObject.activeInHierarchy)
            {
                activeEnemies.Add(enemy);

                // 若敵人的 shouldRespawn = false，則視為 Boss 並加入 Boss 註冊表
                if (!enemy.ShouldRespawn)
                {
                    bossRegistry[enemy.name] = enemy;
                }
            }
        }

        Debug.Log($"EnemyManager: 找到 {activeEnemies.Count} 個活躍敵人。");
    }

    /// <summary>
    /// 取得所有活躍敵人。
    /// </summary>
    public List<BaseEnemy> GetActiveEnemies()
    {
        return new List<BaseEnemy>(activeEnemies);
    }

    /// <summary>
    /// 透過名稱取得特定 Boss（shouldRespawn = false 的敵人）。
    /// </summary>
    public BaseEnemy GetBoss(string bossName)
    {
        if (bossRegistry.TryGetValue(bossName, out BaseEnemy enemy))
        {
            return enemy;
        }
        return null;
    }

    /// <summary>
    /// 獲取已擊敗的Boss列表
    /// </summary>
    public List<string> GetDefeatedBossList()
    {
        return new List<string>(defeatedBosses);
    }

    /// <summary>
    /// 從存檔加載已擊敗的Boss
    /// </summary>
    public void LoadDefeatedBosses(List<string> bossList)
    {
        defeatedBosses = new List<string>(bossList);
        
        // 禁用已擊敗的Boss
        foreach (string bossName in defeatedBosses)
        {
            BaseEnemy boss = GetBoss(bossName);
            if (boss != null && !boss.ShouldRespawn)
            {
                boss.gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"已加載 {defeatedBosses.Count} 個已擊敗的Boss");
    }

    /// <summary>
    /// 記錄Boss被擊敗（在BaseEnemy.Die()中調用）
    /// </summary>
    public void RecordBossDefeated(string bossName)
    {
        if (!defeatedBosses.Contains(bossName))
        {
            defeatedBosses.Add(bossName);
            Debug.Log($"Boss已擊敗: {bossName}");
        }
    }

    /// <summary>
    /// 註冊新生成的敵人。通常由 EnemySpawner 調用。
    /// </summary>
    public void RegisterEnemy(BaseEnemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }

        // 若是 Boss，也加入 Boss 註冊表
        if (!enemy.ShouldRespawn)
        {
            bossRegistry[enemy.name] = enemy;
        }
    }

    /// <summary>
    /// 反註冊敵人（通常在敵人死亡或銷毀時呼叫）。
    /// </summary>
    public void UnregisterEnemy(BaseEnemy enemy)
    {
        activeEnemies.Remove(enemy);

        if (!enemy.ShouldRespawn)
        {
            bossRegistry.Remove(enemy.name);
        }
    }

    /// <summary>
    /// 取得活躍敵人數量。
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    /// <summary>
    /// 重生所有 shouldRespawn = true 的敵人（不包含 Boss）。
    /// 通常在檢查點休息時調用。
    /// </summary>
    public void RespawnRegularEnemies()
    {
        foreach (BaseEnemy enemy in activeEnemies)
        {
            // 僅重生 shouldRespawn = true 且已死亡的敵人
            if (enemy.ShouldRespawn && enemy.IsDead)
            {
                enemy.ResetEnemy();
                enemy.gameObject.SetActive(true);
            }
        }

        Debug.Log("EnemyManager: 普通敵人已重生。");
    }

    /// <summary>
    /// 清除所有 shouldRespawn = true 的敵人（不包含 Boss）。
    /// </summary>
    public void DespawnAllRegularEnemies()
    {
        // 使用臨時列表以避免在迭代中修改原列表
        foreach (BaseEnemy enemy in new List<BaseEnemy>(activeEnemies))
        {
            if (enemy.ShouldRespawn)
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }
}

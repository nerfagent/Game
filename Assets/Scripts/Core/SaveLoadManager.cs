// Assets/Scripts/Core/SaveLoadManager.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    public static UnityAction onLevelLoaded = () => { };
    public static UnityAction onGameSaved = () => { };
    public static UnityAction onGameLoaded = () => { };

    private string saveFilePath;
    private SaveData currentSaveData;
    private bool isLoadingFromSave = false;  // 標記是否正在從存檔讀取
    
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
        
        // 設置存檔路徑
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        Debug.Log($"Save file path: {saveFilePath}");
    }
    
    private void Start()
    {
        // 訂閱關卡加載完成事件
        onLevelLoaded += OnLevelLoadedAfterLoad;
    }
    
    private void OnDestroy()
    {
        onLevelLoaded -= OnLevelLoadedAfterLoad;
    }
    
    private void Update()
    {
        // 臨時測試：按L鍵讀檔
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGameAndRestore();
        }
    }
    
    /// <summary>
    /// 保存遊戲
    /// </summary>
    public void SaveGame(string checkpointID, string sceneName, Vector3 spawnPosition)
    {
        SaveData data = new SaveData();
        
        // 保存檢查點信息
        data.lastCheckpointID = checkpointID;
        data.lastCheckpointSceneName = sceneName;
        data.lastCheckpointPosition = spawnPosition;
        
        // 保存玩家最大生命值
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                data.maxHP = playerHealth.MaxHP;
            }
        }
        
        // 保存技能升級
        if (SkillUpgradeManager.Instance != null)
        {
            data.skill0Upgrades = SkillUpgradeManager.Instance.GetAppliedUpgrades(0);
            data.skill1Upgrades = SkillUpgradeManager.Instance.GetAppliedUpgrades(1);
            data.skill2Upgrades = SkillUpgradeManager.Instance.GetAppliedUpgrades(2);
            data.skill3Upgrades = SkillUpgradeManager.Instance.GetAppliedUpgrades(3);
        }
        
        // 保存持久化狀態
        if (PersistentStateManager.Instance != null)
        {
            data.booleanStates = PersistentStateManager.Instance.GetAllBoolStates();
            data.integerStates = PersistentStateManager.Instance.GetAllIntStates();
            data.floatStates = PersistentStateManager.Instance.GetAllFloatStates();
            data.stringStates = PersistentStateManager.Instance.GetAllStringStates();
        }
        
        // 保存已擊敗的Boss
        if (EnemyManager.Instance != null)
        {
            data.defeatedBosses = EnemyManager.Instance.GetDefeatedBossList();
        }
        
        // 保存時間戳
        data.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 序列化為JSON
        string json = JsonUtility.ToJson(data, true);
        
        // 寫入文件
        File.WriteAllText(saveFilePath, json);
        
        currentSaveData = data;
        
        Debug.Log($"遊戲已保存 - 檢查點: {checkpointID}, 場景: {sceneName}, 位置: {spawnPosition}");
        onGameSaved.Invoke();
    }
    
    /// <summary>
    /// 讀取遊戲
    /// </summary>
    public SaveData LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("存檔文件不存在");
            return null;
        }
        
        try
        {
            // 讀取文件
            string json = File.ReadAllText(saveFilePath);
            
            // 反序列化
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            currentSaveData = data;
            
            Debug.Log($"遊戲已讀取 - 檢查點: {data.lastCheckpointID}, 場景: {data.lastCheckpointSceneName}, 位置: {data.lastCheckpointPosition}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"讀取存檔失敗: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 讀檔並恢復遊戲狀態
    /// </summary>
    public void LoadGameAndRestore()
    {
        SaveData data = LoadGame();
        
        if (data == null)
        {
            Debug.LogWarning("沒有可用的存檔");
            return;
        }
        
        // 驗證存檔數據
        if (string.IsNullOrEmpty(data.lastCheckpointSceneName))
        {
            Debug.LogError("存檔中沒有場景信息");
            return;
        }
        
        Debug.Log($"開始讀檔：轉移到場景 {data.lastCheckpointSceneName}，位置 {data.lastCheckpointPosition}");
        
        // 設置標記：正在從存檔讀取
        isLoadingFromSave = true;
        
        // 轉移到檢查點場景
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.TransitionToLevel(data.lastCheckpointSceneName, data.lastCheckpointPosition);
        }
        else
        {
            Debug.LogError("LevelManager 不存在，無法轉移場景");
            isLoadingFromSave = false;
        }
    }
    
    /// <summary>
    /// 關卡加載完成後應用存檔數據
    /// </summary>
    private void OnLevelLoadedAfterLoad()
    {
        if (isLoadingFromSave && currentSaveData != null)
        {
            Debug.Log("場景加載完成，開始應用存檔數據");
            ApplySaveData(currentSaveData);
            isLoadingFromSave = false;
        }
    }
    
    /// <summary>
    /// 應用存檔數據到遊戲
    /// </summary>
    public void ApplySaveData(SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("存檔數據為空，無法應用");
            return;
        }

        // 應用持久化狀態
        if (PersistentStateManager.Instance != null)
        {
            PersistentStateManager.Instance.LoadStates(
                data.booleanStates,
                data.integerStates,
                data.floatStates,
                data.stringStates
            );
        }

        // 應用已擊敗的Boss
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.LoadDefeatedBosses(data.defeatedBosses);
        }

        // 在應用任何數據之前，清空舊的會話狀態
        if (EnemyManager.Instance != null)
        {
            // 1. 清空所有普通敵人的會話死亡記錄
            EnemyManager.Instance.ClearSessionEnemyDeaths();
            
            // 2. 獲取剛加載完成的場景名稱
            string currentScene = LevelManager.Instance.GetCurrentLevelName();
            
            // 3. 命令 EnemyManager 立即為此場景重新生成所有普通敵人
            if (!string.IsNullOrEmpty(currentScene))
            {
                Debug.Log($"讀檔完成，為場景 {currentScene} 重新生成所有普通敵人...");
                EnemyManager.Instance.ProcessSpawnsForLevel(currentScene);
            }
        }
        
        // 應用玩家最大生命值
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.SetMaxHP(data.maxHP);
                playerHealth.RestoreToFull(); // 檢查點自動滿血
            }
        }
        
        // 應用技能升級
        if (SkillUpgradeManager.Instance != null && CooldownSystem.Instance != null)
        {
            ApplySkillUpgrades(0, data.skill0Upgrades);
            ApplySkillUpgrades(1, data.skill1Upgrades);
            ApplySkillUpgrades(2, data.skill2Upgrades);
            ApplySkillUpgrades(3, data.skill3Upgrades);
        }
        
        // 重置技能冷卻
        if (CooldownSystem.Instance != null)
        {
            CooldownSystem.Instance.ResetAllCooldowns();
        }
        
        Debug.Log("存檔數據已完全應用");
        onGameLoaded.Invoke();
    }
    
    /// <summary>
    /// 應用技能升級
    /// </summary>
    private void ApplySkillUpgrades(int skillIndex, List<string> upgrades)
    {
        if (upgrades == null || upgrades.Count == 0)
            return;
            
        BaseSkill skill = CooldownSystem.Instance.GetSkill(skillIndex);
        
        foreach (string upgradeType in upgrades)
        {
            // 根據升級類型應用升級
            skill = SkillUpgradeManager.Instance.ApplyUpgrade(skill, upgradeType, 0f);
            
            // 記錄已應用的升級
            SkillUpgradeManager.Instance.RecordUpgrade(skillIndex, upgradeType);
        }
        
        // 替換技能
        CooldownSystem.Instance.ReplaceSkill(skillIndex, skill);
    }
    
    /// <summary>
    /// 檢查存檔是否存在
    /// </summary>
    public bool SaveFileExists()
    {
        return File.Exists(saveFilePath);
    }
    
    /// <summary>
    /// 刪除存檔
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            currentSaveData = null;
            Debug.Log("存檔已刪除");
        }
    }
    
    /// <summary>
    /// 獲取當前存檔數據
    /// </summary>
    public SaveData GetCurrentSaveData()
    {
        return currentSaveData;
    }
}

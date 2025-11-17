// Assets/Scripts/Level/LevelManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private string currentLevelName;  // 當前關卡名稱
    [SerializeField] private bool useCircleWipeTransition = true;
    private bool isTransitioning = false;              // 是否正在轉移關卡中
    private Vector3 playerSpawnPosition;               // 玩家在本關卡的生成位置

    // 各系統的參考
    private PersistentStateManager persistentState;    // 持久化狀態管理器（跨關卡數據）
    private EnemyManager enemyManager;                 // 敵人管理器
    private TransitionController transitionController;
    private Transform playerTransform;                 // 玩家位置
    private CharacterController playerCharacterController;  // 玩家角色控制器
    
    private Scene currentLevelScene;                   // 當前關卡場景

    private void Awake()
    {
        // 單例模式：確保全遊戲僅有一個 LevelManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 切換關卡時不銷毀此物件
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        persistentState = PersistentStateManager.Instance;
        transitionController = TransitionController.Instance;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerCharacterController = playerObj.GetComponent<CharacterController>();
        }
    }

    /// <summary>
    /// 轉移至指定關卡。
    /// </summary>
    /// <param name="levelName">目標關卡名稱</param>
    /// <param name="spawnPosition">玩家生成位置</param>
    public void TransitionToLevel(string levelName, Vector3 spawnPosition)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("已在轉移中，無法再次呼叫 TransitionToLevel");
            return;
        }

        if (useCircleWipeTransition && transitionController != null)
        {
            transitionController.PlayCircleWipeTransition(0f);
        }

        StartCoroutine(TransitionCoroutine(levelName, spawnPosition));
    }

    /// <summary>
    /// 關卡轉移的協程。分為多個步驟以確保場景正確卸載與加載。
    /// </summary>
    private IEnumerator TransitionCoroutine(string levelName, Vector3 spawnPosition)
    {
        isTransitioning = true;

        yield return new WaitForSeconds(1f);

        // 第 1 步：卸載前一個關卡
        if (currentLevelScene.IsValid() && currentLevelScene.name != "Bootstrap" && currentLevelScene.name != "Persistent")
        {
            Debug.Log($"卸載關卡: {currentLevelScene.name}");
            yield return SceneManager.UnloadSceneAsync(currentLevelScene);
        }

        // 第 2 步：加載新關卡（以加成模式 Additive 方式）
        Debug.Log($"加載關卡: {levelName}");
        var loadOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        yield return loadOperation;

        // 第 3 步：獲取新加載的場景參考
        currentLevelScene = SceneManager.GetSceneByName(levelName);
        currentLevelName = levelName;

        // 第 4 步：等待場景完全初始化（等待所有 Start() 呼叫完成）
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // 第 5 步：恢復世界狀態（如：開啟的門、啟動的機關等）
        RestoreWorldState();

        // 第 6 步：設定玩家位置（在敵人生成前）
        if (playerTransform != null && playerCharacterController != null)
        {
            playerCharacterController.enabled = false;  // 臨時禁用以允許傳送
            playerTransform.position = spawnPosition;
            playerSpawnPosition = spawnPosition;
            Debug.Log($"玩家已傳送至: {spawnPosition}");
            
            playerCharacterController.enabled = true;  // 重新啟用
        }
        else
        {
            Debug.LogError("找不到玩家或角色控制器！");
        }

        // 第 7 步：觸發完成事件
        SaveLoadManager.onLevelLoaded.Invoke();
        EventManager.TriggerEvent($"OnLevel{levelName}Loaded");

        isTransitioning = false;
        Debug.Log($"關卡轉移完成: {levelName}");
    }

    /// <summary>
    /// 恢復世界狀態（如：開啟的門、完成的謎題等）。
    /// 掃描場景中所有 InteractiveObject 並恢復其狀態。
    /// </summary>
    private void RestoreWorldState()
    {
        InteractiveObject[] interactiveObjects = FindObjectsOfType<InteractiveObject>();
        foreach (InteractiveObject obj in interactiveObjects)
        {
            obj.RestoreState();
        }
        Debug.Log("世界狀態已恢復");
    }

    /// <summary>
    /// 取得當前關卡名稱。
    /// </summary>
    public string GetCurrentLevelName()
    {
        return currentLevelName;
    }

    /// <summary>
    /// 檢查是否正在轉移關卡。
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    /// <summary>
    /// 取得玩家在本關卡的生成位置。
    /// </summary>
    public Vector3 GetPlayerSpawnPosition()
    {
        return playerSpawnPosition;
    }
}

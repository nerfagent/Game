# README

## 目錄結構

為了保持專案清晰且易於擴展，採用以下資料夾分類：

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── EventManager.cs
│   │   ├── InputManager.cs
│   │   ├── GameStarter.cs
│   │   ├── SaveLoadManager.cs
│   │   ├── PauseManager.cs
│   │   └── UIManager.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerMovement.cs
│   │   ├── PlayerCombat.cs
│   │   ├── PlayerState.cs
│   │   ├── PlayerHealth.cs
│   │   └── SlashVFXHandler.cs
│   ├── Skills/
│   │   ├── BaseSkill.cs
│   │   ├── Skill1.cs ~ Skill4.cs
│   │   ├── CooldownSystem.cs
│   │   ├── SkillSystem.cs
│   │   ├── SkillDecorator.cs
│   │   ├── SkillUpgradeManager.cs
│   │   └── Decorators/
│   ├── Enemy/
│   │   ├── BaseEnemy.cs
│   │   ├── EnemyManager.cs
│   │   ├── EnemySpawner.cs
│   │   └── BulletHellEnemy.cs
│   ├── Bullet/
│   │   ├── Bullet.cs
│   │   ├── BulletManager.cs
│   │   ├── IBulletBehavior.cs
│   │   └── Behaviors/
│   │       └── VirtualOrbitBehavior.cs
│   ├── Level/
│   │   ├── LevelManager.cs
│   │   ├── PersistentStateManager.cs
│   │   ├── Checkpoint.cs
│   │   ├── InteractiveObject.cs
│   │   ├── LevelExit.cs
│   │   └── TransitionController.cs
│   ├── UI/
│   │   └── SkillTree.cs
│   └── Data/
│       └── SaveData.cs
├── Scenes/
├── Prefabs/
├── Art/
├── UI/
└── Plugins/
```

---

## Build Settings 配置

**重要：場景順序必須如下設定**

```
Scenes In Build:
Assets/Scenes/Bootstrap.unity      (無需任何指令碼)
Assets/Scenes/Persistent.unity     (包含所有核心管理器)
Assets/Scenes/Level_MainHall.unity (可卸載關卡)
Assets/Scenes/Level_TreasureRoom.unity (可卸載關卡)
... (添加更多關卡)
```

---

## GameManager：單例模式

**責任：** 核心遊戲狀態控制與各管理器協調

**使用方式：**

```csharp
// 獲取單例實例
GameManager manager = GameManager.Instance;

// 查詢當前遊戲狀態
GameManager.GameState state = GameManager.Instance.CurrentState;

// 訂閱遊戲事件
private void OnEnable()
{
GameManager.onGameStart += HandleGameStart;
GameManager.onGameOver += HandleGameOver;
GameManager.onGamePaused += HandleGamePaused;
GameManager.onGameResumed += HandleGameResumed;
}

private void OnDisable()
{
GameManager.onGameStart -= HandleGameStart;
GameManager.onGameOver -= HandleGameOver;
GameManager.onGamePaused -= HandleGamePaused;
GameManager.onGameResumed -= HandleGameResumed;
}

// 開始遊戲
GameManager.Instance.StartGame();

// 暫停遊戲
GameManager.Instance.PauseGame();

// 繼續遊戲
GameManager.Instance.ResumeGame();

// 遊戲結束
GameManager.Instance.GameOver();
```

**核心特性：**
- 使用 `DontDestroyOnLoad()` 確保跨場景持續存在
- 遊戲狀態：`Menu`、`Playing`、`Paused`、`GameOver`
- 公開事件：`onGameStart`、`onGamePaused`、`onGameResumed`、`onGameOver`

---

## EventManager：事件管理系統

**責任：** 系統間通訊的橋梁，採用 Observer 模式解耦

**使用方式：**

```csharp
// 訂閱事件
private void OnEnable()
{
EventManager.StartListening("OnGameStarted", HandleGameStarted);
EventManager.StartListening("OnGameOver", HandleGameOver);
}

// 取消訂閱事件
private void OnDisable()
{
EventManager.StopListening("OnGameStarted", HandleGameStarted);
EventManager.StopListening("OnGameOver", HandleGameOver);
}

// 事件處理函式
private void HandleGameStarted()
{
Debug.Log("遊戲開始！");
}

private void HandleGameOver()
{
Debug.Log("遊戲結束！");
}

// 觸發事件
EventManager.TriggerEvent("OnGameStarted");
EventManager.TriggerEvent("OnGameOver");
```

---

## InputManager：集中式輸入管理

**責任：** 統一管理所有遊戲輸入，便於後續支援控制器或重新綁定

**使用方式：**

```csharp
// 獲取移動輸入
float horizontalInput = InputManager.GetHorizontalInput();
float verticalInput = InputManager.GetVerticalInput();

// 獲取戰鬥輸入
if (InputManager.GetLockOnInput())
{
    // 鎖定敵人
}

if (InputManager.GetDashInput())
{
    // 執行衝刺
}

// 獲取技能輸入
if (InputManager.GetSkill1Input())
{
    // 施放技能1
}

if (InputManager.GetPauseInput())
{
    // 暫停
}
```

---

## SaveLoadManager：存檔與讀檔系統

**責任：** 管理遊戲存檔、讀檔與狀態恢復

**主要方法：**

```csharp
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    // 靜態事件
    public static UnityAction onLevelLoaded;
    public static UnityAction onGameSaved;
    public static UnityAction onGameLoaded;
    
    // 保存遊戲
    public void SaveGame(string checkpointID, string sceneName, Vector3 spawnPosition);
    
    // 讀取遊戲
    public SaveData LoadGame();
    
    // 讀檔並恢復遊戲狀態
    public void LoadGameAndRestore();
    
    // 檢查存檔是否存在
    public bool SaveFileExists();
    
    // 刪除存檔
    public void DeleteSave();
}
```

**存檔數據包含：**
- 檢查點信息（位置、場景、ID）
- 玩家最大生命值
- 技能升級列表
- 持久化狀態（門、機關等）
- 已擊敗的 Boss 列表

**使用方式：**

```csharp
// 訂閱存檔事件
SaveLoadManager.onGameSaved += HandleGameSaved;
SaveLoadManager.onGameLoaded += HandleGameLoaded;
SaveLoadManager.onLevelLoaded += HandleLevelLoaded;

// 在檢查點保存遊戲
SaveLoadManager.Instance.SaveGame("CheckpointA", "Level_MainHall", playerPosition);

// 讀檔
SaveLoadManager.Instance.LoadGameAndRestore();
```

**讀檔流程：**
1. 讀取存檔文件
2. 轉移玩家到檢查點場景
3. 清空當前會話的普通敵人死亡記錄
4. 重新生成當前場景的所有普通敵人
5. 加載持久化狀態與 Boss 擊敗記錄
6. 恢復玩家生命值、技能升級、冷卻狀態

---

## PlayerState：玩家狀態追蹤

**責任：** 追蹤玩家當前的活動狀態（移動、衝刺、施法、眩暈、死亡）

**使用方式：**

```csharp
public class PlayerState : MonoBehaviour
{
    public enum State { Idle, Moving, Dashing, CastingSkill, Stunned, Dead }

    public State CurrentState { get; }
    
    // 靜態事件
    public static UnityAction<State> onPlayerStateChanged;
    
    // 便利屬性
    public bool IsMoving { get; }
    public bool IsDashing { get; }
    public bool IsCasting { get; }
    public bool IsStunned { get; }
    public bool IsDead { get; }
    public bool IsActionLocked { get; } // 檢查是否被鎖定行動
    
    public void SetState(State newState);
    public void ResetToIdle();
}

// 使用
PlayerState playerState = GetComponent<PlayerState>();

// 訂閱狀態變更事件
PlayerState.onPlayerStateChanged += HandleStateChanged;

void HandleStateChanged(PlayerState.State newState)
{
    Debug.Log(\$"玩家狀態變更為: {newState}");
}

if (playerState.IsActionLocked)
{
    // 玩家被鎖定行動，無法移動
    return;
}

playerState.SetState(PlayerState.State.Moving);
```

---

## PlayerMovement：玩家移動控制

**責任：** 處理基於相機的移動輸入與角色控制

**使用方式：**

```csharp
PlayerMovement playerMovement = GetComponent<PlayerMovement>();
playerMovement.ResetVerticalVelocity(); // 在衝刺結束後重置垂直速度
```

---

## PlayerCombat：玩家戰鬥系統

**責任：** 處理鎖定敵人、衝刺攻擊等戰鬥機制

**主要屬性與方法：**

```csharp
public class PlayerCombat : MonoBehaviour
{
    public bool IsLockedOn { get; }

    [Header("Lock-On Settings")]
    public GameObject lockOnIndicatorPrefab;
    public float lockOnRange = 25f;
    public LayerMask enemyLayer;
    
    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashStopDistance = 1f;
    
    [Header("VFX Settings")]
    public GameObject slashVFXPrefab;
}

// 使用
PlayerCombat playerCombat = GetComponent<PlayerCombat>();

if (playerCombat.IsLockedOn)
{
    // 已鎖定敵人
}
```

---

## PlayerHealth：玩家生命值管理

**責任：** 管理玩家生命值、傷害與死亡

**主要方法：**

```csharp
public class PlayerHealth : MonoBehaviour
{
    public int MaxHP { get; }
    public int CurrentHP { get; }

    // 靜態事件
    public static UnityAction OnPlayerDamaged;
    public static UnityAction OnPlayerDied;
    
    // 受傷
    public void TakeDamage(int damage);
    
    // 恢復滿血
    public void RestoreToFull();
    
    // 設置最大生命值（用於存檔恢復）
    public void SetMaxHP(int newMaxHP);
}

// 使用
PlayerHealth.OnPlayerDamaged += HandlePlayerDamaged;
PlayerHealth.OnPlayerDied += HandlePlayerDied;

playerHealth.TakeDamage(10);
playerHealth.RestoreToFull();
```

---

## SlashVFXHandler：斬擊特效與傷害系統

**責任：** 處理玩家衝刺攻擊的特效與敵人傷害

**主要屬性：**

```csharp
public class SlashVFXHandler : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 25;                // 造成的傷害值

    [Header("VFX Settings")]
    public LayerMask enemyLayer;           // 敵人圖層
    public GameObject impactVFXPrefab;     // 衝擊特效
}
```

**核心邏輯：**
- 衝刺結束時生成斬擊特效物件
- 特效碰撞盒與敵人碰撞時，呼叫敵人的 `TakeDamage()` 方法
- 在敵人位置生成衝擊特效
- 特效在粒子系統播完後自動銷毀

**工作流程：**
```
玩家衝刺結束
    ↓
生成 SlashVFX
    ↓
特效的觸發碰撞盒進入敵人
    ↓
敵人.TakeDamage(damage)
    ↓
生成衝擊特效
    ↓
特效自動銷毀
```

---

## 敵人系統（Enemy System）

### 敵人系統架構概述

敵人系統由四個主要元件組成：

1. **BaseEnemy** - 敵人基類，定義所有敵人的核心邏輯
2. **BulletHellEnemy** 及其他派生類 - 具體敵人類型實現
3. **EnemyManager** - 敵人生成與狀態管理（決策中樞）
4. **EnemySpawner** - 單個生成點的敵人生成執行器

### 敵人狀態機

每個敵人擁有四個狀態：

| 狀態 | 描述 | 觸發條件 |
|------|------|---------| 
| **Idle** | 待機狀態，不執行任何行動 | 敵人初始化或失去玩家視線 |
| **Attacking** | 攻擊狀態，執行符卡（彈幕）模式 | 看到玩家進入視野 |
| **Evading** | 走位狀態，遠離玩家並隨機移動 | 符卡攻擊完成後 |
| **Dead** | 死亡狀態，停止所有行動 | 敵人生命值歸零 |

**狀態轉移流程：**
```
Idle → (看到玩家) → Attacking → (符卡完成) → Evading → (走位時長到期) → Attacking
       ↑ (失去視線)                    ↑ (失去視線)
       └─────────────────────────────┘

Dead: 任何狀態都可能轉移至此（當生命值 ≤ 0）
```

---

### BaseEnemy：敵人基類

**責任：** 定義所有敵人的通用邏輯（狀態機、視野檢測、走位、傷害系統）

**主要屬性：**

```csharp
public abstract class BaseEnemy : MonoBehaviour
{
    // 靜態事件
    public static UnityAction OnEnemyDefeated;

    [Header("基礎屬性")]
    public float maxHP = 50f;                  // 最大生命值
    public float CurrentHP { get; }            // 當前生命值
    
    [Header("ID 與重生")]
    [SerializeField] protected string uniqueID;  // 敵人的全局唯一 ID
    [SerializeField] protected bool shouldRespawn = true;  // true = 普通敵人, false = Boss
    
    [Header("移動")]
    public float moveSpeed = 5f;               // 移動速度
    public float stoppingDistance = 2f;        // 停止距離（攻擊範圍）
    
    [Header("偵測")]
    public float sightRange = 20f;             // 視野範圍
    public float fieldOfViewAngle = 90f;       // 視野角度
    public LayerMask playerLayer;              // 玩家圖層
    public LayerMask wallLayer;                // 普通牆壁圖層
    public LayerMask invisibleWallLayer;       // 隱形牆壁圖層
    
    [Header("走位逃亡")]
    public float evadeRangeMin = 10f;          // 最小走位距離
    public float evadeRangeMax = 20f;          // 最大走位距離
    public float evadeDurationMin = 2f;        // 走位時長最小值
    public float evadeDurationMax = 5f;        // 走位時長最大值
    
    // 公開屬性
    public float CurrentHP { get; }
    public float MaxHP { get; }
    public bool IsDead { get; }
    public bool ShouldRespawn { get; }
    public string UniqueID { get; }
}
```

**核心特性：**

**唯一 ID 系統：**
- 每個敵人都必須有一個在整個遊戲中唯一的 ID（例如 "MainHall_Grunt_01"）
- ID 用於追蹤敵人的死亡狀態
- 由 `EnemySpawner` 在生成時分配

**死亡與存檔系統：**
```
普通敵人死亡：
    └─ 記錄到 EnemyManager 的會話死亡列表（不寫入存檔）
    └─ 場景切換時保持死亡狀態
    └─ 檢查點休息時復活
    └─ 讀檔時列表清空，重新生成

Boss 死亡：
    └─ 記錄到 PersistentStateManager（寫入存檔）
    └─ 場景切換時保持死亡狀態
    └─ 檢查點休息時不復活
    └─ 讀檔後永久保持死亡
```

**主要方法：**

```csharp
// 核心系統
protected virtual void CheckForPlayer();      // 檢查玩家是否在視野內
protected virtual void UpdateState();         // 狀態機更新
protected virtual void ApplyGravity();        // 套用重力

// 攻擊相關
protected virtual void ExecuteSpellCard();    // 派生類實作具體攻擊邏輯
protected virtual bool IsSpellCardFinished(); // 判斷符卡是否完成
protected virtual void OnAttackEnd();         // 攻擊結束時重置狀態

// 走位相關
protected virtual void PrepareEvadePhase();        // 準備走位階段
protected virtual void ChooseNewEvadeDirection();  // 選擇新走位方向
protected virtual void UpdateEvadePhase();         // 更新走位邏輯

// 生命週期
public virtual void TakeDamage(float damage);  // 受傷扣減血量
protected virtual void Die();                  // 死亡
public virtual void ResetEnemy();              // 重置敵人（重生時調用）

// 分配 ID（由 EnemySpawner 調用）
public void AssignUniqueID(string id);
```

**視野系統：**
- 使用距離、角度與射線檢測三層驗證
- 支援視野遮擋（普通牆或隱形牆可阻擋視線）
- 玩家必須同時滿足：距離 < sightRange、角度 < fieldOfViewAngle/2、射線不被遮擋

**走位邏輯：**
- 隨機選擇一個 N~M 秒的走位時長
- 每秒根據逐漸升高的概率改變方向（初期 10%，後期最高 80%）
- 遇到牆壁或隱形牆時立即改變方向
- 走位完成後回到攻擊狀態

**使用方式：**

```csharp
// 訂閱敵人擊敗事件
BaseEnemy.OnEnemyDefeated += HandleEnemyDefeated;

// 派生類實例
public class BulletHellEnemy : BaseEnemy
{
    protected override void ExecuteSpellCard()
    {
        // 在此實作具體的彈幕發射邏輯
    }

    protected override bool IsSpellCardFinished()
    {
        return bulletsFired >= maxBullets;
    }
}

// 外部傷害敵人
enemy.TakeDamage(10f);
```

---

### EnemyManager：全局敵人管理

**責任：** 敵人生成決策者，管理敵人生命週期、追蹤 Boss、協調普通敵人重生

**設計理念：**
- `EnemyManager` 是**決策中樞**，決定敵人何時應該生成
- `EnemySpawner` 是**執行器**，執行生成命令
- 敵人不再立即生成，而是在詢問 `EnemyManager` 後才生成

**主要方法：**

```csharp
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; }

    // Spawner 註冊/反註冊
    public void RegisterSpawner(EnemySpawner spawner);        // 在 Spawner.Start() 中調用
    public void UnregisterSpawner(EnemySpawner spawner);      // 在 Spawner.OnDestroy() 中調用
    
    // 敵人生成決策
    public void ProcessSpawnsForLevel(string sceneName);      // 決定場景中哪些敵人應該生成
    
    // 普通敵人生命週期管理
    public void RecordSessionEnemyDeath(string enemyID);      // 記錄普通敵人在會話中的死亡
    public void ClearSessionEnemyDeaths();                    // 清空會話死亡列表（讀檔時調用）
    
    // Boss 管理
    public void RecordBossDefeated(string bossID);            // 記錄已擊敗的 Boss
    public void LoadDefeatedBosses(List<string> bossList);    // 從存檔加載已擊敗的 Boss
    public List<string> GetDefeatedBossList();                // 獲取已擊敗的 Boss 列表（用於存檔）
}
```

**敵人分類與重生規則：**

| 敵人類型 | shouldRespawn | 會話死亡記錄 | 永久死亡記錄 | 場景切換後 | 檢查點後 | 讀檔後 |
|---------|---|---|---|---|---|---|
| **普通敵人** | true | ✓ (HashSet) | ✗ | 保持死亡 | 復活 | 重新生成 |
| **Boss** | false | ✗ | ✓ (PersistentState) | 保持死亡 | 不復活 | 永遠死亡 |

**工作流程：**

```
EnemySpawner.Start()
    ↓
呼叫 EnemyManager.RegisterSpawner(this)
    ↓
EnemyManager 將 Spawner 按場景分類存儲
    ↓
LevelManager 加載完場景，觸發 SaveLoadManager.onLevelLoaded 事件
    ↓
EnemyManager.ProcessSpawnsForLevel()
    ├─ 遍歷該場景的所有已註冊 Spawner
    ├─ 檢查敵人是否已死亡
    │   ├─ 普通敵人：檢查會話列表
    │   └─ Boss：檢查 PersistentStateManager
    ├─ 未死亡 → 命令 Spawner.SpawnEnemy()
    └─ 已死亡 → 不生成

敵人被擊敗
    ↓
敵人.Die()
    ├─ 普通敵人：EnemyManager.RecordSessionEnemyDeath()
    └─ Boss：PersistentStateManager + EnemyManager.RecordBossDefeated()

玩家觸發檢查點
    ↓
Checkpoint.ActivateCheckpoint()
    ↓
Checkpoint.onCheckpointRest.Invoke()
    ↓
EnemyManager.OnCheckpointRest()
    ├─ 遍歷當前場景的 Spawner
    ├─ 若是普通敵人且在會話列表中
    │   ├─ 從列表移除
    │   └─ 呼叫 Spawner.SpawnEnemy() 重生
    └─ Boss 不重生

玩家讀檔
    ↓
SaveLoadManager.ApplySaveData()
    ├─ EnemyManager.ClearSessionEnemyDeaths() 清空會話列表
    ├─ EnemyManager.LoadDefeatedBosses() 加載已擊敗 Boss
    └─ EnemyManager.ProcessSpawnsForLevel() 重新處理敵人生成
        └─ 當前場景所有普通敵人重新生成
```

**關鍵特性：**

**三個主要需求的實現：**

1. ✓ 玩家在 Scene1 殺了普通敵人 A，切換到 Scene2 再回到 Scene1 → 敵人 A 不會生成
   ```
   原因：敵人 ID 在會話列表中被標記為死亡，除非觸發檢查點或讀檔
   ```

2. ✓ 玩家在 Scene1 沒殺普通敵人 B，切換到 Scene2 再回到 Scene1 → 敵人 B 會生成
   ```
   原因：敵人 ID 不在會話列表中，ProcessSpawnsForLevel() 將其生成
   ```

3. ✓ 玩家在 Scene1 殺了普通敵人 A，觸發檢查點 → 敵人 A 復活
   ```
   原因：OnCheckpointRest() 將敵人 ID 從會話列表移除並重生
   ```

**使用方式：**

```csharp
// 不需要外部直接呼叫大多數方法
// EnemyManager 自動監聽 SaveLoadManager.onLevelLoaded 和 Checkpoint.onCheckpointRest 事件

// 如果需要手動查詢已擊敗的 Boss（存檔系統調用）
List<string> defeatedBosses = EnemyManager.Instance.GetDefeatedBossList();
```

---

### EnemySpawner：敵人生成執行器

**責任：** 執行生成命令，不再主動決策

**主要屬性：**

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private BaseEnemy enemyPrefab;
    [Tooltip("此 Spawner 生成的敵人的全遊戲唯一 ID。必須手動設置！")]
    [SerializeField] private string enemyUniqueID;
    
    // 公開屬性供 EnemyManager 查詢
    public string UniqueID { get; }           // 敵人唯一 ID
    public bool IsNormalEnemy { get; }        // 是否為普通敵人（非 Boss）
    public string SceneName { get; }          // Spawner 所在場景
}
```

**核心改變：**

1. **不再主動生成** - `Start()` 時不再立即生成敵人，而是向 `EnemyManager` 註冊自己
2. **被動執行** - 只有當 `EnemyManager` 呼叫 `SpawnEnemy()` 時才生成
3. **重複使用** - 若敵人已生成但在會話中死亡，`SpawnEnemy()` 會重新啟用並重置它

**主要方法：**

```csharp
public class EnemySpawner : MonoBehaviour
{
    // 由 EnemyManager 呼叫以執行生成
    public void SpawnEnemy();
}
```

**工作流程：**

```
Spawner.Start()
    ↓
檢查 enemyUniqueID 是否為空（若為空則禁用腳本）
    ↓
呼叫 EnemyManager.RegisterSpawner(this)
    ↓
Spawner 進入待命狀態，等待 EnemyManager 指令

--- 稍後 ---

EnemyManager.ProcessSpawnsForLevel() 決定應生成此敵人
    ↓
呼叫 spawner.SpawnEnemy()
    ├─ 若敵人未生成：Instantiate() 創建新敵人
    ├─ 若敵人已生成：ResetEnemy() + SetActive(true)
    └─ 敵人進入場景
```

**重要配置：**

在編輯器中，每個 `EnemySpawner` 必須手動設置唯一的 `Enemy Unique ID`：

```
示例命名規則：
{LevelName}_{EnemyType}_{Number}

"MainHall_Grunt_01"
"MainHall_Grunt_02"
"MainHall_Boss_Knight"
"TreasureRoom_Goblin_01"
"CastleTop_Boss_Dragon"
```

**Gizmo 可視化：**
```
紅色球體：生成點位置
Inspector 顯示：敵人的 Unique ID
```

---

### 敵人死亡與重生完整邏輯

**普通敵人的完整生命週期：**

```
1. 初始狀態
   Spawner 向 EnemyManager 註冊
   敵人在場景中活動

2. 敵人被擊敗
   enemy.TakeDamage(damage)
   敵人.Die()
   EnemyManager.RecordSessionEnemyDeath(敵人ID)
   敵人停用 (SetActive(false))

3. 場景切換
   場景卸載時敵人對象仍保留
   Spawner 記得敵人引用
   新場景加載，敵人 ID 仍在會話列表中

4. 返回舊場景
   ProcessSpawnsForLevel() 檢查敵人 ID
   ID 在會話列表中 → 不生成
   Spawner 中的敵人仍停用

5. 觸發檢查點
   OnCheckpointRest() 檢查敵人 ID
   ID 在會話列表中 → 移除 + SpawnEnemy()
   敵人復活、重置狀態、重新啟用

6. 讀檔
   ClearSessionEnemyDeaths() 清空列表
   LoadDefeatedBosses() 加載 Boss 狀態
   ProcessSpawnsForLevel() 重新處理
   當前場景所有普通敵人重新生成
```

**Boss 的完整生命週期：**

```
1. 初始狀態
   Spawner 向 EnemyManager 註冊
   Boss 在場景中活動

2. Boss 被擊敗
   boss.TakeDamage(damage)
   boss.Die()
   PersistentStateManager.SetBoolState("BossDefeated_" + ID, true)
   EnemyManager.RecordBossDefeated(bossID)
   Boss 停用 (SetActive(false))

3. 場景切換 → 返回
   ProcessSpawnsForLevel() 檢查 PersistentStateManager
   狀態為 true → 不生成
   Boss 永遠停用

4. 觸發檢查點
   Boss 不復活（只有普通敵人復活）

5. 讀檔
   LoadDefeatedBosses() 從存檔加載已擊敗 Boss 列表
   PersistentStateManager 更新狀態
   ProcessSpawnsForLevel() 檢查
   已擊敗的 Boss 仍不生成
```

---

## 子彈系統（Bullet System）

### 子彈系統架構概述

子彈系統採用**組件化設計**，使用行為(Behavior)模式使子彈支援複雜的彈幕效果。

主要元件：

1. **Bullet** - 子彈核心類，管理行為與生命週期
2. **IBulletBehavior** - 行為介面，定義子彈的運動方式
3. **VirtualOrbitBehavior** - 環繞旋轉行為範例
4. **BulletManager** - 子彈生成與管理

---

### Bullet：子彈核心

**責任：** 管理子彈的物理、生命週期和行為系統

**主要屬性與方法：**

```csharp
public class Bullet : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;  // 子彈速度向量
    public float lifetime = 5f;    // 子彈最大生存時間
    
    public void AddBehavior(IBulletBehavior behavior);
    public void RemoveBehavior(IBulletBehavior behavior);
    public void ClearBehaviors();
}
```

---

### IBulletBehavior：行為介面

**責任：** 定義子彈的運動邏輯

**介面定義：**

```csharp
public interface IBulletBehavior
{
    bool Update(Bullet bullet, float deltaTime);  // 返回 false 表示行為完成
    void Initialize(Bullet bullet);
    void OnBehaviorEnd(Bullet bullet);
}
```

---

### VirtualOrbitBehavior：環繞旋轉行為

**責任：** 實現子彈圍繞虛擬中心點旋轉並擴散的運動

**使用範例：**

```csharp
var orbitBehavior = new VirtualOrbitBehavior(
    startCenterPos: enemyPosition,
    centerMoveVelocity: directionToPlayer * 20f,
    initialRadius: 1f,
    radiusGrowth: 12f,
    rotSpeed: 100f
);

BulletManager.Instance.SpawnBullet(enemyPosition, orbitBehavior);
```

---

### BulletManager：子彈生成管理

**責任：** 統一管理子彈生成與預製物

**主要方法：**

```csharp
public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; }
    
    public Bullet SpawnBullet(Vector3 position, params IBulletBehavior[] behaviors);
}
```

---

## 關卡與場景系統

### 場景架構

```
Bootstrap (Index 0) - 永不卸載，空場景
    ↓
Persistent (Index 1) - 遊戲進行中持久化
    ├─ GameManager
    ├─ PersistentStateManager
    ├─ LevelManager
    ├─ EnemyManager
    ├─ BulletManager
    ├─ Player (玩家)
    └─ Camera (鏡頭)
        ↓
        ↓ (LevelManager 控制)
        ↓
Level1 (Index 2) - 可卸載
Level2 (Index 3) - 可卸載
... (更多關卡)
```

---

### PersistentStateManager：永久世界狀態

**責任：** 追蹤整個遊戲世界的永久改變

```csharp
public class PersistentStateManager : MonoBehaviour
{
    // 靜態事件
    public static UnityAction<string> onStateChanged;

    // 布林狀態（門是否開啟）
    public void SetBoolState(string stateKey, bool value);
    public bool GetBoolState(string stateKey, bool defaultValue = false);
    
    // 整數狀態（敵人數量）
    public void SetIntState(string stateKey, int value);
    public int GetIntState(string stateKey, int defaultValue = 0);
    public void IncrementIntState(string stateKey, int increment = 1);
    
    // 浮點數和字串狀態類似
    public void SetFloatState(string stateKey, float value);
    public float GetFloatState(string stateKey, float defaultValue = 0f);
    public void SetStringState(string stateKey, string value);
    public string GetStringState(string stateKey, string defaultValue = "");
}

// 使用
PersistentStateManager.onStateChanged += HandleStateChanged;

void HandleStateChanged(string stateKey)
{
    Debug.Log(\$"狀態變更: {stateKey}");
}

PersistentStateManager.Instance.SetBoolState("Door_MainHall_Opened", true);
bool isOpen = PersistentStateManager.Instance.GetBoolState("Door_MainHall_Opened");
```

**狀態鍵命名約定：**
```
{ObjectType}_{LevelName}_{ObjectName}_{Property}

例如：
"Door_MainHall_GoldenGate_Opened"
"Puzzle_TreasureRoom_LeverA_Activated"
"BossDefeated_CastleTop_Dragon"
```

---

### LevelManager：場景與關卡控制

**責任：** 管理場景加載/卸載、玩家位置、敵人重生協調

```csharp
public class LevelManager : MonoBehaviour
{
    // 靜態事件
    public static UnityAction<string> onLevelLoaded;

    // 轉移至新關卡
    public void TransitionToLevel(string levelName, Vector3 spawnPosition);
    
    // 查詢當前關卡
    public string GetCurrentLevelName();
    
    // 檢查是否正在轉換
    public bool IsTransitioning();
}

// 使用
LevelManager.onLevelLoaded += HandleLevelLoaded;

void HandleLevelLoaded(string levelName)
{
    bug.Log(\$"關卡已加載: {levelName}");
}

Vector3 spawnPosition = new Vector3(5, 1, 0);
LevelManager.Instance.TransitionToLevel("Level_TreasureRoom", spawnPosition);
```

**場景轉換流程：**

```
玩家進入出口
    ↓
播放圓形擦拭過渡動畫
    ↓
卸載前一個關卡
    ↓
加載新關卡（Additive 模式）
    ↓
等待場景完全初始化
    ↓
恢復永久狀態（已開啟的門等）
    ↓
禁用 CharacterController → 設定玩家位置 → 重新啟用
    ↓
觸發 SaveLoadManager.onLevelLoaded 和 LevelManager.onLevelLoaded 事件
    ↓
EnemyManager 自動監聽並調用 ProcessSpawnsForLevel()
    ├─ 根據死亡記錄決定敵人是否生成
    └─ 普通敵人重新生成（除已死亡者外）
```

---

### Checkpoint：檢查點系統

**責任：** 提供安全點存檔與敵人重生管理

**主要方法：**

```csharp
public class Checkpoint : MonoBehaviour
{
    // 靜態事件
    public static UnityAction onCheckpointRest;

    [SerializeField] private string checkpointID;
    [SerializeField] private string checkpointSceneName;
    [SerializeField] private Vector3 playerSpawnPosition;
}

// 使用
Checkpoint.onCheckpointRest += HandleCheckpointRest;

// 玩家按 F 鍵激活檢查點
```

**檢查點激活流程：**

```
玩家按 F 鍵
    ↓
檢查點.ActivateCheckpoint()
    ├─ 補滿玩家血量
    ├─ 重置技能冷卻
    ├─ 保存遊戲 (SaveLoadManager.SaveGame())
    └─ 觸發 Checkpoint.onCheckpointRest 事件
        ↓
        EnemyManager.OnCheckpointRest()
        ├─ 遍歷當前場景的 Spawner
        ├─ 普通敵人重生
        └─ Boss 不重生
```

---

### InteractiveObject：可交互物件基類

**責任：** 管理改變永久狀態的物件

```csharp
public abstract class InteractiveObject : MonoBehaviour
{
    // 實例事件
    public UnityAction onObjectActivated;
    public UnityAction onObjectDeactivated;

    protected string stateKey;
    
    public virtual void Activate();
    public virtual void Deactivate();
    public virtual void RestoreState();
    protected abstract void OnStateChanged(bool isActive);
}

// 使用
interactiveObject.onObjectActivated += HandleObjectActivated;
interactiveObject.onObjectDeactivated += HandleObjectDeactivated;
```

---

### TransitionController：場景過渡效果

**責任：** 管理圓形擦拭過渡動畫（基於 Easy Transitions）

```csharp
public void PlayCircleWipeTransition(float delay = 0f);
// 以玩家為中心執行圓形擦拭過渡
```

---

### GameStarter：遊戲啟動腳本

**責任：** Bootstrap 場景中加載 Persistent 和第一個關卡

```csharp
public class GameStarter : MonoBehaviour
{
    public string persistentSceneName = "Persistent";
    public string firstLevelName = "Level_MainHall";
    public Vector3 firstLevelSpawnPosition = Vector3.zero;
}

// 使用方式：
// 1. 在 Bootstrap 場景中創建空 GameObject
// 2. 附加 GameStarter 指令碼
// 3. 在 Inspector 設定關卡名稱和生成位置
// 4. 播放遊戲
```

---

## 技能系統（Skill System）

### 技能系統架構概述

技能系統由四個主要元件組成：

1. **BaseSkill** - 技能基底類別，定義所有技能的核心邏輯
2. **Skill1、Skill2、Skill3、Skill4** - 具體技能實現
3. **CooldownSystem** - 冷卻池管理系統
4. **SkillSystem** - 高層技能管理與輸入協調

### 技能冷卻池系統

技能使用**池模型**而非傳統冷卻系統。此模型允許玩家連續施放技能直到池耗盡，然後隨時間恢復。

**池模型範例：**
- 最大冷卻池 = 40
- 每次施放消耗 = 10
- 可連續施放 = 4 次
- 每秒恢復 = 1 點
- 完全恢復時間 = 40 秒

```
時間線：
t=0s:   施放1 (池: 40 → 30)
t=0.1s: 施放2 (池: 30 → 20)
t=0.2s: 施放3 (池: 20 → 10)
t=0.3s: 施放4 (池: 10 → 0) [無法施放]
t=0.3~10.3s: 恢復階段 (池: 0 → 10，可再施放1次)
```

---

### BaseSkill：技能基類

**責任：** 定義所有技能的通用邏輯

**主要屬性：**

```csharp
public abstract class BaseSkill
{
    // 靜態事件
    public static UnityAction<int> onSkillCast;

    // 技能標識
    public int SkillId { get; }
    public string SkillName { get; }
    
    // 冷卻池配置（所有屬性均為 virtual，支援 Decorator 覆寫）
    public virtual float MaxCooldownPool { get; }
    public virtual float CurrentCooldownPool { get; }
    public virtual float CooldownCostPerCast { get; }
    public virtual float CooldownRegenRate { get; }
    
    // 施放配置
    public virtual float CastTime { get; }
    public virtual float BaseDamage { get; }
    
    // 狀態查詢
    public virtual bool IsCasting { get; }
    public virtual bool IsReady { get; }
    public virtual int AvailableCasts { get; }
    
    public virtual void Cast(Transform casterTransform);
    public virtual void UpdateSkill();
    public virtual void OnCastComplete();
    public virtual void ResetCooldown();
    public float GetPoolPercentage();
    }

// 使用
BaseSkill.onSkillCast += HandleSkillCast;

void HandleSkillCast(int skillId)
{
    Debug.Log(\$"技能 {skillId} 已施放");
}
```

---

### 技能升級系統（Decorator 模式）

使用 Decorator 設計模式實現技能升級。

```csharp
// 增加傷害
BaseSkill upgradedSkill = new DamageUpgradeDecorator(originalSkill, 5f);

// 增加冷卻池
upgradedSkill = new MaxPoolUpgradeDecorator(upgradedSkill, 10f);

// 添加追蹤功能
upgradedSkill = new FireballTrackingDecorator(upgradedSkill);

// 替換系統中的技能
CooldownSystem.Instance.ReplaceSkill(0, upgradedSkill);
```

---

### CooldownSystem：冷卻池管理

**責任：** 管理四個技能的冷卻池

```csharp
public class CooldownSystem : MonoBehaviour
{
    public static CooldownSystem Instance { get; }

    // 靜態事件
    public static UnityAction<int> onSkillReady;
    public static UnityAction<int> onSkillOnCooldown;
    public static UnityAction<int> onSkillCooldownUpdated;
    
    public bool CastSkill(int skillIndex, Transform casterTransform);
    public float GetCooldownNormalized(int skillIndex);
    public float GetCooldownRemaining(int skillIndex);
    public int GetAvailableCasts(int skillIndex);
    public void ResetAllCooldowns();
    public BaseSkill GetSkill(int skillIndex);
    public void ReplaceSkill(int skillIndex, BaseSkill newSkill);
}

// 使用
CooldownSystem.onSkillReady += HandleSkillReady;
CooldownSystem.onSkillOnCooldown += HandleSkillOnCooldown;
CooldownSystem.onSkillCooldownUpdated += HandleSkillCooldownUpdated;

void HandleSkillReady(int skillId)
{
    bug.Log(\$"技能 {skillId} 已就緒");
}
```

---

### SkillSystem：技能高層管理

**責任：** 協調技能施放、與玩家狀態互動

```csharp
public class SkillSystem : MonoBehaviour
{
    lic static UnityAction[] onSkillCastComplete;

    public bool AttemptCastSkill(int skillIndex);
    public float GetSkillCooldownNormalized(int skillIndex);
    public int GetSkillAvailableCasts(int skillIndex);
    public bool IsSkillReady(int skillIndex);
}

// 使用
SkillSystem.onSkillCastComplete += HandleSkill0CastComplete;

void HandleSkill0CastComplete()
{
    Debug.Log("技能 0 施放完成");
}
```

---

## PauseManager：遊戲暫停系統

**責任：** 管理遊戲時間暫停/繼續

```csharp
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; }
    
    public bool IsPaused { get; }
    
    public void Pause();
    public void Resume();
}

// 使用
if (PauseManager.Instance.IsPaused)
{
    // 顯示暫停菜單
}
```

**核心特性：**
- ESC 鍵自動切換暫停狀態
- `Time.timeScale` 控制遊戲時間
- 訂閱 `GameManager.onGamePaused` 和 `GameManager.onGameResumed` 事件
- 與 `GameManager` 集成

---
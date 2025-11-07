# Phase 1：核心基礎架構實作指南

## 目錄結構

為了保持專案清晰且易於擴展，採用以下資料夾分類：

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   ├── EventManager.cs
│   │   ├── InputManager.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerMovement.cs
│   │   ├── PlayerCombat.cs
│   │   ├── PlayerState.cs
│   ├── Data/
│   │   ├── PlayerStats.cs
│   │   ├── PersistentState.cs
│   │   ├── SaveData.cs
├── Scenes/
├── Prefabs/
├── Art/
├── UI/
├── Data/
└── Plugins/
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
- 發送事件：`OnGameStarted`、`OnGamePaused`、`OnGameResumed`、`OnGameOver`

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

**常見事件列表：**
- `OnGameStarted` - 遊戲開始
- `OnGamePaused` - 遊戲暫停
- `OnGameResumed` - 遊戲繼續
- `OnGameOver` - 遊戲結束
- `OnPlayerStateMoving` - 玩家移動
- `OnEnemyLocked` - 鎖定敵人
- `OnLockCleared` - 解除鎖定
- `OnDashStarted` - 開始衝刺
- `OnDashEnded` - 衝刺結束

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

// 獲取技能輸入（Phase 3 使用）
if (InputManager.GetSkill1Input())
{
    // 施放技能1
}

if (InputManager.GetSkill2Input())
{
    // 施放技能2
}

if (InputManager.GetSkill3Input())
{
    // 施放技能3
}

if (InputManager.GetSkill4Input())
{
    // 施放技能4
}
```

**支援的輸入方法：**
- `GetHorizontalInput()` - 水平移動軸
- `GetVerticalInput()` - 垂直移動軸
- `GetLockOnInput()` - 鎖定敵人（Shift）
- `GetDashInput()` - 衝刺（Z）
- `GetSkill1Input()` 至 `GetSkill4Input()` - 技能施放（Q/W/E/R）

---

## 資料模型（Data Models）

### PlayerStats：玩家狀態

**責任：** 存儲玩家的可變性狀態資料

**使用方式：**

```csharp
[System.Serializable]
public class PlayerStats
{
    public int MaxHP = 100;
    public int CurrentHP = 100;
    
    public int MaxMana = 100;
    public int CurrentMana = 100;
    
    public int Level = 1;
    public int Experience = 0;
}

// 使用
PlayerStats stats = new PlayerStats();
stats.CurrentHP -= 10;
```

---

### PersistentState：世界永久狀態

**責任：** 追蹤遊戲世界的永久改變（已開啟的門、已擊敗的Boss等）

**使用方式：**

```csharp
[System.Serializable]
public class PersistentState
{
    public List<string> completedQuests = new List<string>();
    public List<string> openedDoors = new List<string>();
    public List<string> defeatedBosses = new List<string>();
}

// 使用
PersistentState state = new PersistentState();
state.openedDoors.Add("DoorA");
state.defeatedBosses.Add("Boss_Dragon");
```

---

### SaveData：存檔資料

**責任：** 整合玩家狀態與世界狀態，用於序列化存取

**使用方式：**

```csharp
[System.Serializable]
public class SaveData
{
    public PlayerStats playerStats;
    public PersistentState persistentState;
    public Vector3 playerPosition;
    public int slotNumber;
}

// 使用
SaveData saveData = new SaveData();
saveData.playerStats = new PlayerStats();
saveData.persistentState = new PersistentState();
```

---

## PlayerState：玩家狀態追蹤

**責任：** 追蹤玩家當前的活動狀態（移動、衝刺、施法、眩暈、死亡）

**使用方式：**

```csharp
public class PlayerState : MonoBehaviour
{
    public enum State { Idle, Moving, Dashing, CastingSkill, Stunned, Dead }
    
    public State CurrentState { get; }
    
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

if (playerState.IsActionLocked)
{
    // 玩家被鎖定行動，無法移動
    return;
}

playerState.SetState(PlayerState.State.Moving);

if (playerState.IsDashing)
{
    // 執行衝刺邏輯
}
```

---

## PlayerMovement：玩家移動控制

**責任：** 處理基於相機的移動輸入與角色控制

**主要方法：**

```csharp
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 30f;
    public float gravity = 160f;
    public float rotationSpeed = 10f;
    public float ledgeDetectHeight = 10f;
    public float ledgeDetectDistance = 0.05f;
    
    public void ResetVerticalVelocity();
}

// 使用
PlayerMovement playerMovement = GetComponent<PlayerMovement>();
playerMovement.ResetVerticalVelocity(); // 在衝刺結束後重置垂直速度
```

**整合方式：**
- 使用 `InputManager` 獲取移動輸入
- 查詢 `PlayerState.IsActionLocked` 判斷是否能移動
- 訂閱 `EventManager` 事件監聽遊戲狀態變化

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

**觸發的事件：**
- `OnEnemyLocked` - 成功鎖定敵人
- `OnLockCleared` - 解除鎖定
- `OnDashStarted` - 衝刺開始
- `OnDashEnded` - 衝刺結束

---

## PlayerController：玩家高層協調器

**責任：** 協調 PlayerMovement、PlayerCombat、PlayerState 之間的互動

**使用方式：**

```csharp
public class PlayerController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerState playerState;
}

// PlayerController 自動訂閱遊戲事件，當遊戲結束或暫停時自動禁用玩家系統
```

**自動處理的事件：**
- `OnGameOver` - 禁用玩家、設定狀態為 `Dead`
- `OnGamePaused` - 禁用玩家移動與戰鬥系統
- `OnGameResumed` - 啟用玩家系統

---
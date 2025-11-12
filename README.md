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
│   │   ├── PauseManager.cs
│   │   ├── UIManager.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerMovement.cs
│   │   ├── PlayerCombat.cs
│   │   ├── PlayerState.cs
│   │   ├── PlayerTeleport.cs (會在之後成為一個skill)
│   ├── Skills/
│   │   ├── BaseSkill.cs
│   │   ├── Skill1.cs
│   │   ├── Skill2.cs
│   │   ├── Skill3.cs
│   │   ├── Skill4.cs
│   │   ├── CooldownSystem.cs
│   │   ├── SkillSystem.cs
│   ├── Enemy/
│   │   ├── BaseEnemy.cs
│   │   ├── EnemyManager.cs
│   │   ├── EnemySpawner.cs
│   │   ├── BulletHellEnemy.cs
│   ├── Bullet/
│   │   ├── Bullet.cs
│   │   ├── BulletManager.cs
│   │   ├── IBulletBehavior.cs
│   │   ├── Behaviors/
│   │   │   ├── VirtualOrbitBehavior.cs
│   │   │   └── (其他行為類別)
│   ├── Level/
│   │   ├── LevelManager.cs
│   │   ├── PersistentStateManager.cs
│   │   ├── InteractiveObject.cs
│   │   ├── LevelExit.cs
│   │   ├── TransitionController.cs
│   ├── Data/
│   │   ├── PlayerStats.cs
│   │   ├── PersistentState.cs
│   │   ├── SaveData.cs
│   ├── UI/
│   │   ├── SkillUI.cs
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
[0] Assets/Scenes/Bootstrap.unity      (無需任何指令碼)
[1] Assets/Scenes/Persistent.unity     (包含所有核心管理器)
[2] Assets/Scenes/Level_MainHall.unity (可卸載關卡)
[3] Assets/Scenes/Level_TreasureRoom.unity (可卸載關卡)
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

**遊戲狀態事件：**
- `OnGameStarted` - 遊戲開始
- `OnGamePaused` - 遊戲暫停
- `OnGameResumed` - 遊戲繼續
- `OnGameOver` - 遊戲結束

**玩家移動事件：**
- `OnPlayerStateMoving` - 玩家移動
- `OnPlayerStateIdle` - 玩家待機

**戰鬥事件：**
- `OnEnemyLocked` - 鎖定敵人
- `OnLockCleared` - 解除鎖定
- `OnDashStarted` - 開始衝刺
- `OnDashEnded` - 衝刺結束

**技能事件（Phase 3）：**
- `OnSkill{skillIndex}Cast` - 技能開始施放（例如 `OnSkill0Cast`）
- `OnSkill{skillIndex}CastComplete` - 技能施放完成
- `OnSkill{skillIndex}Ready` - 技能冷卻完成，可以施放
- `OnSkill{skillIndex}OnCooldown` - 技能進入冷卻
- `OnSkill{skillIndex}CooldownUpdated` - 技能冷卻更新（每幀）

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

if (InputManager.GetPauseInput())
{
    // 暫停
}
```

**支援的輸入方法：**
- `GetHorizontalInput()` - 水平移動軸
- `GetVerticalInput()` - 垂直移動軸
- `GetLockOnInput()` - 鎖定敵人（Shift）
- `GetDashInput()` - 衝刺（Z）
- `GetSkill1Input()` 至 `GetSkill4Input()` - 技能施放（Q/W/E/R）
- `GetPauseInput()` - 暫停

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
    // 技能標識
    public int SkillId { get; }
    public string SkillName { get; }
    
    // 冷卻池配置
    public float MaxCooldownPool { get; }           // 最大池容量
    public float CurrentCooldownPool { get; }       // 當前池值
    public float CooldownCostPerCast { get; }       // 每次施放的消耗
    public float CooldownRegenRate { get; }         // 每秒恢復量
    
    // 施放配置
    public float CastTime { get; }                  // 施放時間
    public float BaseDamage { get; }                // 基礎傷害
    
    // 狀態查詢
    public bool IsCasting { get; }                  // 正在施放中
    public bool IsReady { get; }                    // 可以施放（池足夠 & 未在施放）
    public int AvailableCasts { get; }              // 可連續施放次數
    
    public virtual void Cast(Transform casterTransform);
    public virtual void UpdateSkill();
    protected virtual void OnCastComplete();
    public virtual void ResetCooldown();
    public virtual void ApplyUpgrade(string upgradeType, float value);
    public float GetPoolPercentage();
}
```

**使用方式：**

```csharp
// 檢查技能是否可施放
if (skill.IsReady)
{
    skill.Cast(playerTransform);
}

// 查詢可連續施放的次數
int casts = skill.AvailableCasts; // 例如 4

// 重置冷卻（例如在檢查點休息時）
skill.ResetCooldown();

// 套用升級效果
skill.ApplyUpgrade("IncreaseDamage", 5f);
skill.ApplyUpgrade("ReduceCost", 2f);
```

**支援的升級類型：**
- `IncreaseMaxPool` - 增加最大池容量
- `ReduceCost` - 減少每次施放的消耗
- `IncreaseRegenRate` - 增加每秒恢復量
- `IncreaseDamage` - 增加基礎傷害
- `ReduceCastTime` - 減少施放時間

---

### CooldownSystem：冷卻池管理

**責任：** 管理四個技能的冷卻池，追蹤狀態變化

**主要方法：**

```csharp
public class CooldownSystem : MonoBehaviour
{
    public static CooldownSystem Instance { get; }
    
    // 施放技能並扣除冷卻池
    public bool CastSkill(int skillIndex, Transform casterTransform);
    
    // 查詢冷卻狀態
    public float GetCooldownNormalized(int skillIndex);    // 0-1 百分比
    public float GetCooldownRemaining(int skillIndex);     // 剩餘池值
    public int GetAvailableCasts(int skillIndex);          // 可連續施放次數
    
    // 重置所有技能冷卻（檢查點使用）
    public void ResetAllCooldowns();
    
    // 獲取技能物件以進行高級操作
    public BaseSkill GetSkill(int skillIndex);
}
```

**使用方式：**

```csharp
CooldownSystem cooldownSystem = CooldownSystem.Instance;

// 嘗試施放技能
if (cooldownSystem.CastSkill(0, playerTransform))
{
    Debug.Log("技能1成功施放");
}

// 查詢冷卻進度（0-1）
float progress = cooldownSystem.GetCooldownNormalized(0);
// 更新 UI 冷卻條為 progress * 100%

// 查詢剩餘池值
float remaining = cooldownSystem.GetCooldownRemaining(0);
// 顯示 "已施放 X 次，可再施放 Y 次"

// 檢查點休息時重置所有冷卻
cooldownSystem.ResetAllCooldowns();
```

**觸發的事件：**
- `OnSkill{skillIndex}Ready` - 技能冷卻完成，可施放
- `OnSkill{skillIndex}OnCooldown` - 技能進入冷卻（池不足）
- `OnSkill{skillIndex}CooldownUpdated` - 冷卻狀態更新（每幀，用於 UI）

---

### SkillSystem：技能高層管理

**責任：** 協調技能施放、與玩家狀態互動、提供 UI 查詢介面

**主要方法：**

```csharp
public class SkillSystem : MonoBehaviour
{
    public static SkillSystem Instance { get; }
    
    // 嘗試施放技能（檢查玩家狀態）
    public bool AttemptCastSkill(int skillIndex);
    
    // UI 查詢方法
    public float GetSkillCooldownNormalized(int skillIndex);    // 冷卻進度 0-1
    public float GetSkillCooldownRemaining(int skillIndex);     // 剩餘池值
    public int GetSkillAvailableCasts(int skillIndex);          // 可連續施放次數
    public bool IsSkillReady(int skillIndex);                   // 技能是否可施放
}
```

**使用方式：**

```csharp
SkillSystem skillSystem = SkillSystem.Instance;

// 嘗試施放技能 1（會檢查玩家是否在施法或被鎖定）
if (skillSystem.AttemptCastSkill(0))
{
    Debug.Log("技能1成功施放");
}
else
{
    Debug.Log("技能1施放失敗（玩家被鎖定或在施法）");
}

// UI 更新冷卻進度
for (int i = 0; i < 4; i++)
{
    float progress = skillSystem.GetSkillCooldownNormalized(i);
    int availableCasts = skillSystem.GetSkillAvailableCasts(i);
    
    // 更新 UI 冷卻條
    cooldownBar[i].fillAmount = progress;
    castCountText[i].text = $"{availableCasts}x";
}
```

**工作流程：**

1. 玩家按下技能鍵（Q/W/E/R）
2. `InputManager` 偵測輸入
3. `SkillSystem.HandleSkillInput()` 調用 `AttemptCastSkill()`
4. `SkillSystem` 檢查 `PlayerState.IsActionLocked`
5. 若允許，呼叫 `CooldownSystem.CastSkill()`
6. `CooldownSystem` 驗證池值是否足夠
7. 若足夠，扣除冷卻池，設定 `PlayerState = CastingSkill`
8. 技能進入施放階段（計時 `castTime` 秒）
9. 施放完成後，觸發 `OnSkill{skillIndex}CastComplete` 事件
10. `SkillSystem` 監聽事件，重置 `PlayerState` 回 `Idle`
11. 同時冷卻池持續恢復

**整合與事件流：**

```csharp
// SkillSystem 內部自動處理的流程

private void HandleSkillInput()
{
    if (InputManager.GetSkill1Input())
        AttemptCastSkill(0);
    // ... 其他技能
}

public bool AttemptCastSkill(int skillIndex)
{
    // 1. 檢查玩家狀態
    if (playerState.IsCasting || playerState.IsActionLocked)
    {
        Debug.Log("Cannot cast: Player is already performing an action.");
        return false;
    }

    // 2. 嘗試施放（CooldownSystem 驗證池值）
    if (cooldownSystem.CastSkill(skillIndex, casterTransform))
    {
        // 3. 設定玩家狀態為施法中
        playerState.SetState(PlayerState.State.CastingSkill);
        
        // 4. 訂閱施放完成事件
        EventManager.StartListening($"OnSkill{skillIndex}CastComplete", OnCastComplete);
        
        return true;
    }

    return false;
}

private void OnCastComplete()
{
    // 5. 施放完成，重置玩家狀態
    playerState.ResetToIdle();
    
    // 6. 解除事件訂閱
    EventManager.StopListening($"OnSkill{currentCastingSkill}CastComplete", OnCastComplete);
    
    currentCastingSkill = -1;
}
```

---

### 具體技能實現示例

**Skill1.cs 範例：**

```csharp
public class Skill1 : BaseSkill
{
    public Skill1()
    {
        skillName = "Fireball";
        description = "Launch a fireball at enemies";
        
        maxCooldownPool = 40f;        // 總池容量
        cooldownCostPerCast = 10f;    // 每次消耗 10 (可施放 4 次)
        cooldownRegenRate = 1f;       // 每秒恢復 1 點 (40 秒完全恢復)
        
        castTime = 1f;
        baseDamage = 10f;
        
        currentCooldownPool = maxCooldownPool;
    }

    public override void Cast(Transform casterTransform)
    {
        base.Cast(casterTransform);
        // Skill1 特定邏輯可在此擴展
    }

    protected override void OnCastComplete()
    {
        base.OnCastComplete();
        Debug.Log("Fireball Effect Applied!");
        // 在此添加火球特效、傷害計算等邏輯
    }
}
```

---

## 敵人系統（Enemy System）

### 敵人系統架構概述

敵人系統由四個主要元件組成：

1. **BaseEnemy** - 敵人基類，定義所有敵人的核心邏輯
2. **BulletHellEnemy** 及其他派生類 - 具體敵人類型實現
3. **EnemyManager** - 敵人生命週期與生成管理
4. **EnemySpawner** - 單個生成點的敵人生成與重生控制

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
    [Header("基礎屬性")]
    public float maxHP = 50f;           // 最大生命值
    public float CurrentHP { get; }     // 當前生命值
    
    [Header("移動")]
    public float moveSpeed = 5f;           // 移動速度
    public float stoppingDistance = 2f;    // 停止距離（攻擊範圍）
    
    [Header("偵測")]
    public float sightRange = 20f;         // 視野範圍
    public float fieldOfViewAngle = 90f;   // 視野角度
    public LayerMask playerLayer;          // 玩家圖層
    public LayerMask wallLayer;            // 普通牆壁圖層
    public LayerMask invisibleWallLayer;   // 隱形牆壁圖層
    
    [Header("走位逃亡")]
    public float evadeRangeMin = 10f;      // 最小走位距離
    public float evadeRangeMax = 20f;      // 最大走位距離
    public float evadeDurationMin = 2f;    // 走位時長最小值
    public float evadeDurationMax = 5f;    // 走位時長最大值
    public float directionChangeInterval = 1f;  // 每秒檢查改變方向的概率
    
    [Header("重生設定")]
    public bool shouldRespawn = true;      // true = 普通敵人, false = Boss
    
    // 狀態查詢
    public float CurrentHP { get; }
    public float MaxHP { get; }
    public bool IsDead { get; }
    public bool ShouldRespawn { get; }
}
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
public virtual void TakeDamage(float damage);  // 受傷
protected virtual void Die();                  // 死亡
public virtual void ResetEnemy();              // 重置敵人（重生時調用）
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
// 派生類實例
public class BulletHellEnemy : BaseEnemy
{
    protected override void ExecuteSpellCard()
    {
        // 在此實作具體的彈幕發射邏輯
    }
    
    protected override bool IsSpellCardFinished()
    {
        // 返回 true 表示符卡完成
        return bulletsFired >= maxBullets;
    }
    
    protected override void OnAttackEnd()
    {
        base.OnAttackEnd();
        // 重置攻擊相關的計數器、計時器等
    }
}

// 外部傷害敵人
enemy.TakeDamage(10f);
```

**觸發的事件：**
- `OnEnemy{enemyID}Damaged` - 敵人受傷時
- `OnEnemy{enemyID}Died` - 敵人死亡時
- `OnEnemyDefeated` - 任何敵人死亡時

---

### EnemyManager：敵人全局管理

**責任：** 管理敵人生命週期、追蹤 Boss、協調敵人重生

**主要方法：**

```csharp
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; }
    
    // 敵人列表管理
    public void RefreshEnemyList();                    // 掃描場景中的所有敵人
    public List<BaseEnemy> GetActiveEnemies();         // 獲取所有活躍敵人
    public int GetActiveEnemyCount();                  // 獲取活躍敵人數量
    
    // Boss 管理
    public BaseEnemy GetBoss(string bossName);         // 透過名稱獲取特定 Boss
    
    // 敵人註冊/反註冊
    public void RegisterEnemy(BaseEnemy enemy);        // 註冊新敵人（生成時調用）
    public void UnregisterEnemy(BaseEnemy enemy);      // 反註冊敵人（死亡時調用）
    
    // 重生管理
    public void RespawnRegularEnemies();               // 重生普通敵人
    public void DespawnAllRegularEnemies();            // 清除所有普通敵人
}
```

**敵人分類：**
- **普通敵人** (`shouldRespawn = true`) - 在檢查點休息時會重生
- **Boss** (`shouldRespawn = false`) - 永遠不會自動重生，需手動管理

**使用方式：**

```csharp
// 取得所有活躍敵人
List<BaseEnemy> enemies = EnemyManager.Instance.GetActiveEnemies();

// 取得特定 Boss
BaseEnemy boss = EnemyManager.Instance.GetBoss("Boss_Dragon");

// 檢查是否還有敵人活著
int aliveCount = EnemyManager.Instance.GetActiveEnemyCount();

// 檢查點重生普通敵人
EnemyManager.Instance.RespawnRegularEnemies();
```

---

### EnemySpawner：敵人生成控制

**責任：** 管理單個生成點的敵人生成與重生

**主要方法：**

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private BaseEnemy enemyPrefab;                    // 敵人預製物
    [SerializeField] private bool shouldRespawnAfterCheckpoint = true; // 是否在檢查點重生
    
    private BaseEnemy spawnedEnemy;   // 已生成的敵人
    private Vector3 spawnPosition;    // 生成點位置
}
```

**工作流程：**

1. 場景啟動時，EnemySpawner 在指定位置生成敵人
2. 敵人被註冊到 EnemyManager
3. 敵人在場景中活動
4. 敵人被擊敗時，設定為 inactive 狀態
5. 玩家在檢查點休息時，EnemySpawner 收到 `OnCheckpointRest` 事件
6. 若敵人設定允許重生（`shouldRespawnAfterCheckpoint = true`），則重置敵人並重新啟動

**使用方式：**

```csharp
// 在編輯器中指定敵人預製物和生成點
// EnemySpawner 自動處理生成與重生
```

**重要：** 每個 EnemySpawner 只控制一個敵人的生成點。若要生成多個相同類型的敵人，需要多個 EnemySpawner 實例。

---

## 子彈系統（Bullet System）

### 子彈系統架構概述

子彈系統採用**組件化設計**，使用行為(Behavior)模式使子彈支援複雜的彈幕效果。

主要元件：

1. **Bullet** - 子彈核心類，管理行為與生命週期
2. **IBulletBehavior** - 行為介面，定義子彈的運動方式
3. **VirtualOrbitBehavior** - 環繞旋轉行為範例
4. **BulletManager** - 子彈生成與管理
5. **BulletHellEnemy** - 使用子彈系統的敵人範例

---

### Bullet：子彈核心

**責任：** 管理子彈的物理、生命週期和行為系統

**主要屬性與方法：**

```csharp
public class Bullet : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;  // 子彈速度向量
    public Vector3 position;                 // 子彈當前位置
    public Rigidbody rb;                     // 剛體元件參考
    
    public float lifetime = 5f;    // 子彈最大生存時間
    
    // 行為系統
    public void AddBehavior(IBulletBehavior behavior);        // 添加行為
    public void AddBehaviors(params IBulletBehavior[] behaviors);  // 一次添加多個行為
    public void RemoveBehavior(IBulletBehavior behavior);     // 移除行為
    public void ClearBehaviors();                             // 清空所有行為
}
```

**特性：**
- 支援添加多個行為，每個行為獨立運作
- 每幀自動更新所有行為
- 行為返回 false 時自動移除
- 超過生存時間自動銷毀

**使用方式：**

```csharp
// 創建子彈並附加行為
Bullet bullet = BulletManager.Instance.SpawnBullet(position, behavior1, behavior2);

// 在運行時添加新行為
bullet.AddBehavior(behavior3);

// 移除行為
bullet.RemoveBehavior(behavior1);
```

---

### IBulletBehavior：行為介面

**責任：** 定義子彈的運動邏輯

**介面定義：**

```csharp
public interface IBulletBehavior
{
    // 每幀更新。返回 true 表示行為繼續，false 表示行為完成
    bool Update(Bullet bullet, float deltaTime);
    
    // 行為初始化時調用
    void Initialize(Bullet bullet);
    
    // 行為結束時調用
    void OnBehaviorEnd(Bullet bullet);
}
```

**設計哲學：**
- 每個行為獨立管理一個方面的運動（軌跡、加速、衝擊等）
- 多個行為可疊加，實現複雜效果
- 返回 false 後自動從子彈中移除

---

### VirtualOrbitBehavior：環繞旋轉行為

**責任：** 實現子彈圍繞虛擬中心點旋轉並擴散的運動

**主要屬性：**

```csharp
public class VirtualOrbitBehavior : IBulletBehavior
{
    private Vector3 virtualCenterPosition;   // 虛擬中心點位置
    private Vector3 centerVelocity;          // 中心點移動速度
    private float currentRadius;             // 當前軌道半徑
    private float radiusGrowthRate;          // 半徑增長速率
    private float rotationSpeed;             // 旋轉速度（度/秒）
    private float currentAngle;              // 當前角度
}
```

**使用範例：**

```csharp
// 創建環繞行為：
// - 中心點在敵人位置，以 20 m/s 速度朝玩家方向移動
// - 初始半徑 1m，每秒擴張 12m
// - 旋轉速度 100 度/秒

var orbitBehavior = new VirtualOrbitBehavior(
    startCenterPos: enemyPosition,
    centerMoveVelocity: directionToPlayer * 20f,
    initialRadius: 1f,
    radiusGrowth: 12f,
    rotSpeed: 100f
);

BulletManager.Instance.SpawnBullet(enemyPosition, orbitBehavior);
```

**效果說明：**
- 子彈以中心點旋轉，形成環形彈幕
- 半徑逐漸擴張，形成推進效果
- 中心點移動，使整個彈幕追向玩家

---

### BulletManager：子彈生成管理

**責任：** 統一管理子彈生成與預製物

**主要方法：**

```csharp
public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; }
    
    // 創建子彈並附加行為（可變參數）
    public Bullet SpawnBullet(Vector3 position, params IBulletBehavior[] behaviors);
}
```

**自動化特性：**
- 若沒有指定子彈預製物，自動創建預設紅色球體子彈
- 預設子彈包含 Rigidbody、SphereCollider、MeshRenderer

**使用方式：**

```csharp
// 簡單生成
Bullet bullet = BulletManager.Instance.SpawnBullet(position, behavior);

// 多行為生成
Bullet bullet = BulletManager.Instance.SpawnBullet(
    position,
    behavior1,
    behavior2,
    behavior3
);
```

---

### BulletHellEnemy：彈幕地獄敵人範例

**責任：** 演示如何使用子彈系統製作複雜彈幕攻擊

**主要邏輯：**

```csharp
public class BulletHellEnemy : BaseEnemy
{
    private float nextBulletTime = 0f;  // 下次發射時間
    private int bulletsFired = 0;       // 已發射次數
    
    // 執行符卡：定時發射圓形旋轉陣型
    protected override void ExecuteSpellCard()
    {
        if (Time.time >= nextBulletTime)
        {
            FireCircleFormationWithVirtualCenter();
            bulletsFired++;
            nextBulletTime = Time.time + 0.6f;
        }
    }
    
    // 符卡結束條件：發射 5 次
    protected override bool IsSpellCardFinished()
    {
        return bulletsFired >= 5;
    }
}
```

**攻擊模式：**

1. 每 0.6 秒發射一次圓形陣型
2. 每次發射 6 顆子彈，均勻分布在圓形軌道上
3. 所有子彈圍繞虛擬中心點旋轉並向玩家方向移動
4. 半徑逐漸擴張，形成推進波效果
5. 發射 5 次後符卡完成，進入走位階段

**視覺效果：**
```
回合 1: 6 顆子彈圍繞形成的圓形→推進波
回合 2: 再發射 6 顆子彈，形成第二波推進波
回合 3-5: 類似，最終形成多波相疊的彈幕攻擊
```

---

### 擴展示例：創建新行為

**步驟：**

```csharp
// 1. 實作 IBulletBehavior 介面
public class SpiralBehavior : IBulletBehavior
{
    private float spiralProgress = 0f;
    private float totalDuration = 3f;
    
    public bool Update(Bullet bullet, float deltaTime)
    {
        spiralProgress += deltaTime;
        
        if (spiralProgress >= totalDuration)
            return false;  // 行為完成
        
        // 計算螺旋軌跡
        float angle = (spiralProgress / totalDuration) * 360f * 3f;  // 3 圈
        float radius = Mathf.Lerp(0f, 10f, spiralProgress / totalDuration);
        
        Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
        bullet.transform.position += offset * deltaTime;
        
        return true;  // 行為繼續
    }
    
    public void Initialize(Bullet bullet) { }
    public void OnBehaviorEnd(Bullet bullet) { }
}

// 2. 使用新行為
BulletManager.Instance.SpawnBullet(position, new SpiralBehavior());
```

---

### 敵人攻擊流程整合

**完整流程圖：**

```
EnemyManager.RefreshEnemyList()
    ↓
每個 BaseEnemy 進入 Update 循環
    ↓
CheckForPlayer() → 檢查玩家是否可見
    ↓
UpdateState() → 狀態機
    ├─ Idle: 等待玩家出現
    ├─ Attacking: 
    │   └─ ExecuteSpellCard() → 發射子彈
    │       └─ BulletManager.SpawnBullet(position, behaviors)
    │           └─ 子彈進入場景，執行行為
    ├─ Evading: 移動與躲閃
    └─ Dead: 停止所有行動

敵人死亡 → TakeDamage() → Die()
    └─ 觸發 OnEnemy{enemyID}Died 事件
    └─ EnemyManager 反註冊敵人
    └─ EnemySpawner 標記敵人需要重生
    
玩家休息於檢查點 → OnCheckpointRest 事件
    └─ EnemyManager.RespawnRegularEnemies()
    └─ 每個 EnemySpawner 重置並重新激活敵人
```

---

### 設計特點與最佳實踐

**優勢：**
- **模組化** - 敵人、子彈、行為各自獨立
- **可擴展** - 輕鬆添加新敵人類型或行為模式
- **可視化調試** - Gizmos 顯示生成點與視野範圍
- **事件驅動** - 敵人與其他系統通過事件通訊

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
// 布林狀態（門是否開啟）
PersistentStateManager.Instance.SetBoolState("Door_MainHall_Opened", true);
bool isOpen = PersistentStateManager.Instance.GetBoolState("Door_MainHall_Opened");

// 整數狀態（敵人數量）
PersistentStateManager.Instance.IncrementIntState("EnemiesDefeated");
int count = PersistentStateManager.Instance.GetIntState("EnemiesDefeated");

// 浮點數狀態
PersistentStateManager.Instance.SetFloatState("BossHealth", 50f);

// 字串狀態
PersistentStateManager.Instance.SetStringState("PlayerName", "Hero");

// 檢查狀態是否存在
if (PersistentStateManager.Instance.HasState("Door_MainHall_Opened"))
{
    // ...
}
```

**狀態鍵命名約定：**
```
{ObjectType}_{LevelName}_{ObjectName}_{Property}

例如：
"Door_MainHall_GoldenGate_Opened"
"Puzzle_TreasureRoom_LeverA_Activated"
"Boss_CastleTop_Dragon_Defeated"
```

**觸發的事件：**
- `OnStateChanged_{stateKey}` - 當狀態改變時

---

### LevelManager：場景與關卡控制

**責任：** 管理場景加載/卸載、玩家位置

```csharp
// 轉移至新關卡
Vector3 spawnPosition = new Vector3(5, 1, 0);
LevelManager.Instance.TransitionToLevel("Level_TreasureRoom", spawnPosition);

// 查詢當前關卡
string currentLevel = LevelManager.Instance.GetCurrentLevelName();

// 檢查是否正在轉換
if (LevelManager.Instance.IsTransitioning())
{
    // 禁止重複轉換
}

// 取得玩家生成位置
Vector3 pos = LevelManager.Instance.GetPlayerSpawnPosition();
```

**場景轉換流程：**

```
玩家進入出口
    ↓
卸載前一個關卡（禁用 CharacterController）
    ↓
加載新關卡（Additive 模式）
    ↓
等待場景完全初始化
    ↓
恢復永久狀態（已開啟的門等）
    ↓
禁用 CharacterController → 設定玩家位置 → 重新啟用
    ↓
重新掃描敵人並重生（規則敵人重生，Boss 不重生）
    ↓
觸發 OnLevelLoaded 事件
```

---

### InteractiveObject：可交互物件基類

**責任：** 管理改變永久狀態的物件

```csharp
public abstract class InteractiveObject : MonoBehaviour
{
    protected string stateKey;  // 唯一狀態鍵
    
    public virtual void Activate();
    public virtual void Deactivate();
    public virtual void RestoreState();  // 關卡加載時調用
    protected abstract void OnStateChanged(bool isActive);
}
```

**Door 派生類範例：**

```csharp
public class Door : InteractiveObject
{
    protected override void OnStateChanged(bool isOpen)
    {
        // 打開時禁用碰撞體，移動門
        objectCollider.enabled = !isOpen;
        // 動畫移動門到位置
    }
}
```

**Ladder 派生類範例：**

```csharp
public class Ladder : InteractiveObject
{
    protected override void OnStateChanged(bool isAvailable)
    {
        // 梯子可用 = 啟用，不可用 = 停用
        gameObject.SetActive(isAvailable);
    }
}
```

---

### LevelExit：關卡出口

**責任：** 定義關卡間的轉移點

```csharp
public class LevelExit : MonoBehaviour
{
    public string targetLevelName;           // 目標關卡名稱
    public Vector3 playerSpawnOffset;        // 玩家進入位置偏移
    public bool showDebugInfo = true;
}
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
    
    // Start() 時：
    // 1. 加載 Persistent 場景（Additive）
    // 2. 等待管理器初始化
    // 3. 加載第一個關卡
}
```

**使用方式：**

1. 在 Bootstrap 場景中創建空 GameObject
2. 附加 GameStarter 指令碼
3. 在 Inspector 設定關卡名稱和生成位置
4. 播放遊戲

---

## PauseManager：遊戲暫停系統

**責任：** 管理遊戲時間暫停/繼續，提供簡單的暫停狀態控制

### 核心概念

暫停系統使用一個簡單的布林旗標來追蹤暫停狀態，通過 `Time.timeScale` 來控制時間流逝。當 `Time.timeScale = 0` 時，所有基於 `Time.deltaTime` 的更新都會停止。

### PauseManager：單例

**主要屬性與方法：**

```csharp
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    
    public bool IsPaused { get; }  // 當前暫停狀態
    
    // 暫停遊戲
    public void Pause();
    
    // 恢復遊戲
    public void Resume();
}
```

**使用方式：**

```csharp
// 暫停遊戲
PauseManager.Instance.Pause();

// 繼續遊戲
PauseManager.Instance.Resume();

// 檢查當前是否暫停
if (PauseManager.Instance.IsPaused)
{
    // 顯示暫停菜單
}
```

**核心特性：**
- ESC 鍵自動切換暫停狀態
- `Time.timeScale` 控制遊戲時間
- 觸發事件 `OnGamePaused` 和 `OnGameResumed`
- 與 `GameManager` 集成，管理遊戲狀態

### 重要：需要更新的系統

某些系統需要使用 `Time.unscaledDeltaTime` 才能在暫停時正常運作。

#### 需要暫停時仍然更新的系統

**UI 冷卻顯示：** 即使遊戲暫停，UI 的冷卻條進度也應該顯示。但由於技能冷卻本身使用 `Time.deltaTime`，所以暫停時冷卻進度會停止（這通常是想要的行為）。

**UI 動畫：** 如果暫停菜單本身有動畫，使用 Canvas Group 的 `alpha` 動畫不會受 `Time.timeScale` 影響。

### 事件系統整合

暫停系統通過 `EventManager` 發送事件，其他系統可以監聽以做出反應：

```csharp
// 監聽暫停事件
EventManager.StartListening("OnGamePaused", () =>
{
    Debug.Log("遊戲暫停，可以顯示 UI");
});

// 監聽繼續事件
EventManager.StartListening("OnGameResumed", () =>
{
    Debug.Log("遊戲繼續，可以隱藏 UI");
});
```

---
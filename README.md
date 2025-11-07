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
│   ├── Data/
│   │   ├── PlayerStats.cs
│   │   ├── PersistentState.cs
│   │   ├── SaveData.cs
├── Scenes/
├── Prefabs/
├── Art/
├── UI/
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

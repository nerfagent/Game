// Assets/Scripts/Level/Skills/SkillSystem.cs
using UnityEngine;
using UnityEngine.Events;

public class SkillSystem : MonoBehaviour
{
    public static UnityAction[] onSkillCastComplete;
    private CooldownSystem cooldownSystem; // 冷卻系統實例
    private PlayerState playerState;       // 玩家當前狀態（用於防止重複施法）
    private Transform casterTransform;     // 玩家位置／施法基準

    private int currentCastingSkill = -1;  // 當前正在施放的技能索引

    public static SkillSystem Instance { get; private set; }

    private void Awake()
    {
        if(onSkillCastComplete == null)
        {
            onSkillCastComplete = new UnityAction[4];
        }
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
        cooldownSystem = CooldownSystem.Instance;
        playerState = GetComponent<PlayerState>();
        casterTransform = transform;

        if (cooldownSystem == null || playerState == null)
        {
            Debug.LogError("SkillSystem missing required components.");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        HandleSkillInput(); // 持續偵測輸入以觸發技能
    }

    private void HandleSkillInput()
    {
        // 偵測技能輸入並嘗試施放
        if (InputManager.GetSkill1Input()) AttemptCastSkill(0);
        else if (InputManager.GetSkill2Input()) AttemptCastSkill(1);
        else if (InputManager.GetSkill3Input()) AttemptCastSkill(2);
        else if (InputManager.GetSkill4Input()) AttemptCastSkill(3);
    }

    /// <summary>
    /// 嘗試施放技能，需檢查玩家狀態與冷卻。
    /// </summary>
    public bool AttemptCastSkill(int skillIndex)
    {
        if (playerState.IsCasting || playerState.IsActionLocked)
        {
            Debug.Log("Cannot cast: Player is already performing an action.");
            return false;
        }

        // 若施放成功則更新狀態與事件監聽
        if (cooldownSystem.CastSkill(skillIndex, casterTransform))
        {
            playerState.SetState(PlayerState.State.CastingSkill);
            currentCastingSkill = skillIndex;

            // 監聽該技能的施放完成事件
            onSkillCastComplete[skillIndex] += OnCastComplete;
            return true;
        }

        return false;
    }

    private void OnCastComplete()
    {
        Debug.Log("Cast Complete");
        playerState.ResetToIdle(); // 施放完畢後恢復閒置

        if (currentCastingSkill >= 0)
        {
            onSkillCastComplete[currentCastingSkill] -= OnCastComplete;
        }
        currentCastingSkill = -1;
    }

    // 取得冷卻進度（0-1）供 UI 顯示
    public float GetSkillCooldownNormalized(int skillIndex)
    {
        return cooldownSystem.GetCooldownNormalized(skillIndex);
    }

    // 取得剩餘冷卻點數
    public float GetSkillCooldownRemaining(int skillIndex)
    {
        return cooldownSystem.GetCooldownRemaining(skillIndex);
    }

    // 取得當前可施放次數
    public int GetSkillAvailableCasts(int skillIndex)
    {
        return cooldownSystem.GetAvailableCasts(skillIndex);
    }

    // 查詢技能是否可用
    public bool IsSkillReady(int skillIndex)
    {
        BaseSkill skill = cooldownSystem.GetSkill(skillIndex);
        return skill != null && skill.IsReady;
    }
}

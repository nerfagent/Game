// Assets/Scripts/Level/Skills/CooldownSystem.cs
using UnityEngine;

public class CooldownSystem : MonoBehaviour
{
    private BaseSkill[] skills = new BaseSkill[4];     // 保存所有技能物件
    private bool[] wasSkillReady = new bool[4];        // 用於追蹤技能源本是否可用，方便觸發事件

    public static CooldownSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeSkills();
    }

    private void Start()
    {
        
    }

    private void InitializeSkills()
    {
        // 建立並初始化四種技能（具體類別如 Skill1~4）
        skills[0] = new Skill1(); skills[0].SetIndex(0);
        skills[1] = new Skill2(); skills[1].SetIndex(1);
        skills[2] = new Skill3(); skills[2].SetIndex(2);
        skills[3] = new Skill4(); skills[3].SetIndex(3);

        // 初始化技能狀態與觸發 UI 更新事件
        for (int i = 0; i < skills.Length; i++)
        {
            wasSkillReady[i] = skills[i].IsReady;
            EventManager.TriggerEvent($"OnSkill{i}CooldownUpdated");
        }
    }

    private void Update()
    {
        // 每幀更新每個技能的冷卻狀態
        for (int i = 0; i < skills.Length; i++)
        {
            bool wasReady = wasSkillReady[i];
            skills[i].UpdateSkill();
            bool isReady = skills[i].IsReady;

            // 若技能從「冷卻中」轉為「可用」，觸發事件
            if (!wasReady && isReady)
            {
                EventManager.TriggerEvent($"OnSkill{i}Ready");
            }
            // 若從「可用」變為「冷卻中」
            else if (wasReady && !isReady)
            {
                EventManager.TriggerEvent($"OnSkill{i}OnCooldown");
            }

            wasSkillReady[i] = isReady;
            EventManager.TriggerEvent($"OnSkill{i}CooldownUpdated"); // 持續同步 UI
        }
    }

    /// <summary>
    /// 嘗試施放指定技能（index 0-3）。
    /// </summary>
    public bool CastSkill(int skillIndex, Transform casterTransform)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
        {
            Debug.LogError($"Invalid skill index: {skillIndex}");
            return false;
        }

        BaseSkill skill = skills[skillIndex];
        if (!skill.IsReady)
        {
            Debug.LogWarning($"Skill {skillIndex} is not ready. Pool: {skill.CurrentCooldownPool}/{skill.MaxCooldownPool} (needs {skill.CooldownCostPerCast})");
            return false;
        }

        // 傳入施法者資訊進行施放
        skill.Cast(casterTransform);
        return true;
    }

    public float GetCooldownNormalized(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
            return 0f;
        return skills[skillIndex].GetPoolPercentage();
    }

    public float GetCooldownRemaining(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
            return 0f;
        return skills[skillIndex].CurrentCooldownPool;
    }

    public int GetAvailableCasts(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
            return 0;
        return skills[skillIndex].AvailableCasts;
    }

    /// <summary>
    /// 重設所有技能冷卻池（通常用於休息點或檢查點）。
    /// </summary>
    public void ResetAllCooldowns()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].ResetCooldown();
            wasSkillReady[i] = true;
            EventManager.TriggerEvent($"OnSkill{i}Ready");
        }
        Debug.Log("All skill cooldown pools reset to full");
    }

    /// <summary>
    /// 取得特定技能實例（提供外部系統直接訪問屬性）。
    /// </summary>
    public BaseSkill GetSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
            return null;
        return skills[skillIndex];
    }

    public int GetSkillCount()
    {
        return skills.Length;
    }
}

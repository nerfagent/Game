// CooldownSystem.cs
using UnityEngine;

public class CooldownSystem : MonoBehaviour
{
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownRegenerationPerSecond = 1f;

    private BaseSkill[] skills = new BaseSkill[4];
    private bool[] skillOnCooldown = new bool[4];

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
    }

    private void Start()
    {
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        // Create instances of each skill
        skills[0] = new Skill1(); skills[0].SetIndex(0);
        skills[1] = new Skill2(); skills[1].SetIndex(1);
        skills[2] = new Skill3(); skills[2].SetIndex(2);
        skills[3] = new Skill4(); skills[3].SetIndex(3);

        // Initialize each skill with default values
        for (int i = 0; i < skills.Length; i++)
        {
            skillOnCooldown[i] = false;
            EventManager.TriggerEvent($"OnSkill{i}CooldownUpdated");
        }
    }

    private void Update()
    {
        // Update all skills every frame
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].UpdateSkill();

            // Broadcast cooldown update
            if (skills[i].CurrentCooldown > 0f && !skillOnCooldown[i])
            {
                skillOnCooldown[i] = true;
                EventManager.TriggerEvent($"OnSkill{i}OnCooldown");
            }
            else if (skills[i].CurrentCooldown <= 0f && skillOnCooldown[i])
            {
                skillOnCooldown[i] = false;
                EventManager.TriggerEvent($"OnSkill{i}Ready");
            }

            EventManager.TriggerEvent($"OnSkill{i}CooldownUpdated");
        }
    }

    /// <summary>
    /// Attempts to cast a skill by index (0-3).
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
            Debug.LogWarning($"Skill {skillIndex} is not ready. Cooldown: {skill.CurrentCooldown}");
            return false;
        }

        // Pass casterTransform to the skill
        skill.Cast(casterTransform);
        return true;
    }

    /// <summary>
    /// Gets the current cooldown of a skill (0-1 normalized).
    /// </summary>
    public float GetCooldownNormalized(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
            return 0f;

        BaseSkill skill = skills[skillIndex];
        return skill.CurrentCooldown / skill.BaseCooldown;
    }

    /// <summary>
    /// Gets the remaining cooldown in seconds.
    /// </summary>
    public float GetCooldownRemaining(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
            return 0f;

        return skills[skillIndex].CurrentCooldown;
    }

    /// <summary>
    /// Resets all skill cooldowns (for checkpoint rest).
    /// </summary>
    public void ResetAllCooldowns()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].ResetCooldown();
            skillOnCooldown[i] = false;
            EventManager.TriggerEvent($"OnSkill{i}Ready");
        }

        Debug.Log("All skill cooldowns reset");
    }

    /// <summary>
    /// Gets a skill by index for advanced interactions.
    /// </summary>
    public BaseSkill GetSkill(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skills.Length)
            return null;

        return skills[skillIndex];
    }
}

// BaseSkill.cs
using UnityEngine;

[System.Serializable]
public abstract class BaseSkill
{
    [SerializeField] protected int skillId;
    [SerializeField] protected string skillName;
    [SerializeField] protected string description;
    [SerializeField] protected float baseCooldown = 40f;
    [SerializeField] protected float castTime = 10f;
    [SerializeField] protected float baseDamage = 10f;

    protected float currentCooldown = 0f;
    protected bool isCasting = false;
    protected float castTimer = 0f;

    public int SkillId => skillId;
    public string SkillName => skillName;
    public float CurrentCooldown => currentCooldown;
    public float BaseCooldown => baseCooldown;
    public float CastTime => castTime;
    public bool IsCasting => isCasting;
    public bool IsReady => currentCooldown <= 0f && !isCasting;

    public void SetIndex(int index)
    {
        skillId = index; // ensure event names match the index used by systems
    }

    /// <summary>
    /// Called when the skill is cast. Override in subclasses for specific behavior.
    /// </summary>
    public virtual void Cast(Transform casterTransform)
    {
        if (!IsReady)
        {
            Debug.LogWarning($"Skill {skillName} is not ready. Cooldown: {currentCooldown}");
            return;
        }

        isCasting = true;
        castTimer = castTime;
        currentCooldown = baseCooldown;

        EventManager.TriggerEvent($"OnSkill{skillId}Cast");
        Debug.Log($"Casting {skillName}");
    }

    /// <summary>
    /// Called every frame to update casting state and cooldowns.
    /// </summary>
    public virtual void UpdateSkill()
    {
        // Update cast timer
        if (isCasting)
        {
            castTimer -= Time.deltaTime;
            if (castTimer <= 0f)
            {
                OnCastComplete();
            }
        }

        // Update cooldown
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown < 0f)
            {
                currentCooldown = 0f;
            }
        }
    }

    /// <summary>
    /// Called when the cast duration finishes. Override for skill effects.
    /// </summary>
    protected virtual void OnCastComplete()
    {
        isCasting = false;
        EventManager.TriggerEvent($"OnSkill{skillId}CastComplete");
        Debug.Log($"{skillName} cast complete");
    }

    /// <summary>
    /// Resets the skill cooldown (for checkpoint rest).
    /// </summary>
    public virtual void ResetCooldown()
    {
        currentCooldown = 0f;
        isCasting = false;
        castTimer = 0f;
    }

    /// <summary>
    /// Applies upgrades to the skill (called by SkillTree).
    /// </summary>
    public virtual void ApplyUpgrade(string upgradeType, float value)
    {
        switch (upgradeType)
        {
            case "ReduceCooldown":
                baseCooldown -= value;
                break;
            case "IncreaseDamage":
                baseDamage += value;
                break;
            case "ReduceCastTime":
                castTime -= value;
                break;
            default:
                Debug.LogWarning($"Unknown upgrade type: {upgradeType}");
                break;
        }
    }
}

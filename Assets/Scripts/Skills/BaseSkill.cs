// Assets/Scripts/Level/Skills/BaseSkill.cs
using UnityEngine;

[System.Serializable]
public abstract class BaseSkill
{
    [SerializeField] protected int skillId;
    [SerializeField] protected string skillName;
    [SerializeField] protected string description;
    
    [SerializeField] protected float maxCooldownPool = 40f;
    [SerializeField] protected float cooldownCostPerCast = 10f;
    [SerializeField] protected float cooldownRegenRate = 1f;
    [SerializeField] protected float castTime = 1f;
    [SerializeField] protected float baseDamage = 10f;

    protected float currentCooldownPool;
    protected bool isCasting = false;
    protected float castTimer = 0f;

    // 公開屬性（可被 Decorator 覆寫）
    public virtual float CurrentCooldownPool => currentCooldownPool;
    public virtual float MaxCooldownPool => maxCooldownPool;
    public virtual float CooldownCostPerCast => cooldownCostPerCast;
    public virtual float CooldownRegenRate => cooldownRegenRate;
    public virtual float CastTime => castTime;
    public virtual float BaseDamage => baseDamage;
    public virtual bool IsCasting => isCasting;
    public virtual bool IsReady => currentCooldownPool >= CooldownCostPerCast && !isCasting;
    public virtual int AvailableCasts => Mathf.FloorToInt(currentCooldownPool / CooldownCostPerCast);

    public BaseSkill()
    {
        currentCooldownPool = maxCooldownPool;
    }

    public void SetIndex(int index)
    {
        skillId = index;
    }

    public virtual void Cast(Transform casterTransform)
    {
        if (!IsReady)
        {
            Debug.LogWarning($"Skill {skillName} is not ready. Pool: {CurrentCooldownPool}/{MaxCooldownPool} (needs {CooldownCostPerCast})");
            return;
        }

        currentCooldownPool -= CooldownCostPerCast;
        currentCooldownPool = Mathf.Max(0f, currentCooldownPool);

        isCasting = true;
        castTimer = CastTime;

        EventManager.TriggerEvent($"OnSkill{skillId}Cast");
        Debug.Log($"Casting {skillName}. Pool: {CurrentCooldownPool}/{MaxCooldownPool}");
    }

    public virtual void UpdateSkill()
    {
        if (isCasting)
        {
            castTimer -= Time.deltaTime;
            if (castTimer <= 0f)
            {
                OnCastComplete();
            }
        }

        if (currentCooldownPool < MaxCooldownPool)
        {
            currentCooldownPool += CooldownRegenRate * Time.deltaTime;
            if (currentCooldownPool > MaxCooldownPool)
            {
                currentCooldownPool = MaxCooldownPool;
            }
        }
    }

    public virtual void OnCastComplete()
    {
        isCasting = false;
        //EventManager.TriggerEvent($"OnSkill{skillId}CastComplete");
        SkillSystem.onSkillCastComplete[skillId].Invoke();
        Debug.Log($"{skillName} cast complete");
    }

    public virtual void ResetCooldown()
    {
        currentCooldownPool = MaxCooldownPool;
        isCasting = false;
        castTimer = 0f;
    }

    public float GetPoolPercentage()
    {
        return CurrentCooldownPool / MaxCooldownPool;
    }
}

// SkillSystem.cs
using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    private CooldownSystem cooldownSystem;
    private PlayerState playerState;
    private Transform casterTransform;

    private int currentCastingSkill = -1;

    public static SkillSystem Instance { get; private set; }

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
        // Handle skill input
        HandleSkillInput();
    }

    private void HandleSkillInput()
    {
        // Check for skill input keys
        if (InputManager.GetSkill1Input())
        {
            AttemptCastSkill(0);
        }
        else if (InputManager.GetSkill2Input())
        {
            AttemptCastSkill(1);
        }
        else if (InputManager.GetSkill3Input())
        {
            AttemptCastSkill(2);
        }
        else if (InputManager.GetSkill4Input())
        {
            AttemptCastSkill(3);
        }
    }

    /// <summary>
    /// Attempts to cast a skill. Checks if player can cast and if skill is ready.
    /// </summary>
    public bool AttemptCastSkill(int skillIndex)
    {
        // Check if player is already casting or action-locked
        if (playerState.IsCasting || playerState.IsActionLocked)
        {
            Debug.Log("Cannot cast: Player is already performing an action.");
            return false;
        }

        // Attempt to cast through cooldown system
        if (cooldownSystem.CastSkill(skillIndex, casterTransform))
        {
            playerState.SetState(PlayerState.State.CastingSkill);
            currentCastingSkill = skillIndex;

            // Listen for when cast completes
            EventManager.StartListening($"OnSkill{skillIndex}CastComplete", OnCastComplete);
            
            return true;
        }

        return false;
    }

    private void OnCastComplete()
    {
        Debug.Log("Complete");

        playerState.ResetToIdle();
        
        if (currentCastingSkill >= 0)
        {
            EventManager.StopListening($"OnSkill{currentCastingSkill}CastComplete", OnCastComplete);
        }

        currentCastingSkill = -1;
    }

    /// <summary>
    /// Gets cooldown information for UI display.
    /// </summary>
    public float GetSkillCooldownNormalized(int skillIndex)
    {
        return cooldownSystem.GetCooldownNormalized(skillIndex);
    }

    public float GetSkillCooldownRemaining(int skillIndex)
    {
        return cooldownSystem.GetCooldownRemaining(skillIndex);
    }

    public bool IsSkillReady(int skillIndex)
    {
        BaseSkill skill = cooldownSystem.GetSkill(skillIndex);
        return skill != null && skill.IsReady;
    }
}

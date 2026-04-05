using UnityEngine;
using System;
using System.Collections.Generic;

public class CombatSystem_Skeleton_A0 : MonoBehaviour
{
    [Header("Combat System")]
    [SerializeField] private Hurtbox[] hurtboxes;
    /*
        0. head
        1. chest
        2. stomach
        3. r up arm
        4. r low arm
        5. l up arm
        6. l low arm
        7. r up leg
        8. r low leg
        9. r foot
        10. l up leg
        11. l low leg
        12. l foot
    */
    [SerializeField] private Hitbox[] hitboxes;
    /*
        0. sword
        1. claw (claw shot) (dk if need)
        2. body (claw shot) (dk if need)
    */
    [SerializeField] private DamageData[] weaponDatas;
    /*
        0. sword (slash)
        1. sword (pierce)
        2. claw (claw shot)
        3. body (claw shot)
    */
    [SerializeField] private DamageData[] weaponBuffDatas;
    /*
        0. null
        1. standard
        2. strike
        3. slash
        4. pierce
        5. magic
        6. fire
        7. lightning
        8. holy (wont use)
        9. dark
    */
    [SerializeField] private DamageData[] skillDatas;
    /*
        0. base attack 1
        1. base attack 2
        2. roll attack
        3. run attack
        4. jump attack
        5. 前步上砍
        6. 弧撼地
        7. 環撼地
        8. 劍氣
        9. 投壺
        10. 抹油 (no need)
        11. 白光突擊
        12. 洛特大蓄力突刺
        13. 盾牌 (no need)
        14. 鉤爪
        15. 火炬 (no need)
    */
    [SerializeField] private DamageData[] weaknessDatas;
    /*
        0. idle
    */
    [SerializeField] public DamageData[] VEDatas;
    /*
        0. heal
        1. buff
        2. BA stamina
        3. RA stamina
        4. JA stamina
        5. DG stamina
        6. Jump stamina
    */

    // [SerializeField] private DamageData weaponData;
    // [SerializeField] private DamageData weaponBuffData;
    // [SerializeField] private DamageData skillData;
    // [SerializeField] private DamageData ownerBuffData;
    // [SerializeField] private DamageData summedData;

    public float maxHealth = 1000f;
    public float maxFocus = 125f;
    public float maxStamina = 100f;
    public float maxPoise = 19f;
    // private float maxStance = 51f;
    public float maxHealAmount = 300f;

    // private float currentStance;
    // private Vector3 knockbackDirection = Vector3.forward; // in .damageData
    // private int knockbackindex = 0; // 0: F, 1: B, 2: L, 3: R, 4: U, 5: D // in .damageData
    public List<bool> baseDizzyDamageLevel = new List<bool> { false, true, true, true, true, true };
    // 0: wont knock // 1: small knock // 2: big knock // 3: knock up // 4: knock down // 5: status effect
    public List<bool> forceDizzyDamageLevel = new List<bool> { false, false, true, true, true, true };

    public float currentPassiveEffectTimeout = 0.5f; // for handling passive effects that have a duration, like poison or regen
    public bool currentPassiveEffectActive = true; // for handling passive effects that have a duration, like poison or regen
    
    public float currentHealth;
    public float currentFocus;
    public float currentStamina;
    public float currentPoise;
    public bool[] currentDizzyDamageLevel = { false, false, false, false, false, false };
    public bool[] currentWeaknesses = { false, false, false, false, false, false };
    /*
        new SpecialWeakness { Name = "FALSE", Value = 1f },
        new SpecialWeakness { Name = "GunAnti", Value = 1f },
        new SpecialWeakness { Name = "AntiAirS", Value = 1f },
        new SpecialWeakness { Name = "AntiAirB", Value = 1f },
        new SpecialWeakness { Name = "HorSlashAnti", Value = 1f },
        new SpecialWeakness { Name = "GroundAttackAnti", Value = 1f }
    */
    public Vector3 curKB_dir = Vector3.left;
    public int curKB_index = 0; // 0: F, 1: B, 2: L, 3: R, 4: U, 5: D
    public int curKB_getHit = 0; // 0: idle, 1: shake, 2: kb
    public float curKB_getHitTimeout = 1f; // just for playing the animation
    public int curKB_getHitType = 1;
        // private string[] getHitStates = {
        //         "GetHit_F1",
        //         "GetHit_F2",
        //         "GetHit_B1",
        //         "GetHit_B2",
        //         "GetHit_L1",
        //         "GetHit_L2",
        //         "GetHit_R1",
        //         "GetHit_R2",
        //         "GetHit_U1", // = B1
        //         "GetHit_U2",
        //         "GetHit_D1", // = F1
        //         "GetHit_D2"
        //     };

    private readonly object _damageLock = new object();
    public List<DamageData> currentPendingDamageDataList = new List<DamageData> { };
    private readonly object _veLock = new object();
    public List<DamageData> currentPendingVEList = new List<DamageData> { }; // Value Editing
    // private bool canGetHit = true;

    private SkeletonSwordMain_A0 _playerMain;
    public bool isReady = false;

    private void Start()
    {
        currentHealth = maxHealth;
        currentFocus = maxFocus;
        currentStamina = maxStamina;
        currentPoise = maxPoise;
        for (int i = 0; i < currentDizzyDamageLevel.Length; i++ )
        {
            currentDizzyDamageLevel[i] = baseDizzyDamageLevel[i];
        }
        for (int i = 0; i < currentWeaknesses.Length && weaknessDatas.Length > 0; i++ )
        {
            currentWeaknesses[i] = weaknessDatas[0].specialWeaknesses[i].Value > 1.1f ? true : false;
        }

        // Register hurtbox events
        foreach (var hurtbox in hurtboxes)
        {
            hurtbox.OnDamageReceived += HandleDamage;
        }

        _playerMain = GetComponent<SkeletonSwordMain_A0>();
        isReady = true;
    }

    private void HandleDamage(DamageData damageData)
    {
        // if (canGetHit)
        // {
        lock (_damageLock)
        {
            currentPendingDamageDataList.Add(damageData);
        }
        //     canGetHit = false;
        // }
    }

    public void UpdateHandleDamage()
    {
        curKB_getHitTimeout -= Time.deltaTime;
        curKB_getHitTimeout = curKB_getHitTimeout <= 0f ? -0.1f : curKB_getHitTimeout;
        if (curKB_getHitTimeout <= 0f && curKB_getHit == 1) curKB_getHit = 0;

        lock (_damageLock)
        {
            if (currentPendingDamageDataList.Count == 0) return;

            DamageData damageData = currentPendingDamageDataList[0];
            currentPendingDamageDataList.RemoveAt(0);

            // main part
            // Debug.Log("Hitted " + currentHealth);
            float H = 0f, F = 0f, S1 = 0f, P = 0f, temp_multiplier = 1f;
            // Health reduction
            if (currentHealth >= 0)
            {
                for (int i = 0; i < currentWeaknesses.Length; i++ )
                {
                    temp_multiplier *= currentWeaknesses[i] ? damageData.specialWeaknesses[i].Value : 1f;              
                }

                for (int i = 0; i < damageData.damageTypes.Count; i++ )
                {

                    H -= damageData.damageTypes[i].Value;
                    P -= damageData.poiseDamageTypes[i].Value;
                    // S2 -= damageData.poiseDamageTypes[i].Value;
                }
                ValueEditing(H * temp_multiplier, F, S1, P * temp_multiplier);
            }
            // if (currentHealth <= 0)
            // {
            //     Die();
            //     return;
            // }
            // HandlePoiseBreak(damageData);

            curKB_dir = damageData.knockbackDirection;
            curKB_index = damageData.damageLevel >= 3 ? damageData.damageLevel + 1 : damageData.knockbackindex;

            // Logic for handling poise break effects
            // if (damageData.damageLevel == 0) return; // shake
            // if (!currentDizzyDamageLevel[damageData.damageLevel]) return; // shake
            if (currentPoise <= 0f || currentHealth <= 0f) // knock
            {
                switch (damageData.damageLevel)
                {
                    case 1:
                    case 2:
                        curKB_getHitType = 2 * curKB_index - 3 + 2;
                        break;
                    case 3:
                        curKB_getHitType = 9;
                        break;
                    case 4:
                        curKB_getHitType = 11;
                        break;
                }
                curKB_getHit = curKB_getHit < 2 ? 2 : curKB_getHit;
                ValueEditing(0f, 0f, 0f, maxPoise); // reset poise after poise break
            }
            else if (forceDizzyDamageLevel[damageData.damageLevel]) // || temp_multiplier > 1.1f) // knock
            {
                switch (damageData.damageLevel)
                {
                    case 1:
                    case 2:
                        curKB_getHitType = 2 * curKB_index - 3 + damageData.damageLevel;
                        break;
                    case 3:
                        curKB_getHitType = 8;
                        break;
                    case 4:
                        curKB_getHitType = 10;
                        break;
                }
                curKB_getHit = curKB_getHit < 2 ? 2 : curKB_getHit;
            }
            else
            {
                curKB_getHitTimeout = 1f;
                curKB_getHit = curKB_getHit < 2 ? 1 : curKB_getHit;
            }

            if (_playerMain._enemyDecision.applyQLearning)
            {
                // can be called in animation event to add reward for player, like heal or stamina regen, based on the damageData and current state
                _playerMain._enemyDecision.PlayerHittedEnemy();
            }
        }
    }

    public void UpdateHandleReward()
    {
        for (int i = 0; i < hitboxes.Length; i++)
        {
            lock (hitboxes[i]._rewardLock)
            {
                if (hitboxes[i].currentPendingRewardList.Count == 0) continue;
                // DamageData damageData = hitboxes[i].currentPendingRewardList[0];
                // todo: q learning
                if (_playerMain._enemyDecision.applyQLearning)
                {
                    // can be called in animation event to add reward for player, like heal or stamina regen, based on the damageData and current state
                    _playerMain._enemyDecision.EnemyHittedPlayer();
                }
                // ValueEditing(0f, 0f, 0f, 5f); // can edit to add reward for player here, like heal or stamina regen

                hitboxes[i].currentPendingRewardList.RemoveAt(0);
            }
        }
    }

    public bool CanHeal()
    {
        return currentHealth > 0f && currentHealth < maxHealth;
    }

    public void ValueEditing(float H, float F, float S1, float P) // H, F, S1, P
    {
        currentHealth += H;
        currentFocus += F;
        currentStamina += S1;
        currentPoise += P;
        // currentStance += S2;

        if (currentHealth <= 0f)
            currentHealth = -1000f;
        if (currentFocus <= 0f)
            currentFocus = 0f;
        // if (currentStamina <= 0f)
        //     currentStamina = 0f;
        if (currentPoise <= 0f)
            currentPoise = -0.1f;
        // if (currentStance <= 0f)
        //     currentStance = -0.1f;

        if (currentHealth >= maxHealth)
            currentHealth = maxHealth;
        if (currentFocus >= maxFocus)
            currentFocus = maxFocus;
        if (currentStamina >= maxStamina)
            currentStamina = maxStamina;
        if (currentPoise >= maxPoise)
            currentPoise = maxPoise;
        // if (currentStance >= maxStance)
        //     currentStance = maxStance;

        // _animator.SetFloat(_animIDHealth, currentHealth);
        // _animator.SetFloat(_animIDPoise, currentPoise);
        // _animator.SetFloat(_animIDStance, currentStance);
        // _playerMain.
    }

    public void PushVE(DamageData VEdata, float H = 0f, float F = 0f, float S = 0f, float P = 0f, bool doEdit = false)
    {
        lock (_veLock)
        {
            if (doEdit)
            {
                float[] HFSP = { 0f, 0f, 0f, 0f };
                HFSP[0] = H; HFSP[1] = F; HFSP[2] = S; HFSP[3] = P;
                for (int i = 0; i < 4; i++ )
                    currentPendingVEList[0].damageTypes[i].Value = HFSP[i];
            }
            currentPendingVEList.Add(VEdata);
        }
    }

    public void UpdatePopVE()
    {
        lock (_veLock)
        {
            if (currentPendingVEList.Count == 0) return;

            float[] HFSP = { 0f, 0f, 0f, 0f }; 
            for (int i = 0; i < 4; i++ )
                HFSP[i] = currentPendingVEList[0].damageTypes[i].Value;
            currentPendingVEList.RemoveAt(0);

            if (currentHealth <= 0f) return; // died
            ValueEditing(HFSP[0], HFSP[1], HFSP[2], HFSP[3]);
        }
    }

    public void SetInvincible(bool hb_head, bool hb_body, bool hb_stomach, bool hb_arms, bool hb_legs, bool hb_wkpt, bool hb_spec)
    {
        foreach (var hurtbox in hurtboxes)
        {
            switch (hurtbox.hurtboxType)
            {
                case Hurtbox.HurtboxType.Head:
                    hurtbox.invincible = hb_head;
                    break;
                case Hurtbox.HurtboxType.Body:
                    hurtbox.invincible = hb_body;
                    break;
                case Hurtbox.HurtboxType.Stomach:
                    hurtbox.invincible = hb_stomach;
                    break;
                case Hurtbox.HurtboxType.Arms:
                    hurtbox.invincible = hb_arms;
                    break;
                case Hurtbox.HurtboxType.Legs: 
                    hurtbox.invincible = hb_legs;
                    break;
                case Hurtbox.HurtboxType.WeakPoint:
                    hurtbox.invincible = hb_wkpt;
                    break;
                case Hurtbox.HurtboxType.Special:
                    hurtbox.invincible = hb_spec;
                    break;
                default:
                    hurtbox.invincible = false;
                    break;
            }
        }
    }

    public void SetWeakness(bool weaknessStatus, int weaknessIndex)
    {
        if (weaknessIndex >= 0 && weaknessIndex < currentWeaknesses.Length)
        {
            currentWeaknesses[weaknessIndex] = weaknessStatus;
        }
    }

    public void ActivateAttack(int hitboxIndex, int weaponDataIndex, int weaponBuffDataIndex, int skillDataIndex)
    {
        if (hitboxIndex >= 0 && hitboxIndex < hitboxes.Length)
        {
            hitboxes[hitboxIndex].UpdateDamageData(weaponDatas[weaponDataIndex], 0);
            hitboxes[hitboxIndex].UpdateDamageData(weaponBuffDatas[weaponBuffDataIndex], 1);
            hitboxes[hitboxIndex].UpdateDamageData(skillDatas[skillDataIndex], 2);
            // hitboxes[hitboxIndex].UpdateDamageData(weaknessDatas[0], 3);
            hitboxes[hitboxIndex].UpdateDamageData(weaponDatas[0], 4);
            hitboxes[hitboxIndex].ActivateHitbox();
        }
    }

    public void DeactivateAttack(int hitboxIndex)
    {
        if (hitboxIndex >= 0 && hitboxIndex < hitboxes.Length)
        {
            hitboxes[hitboxIndex].DisableHitbox();
        }
    }

    public void DeactivateAllHitboxes()
    {
        foreach (var hitbox in hitboxes)
        {
            hitbox.DisableHitbox();
        }
    }

    public void CurrentStatus()
    {
        if (currentHealth <= 0f || currentPoise <= 0f || currentStamina <= 0f)
        {
            _playerMain.nextStateID = 0;
        }
    }

    public void UpdatePassiveEffects()
    {
        if (!currentPassiveEffectActive)
        {
            currentPassiveEffectTimeout = 0.5f;
            return;
        }
        
        currentPassiveEffectTimeout -= Time.deltaTime;

        if (currentPassiveEffectTimeout <= 0f)
        {
            currentPassiveEffectTimeout = -0.1f;
            ValueEditing(0f, 0f, Time.deltaTime * maxStamina * 0.333f, 0f); // stamina regen, can edit to add other passive effects here
        }
    }

    public void UpdateCharmsEffects()
    {
        // todo: Charms
    }

    public void Enable_VFX(int vfxIndex)
    {
        // can be called in animation event
    }

    public void Disable_VFX(int vfxIndex)
    {
        // can be called in animation event
    }

    public void Enable_SFX(int sfxIndex)
    {
        // can be called in animation event
    }

    public void Disable_SFX(int sfxIndex)
    {
        // can be called in animation event
    }

    public void Die()
    {
        // Handle player death logic here (e.g., play death animation, disable controls, etc.)
    }
}
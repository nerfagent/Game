using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

[Serializable]
public class SkeletonSwordDecision : MonoBehaviour
{
    public SkeletonSwordMain_A0 _playerMain;
    [SerializeField] public EnemySkill[] battleSkills;
    /*
        0. idle L
        1. idle R
        2. attack 1
        3. attack 2
        4. attack 3
        5. attack 4
    */
    public int[] equippedSkillsR = { 5, -1, 12, -1, 11 }; // Max 5
    public int[] equippedSkillsL = { -1, 10, -1, 13, -1 }; // Max 5
    public int[] equippedItems = { -1, -1, -1, 14, 15 }; // Max 5
    public int currentEquippedSkillR = 0;
    public int currentEquippedSkillL = 1;
    public int currentEquippedItem = 3;
    public int curNextSkill = 0;

    public float minFloat = 0f;
    public float maxFloat = 100f;
    public float randomFloat = 100f;

    public bool applyQLearning = false; // can be used to enable or disable q learning in Decide function

    /*
        0: idle/walk/run blend
        1: dodge
        2: heal
        3: attack
        4: jump
        5: skillR
        6: skillL
        7: item
        8: sprint
        9: camlock
        10: interact
        11: switchR
        12: switchL
        13: switchItem

        14: get hit
        20: idle
        21: move back
        22: chase
    */

    public int _animID_getHitType;
    public int _animID_getHit;
    public int _animID_HP;
    public int _animID_FP; // temp ui
    public int _animID_SP; // temp ui
    public int _animID_PP; // temp ui
    public int _animIDGrounded;
    public int _animIDNextSkill;
    public int _animIDNextState;
    public int _animIDDeath;
    public int _animIDActionLock;

    public int _animIDSpeed;
    public int _animIDMotionSpeed;
    public int _animIDVV;

    public int _animIDGo;

    // attack behaviour
    [SerializeField] public int[] sideCounter;
    [SerializeField] public int[] farCounter;
    [SerializeField] public int[] drinkCounter;

    // navigation
    public Transform Target;
    public NavMeshAgent m_Agent;
    public float m_Distance;
    public float CloseChaseDistance;
    public float FarChaseDistance;
    public bool isHunting;

    // for debugging
    public bool debugMode = false;

    public void AssignAnimationIDs()
    {
        _animID_getHitType = Animator.StringToHash("getHitType");
        _animID_getHit = Animator.StringToHash("getHit");
        _animID_HP = Animator.StringToHash("HP");
        _animID_FP = Animator.StringToHash("FP"); // temp ui
        _animID_SP = Animator.StringToHash("SP"); // temp ui
        _animID_PP = Animator.StringToHash("PP"); // temp ui
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDNextSkill = Animator.StringToHash("NextSkill");
        _animIDNextState = Animator.StringToHash("NextState");
        _animIDDeath = Animator.StringToHash("Death");
        _animIDActionLock = Animator.StringToHash("ActionLock");

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDVV = Animator.StringToHash("VV"); // temp ui

        _animIDGo = Animator.StringToHash("Go");

        m_Agent = GetComponent<NavMeshAgent>();
    }

    public void Decide()
    {
        // Implement your decision logic here
        if (debugMode) Debug.Log("Deciding next action based on current state and inputs.");

        // Hunt behaviour
        m_Distance = Vector3.Distance(transform.position, Target.position);
        UpdateChaseState();
        
        // Attack behaviour
        nextSkill();
            // side handling
            SideHandling();
            // far handling
            FarHandling();
            // drink handling
            // DrinkHandling();

        if (m_Distance < battleSkills[curNextSkill].minDistance)
        {
            // move back
            _playerMain.nextStateID = _playerMain.nextStateID == 14 ? 14 : 21;
        }
        if (m_Distance > battleSkills[curNextSkill].maxDistance)
        {
            // chase
            _playerMain.nextStateID = _playerMain.nextStateID == 14 ? 14 : 22;
        }
        if (!isHunting)
        {
            // idle
            _playerMain.nextStateID = _playerMain.nextStateID == 14 ? 14 : 20;
        }

        _playerMain._animator.SetInteger(_animID_getHitType, _playerMain._combatSystem.curKB_getHitType);
        _playerMain._animator.SetInteger(_animID_getHit, _playerMain._combatSystem.curKB_getHit);
        _playerMain._animator.SetFloat(_animID_HP, _playerMain._combatSystem.currentHealth);
        _playerMain._animator.SetFloat(_animID_FP, _playerMain._combatSystem.currentFocus); // temp ui
        _playerMain._animator.SetFloat(_animID_SP, _playerMain._combatSystem.currentStamina); // temp ui
        _playerMain._animator.SetFloat(_animID_PP, _playerMain._combatSystem.currentPoise); // temp ui
        _playerMain._animator.SetBool(_animIDGrounded, _playerMain.Grounded);
        _playerMain._animator.SetInteger(_animIDNextSkill, curNextSkill);
        _playerMain._animator.SetInteger(_animIDNextState, _playerMain.nextStateID);

        _playerMain._animator.SetBool(_animIDGo, true); // trigger animation transition, can be used in Animator to control the character's animation state
    }

    private void UpdateChaseState()
    {
        if (isHunting)
        {
            // Implement chase behavior
            isHunting = m_Distance < FarChaseDistance ? true : false;
        }
        else
        {
            isHunting = m_Distance < CloseChaseDistance ? true : false;
        }
    }

    // ===========================

    private void nextSkill()
    {
        // if the current skill is an attack skill, keep the current skill, can be used in Animator to trigger certain skill animation
        if (curNextSkill >= 2)
        {
            return;
        }

        // L R switching
        if (_playerMain._anim_enemySkillIndex < 2 && curNextSkill < 2) // if it is idle or dodge, set next state ID to the index of the skill
        {
            float idleRLFloat = UnityEngine.Random.Range(0f, 1f);
            if (idleRLFloat < Time.deltaTime * 0.333f) // geometric distribution E[X] = 1/p
            {
                curNextSkill = (curNextSkill + 1) % 2; // switch between idle L and idle R
            }
        }

        // check Stamina, if it is below the stamina cost of the current skill, switch to idle
        float idle2AttackFloat = UnityEngine.Random.Range(0f, 1f);
        if (idle2AttackFloat > Time.deltaTime * _playerMain._combatSystem.currentStamina / _playerMain._combatSystem.maxStamina) // the lower the stamina, the more likely to switch to attack
        {
            return; // stay in idle
        }

        // reset (max/min)Float to sum of posibility of all skills, can be used for random skill selection in enemy AI
        minFloat = battleSkills[0].posibility + battleSkills[1].posibility;
        maxFloat = 0f;
        for (int i = 0; i < battleSkills.Length; i++)
        {
            maxFloat += battleSkills[i].posibility;
        }

        // Draw a random float between minFloat and maxFloat, can be used for random skill selection in enemy AI
        randomFloat = UnityEngine.Random.Range(minFloat, maxFloat);
        float cumulativePosibility = 0f;
        for (int i = 0; i < battleSkills.Length; i++)
        {
            if (randomFloat >= cumulativePosibility && randomFloat < cumulativePosibility + battleSkills[i].posibility)
            {
                curNextSkill = i;
                if (debugMode) Debug.Log("Selected Skill: " + battleSkills[i].Name + " with posibility: " + battleSkills[i].posibility);
                break;
            }
            cumulativePosibility += battleSkills[i].posibility;
        }
    }

    private void SideHandling()
    {
        if (Target == null) return;

        // Check if player is outside enemy FOV (120° total -> 60° either side)
        Vector3 toTarget = (Target.position - transform.position).normalized;
        float angleDeg = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
        if (Mathf.Abs(angleDeg) <= 60f)
        {
            // player is inside forward FOV; skip side/behind handling
            return;
        }

        if (curNextSkill >= 2 &&
            sideCounter.Length > 0 &&
            // m_Distance < battleSkills[curNextSkill].maxDistance &&
            m_Distance < battleSkills[sideCounter[0]].maxDistance )
        {
            float tempFloat = UnityEngine.Random.Range(0f, 1f);
            int tempInt = UnityEngine.Random.Range(0, sideCounter.Length);
            tempInt = sideCounter[tempInt];
            if (tempFloat <
                Time.deltaTime *
                battleSkills[tempInt].posibility /
                battleSkills[tempInt].defaultPosibility ) // geometric distribution E[X] = 1/p // times 1 / next expected happen time
            {
                curNextSkill = tempInt;
                if (debugMode) Debug.Log($"SideHandling: player angle {angleDeg:F1}°, switching to skill {curNextSkill}");
            }
        }
    }

    private void FarHandling()
    {
        if (curNextSkill >= 2 &&
            farCounter.Length > 0 &&
            m_Distance > battleSkills[curNextSkill].maxDistance &&
            m_Distance > battleSkills[farCounter[0]].minDistance)
        {
            float tempFloat = UnityEngine.Random.Range(0f, 1f);
            int tempInt = UnityEngine.Random.Range(0, farCounter.Length);
            tempInt = farCounter[tempInt];
            if (tempFloat < 
                Time.deltaTime * 
                battleSkills[tempInt].posibility / 
                battleSkills[tempInt].defaultPosibility ) // geometric distribution E[X] = 1/p // times 1 / next expected happen time
            {
                curNextSkill = tempInt;
            }
        }
    }

    // ===========================

    public void EnemyHittedPlayer()
    {
        // can be called in animation event to add reward for player, like heal or stamina regen, based on the damageData and current state
        if (_playerMain._anim_enemySkillIndex >= 2 && 
            battleSkills[_playerMain._anim_enemySkillIndex].posibility - 10f >= battleSkills[_playerMain._anim_enemySkillIndex].minPosibility) // if it is an attack skill and the posibility is above min, decrease posibility for next time
        {
            battleSkills[_playerMain._anim_enemySkillIndex].posibility -= 10f;
        }
    }

    public void PlayerHittedEnemy()
    {
        // can be called in animation event to add reward for player, like heal or stamina regen, based on the damageData and current state
        if (_playerMain._anim_enemySkillIndex >= 2 && 
            battleSkills[_playerMain._anim_enemySkillIndex].posibility + 30f <= battleSkills[_playerMain._anim_enemySkillIndex].maxPosibility) // if it is an attack skill and the posibility is above min, decrease posibility for next time
        {
            battleSkills[_playerMain._anim_enemySkillIndex].posibility += 30f;
        }
    }
}
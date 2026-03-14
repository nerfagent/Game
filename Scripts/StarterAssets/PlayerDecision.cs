using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

[Serializable]
public class BattleSkill
{
    public string Name;
    public int index;
    public bool equipped;
}
[Serializable]
public class PlayerDecision : MonoBehaviour
{
    public PlayerMain_A0 _playerMain;
    public List<BattleSkill> battleSkills = new List<BattleSkill>
    {
        // Inherit from Elden Ring
        new BattleSkill { Name = "base attack 1", index = 0, equipped = false },
        new BattleSkill { Name = "base attack 2", index = 1, equipped = false },
        new BattleSkill { Name = "roll attack", index = 2, equipped = false },
        new BattleSkill { Name = "run attack", index = 3, equipped = false },
        new BattleSkill { Name = "jump attack", index = 4, equipped = false },
        new BattleSkill { Name = "前步上砍", index = 5, equipped = true },
        new BattleSkill { Name = "弧撼地", index = 6, equipped = true },
        new BattleSkill { Name = "環撼地", index = 7, equipped = true },
        new BattleSkill { Name = "劍氣", index = 8, equipped = true },
        new BattleSkill { Name = "投壺", index = 9, equipped = true },
        new BattleSkill { Name = "抹油", index = 10, equipped = true },
        new BattleSkill { Name = "白光突擊", index = 11, equipped = true },
        new BattleSkill { Name = "洛特大蓄力突刺", index = 12, equipped = true },
        new BattleSkill { Name = "盾牌", index = 13, equipped = true },
        new BattleSkill { Name = "鉤爪", index = 14, equipped = true },
        new BattleSkill { Name = "火炬", index = 15, equipped = true }
    };
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
    public int[] equippedSkillsR = { 5, -1, 12, -1, 11 }; // Max 5
    public int[] equippedSkillsL = { -1, 10, -1, 13, -1 }; // Max 5
    public int[] equippedItems = { -1, -1, -1, 14, 15 }; // Max 5
    public int currentEquippedSkillR = 0;
    public int currentEquippedSkillL = 1;
    public int currentEquippedItem = 3;
    public int curNextSkill = -1;

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
    public int _animIDSpeed;
    public int _animIDMotionSpeed;

    public int _animIDGo;

    // for debugging
    public bool debugMode = false;

    public void SyncInventory()
    {

    }

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

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        _animIDGo = Animator.StringToHash("Go");
    }

    public void Decide()
    {
        // Implement your decision logic here
        if (debugMode) Debug.Log("Deciding next action based on current state and inputs.");

        // Example decision logic:
        // if (input.attackPressed)
        // {
        //     _playerMain.PerformAttack();
        // }
        // else if (input.dodgePressed)
        // {
        //     _playerMain.PerformDodge();
        // }
        // else
        // {
        //     _playerMain.PerformIdle();
        // }
        if (_playerMain.BT_SwitchR)
        {
            nextSkill(0);
            _playerMain.BT_SwitchR = false; // reset input
        }
        if (_playerMain.BT_SwitchL)
        {
            nextSkill(1);
            _playerMain.BT_SwitchL = false; // reset input
        }
         if (_playerMain.BT_Interact)
        {
            nextSkill(2);
            _playerMain.BT_Interact = false; // reset input
        }

        switch (_playerMain.nextStateID) // curNextSkill
        {
            case 5:
                curNextSkill = equippedSkillsR[currentEquippedSkillR];
                break;
            case 6:
                curNextSkill = equippedSkillsL[currentEquippedSkillL];
                break;
            case 7:
                curNextSkill = equippedItems[currentEquippedItem];
                break;
            default:
                break;
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

    private void nextSkill(int hand) // 0: right, 1: left, 2: item
    {
        switch (hand)
        {
            case 0: // Right hand
                for (int i = 0; i < equippedSkillsR.Length; i++)
                {
                    currentEquippedSkillR = (currentEquippedSkillR + 1) % equippedSkillsR.Length;
                    if (equippedSkillsR[currentEquippedSkillR] != -1) break;
                }
                break;
            case 1: // Left hand
                for (int i = 0; i < equippedSkillsL.Length; i++)
                {
                    currentEquippedSkillL = (currentEquippedSkillL + 1) % equippedSkillsL.Length;
                    if (equippedSkillsL[currentEquippedSkillL] != -1) break;
                }
                break;
            case 2: // Item
                for (int i = 0; i < equippedItems.Length; i++)
                {
                    currentEquippedItem = (currentEquippedItem + 1) % equippedItems.Length;
                    if (equippedItems[currentEquippedItem] != -1) break;
                }
                break;
        }
    }
}
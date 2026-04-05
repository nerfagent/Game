using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// ESB = EnemyStateBehaviour
public class ESB_DIE_1 : StateMachineBehaviour
{
    public SkeletonSwordMain_A0 _playerMain;
    public int lastInfo;
    public int currentInfo = -1;

    public bool debugMode = false;
    // This function is called when the state is entered
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_playerMain == null)
        {
            _playerMain = animator.GetComponent<SkeletonSwordMain_A0>();
        }
        lastInfo = currentInfo;
        currentInfo = stateInfo.shortNameHash;
        // Call the custom function when entering this state
        CustomFunction(animator, stateInfo, layerIndex);
    }

    private void CustomFunction(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement your custom logic here
        if (debugMode) Debug.Log("Entered State: " + stateInfo.shortNameHash);
        // You can access other components or the Animation state here
        // Example: animator.gameObject.GetComponent<MyComponent>().DoSomething();

        /*
            order from anim:

            clear anim flag
            control passive effect
            clear get hit trigger
            clear input
            set invincible
            move & look (sometimes in update) (include jump)
            hitbox
            value editing
            call other functions

            vfx
            sfx
        */
        _playerMain._animator.SetBool(_playerMain._enemyDecision._animIDGo, false); // reset animation trigger, can be used in Animator to control the character's animation state
        _playerMain._combatSystem.currentPassiveEffectActive = false; // control passive effect, can be used in UpdatePassiveEffects to handle passive effects that have a duration, like poison or regen
        _playerMain._combatSystem.curKB_getHit %= 2;
            _playerMain.nextStateID = 0; // set next state ID and reset enemy skill index, can be used in PlayerMain_A0 to trigger certain events in next state and control enemy skills
            _playerMain._anim_enemySkillIndex = 0;
            _playerMain._enemyDecision.curNextSkill = 0;
        _playerMain._combatSystem.SetInvincible(true, true, true, true, true, true, true); // set invincibility, can be used in Hurtbox to ignore damage
            _playerMain._anim_agentStop = true;
            _playerMain._anim_agentSpeed = 0f;
            _playerMain._anim_VV = -1f; // reset move and look direction, can be used in PlayerMain_A0 to control the character's movement and rotation
            _playerMain._animator.SetFloat(_playerMain._enemyDecision._animIDSpeed, 0f);
        _playerMain._combatSystem.DeactivateAllHitboxes();
        // _playerMain._combatSystem.PushVE(_playerMain._combatSystem.VEDatas[2]); // 2 is BA
        _playerMain._animator.SetBool(_playerMain._enemyDecision._animIDDeath, true);
        // vfx
        // sfx
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // public bool _anim_flag = false; // general use, can be set to trigger certain events in anim, reset by anim
        // public float _anim_moveSpeed = 0f;
        // public Vector3 _anim_moveDirection = Vector3.forward;
        // public float _anim_lookRotation = 0f;
        // public float _anim_VV = -1f;
        // public int _anim_enemySkillIndex = 0; // can be used to trigger certain enemy skill in anim, reset by anim

        // Implement any logic that needs to be checked or updated every frame while in this state
        _playerMain._anim_moveSpeed = 0f;
        _playerMain._anim_moveDirection = Vector3.zero;
        _playerMain._anim_lookRotation = _playerMain.GetYaw();

        // float targetSpeed = _playerMain._anim_moveSpeed;
        // _playerMain._animator.SetFloat(_playerMain._enemyDecision._animIDSpeed, targetSpeed);
    }
}
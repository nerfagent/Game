using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// ESB = EnemyStateBehaviour
public class ESB_DEC_1 : StateMachineBehaviour
{
    public SkeletonSwordMain_A0 _playerMain;
    public int lastInfo;
    public int currentInfo = -1;

    // public bool GoFlag = false;
        public bool passiveEffectFlag = true;
    // clear get hit trigger
        public bool actionLockFlag = false;
        public bool deathFlag = false;
    public bool hb_head = false, hb_body = false, hb_stomach = false, hb_arms = false, hb_legs = false, hb_wkpt = false, hb_spec = false;
        public float VV = -1f;
        public float targetSpeed = 0f;
        public Vector3 targetMovingDirection = Vector3.forward;
        public bool lookingPlayer = false;
        public bool agentStopped = true;
        // public Vector3 agentDestination = Vector3.zero;
        public float agentSpeed = 0f;
    public bool DeactivateAttack = true;
    public bool ActivateAttack = false;
    public int hitboxIndex = 0, weaponDataIndex = 0, weaponBuffDataIndex = 0, skillDataIndex = 0;
        public bool VEFlag = false; 
        public int VEDataIndex = 0;

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
            _playerMain._combatSystem.currentPassiveEffectActive = passiveEffectFlag; // control passive effect, can be used in UpdatePassiveEffects to handle passive effects that have a duration, like poison or regen
        // _playerInput._combatSystem.curKB_getHit %= 2;
            _playerMain.nextStateID = 0; // set next state ID and reset enemy skill index, can be used in PlayerMain_A0 to trigger certain events in next state and control enemy skills
            _playerMain._anim_enemySkillIndex = 0; // set enemy skill index to current next skill, can be used in anim to trigger certain enemy skill
            // _playerMain._enemyDecision.curNextSkill = 0;
            _playerMain._animator.SetBool(_playerMain._enemyDecision._animIDActionLock, actionLockFlag);
            _playerMain._animator.SetBool(_playerMain._enemyDecision._animIDDeath, deathFlag);
        _playerMain._combatSystem.SetInvincible(hb_head, hb_body, hb_stomach, hb_arms, hb_legs, hb_wkpt, hb_spec); // set invincibility, can be used in Hurtbox to ignore damage
            // _playerMain._anim_agentStop = true;
            // _playerMain._anim_agentSpeed = 0f;
            _playerMain._anim_VV = VV; // reset move and look direction, can be used in PlayerMain_A0 to control the character's movement and rotation
            _playerMain._animator.SetFloat(_playerMain._enemyDecision._animIDSpeed, Mathf.Max(targetSpeed, agentSpeed));
        if (DeactivateAttack) _playerMain._combatSystem.DeactivateAllHitboxes();
        if (ActivateAttack) _playerMain._combatSystem.ActivateAttack(hitboxIndex, weaponDataIndex, weaponBuffDataIndex, skillDataIndex);
            if (VEFlag) _playerMain._combatSystem.PushVE(_playerMain._combatSystem.VEDatas[VEDataIndex]);
        // call other functions
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
        _playerMain._anim_moveSpeed = targetSpeed;
        _playerMain._anim_moveDirection = targetMovingDirection;
        _playerMain._anim_lookRotation = lookingPlayer ? _playerMain.GetRotationToFace(_playerMain._enemyDecision.Target) : _playerMain.GetYaw();

        _playerMain._anim_agentStop = agentStopped;
        _playerMain._anim_agentSpeed = agentSpeed;
        // float targetSpeed = _playerMain._anim_moveSpeed;
        // _playerMain._animator.SetFloat(_playerMain._enemyDecision._animIDSpeed, targetSpeed);
    }
}
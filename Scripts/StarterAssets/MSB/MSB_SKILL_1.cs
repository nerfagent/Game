using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// MSB = MyStateBehaviour
public class MSB_SKILL_1 : StateMachineBehaviour
{
    public PlayerMain_A0 _playerMain;
    public int lastInfo;
    public int currentInfo = -1;

    // public bool GoFlag = false;
        public bool passiveEffectFlag = false;
    // clear get hit trigger
        public bool resetNextStateFlag = true;
        public bool deathFlag = false;
    public bool hb_head = false, hb_body = false, hb_stomach = false, hb_arms = false, hb_legs = false, hb_wkpt = false, hb_spec = false;
        public float VV = -1f;
        public float f_speed = 0f;
        public Vector3 v_moveDirection = Vector3.forward;
        public bool b_useGetYaw = false;
        public float f_camAngleFactor = 1f;
        public bool b_useCam = true;
        public float f_localLookOffset = 0f;
        public bool b_useLocalLook = true;
    public bool DeactivateAttack = true;
    public bool ActivateAttack = false;
    public int hitboxIndex = 0, weaponDataIndex = 0, weaponBuffDataIndex = 0, skillDataIndex = 0;
        public bool VEFlag = true;
        public int VEDataIndex = 0;

    public bool debugMode = false;
    // This function is called when the state is entered
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_playerMain == null)
        {
            _playerMain = animator.GetComponent<PlayerMain_A0>();
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
        _playerMain._animator.SetBool(_playerMain._playerDecision._animIDGo, false); // reset animation trigger, can be used in Animator to control the character's animation state
            _playerMain._combatSystem.currentPassiveEffectActive = passiveEffectFlag; // control passive effect, can be used in UpdatePassiveEffects to handle passive effects that have a duration, like poison or regen
        // _playerInput._combatSystem.curKB_getHit %= 2;
            if (resetNextStateFlag) _playerMain.nextStateID = 0; // set next state ID and reset enemy skill index, can be used in PlayerMain_A0 to trigger certain events in next state and control enemy skills
            // _playerMain._anim_enemySkillIndex = _playerMain._playerDecision.curNextSkill; // set enemy skill index to current next skill, can be used in anim to trigger certain enemy skill
            // _playerMain._playerDecision.curNextSkill = 0;
            _playerMain._animator.SetBool(_playerMain._playerDecision._animIDDeath, deathFlag);
        _playerMain._combatSystem.SetInvincible(hb_head, hb_body, hb_stomach, hb_arms, hb_legs, hb_wkpt, hb_spec); // set invincibility, can be used in Hurtbox to ignore damage
            _playerMain._anim_VV = VV; // reset move and look direction, can be used in PlayerMain_A0 to control the character's movement and rotation
            // _playerMain._animator.SetFloat(_playerMain._playerDecision._animIDSpeed, targetSpeed);
        if (DeactivateAttack) _playerMain._combatSystem.DeactivateAllHitboxes();
        if (ActivateAttack) _playerMain._combatSystem.ActivateAttack(hitboxIndex, weaponDataIndex, weaponBuffDataIndex, skillDataIndex);
            if (VEFlag) _playerMain._combatSystem.PushVE(_playerMain._combatSystem.VEDatas[VEDataIndex]);
        // call other functions
        // vfx
        // sfx
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // float targetSpeed,
        // Vector3 localMovingDirection,
        // float globalTargetRotationOfPlayer,
        // float cameraAngleFactor,
        // bool useCameraRotation,
        // float localLookOffset = 0f,
        // bool useLocalLook = false

        // Implement any logic that needs to be checked or updated every frame while in this state
        _playerMain._anim_targetSpeed = f_speed;
        _playerMain._anim_localMovingDirection = v_moveDirection;
        _playerMain._anim_globalTargetRotationOfPlayer = b_useGetYaw ? _playerMain.GetYaw() : 0f;
        _playerMain._anim_cameraAngleFactor = f_camAngleFactor;
        _playerMain._anim_useCameraRotation = b_useCam;
        _playerMain._anim_localLookOffset = f_localLookOffset;
        _playerMain._anim_useLocalLook = b_useLocalLook;
    }
}
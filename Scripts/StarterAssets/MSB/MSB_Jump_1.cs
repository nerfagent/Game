using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// MSB = MyStateBehaviour
public class MSB_Jump_1 : StateMachineBehaviour
{
    public PlayerMain_A0 _playerMain;
    public int lastInfo;
    public int currentInfo = -1;

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
            set invinci
            move & look (sometimes in update) (include jump)
            hitbox
            vfx
            sfx
        */
        _playerMain._animator.SetBool(_playerMain._playerDecision._animIDGo, false); // reset animation trigger, can be used in Animator to control the character's animation state
        _playerMain._combatSystem.currentPassiveEffectActive = false; // control passive effect, can be used in UpdatePassiveEffects to handle passive effects that have a duration, like poison or regen
        // _playerInput._combatSystem.curKB_getHit %= 2;
        _playerMain.nextStateID = 0; // set next state ID, can be used in PlayerMain_A0 to trigger certain events in next state
        _playerMain._combatSystem.SetInvincible(false, false, true, false, true, false, false); // set invincibility, can be used in Hurtbox to ignore damage
        _playerMain._anim_VV = Mathf.Sqrt(1f * -1f * _playerMain.Gravity);// reset move and look direction, can be used in PlayerMain_A0 to control the character's movement and rotation
        _playerMain._combatSystem.DeactivateAllHitboxes();
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
        // _playerMain._anim_targetSpeed = _playerMain._input.move != Vector2.zero ? 3.5f : 0f;
        // _playerMain._anim_targetSpeed = _playerMain._input.sprint ? 6.5f : _playerMain._anim_targetSpeed;

        // _playerMain._anim_localMovingDirection = _playerMain.GetLocalMoveDirection();
        _playerMain._anim_globalTargetRotationOfPlayer = 0f;
        _playerMain._anim_cameraAngleFactor = 1f;
        _playerMain._anim_useCameraRotation = true;
        // _playerMain._anim_localLookOffset = _playerMain.GetRotateFromInput();
        _playerMain._anim_useLocalLook = true;
    }
}
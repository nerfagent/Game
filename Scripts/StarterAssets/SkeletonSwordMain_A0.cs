using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class SkeletonSwordMain_A0 : MonoBehaviour
{

    public Animator _animator;
    private CharacterController _controller;

    public CombatSystem_Skeleton_A0 _combatSystem;
    public SkeletonSwordDecision _enemyDecision;
    public StateMachineBehaviour[] SMBs;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    // movement
    public float _speed = 0f;
    public const float SpeedChangeRate = 10.0f;

    public bool Grounded = true;
    public const float GroundedOffset = -0.14f;
    public const float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    public float _verticalVelocity = 0f;
    public const float Gravity = -9.81f;
    public const float TerminalVelocity = -53f;

    // COMMANDS FROM ANIM
    public bool _anim_flag = false; // general use, can be set to trigger certain events in anim, reset by anim
    public float _anim_moveSpeed = 0f;
    public Vector3 _anim_moveDirection = Vector3.forward;
    public float _anim_lookRotation = 0f;
    public float _anim_VV = -1f;
    public bool _anim_agentStop = true;
    public float _anim_agentSpeed = 0f;

    public int _anim_enemySkillIndex = 0; // can be used to trigger certain enemy skill in anim, reset by anim

    // FUNCTIONAL INPUTS // sprint, interact, item, skill
    public int nextStateID = 0;
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

    private void Awake()
    {
    }

    private void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _combatSystem = GetComponent<CombatSystem_Skeleton_A0>();
        _enemyDecision = GetComponent<SkeletonSwordDecision>();

        _enemyDecision.AssignAnimationIDs();
    }

    private void Update()
    {
        GroundedCheck();
        FunctionalInput();

        // get hitted
        if (_combatSystem.isReady)
        {
            _combatSystem.CurrentStatus();
            _combatSystem.UpdatePassiveEffects();
            _combatSystem.UpdateHandleDamage();
            _combatSystem.UpdateHandleReward();
            nextStateID = _combatSystem.curKB_getHit > 1 ? 14 : nextStateID;
        }

        // Grounded, nextStateID, _combatSystem.curKB_getHit
        // enter anim
        _enemyDecision.Decide();

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
        // while (_anim_flag)
        // {
        //     // do nth
        // }
        MoveAndLook(_anim_moveSpeed, GetGlobalMovingDirection(_anim_moveDirection, _anim_lookRotation), _anim_lookRotation, _anim_VV);
        NavLogic();
        _combatSystem.UpdatePopVE();
    }

    private void LateUpdate()
    {
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void FunctionalInput()
    {
        nextStateID = 5; // skillR
    }

    // ============================

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character // left as final step
        // if (_hasAnimator)
        // {
        //     _animator.SetBool(_animIDGrounded, Grounded);
        // }
    }

    // ============================

    private void GradMove(float targetSpeed)
    {
        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * 1f,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
    }

    /// <summary>
    /// Compute a global Y rotation (degrees) such that:
    /// Quaternion.Euler(0f, result, 0f) * localDir ~= worldMovingDirection
    /// Projects both vectors onto the XZ plane and returns the needed yaw.
    /// If either vector is near-zero, returns the player's current yaw.
    /// </summary>
    public float GetGlobalRotationFromWorldAndLocal(Vector3 worldMovingDirection, Vector3 localDir)
    {
        // flatten to XZ plane
        Vector3 w = Vector3.ProjectOnPlane(worldMovingDirection, Vector3.up);
        Vector3 l = Vector3.ProjectOnPlane(localDir, Vector3.up);

        // if either is invalid, fall back to current yaw
        if (w.sqrMagnitude < 1e-6f || l.sqrMagnitude < 1e-6f)
        {
            return transform.rotation.eulerAngles.y;
        }

        w.Normalize();
        l.Normalize();

        // yaw for a vector v is atan2(v.x, v.z) in degrees (Unity forward = +Z)
        float worldYaw = Mathf.Atan2(w.x, w.z) * Mathf.Rad2Deg;
        float localYaw = Mathf.Atan2(l.x, l.z) * Mathf.Rad2Deg;

        // final rotation that when applied to localDir yields worldMovingDirection
        float finalGlobalRotation = worldYaw - localYaw;

        // normalize to shortest angle [-180,180)
        finalGlobalRotation = Mathf.DeltaAngle(0f, finalGlobalRotation);

        return finalGlobalRotation;
    }

    /// <summary>
    /// Returns the yaw (Y rotation in degrees) the player should have to face the given target Transform.
    /// If target is null or too close, returns the current player yaw.
    /// </summary>
    public float GetRotationToFace(Transform target)
    {
        if (target == null) return GetYaw();

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 1e-6f) return GetYaw();

        // atan2(x, z) because Unity's forward is +Z
        return Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
    }

    public Vector3 GetGlobalMovingDirection(Vector3 localMovingDirection, float rotationDegrees)
    {
        // project to XZ and bail out on near-zero input
        Vector3 localFlat = Vector3.ProjectOnPlane(localMovingDirection, Vector3.up);

        // build yaw rotation from provided float
        Quaternion yawRot = Quaternion.Euler(0f, rotationDegrees, 0f);

        if (localFlat.sqrMagnitude < 1e-6f)
        {
            // fallback: use forward of provided yaw
            Vector3 forward = yawRot * Vector3.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        Vector3 worldDir = yawRot * localFlat;
        worldDir.y = 0f;
        return worldDir.normalized;
    }

    // Backwards-compatible overload that uses the current transform yaw
    public Vector3 GetGlobalMovingDirection(Vector3 localMovingDirection)
    {
        return GetGlobalMovingDirection(localMovingDirection, GetYaw());
    }

    public float GetYaw()
    {
        return transform.rotation.eulerAngles.y;
    }

    // ============================

    /// <summary>
    /// Move the player and orient its view based on the supplied targets.
    /// - targetSpeed: desired horizontal speed (units/sec)
    /// - targetMovingDirection: desired horizontal movement direction (world space)
    /// - targetRotationOfPlayer: desired player Y rotation in degrees (world space)
    /// This updates internal animation-driving fields and moves/rotates the CharacterController.
    /// </summary>
    public void MoveAndLook(float targetSpeed, Vector3 targetMovingDirection, float targetRotationOfPlayer, float VV = -1f)
    {
        // Update animation-driving fields
        targetMovingDirection = targetMovingDirection.sqrMagnitude > 0.000001f ? targetMovingDirection.normalized : Vector3.forward;

        // Horizontal velocity (world-space)
        Vector3 horizontal = new Vector3(targetMovingDirection.x, 0f, targetMovingDirection.z) * targetSpeed;

        if (_controller != null)
        {
            // Ground check and vertical velocity handling
            if (Grounded && _verticalVelocity < 0f)
            {
                // small negative to keep the controller grounded
                _verticalVelocity = -5f;
            }

            // VV
            if (VV > -0.1f) _verticalVelocity = VV;

            // apply gravity
            _verticalVelocity += Gravity * Time.deltaTime;
            _verticalVelocity = Mathf.Max(_verticalVelocity, TerminalVelocity);

            // combine horizontal + vertical and move using CharacterController.Move
            Vector3 move = horizontal;
            move.y = _verticalVelocity;

            _controller.Move(move * Time.deltaTime);
        }
        else
        {
            // Fallback (no collision handling) -- use only if you intentionally don't have a CharacterController
            transform.position += (horizontal + Vector3.up * _verticalVelocity) * Time.deltaTime;
        }

        // Smooth rotation
        const float RotationSpeedDegreesPerSecond = 720f;
        Quaternion targetRot = Quaternion.Euler(0f, targetRotationOfPlayer, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotationSpeedDegreesPerSecond * Time.deltaTime);

        // Update animator parameters if present // left as final step
        if (_hasAnimator && _animator != null)
        {
        //     // these parameter names are generic; change to match your animator if needed
        //     // _animator.SetFloat("MoveSpeed", _anim_moveSpeed);
        //     // _animator.SetFloat("MoveX", _anim_moveDirection.x);
        //     // _animator.SetFloat("MoveZ", _anim_moveDirection.z);
        //     // _animator.SetFloat("LookRotation", _anim_lookRotation);

            // _animator.SetFloat(_enemyDecision._animIDSpeed, targetSpeed);
            _animator.SetFloat(_enemyDecision._animIDMotionSpeed, 1f);
        }
    }

    // ============================
    public void NavLogic()
    {
        if (_enemyDecision.m_Agent == null || _enemyDecision.Target == null) return;
        _enemyDecision.m_Agent.isStopped = _anim_agentStop;
        if (_anim_agentStop == false) _enemyDecision.m_Agent.destination = _enemyDecision.Target.position;
        _enemyDecision.m_Agent.speed = _anim_agentSpeed;  
    }
}
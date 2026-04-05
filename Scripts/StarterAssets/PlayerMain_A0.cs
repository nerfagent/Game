using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerMain_A0 : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        public Animator _animator;
        private CharacterController _controller;
        public SAInputs _input;
        public CombatSystem_Player_A0 _combatSystem;
        public PlayerDecision _playerDecision;
        public StateMachineBehaviour[] SMBs;
        private GameObject _mainCamera;

        public GameObject CinemachineCameraTarget;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        public float CameraAngleOverride = 0.0f;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        // movement
        public float _speed = 0f;
        public const float SpeedChangeRate = 10.0f;

        public bool Grounded = true;
        public const float GroundedOffset = -0.14f;
        public const float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        public float _verticalVelocity = 0f;
        public float Gravity = -9.81f;
        public float TerminalVelocity = -53f;

        // COMMANDS FROM ANIM
        public bool _anim_flag = false; // general use, can be set to trigger certain events in anim, reset by anim

        public float _anim_targetSpeed = 0f;
        public Vector3 _anim_localMovingDirection = Vector3.forward;
        public float _anim_globalTargetRotationOfPlayer = 0f;
        public float _anim_cameraAngleFactor = 0f;
        public bool _anim_useCameraRotation = false;
        public float _anim_localLookOffset = 0f;
        public bool _anim_useLocalLook = false;
        public float _anim_VV = -1f;

        public int _anim_commandIndex = 0; // 

        // FUNCTIONAL INPUTS // sprint, interact, item, skill
        public int F_SwitchItem = 0;
        public int F_SwitchR = 0;
        public int F_SwitchL = 0;
        public bool BT_SwitchItem = false;
        public bool BT_SwitchR = false;
        public bool BT_SwitchL = false;
        public bool RL_SwitchItem = false;
        public bool RL_SwitchR = false;
        public bool RL_SwitchL = false;

        public int F_Interact = 0;
        public bool BT_Interact = false;
        public bool RL_Interact = false;

        public int F_Camlock = 0;
        public int F_Jump = 0;
        public int F_Attack = 0;
        public int F_Heal = 0;
        public int F_Dodge = 0;
        public bool BT_Camlock = false;
        public bool BT_Jump = false;
        public bool BT_Attack = false;
        public bool BT_Heal = false;
        public bool BT_Dodge = false;
        public bool RL_Camlock = false;
        public bool RL_Jump = false;
        public bool RL_Attack = false;
        public bool RL_Heal = false;
        public bool RL_Dodge = false;

        public int F_Item = 0;
        public int F_SkillL = 0;
        public int F_SkillR = 0;
        public bool BT_Item = false;
        public bool BT_SkillL = false;
        public bool BT_SkillR = false;
        public bool RL_Item = false;
        public bool RL_SkillL = false;
        public bool RL_SkillR = false;

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
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<SAInputs>();
            _combatSystem = GetComponent<CombatSystem_Player_A0>();
            _playerDecision = GetComponent<PlayerDecision>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            _playerDecision.AssignAnimationIDs();
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
            _playerDecision.Decide();

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
            MoveAndLookWithCamera(_anim_targetSpeed, 
                _anim_localMovingDirection, 
                _anim_globalTargetRotationOfPlayer, 
                _anim_cameraAngleFactor, 
                _anim_useCameraRotation, 
                _anim_localLookOffset, 
                _anim_useLocalLook,
                _anim_VV); // set move and look direction, can be used in PlayerMain_A0 to control the character's movement and rotation
            _combatSystem.UpdatePopVE();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold) // && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void FunctionalInput()
        {
            // Priorty in accending order

            // switchItem
            // switchL
            // switchR
            RL_SwitchItem = F_SwitchItem > 0 ? !_input.switchItem : false;
            BT_SwitchItem = F_SwitchItem == 0 ? _input.switchItem : false;
            F_SwitchItem = _input.switchItem ? F_SwitchItem + 1 : 0;
            RL_SwitchR = F_SwitchR > 0 ? !_input.switchR : false;
            BT_SwitchR = F_SwitchR == 0 ? _input.switchR : false;
            F_SwitchR = _input.switchR ? F_SwitchR + 1 : 0;
            RL_SwitchL = F_SwitchL > 0 ? !_input.switchL : false;
            BT_SwitchL = F_SwitchL == 0 ? _input.switchL : false;
            F_SwitchL = _input.switchL ? F_SwitchL + 1 : 0;

            // interact
            RL_Interact = F_Interact > 0 ? !_input.interact : false;
            BT_Interact = F_Interact == 0 ? _input.interact : false;
            F_Interact = _input.interact ? F_Interact + 1 : 0;

            // Battle

            RL_Camlock = F_Camlock > 0 ? !_input.camlock : false;
            BT_Camlock = F_Camlock == 0 ? _input.camlock : false;
            F_Camlock = _input.camlock ? F_Camlock + 1 : 0;

            RL_Jump = F_Jump > 0 ? !_input.jump : false;
            BT_Jump = F_Jump == 0 ? _input.jump : false;
            F_Jump = _input.jump ? F_Jump + 1 : 0;

            RL_Attack = F_Attack > 0 ? !_input.attack : false;
            BT_Attack = F_Attack == 0 ? _input.attack : false;
            F_Attack = _input.attack ? F_Attack + 1 : 0;

            RL_Heal = F_Heal > 0 ? !_input.heal : false;
            BT_Heal = F_Heal == 0 ? _input.heal : false;
            F_Heal = _input.heal ? F_Heal + 1 : 0;

            RL_Dodge = F_Dodge > 0 ? !_input.dodge : false;
            BT_Dodge = F_Dodge == 0 ? _input.dodge : false;
            F_Dodge = _input.dodge ? F_Dodge + 1 : 0;

            // Long

            RL_Item = F_Item > 0 ? !_input.item : false;
            RL_SkillL = F_SkillL > 0 ? !_input.skillL : false;
            RL_SkillR = F_SkillR > 0 ? !_input.skillR : false;

            BT_Item = F_Item == 0 ? _input.item : false;
            BT_SkillL = F_SkillL == 0 ? _input.skillL : false;
            BT_SkillR = F_SkillR == 0 ? _input.skillR : false;

            F_Item = _input.item ? F_Item + 1 : 0;
            F_SkillL = _input.skillL ? F_SkillL + 1 : 0;
            F_SkillR = _input.skillR ? F_SkillR + 1 : 0;

            bool[] inputState = {
                false,
                RL_Dodge,
                RL_Heal,
                RL_Attack,
                RL_Jump,
                _input.skillR || RL_SkillR,
                _input.skillL || RL_SkillL,
                _input.item || RL_Item,
                false, 
                false, 
                _input.interact, 
                false, 
                false, 
                false
            };

            for (int i = 0; i < inputState.Length; i++)
            {
                if (inputState[i])
                {
                    // if (i == 4 && !Grounded) continue;
                    if (i == 2 && !_combatSystem.CanHeal()) continue;

                    nextStateID = i;
                    if (nextStateID != 1 && nextStateID != 2 && nextStateID != 4) break;
                    if (Grounded) break;
                }
            }
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

        public float GetYaw()
        {
            return transform.rotation.eulerAngles.y;
        }

        /// <summary>
        /// Convert the raw input.move (Vector2) into a local-space Vector3 suitable for MoveAndLookWithCamera.
        /// Returns a normalized XZ vector or Vector3.zero when input is below the deadzone.
        /// </summary>
        public Vector3 GetLocalMoveDirection()
        {
            Vector2 move2 = _input.move;
            if (move2.sqrMagnitude < _threshold * _threshold)
                return Vector3.zero;

            Vector3 local = new Vector3(move2.x, 0f, move2.y);
            return local.normalized;
        }

        // ============================

        // // Returns the player's current global facing direction (world-space forward, normalized).
        // public Vector3 GetGlobalFacingDirection()
        // {
        //     return transform.forward.normalized;
        // }

        // // Convert WASD/left-stick input (local input space) to a world-space direction relative to the camera yaw.
        // // If there's no camera target, fall back to the cached cinemachine yaw.
        // public Vector3 GetCameraRelativeMoveDirection()
        // {
        //     Vector2 mv = _input.move;
        //     // if no meaningful input return zero
        //     if (mv.sqrMagnitude < _threshold * _threshold) return Vector3.zero;

        //     float cameraYaw = _cinemachineTargetYaw;
        //     if (CinemachineCameraTarget != null)
        //     {
        //         cameraYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        //     }

        //     Quaternion camRot = Quaternion.Euler(0f, cameraYaw, 0f);
        //     Vector3 localMove = new Vector3(mv.x, 0f, mv.y);
        //     Vector3 worldMove = camRot * localMove;
        //     return worldMove.sqrMagnitude > 1e-6f ? worldMove.normalized : Vector3.zero;
        // }

        // // Gradually rotate the player's global facing yaw toward the camera-relative move direction.
        // // Returns a local look offset (degrees) you can pass to MoveAndLookWithCamera as localLookOffset.
        // // - turnSpeedDegreesPerSecond: maximum yaw change speed
        // // - uses input intensity to scale rotation when partial input is given (e.g. analog stick).
        // public float RotateFacingGraduallyToCameraMove(float turnSpeedDegreesPerSecond = 360f)
        // {
        //     Vector3 worldMoveDir = GetCameraRelativeMoveDirection();
        //     // require at least some horizontal input to start rotating
        //     if (worldMoveDir == Vector3.zero) return 0f;

        //     // compute target yaw from direction
        //     float targetYaw = Mathf.Atan2(worldMoveDir.x, worldMoveDir.z) * Mathf.Rad2Deg;
        //     float currentYaw = GetYaw();

        //     // scale rotation speed by input intensity so holding 'D' rotates smoothly toward the right
        //     float inputIntensity = Mathf.Clamp01(Mathf.Abs(_input.move.x) + Mathf.Abs(_input.move.y));

        //     float maxDelta = turnSpeedDegreesPerSecond * Time.deltaTime * inputIntensity;
        //     float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, maxDelta);

        //     // transform.rotation = Quaternion.Euler(0f, newYaw, 0f);

        //     // localLookOffset = angle from the player's new facing (newYaw) to the desired targetYaw.
        //     // When you call MoveAndLookWithCamera, pass globalTargetRotationOfPlayer = newYaw and this localLookOffset,
        //     // so the character can keep looking toward the full target direction while rotating gradually.
        //     float localLookOffset = Mathf.DeltaAngle(newYaw, targetYaw);
        //     return localLookOffset;
        // }

        public float GetRotateFromInput()
        {
            Vector2 mv = _input.move;
            if (mv.sqrMagnitude < _threshold * _threshold)
                return _anim_localLookOffset;

            float yaw = Mathf.Atan2(mv.x, mv.y) * Mathf.Rad2Deg;
            return yaw;
        }

        // ============================

        /// <summary>
        /// Move using the given speed/direction but combine the player's global target rotation with the camera's yaw.
        /// This overload treats the provided moving direction as a local-space direction (relative to the player's forward).
        /// - cameraAngleFactor: weight [0,1] where 0 = only global player rotation, 1 = only camera yaw.
        /// - useCameraRotation: if false, cameraAngleFactor is ignored and only global player rotation is used.
        /// - localLookOffset: an optional local-space yaw (degrees) relative to the player's forward. When enabled via
        ///   useLocalLook this offset is applied on top of the computed global rotation and the resulting world yaw is
        ///   forwarded to the animator so the character can "look" locally (for example, toward a right hand).
        ///
        /// Behaviour summary:
        /// - Camera only affects the global rotation calculation (finalGlobalRotation).
        /// - The supplied moving direction is treated as local and rotated into world space by finalGlobalRotation.
        /// - If useLocalLook is true, the animator will receive a look yaw equal to finalGlobalRotation + localLookOffset.
        ///
        /// This method forwards the computed world-space moving direction and world-space look yaw to MoveAndLook(...).
        ///
        /// Backwards compatible: if you don't use local look features, call this the same as the previous signature.
        /// </summary>
        public void MoveAndLookWithCamera(
            float targetSpeed,
            Vector3 localMovingDirection,
            float globalTargetRotationOfPlayer,
            float cameraAngleFactor,
            bool useCameraRotation,
            float localLookOffset = 0f,
            bool useLocalLook = false,
            float VV = -1f)
        {
            // get camera yaw (prefer active Cinemachine target; fall back to cached yaw)
            float cameraYaw = _cinemachineTargetYaw;
            if (CinemachineCameraTarget != null)
            {
                cameraYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            }

            // clamp factor to valid range
            cameraAngleFactor = Mathf.Clamp01(cameraAngleFactor);

            // Compute the global final rotation (camera only affects this)
            float finalGlobalRotation = globalTargetRotationOfPlayer;
            if (useCameraRotation)
            {
                // include CameraAngleOverride so the same offset used by CameraRotation is respected
                float cameraTarget = cameraYaw + CameraAngleOverride;
                finalGlobalRotation = Mathf.LerpAngle(globalTargetRotationOfPlayer, cameraTarget, cameraAngleFactor);
            }

            // Treat the provided moving direction as local-space and rotate it by the computed global rotation to get world-space motion
            Vector3 localDir = localMovingDirection.sqrMagnitude > 0.000001f ? localMovingDirection.normalized : Vector3.forward;
            Quaternion globalRotationQuat = Quaternion.Euler(0f, finalGlobalRotation, 0f);
            Vector3 worldMovingDirection = globalRotationQuat * localDir;

            // Compute where the character should "look" in world yaw. If useLocalLook is enabled, apply the local offset.
            float worldLookYaw = finalGlobalRotation;
            if (useLocalLook)
            {
                worldLookYaw = finalGlobalRotation + localLookOffset;
            }

            MoveAndLook(targetSpeed, worldMovingDirection, worldLookYaw, VV);
        }

        /// <summary>
        /// Move the player and orient its view based on the supplied targets.
        /// - targetSpeed: desired horizontal speed (units/sec)
        /// - targetMovingDirection: desired horizontal movement direction (world space)
        /// - targetRotationOfPlayer: desired player Y rotation in degrees (world space)
        /// This updates internal animation-driving fields and moves/rotates the CharacterController.
        /// </summary>
        public void MoveAndLook(float targetSpeed, Vector3 targetMovingDirection, float targetRotationOfPlayer, float VV)
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
                
                _animator.SetFloat(_playerDecision._animIDSpeed, targetSpeed);
                _animator.SetFloat(_playerDecision._animIDMotionSpeed, 1f);
                _animator.SetFloat(_playerDecision._animIDVV, _verticalVelocity);
            }
        }
    }
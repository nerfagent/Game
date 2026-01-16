using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

// namespace StarterAssets
// {
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class SAThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 3.5f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.5f;

        [Tooltip("Rolling speed of the character in m/s")]
        public float RollSpeed = 5.5f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        // new action variables
        // Timeout unit in seconds // all animations should be 60fps
        [Tooltip("Time required to pass before being able to attack again. Set to 0f to instantly attack again")]
        public float AttackTimeout = 1.00f; // attack ended at 65th frame

        [Tooltip("Time required to pass before being able to attack again. Set to 0f to instantly attack again")]
        public float AirAttackTimeout = 0.7f; // attack ended at 42nd frame

        [Tooltip("Time required to pass before being able to heal again. Set to 0f to instantly heal again")]
        public float HealTimeout = 1.4f; // heal ended at 100th frame

        [Tooltip("Time required to pass before being able to dodge again. Set to 0f to instantly dodge again")]
        public float DodgeTimeout = 0.70f; // dodge ended at 42nd frame
        // ADDNEWACTIONS

        [Tooltip("Time required to pass before being able to dodge again. Set to 0f to instantly dodge again")]
        public float Dodge_Invincible = 0.55f; // 0th to 33rd frame
        // Invincible time

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers; // BIGREMINDER // Set to default in inspector

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Combat System")]
        [SerializeField] private Hurtbox[] hurtboxes;
        [SerializeField] private Hitbox[] hitboxes;
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float maxFocus = 125f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float maxPoise = 51f;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // combat system
        private bool isInvincible = false;
        private bool _poiseBreak = false;
        private float currentHealth;
        private float currentFocus;
        private float currentStamina;
        private float currentPoise;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        //private float _attackTimeoutDelta;
        //private float _dodgeTimeoutDelta;
        private float _invincibleTimeoutDelta;

        private float _actionTimeoutDelta;
        //private float _globalTimeoutDelta;
        // ADDNEWACTIONS

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // ADDNEWACTIONS
        private int nextStateID = 0;
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
        */

        private int _animIDActionLock; // boolean // avoid action go back to idle

        private int _animIDInteract; // boolean
        private int _animIDInteractTarget; // int

        private int _animIDItem; // boolean
        private int _animIDItemID; // int

        private int _animIDSpell; // boolean
        private int _animIDSpellID; // int

        private int _animIDSkill; // boolean
        private int _animIDSkillID; // int

        private int _animIDAttack; // boolean
        private int _animIDAttackControl; // boolean

        private int _animIDHeal; // boolean
        private int _animIDHealControl; // boolean

        private int _animIDDodge; // boolean
        private int _animIDDodgeControl; // boolean

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private SAInputs _input;
        private GameObject _mainCamera;

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

        // HELPER VARIABLES
        private int lastInfo = -1;
        private AnimatorStateInfo laststateInfo;
        private AnimatorStateInfo stateInfo;
        private int currentInfo = -1;
        //private Dictionary<int, string> stateNameCache;

        // FUNCTIONAL INPUTS // sprint, interact, item, skill
        private int F_Camlock = 0;
        private int F_Attack = 0;
        private int F_Heal = 0;
        private int F_Dodge = 0;
        private bool BT_Camlock = false;
        private bool BT_Attack = false;
        private bool BT_Heal = false;
        private bool BT_Dodge = false;
        private bool RL_Camlock = false;
        private bool RL_Attack = false;
        private bool RL_Heal = false;
        private bool RL_Dodge = false;

        private int F_Item = 0;
        private int F_SkillL = 0;
        private int F_SkillR = 0;
        private bool BT_Item = false;
        private bool BT_SkillL = false;
        private bool BT_SkillR = false;
        private bool RL_Item = false;
        private bool RL_SkillL = false;
        private bool RL_SkillR = false;

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
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();
            HurtBoxActor();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            //_attackTimeoutDelta = AttackTimeout;
            //_dodgeTimeoutDelta = DodgeTimeout;
            _invincibleTimeoutDelta = 0.0f;
            _actionTimeoutDelta = 0.0f;
            //_globalTimeoutDelta = 0.0f;
            // ADDNEWACTIONS

            //CacheStateNames();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);
            // BIGREMINDER // I did not do check _hasAnimator for all functions called here, please add if necessary

            FunctionalInput();
            GroundedCheck();
            Idle();

            // Todo: add stamina consumption
            dodgeAnimation();
            Dodge();
            healAnimation();
            Heal();
            attackAnimation();
            Attack();
            // ADDNEWACTIONS
            JumpAndGravity();
            //GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        // Start() helpers
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

            // ADDNEWACTIONS
            _animIDActionLock = Animator.StringToHash("ActionLock"); // boolean // avoid action go back to idle
            
            _animIDAttack = Animator.StringToHash("Attack"); // boolean
            _animIDAttackControl = Animator.StringToHash("AttackControl"); // boolean

            _animIDHeal = Animator.StringToHash("Heal"); // boolean
            _animIDHealControl = Animator.StringToHash("HealControl"); // boolean

            _animIDDodge = Animator.StringToHash("Dodge"); // boolean
            _animIDDodgeControl = Animator.StringToHash("DodgeControl"); // boolean
        }

        private void HurtBoxActor()
        {
            currentHealth = maxHealth;

            // Register hurtbox events
            foreach (var hurtbox in hurtboxes)
            {
                hurtbox.OnDamageReceived += HandleDamage;
            }
        }

        // end of Start() helpers

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
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

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            //Debug.Log("Target Speed: " + targetSpeed);

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // a reference to the players current horizontal velocity
            GradMove(targetSpeed, inputMagnitude);

            // ======================================

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (IsInAnimationStateArray(new string[] { "Idle Walk Run Blend", "InAir" }))
            {
                if (_input.move != Vector2.zero)
                {
                    // Camlook part do seperately (not here)
                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                      _mainCamera.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            // ======================================

            // update animator if using character
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void FunctionalInput() // ADDNEWACTIONS
        {
            // Priorty in accending order

            // switchItem
            // switchL
            // switchR
            // interact
            RL_Camlock = F_Camlock > 0 ? !_input.camlock : false;
            BT_Camlock = F_Camlock == 0 ? _input.camlock : false;
            F_Camlock = _input.camlock ? F_Camlock + 1 : 0;
            // jump (given)
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
        }

        private void Idle() // ADDNEWACTIONS
        {
            // Placeholder for idle behavior
            laststateInfo = stateInfo;
            lastInfo = currentInfo;

            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            currentInfo = stateInfo.shortNameHash;
            
            if (_actionTimeoutDelta >= 0.0f && Grounded)
            {
                _actionTimeoutDelta -= Time.deltaTime;
                _animator.SetBool(_animIDActionLock, true);
            }

            if (_invincibleTimeoutDelta >= 0.0f)
            {
                _invincibleTimeoutDelta -= Time.deltaTime;
            }

            bool[] inputState = {
                false,
                RL_Dodge,
                RL_Heal,
                RL_Attack,
                _input.jump,
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
                    nextStateID = i;
                    if (nextStateID != 1 && nextStateID != 2 && nextStateID != 4) break;
                    if (Grounded) break;
                }
            }
            //_globalTimeoutDelta += Time.deltaTime;
            //Debug.Log("Global Timeout Delta: " + _globalTimeoutDelta);
            //Debug.Log("Dodge Input: " + RL_Dodge + ", Attack Input: " + RL_Attack + ", Heal Input: " + RL_Heal);
            //Debug.Log("RL_Heal: " + RL_Heal + " BT_Heal: " + BT_Heal + " F_Heal: " + F_Heal + " _input.heal: " + _input.heal);
            //Debug.Log("Next State ID: " + nextStateID);
            //Debug.Log("Next State ID: " + nextStateID + "Action Timeout Delta: " + _actionTimeoutDelta);
            //Debug.Log(nextStateID == 2 && _actionTimeoutDelta <= 0.0f && _hasAnimator && Grounded);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDActionLock, true);
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        // new functions
        // ADDNEWACTIONS
        private void dodgeAnimation()
        {
            if (currentInfo != lastInfo)
            {
                _animator.SetBool(_animIDDodge, false);
                _animator.SetBool(_animIDDodgeControl, false);
            }

            if (nextStateID == 1 && _actionTimeoutDelta <= 0.0f && _hasAnimator)
            {
                // update animator if using character
                string[] usingDodge = {
                    "Idle Walk Run Blend",
                    "InAir",
                    "Heal",
                    "Heal 0",
                    "OneHand_Up_Roll_F 0",
                    "OneHand_Up_Attack_A_1",
                    "OneHand_Up_Attack_A_2",
                    "OneHand_Up_Attack_A_5 (Run)",
                    "OneHand_Up_Attack_A_2 (Dodge)",
                    "FallAttack"
                };
                string[] usingDodgeControl = {
                    "OneHand_Up_Roll_F"
                };
                if (IsInAnimationStateArray(usingDodge))
                {
                    _animator.SetBool(_animIDDodge, true);
                    _animator.SetBool(_animIDDodgeControl, false);
                    _actionTimeoutDelta = DodgeTimeout;
                    nextStateID = 0;
                }
                else if (IsInAnimationStateArray(usingDodgeControl))
                {
                    _animator.SetBool(_animIDDodge, false);
                    _animator.SetBool(_animIDDodgeControl, true);
                    _actionTimeoutDelta = DodgeTimeout;
                    nextStateID = 0;
                }
                else
                {
                    _animator.SetBool(_animIDActionLock, false);
                }
            }
        }

        private void Dodge()
        {
            string[] dodgeStates = {
                "OneHand_Up_Roll_F",
                "OneHand_Up_Roll_F 0"
            };
            if (IsInAnimationStateArray(dodgeStates))
            {
                if (currentInfo != lastInfo)
                {
                    isInvincible = true;
                    _invincibleTimeoutDelta = Dodge_Invincible;
                }

                float targetSpeed = RollSpeed;
                GradMove(targetSpeed, 1f);

                Vector3 targetDirection = transform.forward;

                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

                if (_invincibleTimeoutDelta <= 0.0f)
                {
                    isInvincible = false;
                }

                // Todo: add stamina consumption
            }
        }

        private void healAnimation()
        {
            if (currentInfo != lastInfo)
            {
                _animator.SetBool(_animIDHeal, false);
                _animator.SetBool(_animIDHealControl, false);
            }

            if (nextStateID == 2 && _actionTimeoutDelta <= 0.0f && _hasAnimator && Grounded)
            {
                // update animator if using character
                string[] usingHeal = {
                    "Idle Walk Run Blend",
                    "Heal 0",
                    "OneHand_Up_Roll_F",
                    "OneHand_Up_Roll_F 0"
                };
                string[] usingHealControl = {
                    "Heal"
                };
                if (IsInAnimationStateArray(usingHeal))
                {
                    _animator.SetBool(_animIDHeal, true);
                    _animator.SetBool(_animIDHealControl, false);
                    _actionTimeoutDelta = HealTimeout;
                    if (laststateInfo.IsName("Heal 0"))
                    {
                        _actionTimeoutDelta -= 1.00f;
                    }
                    nextStateID = 0;
                }
                else if (IsInAnimationStateArray(usingHealControl))
                {
                    _animator.SetBool(_animIDHeal, false);
                    _animator.SetBool(_animIDHealControl, true);
                    _actionTimeoutDelta = HealTimeout - 1.00f;
                    nextStateID = 0;
                }
                else
                {
                    _animator.SetBool(_animIDActionLock, false);
                }
            }
        }

        private void Heal()
        { }

        private void attackAnimation()
        {
            if (currentInfo != lastInfo)
            {
                _animator.SetBool(_animIDAttack, false);
                _animator.SetBool(_animIDAttackControl, false);
            }

            if (nextStateID == 3 && _actionTimeoutDelta <= 0.0f && _hasAnimator)
            {
                // update animator if using character
                string[] usingAttack = {
                    "Idle Walk Run Blend",
                    "InAir",
                    "OneHand_Up_Attack_A_2 (Dodge)",
                    "OneHand_Up_Attack_A_2"
                };
                string[] usingAttackControl = {
                    "OneHand_Up_Roll_F",
                    "OneHand_Up_Roll_F 0",
                    "OneHand_Up_Attack_A_1",
                    "OneHand_Up_Attack_A_5 (Run)",
                    "FallAttack"
                };
                if (IsInAnimationStateArray(usingAttack))
                {
                    _animator.SetBool(_animIDAttack, true);
                    _animator.SetBool(_animIDAttackControl, false);
                    if (IsInAnimationStateArray(new string[] { "InAir" }))
                    {
                        _actionTimeoutDelta = AirAttackTimeout;
                    }
                    else
                    {
                        _actionTimeoutDelta = AttackTimeout;
                    }
                    nextStateID = 0;
                }
                else if (IsInAnimationStateArray(usingAttackControl))
                {
                    _animator.SetBool(_animIDAttack, false);
                    _animator.SetBool(_animIDAttackControl, true);
                    _actionTimeoutDelta = AttackTimeout;
                    nextStateID = 0;
                }
                else
                {
                    _animator.SetBool(_animIDActionLock, false);
                }
            }
        }

        private void Attack()
        { }

        // HELPER METHODS
        private bool IsInAnimationStateArray(string[] stateNameArray)
        {
            foreach (string name in stateNameArray)
            {
                if (stateInfo.IsName(name))
                    return true;
            }
            return false;
        }

        private void GradMove(float targetSpeed, float inputMagnitude)
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
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }
        }

        private void HandleDamage(DamageData damageData)
        {
            if (isInvincible) return;

            // Health reduction
            if (currentHealth >= 0)
            {
                currentHealth -= damageData.damage;
            }
            if (currentHealth <= 0)
            {
                Die();
                return;
            }

            // Poise reduction
            if (currentPoise >= 0 && !_poiseBreak)
            {
                currentPoise -= damageData.poiseDamage;
            }
            if (currentPoise <= 0 && !_poiseBreak)
            {
                // Poise break logic
                _poiseBreak = true;
                currentPoise = maxPoise;
                HandlePoiseBreak();
                // You can add additional effects here, such as staggering the player
            }
        }

        private void HandlePoiseBreak()
        {
            // Logic for handling poise break effects
            // For example, play a stagger animation or sound effect
            Debug.Log("Poise Break! Player is staggered.");
        }

        private void Die()
        {
            // Logic for player death
            Debug.Log("Player has died.");
            // You can add death animations, respawn logic, etc. here
        }

        // Public properties
        public void ActivateAttack(int hitboxIndex)
        {
            if (hitboxIndex >= 0 && hitboxIndex < hitboxes.Length)
            {
                hitboxes[hitboxIndex].ActivateHitbox(gameObject);
            }
        }

        public void DeactivateAllHitboxes()
        {
            foreach (var hitbox in hitboxes)
            {
                hitbox.DisableHitbox();
            }
        }

        public Vector3 getAttackDirection()
        {
            Vector3 attackDirection = transform.forward.normalized; // temporary
            return attackDirection;
        }
    }
// }
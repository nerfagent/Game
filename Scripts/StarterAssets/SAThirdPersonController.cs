// using UnityEngine;
// using UnityEngine.Animations;
// using System.Collections.Generic; // must
// using System; // must
// // using System.Diagnostics;
// // using System.Numerics;
// #if ENABLE_INPUT_SYSTEM
// using UnityEngine.InputSystem;
// #endif

// /* Note: animations are called via the controller for both the character and capsule using animator null checks
//  */

// // namespace StarterAssets
// // {
// [RequireComponent(typeof(CharacterController))]
// #if ENABLE_INPUT_SYSTEM 
//     [RequireComponent(typeof(PlayerInput))]
// #endif
//     public class SAThirdPersonController : MonoBehaviour
//     {
//         [Header("Player")]
//         [Tooltip("Walk speed of the character in m/s")]
//         public float WalkSpeed = 1.5f;

//         [Tooltip("Move speed of the character in m/s")]
//         public float MoveSpeed = 3.5f;

//         [Tooltip("Sprint speed of the character in m/s")]
//         public float SprintSpeed = 6.5f;

//         [Tooltip("Rolling speed of the character in m/s")]
//         public float RollSpeed = 3.5f;

//         [Tooltip("How fast the character turns to face movement direction")]
//         [Range(0.0f, 0.3f)]
//         public float RotationSmoothTime = 0.12f;

//         [Tooltip("Acceleration and deceleration")]
//         public float SpeedChangeRate = 10.0f;

//         public AudioClip LandingAudioClip;
//         public AudioClip[] FootstepAudioClips;
//         [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

//         [Space(10)]
//         [Tooltip("The height the player can jump")]
//         public float JumpHeight = 1.2f;

//         [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
//         public float Gravity = -15.0f;

//         [Space(10)]
//         // Timeout unit in seconds // all animations should be 60fps
//         [Tooltip("Time required to pass before being able to dodge again. Set to 0f to instantly dodge again")]
//         public float DodgeTimeout = 0.70f; // dodge ended at 42nd frame

//         [Tooltip("Time required to pass before being able to heal again. Set to 0f to instantly heal again")]
//         public float HealTimeout = 1.4f; // heal ended at 100th frame

//         [Tooltip("Time required to pass before being able to attack again. Set to 0f to instantly attack again")]
//         public float AttackTimeout = 1.00f; // attack ended at 65th frame

//         [Tooltip("Time required to pass before being able to attack again. Set to 0f to instantly attack again")]
//         public float FallAttackTimeout = 0.7f; // attack ended at 42nd frame

//         [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
//         public float JumpTimeout = 0.3f;

//         [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
//         public float FallTimeout = 0.15f;

//         // [Tooltip("Time required to pass before being able to skillR again. Set to 0f to instantly skillR again")]
//         // public float SkillRTimeout = 0.7f; // attack ended at 42nd frame

//         // [Tooltip("Time required to pass before being able to skillL again. Set to 0f to instantly skillL again")]
//         // public float SkillLTimeout = 0.7f; // attack ended at 42nd frame

//         // [Tooltip("Time required to pass before being able to item again. Set to 0f to instantly item again")]
//         // public float ItemTimeout = 0.7f; // attack ended at 42nd frame

//         // [Tooltip("Time required to pass before being able to sprint again. Set to 0f to instantly sprint again")]
//         // public float SprintTimeout = 0.50f;

//         [Tooltip("Time required to pass before being able to camlock again. Set to 0f to instantly camlock again")]
//         public float CamlockTimeout = 0.50f;

//         [Tooltip("Time required to pass before being able to interact again. Set to 0f to instantly interact again")]
//         public float InteractTimeout = 0.50f;

//         [Tooltip("Time required to pass before being able to switch right again. Set to 0f to instantly switch right again")]
//         public float SwitchRTimeout = 0.50f;

//         [Tooltip("Time required to pass before being able to switch left again. Set to 0f to instantly switch left again")]
//         public float SwitchLTimeout = 0.50f;

//         [Tooltip("Time required to pass before being able to switch item again. Set to 0f to instantly switch item again")]
//         public float SwitchItemTimeout = 0.50f;
//         // ADDNEWACTIONS

//         [Tooltip("Time required to pass before being able to dodge again. Set to 0f to instantly dodge again")]
//         public float Dodge_Invincible = 0.55f; // 0th to 33rd frame
//         // Invincible time

//         [Header("Player Grounded")]
//         [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
//         public bool Grounded = true;

//         [Tooltip("Useful for rough ground")]
//         public float GroundedOffset = -0.14f;

//         [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
//         public float GroundedRadius = 0.28f;

//         [Tooltip("What layers the character uses as ground")]
//         public LayerMask GroundLayers; // BIGREMINDER // Set to default in inspector

//         [Header("Cinemachine")]
//         [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
//         public GameObject CinemachineCameraTarget;

//         [Tooltip("How far in degrees can you move the camera up")]
//         public float TopClamp = 70.0f;

//         [Tooltip("How far in degrees can you move the camera down")]
//         public float BottomClamp = -30.0f;

//         [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
//         public float CameraAngleOverride = 0.0f;

//         [Tooltip("For locking the camera position on all axis")]
//         public bool LockCameraPosition = false;

//         [Header("Combat System")]
//         [SerializeField] private Hurtbox[] hurtboxes;
//         /*
//             0. head
//             1. chest
//             2. stomach
//             3. r up arm
//             4. r low arm
//             5. l up arm
//             6. l low arm
//             7. r up leg
//             8. r low leg
//             9. r foot
//             10. l up leg
//             11. l low leg
//             12. l foot
//         */
//         [SerializeField] private Hitbox[] hitboxes;
//         /*
//             0. sword
//             1. ground
//         */
//         [SerializeField] private DamageData[] damageDatas;
//         /*
//             0. Heal
//             1. Base Attack
//             2. Fall Attack
//         */
//         [SerializeField] private float maxHealth = 1000f;
//         [SerializeField] private float maxFocus = 125f;
//         [SerializeField] private float maxStamina = 100f;
//         [SerializeField] private float maxPoise = 51f;
//         [SerializeField] private float maxStance = 51f;
//         [SerializeField] private float stanceStunDuration = 5f;
//         [SerializeField] private float HealAmount = 300f;

//         // cinemachine
//         private float _cinemachineTargetYaw;
//         private float _cinemachineTargetPitch;

//         // player
//         private Vector2 _lastMoveInput = Vector2.zero;
//         private bool _attackFollowCamera = false;
//         private float _speed;
//         private float _lastSpeed;
//         private float _animationBlend;
//         private float _targetRotation = 0.0f;
//         private float _lastTargetRotation = 0.0f;
//         private float _attackRotation = 0.0f;
//         private float _rotationVelocity;
//         private float _verticalVelocity;
//         private float _terminalVelocity = 53.0f;

//         // combat system
//         private bool _poiseBreak = false;
//         private float currentHealth;
//         private float currentFocus;
//         private float currentStamina;
//         private float currentPoise;
//         private float currentStance;
//         private int currentWeakness;
//         private Vector3 knockbackDirection = Vector3.forward;
//         private int knockbackindex = 0; // 0: F, 1: B, 2: L, 3: R, 4: U, 5: D
//         public List<bool> dizzyDamageLevel = new List<bool> { false, true, true, true, true, true };
//         // 0: wont knock // 1: small knock // 2: big knock // 3: knock up // 4: knock down // 5: status effect
//         public List<bool> forceDizzyDamageLevel = new List<bool> { false, false, true, true, true, true };
//         private bool[] currentDizzyDamageLevel = { false, true, true, true, true, true };
//         [SerializeField] private List<DamageData> pendingDamageDataList;

//         // timeout deltatime
//         private float _jumpTimeoutDelta;
//         private float _fallTimeoutDelta;

//         private float CurAttackTimeout;
//         //private float _dodgeTimeoutDelta;
//         private float _invincibleTimeoutDelta;
//         private float _actionTimeoutDelta;
//         private float _forceLeaveTimeoutDelta = 0.5f;
//         private float _forceEnterTimeoutDelta = 0.1f;
//         //private float _globalTimeoutDelta;
//         // ADDNEWACTIONS

//         // animation IDs
//         private int _animIDSpeed;
//         private int _animIDGrounded;
//         private int _animIDJump;
//         private int _animIDFreeFall;
//         private int _animIDMotionSpeed;

//         // ADDNEWACTIONS
//         private int forceEnter = 0;
//         private int nextStateID = 0;
//         /*
//             0: idle/walk/run blend
//             1: dodge
//             2: heal
//             3: attack
//             4: jump
//             5: skillR
//             6: skillL
//             7: item
//             8: sprint
//             9: camlock
//             10: interact
//             11: switchR
//             12: switchL
//             13: switchItem
//         */

//         private int _animaIDNextState; // int
//         private int _animIDActionLock; // boolean // avoid action go back to idle

//         private int _animIDInteract; // boolean
//         private int _animIDInteractTarget; // int

//         private int _animIDItem; // boolean
//         private int _animIDItemID; // int

//         private int _animIDSpell; // boolean
//         private int _animIDSpellID; // int

//         private int _animIDSkill; // boolean
//         private int _animIDSkillID; // int

//         private int _animIDAttack; // boolean
//         private int _animIDAttackControl; // boolean

//         private int _animIDHeal; // boolean
//         private int _animIDHealControl; // boolean

//         private int _animIDDodge; // boolean
//         private int _animIDDodgeControl; // boolean

//         // Get Hit
//         private int _animIDHealth; // float
//         private int _animIDPoise; // float
//         private int _animIDStance; // float
//         private int _animIDGetHit; // boolean
//         private int _animIDGetHitControl; // boolean
//         private int _animIDGetHitType; // int

// #if ENABLE_INPUT_SYSTEM 
//         private PlayerInput _playerInput;
// #endif
//         private Animator _animator;
//         private CharacterController _controller;
//         private SAInputs _input;
//         private GameObject _mainCamera;

//         private const float _threshold = 0.01f;

//         private bool _hasAnimator;

//         private bool IsCurrentDeviceMouse
//         {
//             get
//             {
// #if ENABLE_INPUT_SYSTEM
//                 return _playerInput.currentControlScheme == "KeyboardMouse";
// #else
// 				return false;
// #endif
//             }
//         }

//         // HELPER VARIABLES
//         private int lastInfo = -1;
//         private AnimatorStateInfo laststateInfo;
//         private AnimatorStateInfo stateInfo;
//         private int currentInfo = -1;
//         //private Dictionary<int, string> stateNameCache;

//         // FUNCTIONAL INPUTS // sprint, interact, item, skill
//         private int F_Camlock = 0;
//         private int F_Jump = 0;
//         private int F_Attack = 0;
//         private int F_Heal = 0;
//         private int F_Dodge = 0;
//         private bool BT_Camlock = false;
//         private bool BT_Jump = false;
//         private bool BT_Attack = false;
//         private bool BT_Heal = false;
//         private bool BT_Dodge = false;
//         private bool RL_Camlock = false;
//         private bool RL_Jump = false;
//         private bool RL_Attack = false;
//         private bool RL_Heal = false;
//         private bool RL_Dodge = false;

//         private int F_Item = 0;
//         private int F_SkillL = 0;
//         private int F_SkillR = 0;
//         private bool BT_Item = false;
//         private bool BT_SkillL = false;
//         private bool BT_SkillR = false;
//         private bool RL_Item = false;
//         private bool RL_SkillL = false;
//         private bool RL_SkillR = false;

//         // States
//         private string[] dodgeStates = {
//                 "OneHand_Up_Roll_F",
//                 "OneHand_Up_Roll_F 0"
//             };
//         private string[] healStates = {
//                 "Heal",
//                 "Heal 0"
//             };
//         private string[] attackStates = {
//                 "OneHand_Up_Attack_A_1",
//                 "OneHand_Up_Attack_A_2",
//                 "OneHand_Up_Attack_A_2 (Dodge)",
//                 "OneHand_Up_Attack_A_5 (Run)",
//                 "FallAttack"
//             };
//         private string[] airStates = {
//                 "JumpStart",
//                 "InAir"
//             };
//         private string[] getHitStates = {
//                 "GetHit_F1",
//                 "GetHit_F2",
//                 "GetHit_B1",
//                 "GetHit_B2",
//                 "GetHit_L1",
//                 "GetHit_L2",
//                 "GetHit_R1",
//                 "GetHit_R2",
//                 "GetHit_U1", // = B1
//                 "GetHit_U2",
//                 "GetHit_D1", // = F1
//                 "GetHit_D2"
//             };
//         private string[] farHitStates = {
//                 "GetHit_F2",
//                 "GetHit_B2",
//                 "GetHit_L2",
//                 "GetHit_R2"
//             };

//         // Enemy setting
//         public bool isPlayer = true;
//         public bool isNPC = false;
//         public bool isEnemy = false;

//         // Debug
//         public bool showDebug = true;

//         private void Awake()
//         {
//             // get a reference to our main camera
//             if (_mainCamera == null)
//             {
//                 _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
//             }
//         }

//         private void Start()
//         {
//             _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
//             _hasAnimator = TryGetComponent(out _animator);
//             _controller = GetComponent<CharacterController>();
//             _input = GetComponent<SAInputs>();
// #if ENABLE_INPUT_SYSTEM
//             _playerInput = GetComponent<PlayerInput>();
// #else
//             Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
// #endif

//             AssignAnimationIDs();
//             HurtBoxActor();

//             // reset our timeouts on start
//             _jumpTimeoutDelta = JumpTimeout;
//             _fallTimeoutDelta = FallTimeout;

//             CurAttackTimeout = AttackTimeout;
//             //_dodgeTimeoutDelta = DodgeTimeout;
//             _invincibleTimeoutDelta = 0.0f;
//             _actionTimeoutDelta = 0.0f;
//             //_globalTimeoutDelta = 0.0f;
//             // ADDNEWACTIONS

//             //CacheStateNames();
//         }

//         private void Update()
//         {
//             // Debug.Log(_enemyAI != null);
//             // Debug.Log(currentHealth);
//             _hasAnimator = TryGetComponent(out _animator);
//             // BIGREMINDER // I did not do check _hasAnimator for all functions called here, please add if necessary

//             FunctionalInput();
//             GroundedCheck();
//             Idle();

//             // Todo: add stamina consumption
//             UpdateHandleDamage();
//             dodgeAnimation();
//             Dodge();
//             healAnimation();
//             Heal();
//             attackAnimation();
//             Attack();
//             // ADDNEWACTIONS
//             JumpAndGravity();
//             InAirInvincible();
//             //GroundedCheck();
//             Move();
//             // ShowHurtboxes();
//             // Debug.Log("Player: " + currentHealth);
//         }

//         private void LateUpdate()
//         {
//             CameraRotation();
//         }

//         // Start() helpers
//         private void AssignAnimationIDs()
//         {
//             _animIDSpeed = Animator.StringToHash("Speed");
//             _animIDGrounded = Animator.StringToHash("Grounded");
//             _animIDJump = Animator.StringToHash("Jump");
//             _animIDFreeFall = Animator.StringToHash("FreeFall");
//             _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

//             // ADDNEWACTIONS
//             _animaIDNextState = Animator.StringToHash("NextState");
//             _animIDActionLock = Animator.StringToHash("ActionLock"); // boolean // avoid action go back to idle
            
//             _animIDAttack = Animator.StringToHash("Attack"); // boolean
//             _animIDAttackControl = Animator.StringToHash("AttackControl"); // boolean

//             _animIDHeal = Animator.StringToHash("Heal"); // boolean
//             _animIDHealControl = Animator.StringToHash("HealControl"); // boolean

//             _animIDDodge = Animator.StringToHash("Dodge"); // boolean
//             _animIDDodgeControl = Animator.StringToHash("DodgeControl"); // boolean

//             _animIDHealth = Animator.StringToHash("Health"); // float
//             _animIDPoise = Animator.StringToHash("Poise"); // float
//             _animIDStance = Animator.StringToHash("Stance"); // float

//             _animIDGetHit = Animator.StringToHash("GetHit"); // boolean
//             _animIDGetHitControl = Animator.StringToHash("GetHitControl"); // boolean
//             _animIDGetHitType = Animator.StringToHash("GetHitType"); // int
//         }

//         private void HurtBoxActor()
//         {
//             currentHealth = maxHealth;

//             // Register hurtbox events
//             foreach (var hurtbox in hurtboxes)
//             {
//                 hurtbox.OnDamageReceived += HandleDamage;
//             }
//         }

//         // end of Start() helpers

//         private void GroundedCheck()
//         {
//             // set sphere position, with offset
//             Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
//                 transform.position.z);
//             Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
//                 QueryTriggerInteraction.Ignore);

//             // update animator if using character
//             if (_hasAnimator)
//             {
//                 _animator.SetBool(_animIDGrounded, Grounded);
//             }
//         }

//         private void CameraRotation()
//         {
//             // if there is an input and camera position is not fixed
//             if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
//             {
//                 //Don't multiply mouse input by Time.deltaTime;
//                 float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

//                 _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
//                 _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
//             }

//             // clamp our rotations so our values are limited 360 degrees
//             _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
//             _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

//             // Cinemachine will follow this target
//             CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
//                 _cinemachineTargetYaw, 0.0f);
//         }

//         private void Move()
//         {
//             // set target speed based on move speed, sprint speed and if sprint is pressed
//             float targetSpeed = _lastSpeed;
//             if (IsInAnimationStateArray(new string[] { 
//                     "Idle Walk Run Blend",
//                     "JumpStart",
//                     "JumpLand"
//                 }))
//             {
//                 _lastMoveInput = _input.move;
//                 if (_input.sprint)
//                     targetSpeed = SprintSpeed;
//                 else
//                     targetSpeed = MoveSpeed;
//             }
//             else if (IsInAnimationStateArray(healStates))
//             {
//                 _lastMoveInput = _input.move;
//                 targetSpeed = WalkSpeed;
//             }
//             _lastSpeed = targetSpeed;

//             bool movable = IsInAnimationStateArray(new string[] { 
//                     "Idle Walk Run Blend", 
//                     "InAir",
//                     "JumpStart",
//                     "JumpLand",
//                     "Heal",
//                     "Heal 0",
//                     "FallAttack_InAir"
//                 });

//             bool rolling = IsInAnimationStateArray(dodgeStates);
//             bool rolling_last = lastInfoIsInAnimationStateArray(dodgeStates);

//             bool farHit = IsInAnimationStateArray(farHitStates);
//             //Debug.Log("Target Speed: " + targetSpeed);

//             // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

//             // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
//             // if there is no input, set the target speed to 0
//             if (_lastMoveInput == Vector2.zero) 
//                 targetSpeed = 0.0f;
//             if (!movable && !rolling)
//             {
//                 _jumpTimeoutDelta = JumpTimeout;
//                 _lastMoveInput = Vector2.zero;
//                 targetSpeed = 0.0f;
//                 if (_input.move != Vector2.zero && _actionTimeoutDelta <= -_forceLeaveTimeoutDelta)
//                     _animator.SetBool(_animIDActionLock, false);
//             }

//             // TEMPORARY USELESS
//             float inputMagnitude = _input.analogMovement ? _lastMoveInput.magnitude : 1f;

//             if (rolling)
//             {
//                 targetSpeed = SprintSpeed;
//                 _lastMoveInput = _input.move;
//                 if (DodgeTimeout - _actionTimeoutDelta <= 0.1f)
//                 {
//                     targetSpeed = RollSpeed;
//                 }
//                 if (_actionTimeoutDelta <= 0.0f)
//                 {
//                     targetSpeed = 2.0f;
//                 }
//                 if (_actionTimeoutDelta <= -_forceLeaveTimeoutDelta + 0.3f)
//                 {
//                     targetSpeed = 0.0f;
//                     if (_input.move != Vector2.zero || _actionTimeoutDelta <= -_forceLeaveTimeoutDelta)
//                         _animator.SetBool(_animIDActionLock, false);
//                 }
//             }
//             if (farHit)
//             {
//                 targetSpeed = SprintSpeed;
//             }

//             // accerelrate or decelerate to target speed
//             GradMove(targetSpeed, 1f);
//             // GradMove(targetSpeed, inputMagnitude);

//             // ======================================

//             // normalise input direction
//             Vector3 inputDirection = new Vector3(_lastMoveInput.x, 0.0f, _lastMoveInput.y).normalized;

//             // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
//             // if there is a move input rotate player when the player is moving
//             if ((_lastMoveInput != Vector2.zero && movable) || rolling) // rotation only
//             {
//                 // Camlook part do seperately (not here)
//                 if (!rolling || (rolling && rolling_last && currentInfo != lastInfo))
//                     _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + 
//                                         _mainCamera.transform.eulerAngles.y;

//                 float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
//                     RotationSmoothTime);

//                 // rotate to face input direction relative to camera position
//                 transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
//             }
//             else if (_attackFollowCamera)
//             {
//                 // Camlook part do seperately (not here)
//                 _targetRotation = _attackRotation;

//                 float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
//                     RotationSmoothTime);

//                 // rotate to face input direction relative to camera position
//                 transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

//                 if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, _attackRotation)) < 0.1f)
//                 {
//                     _attackFollowCamera = false;
//                 }
//             }
//             else if (farHit)
//             {
//                 Quaternion targetRotation = Quaternion.LookRotation(knockbackDirection);

//                 switch (knockbackindex)
//                 {
//                     case 0: // F
//                         targetRotation = Quaternion.LookRotation(knockbackDirection);
//                         break;
//                     case 1: // B
//                         targetRotation = Quaternion.LookRotation(-knockbackDirection);
//                         break;
//                     case 2: // L
//                         targetRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, knockbackDirection));
//                         break;
//                     case 3: // R
//                         targetRotation = Quaternion.LookRotation(Vector3.Cross(knockbackDirection, Vector3.up));
//                         break;
//                 }

//                 // Smoothly rotate towards the target rotation
//                 transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
//             }

//             // moving direction
//             Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
//             if (farHit)
//             {
//                 targetDirection = knockbackDirection;
//             }

//             // move the player
//             if (movable || rolling || farHit)
//                 _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
//                                     new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

//             // ======================================

//             // update animator if using character
//             _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
//             if (_animationBlend < 0.01f) _animationBlend = 0f;

//             if (_hasAnimator)
//             {
//                 _animator.SetFloat(_animIDSpeed, _animationBlend);
//                 _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
//             }
//         }

//         private void FunctionalInput() // ADDNEWACTIONS
//         {
//             // Priorty in accending order

//             // switchItem
//             // switchL
//             // switchR
//             // interact
//             RL_Camlock = F_Camlock > 0 ? !_input.camlock : false;
//             BT_Camlock = F_Camlock == 0 ? _input.camlock : false;
//             F_Camlock = _input.camlock ? F_Camlock + 1 : 0;

//             RL_Jump = F_Jump > 0 ? !_input.jump : false;
//             BT_Jump = F_Jump == 0 ? _input.jump : false;
//             F_Jump = _input.jump ? F_Jump + 1 : 0;

//             RL_Attack = F_Attack > 0 ? !_input.attack : false;
//             BT_Attack = F_Attack == 0 ? _input.attack : false;
//             F_Attack = _input.attack ? F_Attack + 1 : 0;

//             RL_Heal = F_Heal > 0 ? !_input.heal : false;
//             BT_Heal = F_Heal == 0 ? _input.heal : false;
//             F_Heal = _input.heal ? F_Heal + 1 : 0;

//             RL_Dodge = F_Dodge > 0 ? !_input.dodge : false;
//             BT_Dodge = F_Dodge == 0 ? _input.dodge : false;
//             F_Dodge = _input.dodge ? F_Dodge + 1 : 0;

//             // Long

//             RL_Item = F_Item > 0 ? !_input.item : false;
//             RL_SkillL = F_SkillL > 0 ? !_input.skillL : false;
//             RL_SkillR = F_SkillR > 0 ? !_input.skillR : false;

//             BT_Item = F_Item == 0 ? _input.item : false;
//             BT_SkillL = F_SkillL == 0 ? _input.skillL : false;
//             BT_SkillR = F_SkillR == 0 ? _input.skillR : false;

//             F_Item = _input.item ? F_Item + 1 : 0;
//             F_SkillL = _input.skillL ? F_SkillL + 1 : 0;
//             F_SkillR = _input.skillR ? F_SkillR + 1 : 0;
//         }

//         private void Idle() // ADDNEWACTIONS
//         {
//             // Placeholder for idle behavior
//             laststateInfo = stateInfo;
//             lastInfo = currentInfo;

//             stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
//             currentInfo = stateInfo.shortNameHash;

//             if (currentInfo != lastInfo)
//             {
//                 _animator.SetInteger(_animaIDNextState, 0);
//             }
            
//             if (_actionTimeoutDelta >= -_forceLeaveTimeoutDelta && Grounded)
//             {
//                 _actionTimeoutDelta -= Time.deltaTime;
//                 if (_actionTimeoutDelta >= 0.0f && currentHealth >= 0.0f)
//                     _animator.SetBool(_animIDActionLock, true);
//             }

//             if (_invincibleTimeoutDelta >= 0.0f)
//             {
//                 _invincibleTimeoutDelta -= Time.deltaTime;
//             }

//             if (IsInAnimationStateArray(new string[] { 
//                     "Idle Walk Run Blend",
//                 }))
//             {
//                 foreach (var hurtbox in hurtboxes)
//                 {
//                     hurtbox.invincible = false;
//                 }
//                 ValueEditing(0, 0, maxStamina * Time.deltaTime * 0.333f, 0, 0);
//                 for (int i = 0; i < currentDizzyDamageLevel.Length; i++)
//                 {
//                     currentDizzyDamageLevel[i] = dizzyDamageLevel[i];
//                 }
//             }

//             // if (_forceEnterTimeoutDelta >= 0.0f)
//             // {
//             //     _forceEnterTimeoutDelta -= Time.deltaTime;
//             // }
            
//             bool[] inputState = {
//                 false,
//                 RL_Dodge,
//                 RL_Heal,
//                 RL_Attack,
//                 RL_Jump,
//                 _input.skillR || RL_SkillR,
//                 _input.skillL || RL_SkillL,
//                 _input.item || RL_Item,
//                 false, 
//                 false, 
//                 _input.interact, 
//                 false, 
//                 false, 
//                 false
//             };

//             for (int i = 0; i < inputState.Length; i++)
//             {
//                 if (inputState[i])
//                 {
//                     // if (i == 4 && !Grounded) continue;
//                     if (i == 2 && currentHealth >= maxHealth) continue;

//                     nextStateID = i;
//                     _animator.SetInteger(_animaIDNextState, nextStateID);
//                     if (nextStateID != 1 && nextStateID != 2 && nextStateID != 4) break;
//                     if (Grounded) break;
//                 }
//             }
//             //_globalTimeoutDelta += Time.deltaTime;
//             //Debug.Log("Global Timeout Delta: " + _globalTimeoutDelta);
//             //Debug.Log("Dodge Input: " + RL_Dodge + ", Attack Input: " + RL_Attack + ", Heal Input: " + RL_Heal);
//             //Debug.Log("RL_Heal: " + RL_Heal + " BT_Heal: " + BT_Heal + " F_Heal: " + F_Heal + " _input.heal: " + _input.heal);
//             //Debug.Log("Next State ID: " + nextStateID);
//             //Debug.Log("Next State ID: " + nextStateID + "Action Timeout Delta: " + _actionTimeoutDelta);
//             //Debug.Log(nextStateID == 2 && _actionTimeoutDelta <= 0.0f && _hasAnimator && Grounded);
//         }

//         private void JumpAndGravity()
//         {
//             if (Grounded)
//             {
//                 // reset the fall timeout timer
//                 _fallTimeoutDelta = FallTimeout;

//                 // update animator if using character
//                 if (_hasAnimator)
//                 {
//                     _animator.SetBool(_animIDJump, false);
//                     _animator.SetBool(_animIDFreeFall, false);
//                 }

//                 // stop our velocity dropping infinitely when grounded
//                 if (_verticalVelocity < 0.0f)
//                 {
//                     _verticalVelocity = -4f;
//                 }

//                 // Jump
//                 if (nextStateID == 4 && _jumpTimeoutDelta <= 0.0f && IsInAnimationStateArray(new string[] { 
//                     "Idle Walk Run Blend"
//                 }))
//                 {
//                     // the square root of H * -2 * G = how much velocity needed to reach desired height
//                     _verticalVelocity = Mathf.Sqrt(JumpHeight * -4f * Gravity);

//                     // update animator if using character
//                     if (_hasAnimator)
//                     {
//                         _animator.SetBool(_animIDJump, true);
//                     }

//                     nextStateID = 0;
//                 }

//                 // jump timeout
//                 if (_jumpTimeoutDelta >= 0.0f)
//                 {
//                     _jumpTimeoutDelta -= Time.deltaTime;
//                 }
//                 else
//                 {
//                     _jumpTimeoutDelta = -1f;
//                 }
//             }
//             else
//             {
//                 // reset the jump timeout timer
//                 _jumpTimeoutDelta = JumpTimeout;

//                 // fall timeout
//                 if (_fallTimeoutDelta >= 0.0f)
//                 {
//                     _fallTimeoutDelta -= Time.deltaTime;
//                 }
//                 else
//                 {
//                     _fallTimeoutDelta = -1f;
//                     // update animator if using character
//                     if (_hasAnimator)
//                     {
//                         _animator.SetBool(_animIDFreeFall, true);
//                     }
//                 }

//                 // if we are not grounded, do not jump
//                 // _input.jump = false;
//             }

//             // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
//             if (_verticalVelocity < _terminalVelocity)
//             {
//                 _verticalVelocity += Gravity * Time.deltaTime;
//             }
//         }

//         private void InAirInvincible()
//         {
//             if (IsInAnimationStateArray(airStates) && currentInfo != lastInfo)
//             {
//                 foreach (var hurtbox in hurtboxes)
//                 {
//                     switch (hurtbox.hurtboxType)
//                     {
//                         case Hurtbox.HurtboxType.Stomach:
//                         case Hurtbox.HurtboxType.Legs: 
//                             hurtbox.invincible = true;
//                             break;
//                         default:
//                             break;
//                     }
//                 }      
//             }
//         }

//         private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
//         {
//             if (lfAngle < -360f) lfAngle += 360f;
//             if (lfAngle > 360f) lfAngle -= 360f;
//             return Mathf.Clamp(lfAngle, lfMin, lfMax);
//         }

//         // private void OnDrawGizmosSelected()
//         // {
//         //     Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
//         //     Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

//         //     if (Grounded) Gizmos.color = transparentGreen;
//         //     else Gizmos.color = transparentRed;

//         //     // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
//         //     Gizmos.DrawSphere(
//         //         new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
//         //         GroundedRadius);
//         // }

//         private void OnFootstep(AnimationEvent animationEvent)
//         {
//             if (animationEvent.animatorClipInfo.weight > 0.5f)
//             {
//                 if (FootstepAudioClips.Length > 0)
//                 {
//                     var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
//                     AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
//                 }
//             }
//         }

//         private void OnLand(AnimationEvent animationEvent)
//         {
//             if (animationEvent.animatorClipInfo.weight > 0.5f)
//             {
//                 AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
//             }
//         }

//         // new functions
//         // ADDNEWACTIONS
//         private void dodgeAnimation()
//         {
//             if (currentInfo != lastInfo)
//             {
//                 _animator.SetBool(_animIDDodge, false);
//                 _animator.SetBool(_animIDDodgeControl, false);
//             }

//             // if (forceEnter == 1)
//             // {
//             //     if (IsInAnimationStateArray(new string[] { "Idle Walk Run Blend" }))
//             //     {
//             //         _animator.SetBool(_animIDDodge, true);
//             //         _animator.SetBool(_animIDDodgeControl, false);
//             //         _actionTimeoutDelta = DodgeTimeout;
//             //         nextStateID = 0;

//             //         _forceEnterTimeoutDelta = 0.2f;
//             //     }
//             //     else
//             //     {
//             //         forceEnter = 0;
//             //     }
//             // }

//             if (nextStateID == 1 && _actionTimeoutDelta <= 0.0f && _hasAnimator)
//             {
//                 // update animator if using character
//                 string[] usingDodge = {
//                     "InAir",
//                     "Heal",
//                     "Heal 0",
//                     "OneHand_Up_Roll_F 0",
//                     "OneHand_Up_Attack_A_1",
//                     "OneHand_Up_Attack_A_2",
//                     "OneHand_Up_Attack_A_5 (Run)",
//                     "OneHand_Up_Attack_A_2 (Dodge)",
//                     "FallAttack"
//                 };
//                 string[] usingDodgeControl = {
//                     "OneHand_Up_Roll_F"
//                 };
//                 if ((IsInAnimationStateArray(usingDodge) &&
//                     _actionTimeoutDelta >= -_forceLeaveTimeoutDelta + 0.3f) ||
//                     IsInAnimationStateArray(new string[] { "Idle Walk Run Blend" }))
//                 {
//                     _animator.SetBool(_animIDDodge, true);
//                     _animator.SetBool(_animIDDodgeControl, false);
//                     _actionTimeoutDelta = DodgeTimeout;
//                     nextStateID = 0;
//                 }
//                 else if (IsInAnimationStateArray(usingDodgeControl) && 
//                     _actionTimeoutDelta >= -_forceLeaveTimeoutDelta + 0.3f)
//                 {
//                     _animator.SetBool(_animIDDodge, false);
//                     _animator.SetBool(_animIDDodgeControl, true);
//                     _actionTimeoutDelta = DodgeTimeout;
//                     nextStateID = 0;
//                 }
//                 else
//                 {
//                     _animator.SetBool(_animIDActionLock, false);
//                 }
//             }
//         }

//         private void Dodge()
//         {
//             if (IsInAnimationStateArray(dodgeStates))
//             {
//                 if (currentInfo != lastInfo)
//                 {
//                     foreach (var hurtbox in hurtboxes)
//                     {
//                         hurtbox.invincible = true;
//                     }
//                     _invincibleTimeoutDelta = Dodge_Invincible;
//                 }

//                 // float targetSpeed = RollSpeed;
//                 // if (_actionTimeoutDelta <= 0.0f)
//                 // {
//                 //     _lastMoveInput = _input.move;
//                 //     targetSpeed = 1.0f;
//                 // }
//                 // if (_actionTimeoutDelta <= -_forceLeaveTimeoutDelta)
//                 //     targetSpeed = 0.0f;
//                 // GradMove(targetSpeed, 1f);

//                 // Vector3 targetDirection = transform.forward;

//                 // // move the player
//                 // _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
//                 //                  new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

//                 if (_invincibleTimeoutDelta <= 0.0f)
//                 {
//                     foreach (var hurtbox in hurtboxes)
//                     {
//                         hurtbox.invincible = false;
//                     }
//                 }

//                 // Todo: add stamina consumption
//             }
//         }

//         private void healAnimation()
//         {
//             if (currentInfo != lastInfo)
//             {
//                 _animator.SetBool(_animIDHeal, false);
//                 _animator.SetBool(_animIDHealControl, false);
//             }

//             // if (forceEnter == 2 && _forceEnterTimeoutDelta <= 0.0f)
//             // {
//             //     if (IsInAnimationStateArray(new string[] { "Idle Walk Run Blend" }))
//             //     {
//             //         _animator.SetBool(_animIDHeal, true);
//             //         _animator.SetBool(_animIDHealControl, false);
//             //         _actionTimeoutDelta = HealTimeout;
//             //         nextStateID = 0;

//             //         _forceEnterTimeoutDelta = 0.2f;
//             //     }
//             //     else
//             //     {
//             //         forceEnter = 0;
//             //     }
//             // }

//             if (nextStateID == 2 && _actionTimeoutDelta <= 0.0f && _hasAnimator && Grounded)
//             {
//                 // update animator if using character
//                 string[] usingHeal = {
//                     "Heal 0",
//                     "OneHand_Up_Roll_F",
//                     "OneHand_Up_Roll_F 0"
//                 };
//                 string[] usingHealControl = {
//                     "Heal"
//                 };
//                 if ((IsInAnimationStateArray(usingHeal) &&
//                     _actionTimeoutDelta >= -_forceLeaveTimeoutDelta + 0.3f) ||
//                     IsInAnimationStateArray(new string[] { "Idle Walk Run Blend" }))
//                 {
//                     _animator.SetBool(_animIDHeal, true);
//                     _animator.SetBool(_animIDHealControl, false);
//                     _actionTimeoutDelta = HealTimeout;
//                     if (lastInfoIsInAnimationStateArray(healStates))
//                     {
//                         _actionTimeoutDelta -= 1.00f;
//                     }
//                     nextStateID = 0;
//                 }
//                 else if (IsInAnimationStateArray(usingHealControl) && 
//                     _actionTimeoutDelta >= -_forceLeaveTimeoutDelta + 0.3f)
//                 {
//                     _animator.SetBool(_animIDHeal, false);
//                     _animator.SetBool(_animIDHealControl, true);
//                     _actionTimeoutDelta = HealTimeout;
//                     if (lastInfoIsInAnimationStateArray(healStates))
//                     {
//                         _actionTimeoutDelta -= 1.00f;
//                     }
//                     nextStateID = 0;
//                 }
//                 else
//                 {
//                     _animator.SetBool(_animIDActionLock, false);
//                 }
//             }
//         }

//         private void Heal()
//         {
//             if (IsInAnimationStateArray(healStates))
//             {
//                 if (currentInfo != lastInfo)
//                 {
//                     // Heal effect here
//                     ValueEditing(HealAmount, 0, 0, 0, 0);
//                 }
//             }
//         }

//         private void attackAnimation()
//         {
//             if (currentInfo != lastInfo)
//             {
//                 _animator.SetBool(_animIDAttack, false);
//                 _animator.SetBool(_animIDAttackControl, false);
//                 // if (lastInfoIsInAnimationStateArray(new string[] { "Idle Walk Run Blend" }) && 
//                 //     IsInAnimationStateArray(new string[] { "Idle Walk Run Blend" }))
//                 // {
//                 //     _animator.SetBool(_animIDActionLock, false);
//                 // }
//             }

//             // if (forceEnter == 3 && _forceEnterTimeoutDelta <= 0.0f)
//             // {
//             //     if (IsInAnimationStateArray(new string[] { "Idle Walk Run Blend" }))
//             //     {
//             //         _animator.SetBool(_animIDAttack, true);
//             //         _animator.SetBool(_animIDAttackControl, false);
//             //         _actionTimeoutDelta = AttackTimeout;
//             //         nextStateID = 0;

//             //         _forceEnterTimeoutDelta = 0.2f;
//             //     }
//             //     else
//             //     {
//             //         forceEnter = 0;
//             //     }
//             // }
//             if (nextStateID == 3 && _actionTimeoutDelta <= 0.0f && _hasAnimator)
//             {
//                 // update animator if using character
//                 string[] usingAttack = {
//                     "InAir",
//                     "OneHand_Up_Attack_A_2 (Dodge)",
//                     "OneHand_Up_Attack_A_2"
//                 };
//                 string[] usingAttackControl = {
//                     "OneHand_Up_Roll_F",
//                     "OneHand_Up_Roll_F 0",
//                     "OneHand_Up_Attack_A_1",
//                     "OneHand_Up_Attack_A_5 (Run)",
//                     "FallAttack"
//                 };
//                 Debug.Log("Next State ID: " + nextStateID + "Action Timeout Delta: " + _actionTimeoutDelta);
//                 if ((IsInAnimationStateArray(usingAttack) &&
//                     _actionTimeoutDelta >= -_forceLeaveTimeoutDelta) ||
//                     IsInAnimationStateArray(new string[] { "Idle Walk Run Blend", "InAir" }))
//                 {
//                     _animator.SetBool(_animIDAttack, true);
//                     _animator.SetBool(_animIDAttackControl, false);
//                     if (IsInAnimationStateArray(new string[] { "InAir" }))
//                     {
//                         _actionTimeoutDelta = FallAttackTimeout;
//                     }
//                     else
//                     {
//                         _actionTimeoutDelta = AttackTimeout;
//                         updateAttackRotation();
//                     }
//                     nextStateID = 0;
//                 }
//                 else if (IsInAnimationStateArray(usingAttackControl) &&
//                     _actionTimeoutDelta >= -_forceLeaveTimeoutDelta)
//                 {
//                     _animator.SetBool(_animIDAttack, false);
//                     _animator.SetBool(_animIDAttackControl, true);
//                     _actionTimeoutDelta = AttackTimeout;
//                     updateAttackRotation();
//                     nextStateID = 0;
//                 }
//                 else
//                 {
//                     _animator.SetBool(_animIDActionLock, false);
//                 }
//             }
//         }

//         private void Attack()
//         {
//             if (IsInAnimationStateArray(attackStates))
//             {
//                 if (currentInfo != lastInfo)
//                 {
//                     CurAttackTimeout = AttackTimeout;
//                     if (IsInAnimationStateArray(new string[] { "FallAttack" }))
//                         CurAttackTimeout = FallAttackTimeout;
//                 }

//                 if (CurAttackTimeout - _actionTimeoutDelta >= 0.1f &&
//                     _actionTimeoutDelta >= 0.1f)
//                 {
//                     if (IsInAnimationStateArray(new string[] { "FallAttack" }))
//                     {
//                         ActivateAttack(0, 2); // fall attack
//                     }
//                     else
//                     {
//                         ActivateAttack(0, 1); // base attack
//                     }
//                 }
//                 else
//                 {
//                     DeactivateAllHitboxes();
//                 }
//             }
//         }

//         // HELPER METHODS
//         // move and look
//         private bool IsInAnimationStateArray(string[] stateNameArray)
//         {
//             foreach (string name in stateNameArray)
//             {
//                 if (stateInfo.IsName(name))
//                     return true;
//             }
//             return false;
//         }

//         private bool lastInfoIsInAnimationStateArray(string[] stateNameArray)
//         {
//             foreach (string name in stateNameArray)
//             {
//                 if (laststateInfo.IsName(name))
//                     return true;
//             }
//             return false;
//         }

//         private void GradMove(float targetSpeed, float inputMagnitude)
//         {

//             // a reference to the players current horizontal velocity
//             float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

//             float speedOffset = 0.1f;

//             // accelerate or decelerate to target speed
//             if (currentHorizontalSpeed < targetSpeed - speedOffset ||
//                 currentHorizontalSpeed > targetSpeed + speedOffset)
//             {
//                 // creates curved result rather than a linear one giving a more organic speed change
//                 // note T in Lerp is clamped, so we don't need to clamp our speed
//                 _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
//                     Time.deltaTime * SpeedChangeRate);

//                 // round speed to 3 decimal places
//                 _speed = Mathf.Round(_speed * 1000f) / 1000f;
//             }
//             else
//             {
//                 _speed = targetSpeed;
//             }
//         }

//         private void updateAttackRotation()
//         {
//             _attackFollowCamera = true;
//             _attackRotation = _mainCamera.transform.eulerAngles.y;
//         }

//         // combat system
//         private void ValueEditing(float H, float F, float S1, float P, float S2) // H, F, S, P, S
//         {
//             currentHealth += H;
//             currentFocus += F;
//             currentStamina += S1;
//             currentPoise += P;
//             currentStance += S2;

//             if (currentHealth <= 0f)
//                 currentHealth = -0.1f;
//             if (currentFocus <= 0f)
//                 currentFocus = 0f;
//             // if (currentStamina <= 0f)
//             //     currentStamina = 0f;
//             if (currentPoise <= 0f)
//                 currentPoise = -0.1f;
//             if (currentStance <= 0f)
//                 currentStance = -0.1f;

//             if (currentHealth >= maxHealth)
//                 currentHealth = maxHealth;
//             if (currentFocus >= maxFocus)
//                 currentFocus = maxFocus;
//             if (currentStamina >= maxStamina)
//                 currentStamina = maxStamina;
//             if (currentPoise >= maxPoise)
//                 currentPoise = maxPoise;
//             if (currentStance >= maxStance)
//                 currentStance = maxStance;

//             _animator.SetFloat(_animIDHealth, currentHealth);
//             _animator.SetFloat(_animIDPoise, currentPoise);
//             _animator.SetFloat(_animIDStance, currentStance);
//         }
//         private void StatusEffectValueEditing() // later sin deal with
//         {}
//         private void HandleDamage(DamageData damageData)
//         {
//             if (!IsInAnimationStateArray(getHitStates))
//                 pendingDamageDataList.Add(damageData);
//         }
//         private void UpdateHandleDamage()
//         {
//             if (pendingDamageDataList.Count == 0) return;

//             DamageData damageData = pendingDamageDataList[0];
//             pendingDamageDataList.RemoveAt(0);

//             // main part
//             // Debug.Log("Hitted " + currentHealth);
//             float H = 0f, F = 0f, S1 = 0f, P = 0f, S2 = 0f;
//             // Health reduction
//             if (currentHealth >= 0)
//             {
//                 for (int i = 0; i < damageData.damageTypes.Count; i++)
//                 {
//                     H -= damageData.damageTypes[i].Value;
//                     P -= damageData.poiseDamageTypes[i].Value;
//                     S2 -= damageData.poiseDamageTypes[i].Value;
//                 }
//                 // _animator.SetFloat(_animIDHealth, currentHealth);
//                 ValueEditing(H, F, S1, P, S2);
//             }
//             if (currentHealth <= 0)
//             {
//                 Die();
//                 return;
//             }
//             HandlePoiseBreak(damageData);
//         }

//         private void HandlePoiseBreak(DamageData damageData)
//         {
//             if (currentInfo != lastInfo)
//             {
//                 _animator.SetBool(_animIDGetHit, false);
//                 _animator.SetBool(_animIDGetHitControl, false);
//             }
//             knockbackDirection = damageData.knockbackDirection;
//             knockbackindex = damageData.knockbackindex;

//             // Logic for handling poise break effects
//             // if (damageData.damageLevel == 0) return; // shake
//             // if (!currentDizzyDamageLevel[damageData.damageLevel]) return; // shake
//             if (currentPoise <= 0f) // knock
//             {
//                 switch (damageData.damageLevel)
//                 {
//                     case 1:
//                     case 2:
//                         _animator.SetInteger(_animIDGetHitType, 2 * knockbackindex - 3 + 2);
//                         break;
//                     case 3:
//                         _animator.SetInteger(_animIDGetHitType, 9);
//                         break;
//                     case 4:
//                         _animator.SetInteger(_animIDGetHitType, 11);
//                         break;
//                 }
//                 _animator.SetBool(_animIDGetHit, true);
//                 ValueEditing(0, 0, 0, -maxPoise, 0);
//             }
//             else if (forceDizzyDamageLevel[damageData.damageLevel]) // knock
//             {
//                 switch (damageData.damageLevel)
//                 {
//                     case 1:
//                     case 2:
//                         _animator.SetInteger(_animIDGetHitType, 2 * knockbackindex - 3 + damageData.damageLevel);
//                         break;
//                     case 3:
//                         _animator.SetInteger(_animIDGetHitType, 8);
//                         break;
//                     case 4:
//                         _animator.SetInteger(_animIDGetHitType, 10);
//                         break;
//                 }
//                 _animator.SetBool(_animIDGetHit, true);
//             }
//             // For example, play a stagger animation or sound effect
//             // Debug.Log("Poise Break! Player is staggered.");
//         }

//         private void Die()
//         {
//             // Logic for player death
//             Debug.Log("Player has died.");
//             // You can add death animations, respawn logic, etc. here
//         }

//         // Debug
//         // void OnDrawGizmos()
//         // {
//         //     if (!showDebug) return;

//         //     Color blue = new Color(0, 0, 1, 0.3f);
//         //     Color green = new Color(0, 1, 0, 0.3f);

//         //     foreach (var hurtbox in hurtboxes)
//         //     {
//         //         if (hurtbox.invincible)
//         //             Gizmos.color = green;
//         //         else
//         //             Gizmos.color = blue;

//         //         if (hurtbox.collider is BoxCollider box)
//         //         {
//         //             Gizmos.matrix = transform.localToWorldMatrix;
//         //             Gizmos.DrawCube(box.center, box.size);
//         //         }
//         //         else if (hurtbox.collider is SphereCollider sphere)
//         //         {
//         //             Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
//         //         }
//         //         else if (hurtbox.collider is CapsuleCollider capsule)
//         //         {
//         //             // Set the gizmo matrix to the localToWorldMatrix of the GameObject
//         //             Gizmos.matrix = transform.localToWorldMatrix;

//         //             // Calculate the position for the top hemisphere
//         //             Vector3 topSpherePosition = capsule.center + Vector3.up * (capsule.height / 2 - capsule.radius);
//         //             // Calculate the position for the bottom hemisphere
//         //             Vector3 bottomSpherePosition = capsule.center + Vector3.down * (capsule.height / 2 - capsule.radius);

//         //             // Draw the cylindrical part of the capsule
//         //             Gizmos.DrawCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height - capsule.radius * 2, capsule.radius * 2));

//         //             // Draw the top and bottom hemispheres
//         //             Gizmos.DrawSphere(topSpherePosition, capsule.radius);
//         //             Gizmos.DrawSphere(bottomSpherePosition, capsule.radius);
//         //         }
//         //     }
//         // }

//         // Public properties
//         public void ActivateAttack(int hitboxIndex, int damageDataIndex)
//         {
//             if (hitboxIndex >= 0 && hitboxIndex < hitboxes.Length)
//             {
//                 hitboxes[hitboxIndex].UpdateDamageData(damageDatas[damageDataIndex]);
//                 hitboxes[hitboxIndex].ActivateHitbox();
//             }
//         }

//         public void DeactivateAllHitboxes()
//         {
//             foreach (var hitbox in hitboxes)
//             {
//                 hitbox.DisableHitbox();
//             }
//         }
//     }
 
// // todo: feint, ash of war
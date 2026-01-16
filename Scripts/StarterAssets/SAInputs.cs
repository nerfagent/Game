using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// namespace StarterAssets
// {
	public class SAInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint; // long
		//public bool topdown; // new
        public bool switchItem; // new
        public bool switchL; // new
        public bool switchR; // new
        public bool interact; // new // long
		public bool camlock; // new
        public bool item; // new // long
        public bool skillL; // new // long
        public bool skillR; // new // long
        public bool attack; // new
		public bool heal;  // new
        public bool dodge; // new
		// ADDNEWACTIONS

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		// ADDNEWACTIONS
		public void OnSwitchItem(InputValue value)
		{
			SwitchItemInput(value.isPressed);
		}

		public void OnSwitchL(InputValue value)
		{
			SwitchLInput(value.isPressed);
		}

		public void OnSwitchR(InputValue value)
		{
			SwitchRInput(value.isPressed);
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnCamlock(InputValue value)
		{
			CamlockInput(value.isPressed);
		}

		public void OnItem(InputValue value)
		{
			ItemInput(value.isPressed);
		}

		public void OnSkillL(InputValue value)
		{
			SkillLInput(value.isPressed);
		}

		public void OnSkillR(InputValue value)
		{
			SkillRInput(value.isPressed);
		}

		public void OnAttack(InputValue value)
		{
			AttackInput(value.isPressed);
		}

		public void OnHeal(InputValue value)
		{
			HealInput(value.isPressed);
		}

		public void OnDodge(InputValue value)
		{
			DodgeInput(value.isPressed);
		}
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

        // =============================== // IMPORTANT ACTIONS

        public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
            //Debug.Log("jump");
        }

        public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
            //Debug.Log("run");
        }

        // =============================== // ADDNEWACTIONS

		public void SwitchItemInput(bool newSwitchItemState)
		{
			switchItem = newSwitchItemState;
            //Debug.Log("switchItem");
        }

		public void SwitchLInput(bool newSwitchLState)
		{
			switchL = newSwitchLState;
            //Debug.Log("switchL");
        }

		public void SwitchRInput(bool newSwitchRState)
		{
			switchR = newSwitchRState;
            //Debug.Log("switchR");
        }

		public void InteractInput(bool newInteractState)
        {
			interact = newInteractState;
            //Debug.Log("interact");
        }

		public void CamlockInput(bool newCamlockState)
        {
			camlock = newCamlockState;
            //Debug.Log("camlock");
        }

		public void ItemInput(bool newItemState)
        {
			item = newItemState;
            //Debug.Log("item");
        }

		public void SkillLInput(bool newSkillLState)
        {
			skillL = newSkillLState;
            //Debug.Log("skillL");
        }

		public void SkillRInput(bool newSkillRState)
        {
			skillR = newSkillRState;
            //Debug.Log("skillR");
        }

        public void AttackInput(bool newAttackState)
		{
			attack = newAttackState;
            //Debug.Log("attack");
        }

		public void HealInput(bool newHealState)
		{
			heal = newHealState;
            //Debug.Log("heal");
        }

        public void DodgeInput(bool newDodgeState)
		{
			dodge = newDodgeState;
            //Debug.Log("dodge");
        }

        // ===============================

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
// }
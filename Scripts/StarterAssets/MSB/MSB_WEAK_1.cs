using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;

// MSB = MyStateBehaviour
public class MSB_WEAK_1 : StateMachineBehaviour
{
    public PlayerMain_A0 _playerMain;
    public int lastInfo;
    public int currentInfo = -1;

    public bool[] currentWeaknesses = { false, false, false, false, false, false };

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
        for (int i = 0; i < currentWeaknesses.Length; i++)
        {
            if (currentWeaknesses[i])
            {
                _playerMain._combatSystem.SetWeakness(currentWeaknesses[i], i);
            }
        }
        // other function
        // vfx
        // sfx
    }
}
// Assets/Scripts/Core/UIManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private List<SkillUI> skillUIList;
    [SerializeField]
    private GameObject pauseMenu;
    [SerializeField]
    private Button continueButton;

    private void Awake()
    {
        
    }

    private void Start()
    {
        GameManager.onGamePaused += ShowPauseMenu;
        GameManager.onGameResumed += HidePauseMenu;
        continueButton.onClick.AddListener(()=>GameManager.onGameResumed?.Invoke());
        if(CooldownSystem.Instance != null)
        {
            int skillCount = CooldownSystem.Instance.GetSkillCount();
            if (skillUIList.Count > skillCount)
            {
                Debug.LogError("SkillUI and skill size mismatch");
            }
            else
            {
                for (int i = 0; i < skillCount; i++)
                {
                    skillUIList[i].AssignSkill(CooldownSystem.Instance.GetSkill(i));
                }
            }
        }
        else
        {
            Debug.LogError("CooldownSystem Instance is null");
        }

        if(GameManager.Instance.CurrentState != GameManager.GameState.Paused)HidePauseMenu();

    }

    private void HidePauseMenu()
    {
        pauseMenu.SetActive(false);
    }

    private void ShowPauseMenu()
    {
        pauseMenu.SetActive(true);
    }
}

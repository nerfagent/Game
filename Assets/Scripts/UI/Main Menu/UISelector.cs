using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UISelector : MonoBehaviour
{
    [SerializeField]
    private MenuUI[] uis;

    private int currentIndex = 0;

    // Start is called before the first frame update
    private void Start()
    {
        if (uis.Length < 2)
        {
            Debug.LogError("Menu has only one option");
            return;
        }
        for(int i = 1; i < uis.Length; i++)
        {
            uis[i].UnHighlight();
        }
        uis[0].Highlight();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentIndex == 0)
            {
                currentIndex = uis.Length - 1;
            }
            else
            {
                currentIndex--;
            }
            UIHover();
        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentIndex == uis.Length-1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }

            UIHover();
        }
    }

    private void UIHover()
    {
        for (int i = 0; i < uis.Length; i++)
        {
            if (i != currentIndex)
            {
                uis[i].UnHighlight();
            }
            else
            {
                uis[i].Highlight();
            }
        }
    }

    public void UIHover(MenuUI UI)
    {
        for (int i = 0; i < uis.Length; i++)
        {
            if (uis[i] != UI)
            {
                uis[i].UnHighlight();
            }
            else
            {
                currentIndex = i;
                uis[i].Highlight();
            }
        }
    }

    public void UIClicked(MenuUI ui)
    {
        switch (ui.Command)
        {
            case Command.START:
                Debug.Log("Start Game.");
                break;
            case Command.SETTING:
                Debug.Log("Setting.");
                break;
            case Command.EXIT:
                Debug.Log("Exit Game.");
                break;
            case Command.YES:
                Debug.Log("YES");
                break;
            case Command.NO:
                Debug.Log("NO");
                break;
        }
    }
}

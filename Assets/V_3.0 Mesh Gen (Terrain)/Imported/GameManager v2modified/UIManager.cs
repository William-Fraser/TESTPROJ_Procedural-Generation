using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Canvas titleMenu;
    public Canvas gameplay;
    public Canvas pause;
    public Canvas options;
    public Canvas credits;

    private void Awake()
    {
        LoadTitleMenu();
    }

    public void DisableAll()
    {
        titleMenu.enabled = false;
        options.enabled = false;
        credits.enabled = false;
        gameplay.enabled = false;
        pause.enabled = false;
    }

    public void LoadTitleMenu()
    {
        DisableAll();
        titleMenu.enabled = true;
    }

    public void LoadGameplay()
    {
        DisableAll();
        gameplay.enabled = true;
    }

    public void LoadPauseScreen()
    {
        credits.enabled = false;
        options.enabled = false;
        pause.enabled = true;
    }

    public void LoadOptions()
    {
        options.enabled = true;
    }

    public void LoadCredits()
    {
        credits.enabled = true;
    }
}

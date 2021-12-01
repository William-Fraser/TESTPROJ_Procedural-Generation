using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager control;
    
    public void ChangeGameStateToTitleMenu()
    {
        GameManager.manager.ChangeState(GameState.TITLEMENU);
    }
    
    public void ChangeGameStateToGamePlay()
    {
        GameManager.manager.ChangeState(GameState.GAMEPLAY);
    }
    
    public void ChangeGameStateToWin()
    {
        GameManager.manager.ChangeState(GameState.WIN);
    }
    
    public void ChangeGameStateToLose()
    {
        GameManager.manager.ChangeState(GameState.LOSE);
    }

    public void ChangeGameStateToPause()
    {
        GameManager.manager.ChangeState(GameState.PAUSE);
    }

    public void ChangeGameStateToOptions()
    {
        GameManager.manager.ChangeState(GameState.OPTIONS);
    }

    public void ChangeGameStateToCredits()
    {
        GameManager.manager.ChangeState(GameState.CREDITS);
    }

    public void ChangeGameStateToPrevious()
    {
        GameManager.manager.ReturnToPreviousState();
    }

    public void Save()
    {
        GameManager.manager.Save();
    }

    public void Load()
    {
        GameManager.manager.Load();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}

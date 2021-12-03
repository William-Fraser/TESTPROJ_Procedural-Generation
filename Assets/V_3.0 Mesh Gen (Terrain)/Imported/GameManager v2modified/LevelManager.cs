using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject player;

    public void ChangeGameStateToTitleMenu()
    {
        GameManager.manager.ChangeState(GameState.TITLEMENU);
    }
    
    public void ChangeGameStateToGamePlay()
    {
        GameManager.manager.ChangeState(GameState.GAMEPLAY);
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
        Time.timeScale = 1;
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

    public void SpawnDebugPlayer(Vector3 spawnPoint, ChunkGenerator chunkGenerator)
    {

        Debug.LogError("this runs once");
        Instantiate(player, spawnPoint, Quaternion.identity); // #MN: ensures spawning above the terrain
    }
}

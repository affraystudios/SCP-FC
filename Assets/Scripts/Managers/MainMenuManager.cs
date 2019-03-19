using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    public void New()
    {
        GameManager.manager.saveManager.New(GameManager.manager.playerData, GameManager.manager.worldSettings);
    }

    public void Load(string save)
    {
        GameManager.manager.saveManager.StartLoad(save);
    }

    public void Quit()
    {
        GameManager.manager.Quit();
    }

    public void QuitGameButForReal(){
      Application.Quit();
    }
}

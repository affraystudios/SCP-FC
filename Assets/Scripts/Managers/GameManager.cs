using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {

    public static GameManager manager;

    [Header("Persistant Managers")]
    //Persistant variables, these are attached to the GameManager
    public InputManager inputManager;
    public AIManager AIManager;
    public SaveManager saveManager;
    public EventSystem eventSystem;

    [Header("Local Managers")]
    //These variables are in the scene
    public GameUIManager UIManager;
    public TileManager tileManager;
    public MenuManager menuManager;
    public TimeManager timeManager;

    [Header("Data")]
    public PlayerData playerData;
    public WorldSettings worldSettings;

    void Awake ()
    {
        //Singleton
        if (manager == null)
        {
            //Give the gamemanager a reference to itself so everything can access it
            manager = this;
            //Make it persistant between scenes
            DontDestroyOnLoad(this.gameObject);
        }
        else if (manager != this)
        {
            Destroy(gameObject);
        }
        //Subscribe the scene load method to sceneLoad
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    public void Quit ()
    {
        Application.Quit();
    }

    //Load scenes ASync, so that there is no need for loading screens
    public IEnumerator GoToScene (int scene)
    {
        Debug.Log("Going to scene: " + SceneManager.GetSceneByBuildIndex(scene).name);
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(scene);
        while (loadScene.progress < 1)
        {
            //Wait a frame so that it does not freeze
            yield return new WaitForEndOfFrame();
        }
    }

    //This is called whenever a scene is loaded
    public void OnSceneLoad (Scene scene, LoadSceneMode mode)
    {
    }
}
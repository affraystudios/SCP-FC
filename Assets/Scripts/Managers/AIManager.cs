using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public enum AccessLevel
{
    DClass,
    Staff,
    Scientist,
    Security,
    ZoneManager,
    FacilityManager,
    O5
}

public class AIManager : MonoBehaviour
{
    SaveManager saveManager;

    public AccessLevel accessLevelOverride;

    public List<AIBase> AIList;

    public GameObject[] builderPrefabs;

    public GameObject[] characterPrefabs,
        DClassPrefabs, 
        staffPrefabs, 
        scientistPrefabs,
        securityPrefabs,
        zoneManagerPrefabs,
        facilityManagerPrefabs,
        O5Prefabs;

    public AINames names;

    public void Start ()
    {
        saveManager = GameManager.manager.saveManager;
        GameManager.manager.AIManager = this;
    }

    public void AddEventTask(Task taskToAdd, Vector3 position, float audioRadius = 30, float visualRadius = 100, float loudness = 1)
    {
        foreach(AIBase AI in AIList)
        {
            float distance = Vector3.Distance(position, AI.transform.position);
            float chance = (1 - (distance / audioRadius)) * loudness;

            //We only use visual checks if the audio fails
            if(Random.value < chance || Physics2D.Linecast(position, AI.transform.position).collider.gameObject == AI.gameObject)
            {
                AI.AddTask(taskToAdd);
            }
        }
    }

    public void AddGlobalTask(Task taskToAdd)
    {
        foreach (AIBase AI in AIList)
        {
            AI.AddTask(taskToAdd);
        }
    }

    public void RemoveGlobalTask(Task taskToRemove)
    {
        foreach (AIBase AI in AIList)
        {
            AI.tasks.Remove(taskToRemove);
        }
    }

    public AINames LoadAINames()
    {
        AINames aiNames = names;

        aiNames = Resources.Load<AINames>("AINames");
        if (aiNames == null)
        {
            aiNames = ScriptableObject.CreateInstance<AINames>();
            AssetDatabase.CreateAsset(aiNames, "Assets/Resources/AINames.asset");
        }
        return aiNames;
    }

    public void SaveAINames (AINames aiNames)
    {
        AssetDatabase.SaveAssets();
    }

    public void Load()
    {
        AIBase AIObject;

        AIList.Clear();

        //We do this for all the lists of characters
        foreach (CharacterData data in saveManager.SaveData.worldData.AIData.characters)
        {
            AIObject = Instantiate(characterPrefabs[data.id], data.position, Quaternion.identity).GetComponent<AIBase>();
            AIObject.loaded = true;
            AIObject.Load(data);
        }

        foreach (BuilderData data in saveManager.SaveData.worldData.AIData.builders)
        {
            AIObject = Instantiate(builderPrefabs[data.id], data.position, Quaternion.identity).GetComponent<AIBase>();
            AIObject.loaded = true;
            AIObject.Load(data);
        }
    }

    public AIData Save ()
    {
        AIData data = new AIData();

        data.characters = new List<CharacterData>();
        data.builders = new List<BuilderData>();

        foreach(AIBase ai in AIList)
        {
            switch (ai.GetType().ToString())
            {
                case "AIBuilder":
                    data.builders.Add((BuilderData)ai.Save(new BuilderData()));
                    break;
                default:
                    data.characters.Add((CharacterData)ai.Save(new CharacterData()));
                    break;
            }
        }

        return data;
    }

    private void OnEnable()
    {
        names = LoadAINames();
    }

    private void OnDisable()
    {
        SaveAINames(names);
    }
}

public class AINames : ScriptableObject
{
    public string[] names = new string[1];
}

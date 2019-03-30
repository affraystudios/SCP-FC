using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    public bool loading;
    private SaveData saveData;
    public SaveData SaveData
    {
        get
        {
            return saveData;
        }
    }

    GameManager manager;

    private void Start()
    {
        manager = GameManager.manager;
        manager.saveManager = this;
    }

    public void Save()
    {
        if(!Directory.Exists(Application.persistentDataPath + "/Saves"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

        BinaryFormatter bf = new BinaryFormatter();
        Debug.Log("Saving Data");
        loading = false;

        Debug.Log("Saving Player Data");
        saveData.playerData = manager.playerData;

        Debug.Log("Saving World Data");
        saveData.worldData.tileData = manager.tileManager.Save();
        saveData.worldData.time = manager.timeManager.timeStamp;

        Debug.Log("Saving AI Data");
        saveData.worldData.AIData = manager.AIManager.Save();

        Debug.Log("Saving Settings Data");
        saveData.settings = manager.worldSettings;

        Debug.Log("Saving Data to Disk");
        FileStream file = File.Create(Application.persistentDataPath + "/Saves/" + saveData.settings.name + ".FC");

        bf.Serialize(file, saveData);

        file.Close();
        Debug.Log("Data Successfully saved");
    }

    public void StartLoad(string saveName)
    {
        if (!File.Exists(Application.persistentDataPath + "/Saves/" + saveName))
            return;

        Debug.Log("Loading Data");
        Debug.Log("Loading Data From Disk");
        //Deserialize saved data
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/Saves/" + saveName, FileMode.Open);
        saveData = (SaveData)bf.Deserialize(file);

        //Close the file to prevent issues
        file.Close();

        manager.playerData = saveData.playerData;
        manager.worldSettings = saveData.settings;
        StartCoroutine(Load());

    }

    public IEnumerator Load ()
    {
        loading = true;
        Debug.Log("Loading Scene");
        yield return StartCoroutine(manager.GoToScene(1));
        loading = false;

        Debug.Log("Loading Tile Data");
        manager.tileManager.Load();

        Debug.Log("Loading AI Data");
        manager.AIManager.Load();

        Debug.Log("Loading Time Data");
        manager.timeManager.Load();

        Debug.Log("Data Successfully Loaded");
    }

    public void New(PlayerData data, WorldSettings settings)
    {
        manager.playerData = data;
        manager.worldSettings = settings;

        StartCoroutine(manager.GoToScene(1));
    }
}

[System.Serializable]
public struct PlayerData
{
    public int money;

    //I'm thinking that we use this as the 05'ometer.
    //100 is neutral, 200 is win, 0 is loss because of facility shutdown.
    //Normal mode could start with this as 100, easy would start at 120, hard would start at 70.
    //The player would not see this
    public int reputation;

    public PlayerData(int startingDifficulty)
    {
        switch (startingDifficulty)
        {
            default:
                //$2M starting balance
                money = 2000000;
                //Rep starts medium
                reputation = 1;
                break;
            case 0:
                //$20M starting balance
                money = 20000000;
                //Rep starts high
                reputation = 120;
                break;
            case 1:
                //$5M starting balance
                money = 5000000;
                //Rep starts medium
                reputation = 100;
                break;
            case 2:
                //$1M starting balance
                money = 1000000;
                //Rep starts low
                reputation = 70;
                break;
        }
    }

    public PlayerData(int startingDifficulty, int startingMoney, int startingReputation)
    {
        money = startingMoney;
        reputation = startingReputation;
    }
}

[System.Serializable]
public struct SaveData
{
    public PlayerData playerData;
    public WorldSettings settings;
    public WorldData worldData;
}

//----------NOTE----------
//Unity's serialization DOES NOT support polymorphism
//As such, we have to store different data types in separate arrays
//I know, I hate it as well
//----------NOTE----------
[System.Serializable]
public struct WorldData
{
    public TimeManager.TimeStamp time;
    public TileData tileData;
    public AIData AIData;

    public List<ObjectData> objects;
    public List<EntityData> entities;
}

[System.Serializable]
public struct AIData
{
    public List<CharacterData> characters;
    public List<BuilderData> builders;
}

[System.Serializable]
public struct WorldSettings
{
    public string name;
    public SerializableVector2 size;
    public int biome;
    public int difficulty;

    public WorldSettings(string worldName, int worldBiome, int worldDifficulty, Vector2Int worldSize)
    {
        name = worldName;
        size = (Vector2)worldSize;
        difficulty = worldDifficulty;
        biome = worldBiome;
    }

    public WorldSettings(string worldName,int worldBiome, int worldDifficulty, int worldSize)
    {
        name = worldName;
        size = new Vector2(worldSize, worldSize);
        difficulty = worldDifficulty;
        //Sandy Biome
        biome = worldBiome;
    }
}

[System.Serializable]
public class ObjectData
{
    public bool useUtilityList = false;
    public SerializableVector3 position;
}

[System.Serializable]
public class EntityData : ObjectData
{
    public int health = 100;
    public bool dead;
}

[System.Serializable]
public class CharacterData : EntityData
{
    public int id;
    public List<SerializableTask> tasks;
    public SerializableTask task;
    public AccessLevel accessLevel;
    public float loyalty;
    public int home;
    public string name;
}

[System.Serializable]
public class BuilderData : CharacterData
{
    public float buildDistance;
}

[System.Serializable]
public class InteractableData : EntityData
{
    public bool on;
}

[System.Serializable]
public class ElectronicData : InteractableData
{
    public bool hasPower;
    public int requiredPower;
    public List<SerializableVector3> input;
    public SerializableVector3 generator,
        breaker;
}

[System.Serializable]
public class WireData : ElectronicData
{
    public List<SerializableVector3> output;
}

[System.Serializable]
public class GeneratorData : InteractableData
{
    public int totalPower, availablePower;
    public bool blackout;

    public List<SerializableVector3> connected;
}

[System.Serializable]
public class BreakerData : WireData
{
    public int takenPower;
    public bool blackout;

    public List<SerializableVector3> connected;
}

[System.Serializable]
public struct TileData
{
    public float[,] pathfindingCosts, pathfindingRatios, waterLevels;
    public int[,] wallHealth;

    public int[,] backgroundTileTypes,
        floorTileTypes,
        wallTileTypes,
        objectTileTypes,
        utilityTileTypes,
        
        tileRotation,

        backgroundTileTypesToBuild,
        floorTileTypesToBuild,
        wallTileTypesToBuild,
        objectTileTypesToBuild,
        utilityTileTypesToBuild;

    public List<SerializableTask> tasks;

    public List<SerializableZone> zones;

    public List<SerializableVector3> waterToSimulate;

    public List<InteractableData> interactables;
    public List<ElectronicData> electronics;
    public List<GeneratorData> generators;
    public List<WireData> wires;
    public List<BreakerData> breakers;

    public List<ObjectData> objects;

    public TileData(WorldSettings settings)
    {
        Vector2 size = settings.size;

        int xSize = (int)size.x;
        int ySize = (int)size.y;

        int[,] tileTypes = new int[xSize, ySize];

        //We have to default everything to -1 so that when we load it doesn't mistake empty tiles for the first tile in the list.
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                tileTypes[x, y] = -1;
            }
        }

        zones = new List<SerializableZone>();

        waterToSimulate = new List<SerializableVector3>();

        floorTileTypesToBuild = (int[,])tileTypes.Clone();
        wallTileTypesToBuild = (int[,])tileTypes.Clone();
        objectTileTypesToBuild = (int[,])tileTypes.Clone();
        backgroundTileTypesToBuild = (int[,])tileTypes.Clone();
        utilityTileTypesToBuild = (int[,])tileTypes.Clone();

        tileRotation = (int[,])tileTypes.Clone();

        floorTileTypes = (int[,])tileTypes.Clone();
        wallTileTypes = (int[,])tileTypes.Clone();
        objectTileTypes = (int[,])tileTypes.Clone();
        backgroundTileTypes = (int[,])tileTypes.Clone();
        utilityTileTypes = (int[,])tileTypes.Clone();

        pathfindingCosts = new float[xSize, ySize];
        pathfindingRatios = new float[xSize, ySize];
        waterLevels = new float[xSize, ySize];
        wallHealth = new int[xSize, ySize];

        tasks = new List<SerializableTask>();

        objects = new List<ObjectData>();

        interactables = new List<InteractableData>();

        generators = new List<GeneratorData>();
        electronics = new List<ElectronicData>();
        wires = new List<WireData>();
        breakers = new List<BreakerData>();
    }
}


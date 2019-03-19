using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class LoadGameMenuManager : MonoBehaviour {

    [Header("References")]
    public MainMenuManager mainMenuManager;
    public Transform saveList;

    [Header("Settings")]
    public string fileExtension = ".fc";
    public GameObject savePanelPrefab;

    [Header("Data")]
    //We have this as an array because I don't want anyone to change/insert anything outside of the LoadSaveList() method.
    public string[] saves;
    string selectedSave;

    private void OnEnable()
    {
        saves = LoadSaveList();
        ResetList();
    }

    string[] LoadSaveList ()
    {
        List<string> saveListLS = new List<string>();
        DirectoryInfo fileInfo = new DirectoryInfo(Application.persistentDataPath + "/Saves/");
        FileInfo[] info = fileInfo.GetFiles();

        for( int i = 0; i < info.Length; i++ )
        {
            if(info[i].Extension.ToLower() == fileExtension.ToLower())
            {
                saveListLS.Add(info[i].Name);
            }
        }
        return saveListLS.ToArray();
    }

    public void ResetList ()
    {
        foreach ( Transform child in saveList.transform ) {
            GameObject.Destroy( child.gameObject );
        }
        
        for( int i = 0; i < saves.Length; i++ )
        {
            SavePanel savePanel = Instantiate(savePanelPrefab, saveList).GetComponent<SavePanel>();
            savePanel.saveName = saves[i];
            savePanel.nameText.text = saves[i];
            savePanel.GetComponent<Button>().onClick.AddListener(() => ReadPanel(savePanel));
        }
    }

    public void ReadPanel (SavePanel panel)
    {
        selectedSave = panel.saveName;

        if (panel.open)
            return;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/Saves/" + panel.saveName, FileMode.Open);

        panel.difficultyText.gameObject.SetActive(true);
        panel.moneyText.gameObject.SetActive(true);
        panel.reputationText.gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(saveList.GetComponent<RectTransform>());

        SaveData data = (SaveData)bf.Deserialize(file);
        file.Close();

        panel.nameText.text = "Name: " + panel.saveName;
        panel.difficultyText.text = "Difficulty: " + (Difficulty)data.settings.difficulty;
        panel.moneyText.text = "Funds: " + data.playerData.money;
        panel.reputationText.text = "Reputation: " + data.playerData.reputation;

        panel.open = true;
    }

    public void DeleteSave() {
        if( selectedSave != null ) {
            File.Delete( Application.persistentDataPath + "/Saves/" + selectedSave );
            selectedSave = null;
            
            saves = LoadSaveList();
            ResetList();
        }
    }

    public void LoadSave()
    {
        if( selectedSave != null )
            mainMenuManager.Load(selectedSave);
    }
}

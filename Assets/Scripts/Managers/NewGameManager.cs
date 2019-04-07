using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewGameManager : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown difficultyDropdown;
    public TMP_Dropdown biomeDropdown;

    public TMP_InputField sizeField,
        nameField;

    [Header("Settings")]
    public WorldSettings defaultSettings;
    WorldSettings settings = new WorldSettings();

    private void OnEnable()
    {
        SetDefaultValues();
    }

    public void UpdateValues()
    {
        settings.biome = biomeDropdown.value;
        settings.difficulty = difficultyDropdown.value;
        settings.size = Vector2.one*(int.Parse(sizeField.text));
        settings.name = nameField.text;

        GameManager.manager.worldSettings = settings;
    }

    public void SetDefaultValues ()
    {
        settings = defaultSettings;
        biomeDropdown.value = settings.biome;
        difficultyDropdown.value = settings.difficulty;
        sizeField.text = settings.size.x.ToString();
        nameField.text = settings.name;

        UpdateValues();
    }
}

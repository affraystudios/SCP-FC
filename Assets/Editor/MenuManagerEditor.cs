using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MenuManager), true)]
public class MenuManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MenuManager menu = (MenuManager)target;
        int targetMenu = menu.currentMenu;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Last Menu"))
        {
            targetMenu = (menu.currentMenu - 1) % menu.menus.Length;
            targetMenu = targetMenu < 0 ? menu.menus.Length - 1 : targetMenu;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        if (GUILayout.Button("Next Menu"))
        {
            targetMenu = (menu.currentMenu + 1) % menu.menus.Length;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        EditorGUILayout.EndHorizontal();
        string[] displayedOptions = new string[menu.menus.Length];
        for (int i = 0; i < displayedOptions.Length; i++)
        {
            displayedOptions[i] = menu.menus[i].gameObject.name;
        }
        targetMenu = EditorGUILayout.Popup("Set Menu", targetMenu, displayedOptions);
        if (targetMenu != menu.currentMenu)
        {
            menu.ChangeMenu(targetMenu);
        }

        if (GUILayout.Button("Select Current Menu"))
        {
            Selection.activeObject = menu.menus[targetMenu];
        }
    }
}

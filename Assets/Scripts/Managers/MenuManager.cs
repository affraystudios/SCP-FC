using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public GameObject[] menus;
    public int currentMenu = 0;

    private void Start()
    {
        GameManager.manager.menuManager = this;
        ChangeMenu(currentMenu);
    }

    public void ChangeMenu (int newMenu)
    {
        foreach(GameObject menu in menus)
        {
            menu.SetActive(false);
        }
        if (newMenu >= 0)
        {
            menus[newMenu].SetActive(true);
            if(!Application.isEditor && menus[newMenu].GetComponentInChildren<Selectable>() != null)
                GameManager.manager.eventSystem.SetSelectedGameObject(menus[newMenu].GetComponentInChildren<Selectable>().gameObject);
        }
        else
        {
            GameManager.manager.eventSystem.SetSelectedGameObject(null);
        }
        currentMenu = newMenu;
    }
}

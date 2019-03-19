using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class rTerminalDesc : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private bool infocus;
    public GameObject descTitle;
    public string Description;

    public void Update()
    {
        if (infocus)
            refreshDesc();
    }
    public void refreshDesc()
    {
        string _updatedname = transform.Find("Text").GetComponent<Text>().text + "\n";
        descTitle.GetComponent<Text>().text = _updatedname + Description;
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        infocus = true;
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        infocus = false;
    }
}

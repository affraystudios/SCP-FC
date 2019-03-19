using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchTerminal : MonoBehaviour {

    bool globalmousein;
    public GameObject TerminalPanel;

    public void Update()
    {
        if (Input.GetMouseButtonDown(1) && globalmousein && !canvaspointerhold.pointerOnUI)
        {
            ExistToggleTrue();
        }
    }
    public void ExistToggleTrue()
    {
        GameObject.Find("Canvas").transform.Find("ResearchTerminalPanel").gameObject.SetActive(true);
    }
    public void ExistToggleFalse()
    {
        canvaspointerhold.pointerOnUI = false;
        GameObject.Find("Canvas").transform.Find("ResearchTerminalPanel").gameObject.SetActive(false);
    }
    private void OnMouseEnter()
    {
        globalmousein = true;
    }

    private void OnMouseExit()
    {
        globalmousein = false;
    }
}

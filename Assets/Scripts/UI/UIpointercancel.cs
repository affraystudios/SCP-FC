
using UnityEngine;
using UnityEngine.EventSystems;

public class UIpointercancel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        canvaspointerhold.pointerOnUI = true;
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        canvaspointerhold.pointerOnUI = false;
    }
}
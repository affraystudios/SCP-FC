using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipUI : Tooltip
{
    RectTransform rectT;

    protected new void Awake()
    {
        base.Awake();
        rectT = GetComponent<RectTransform>();
    }

    protected override void CheckTooltip()
    {
        mousePos = GameManager.manager.inputManager.cursorPosition;

        if (Vector2.Distance(mousePos, transform.position) <= activationDist)
        {
            currentHoverTime += Time.deltaTime;

            if (currentHoverTime >= requiredHoverTime && !UIManager.tooltipOpen)
                SetTooltip();
        }
        else if (tooltipOpen && Vector2.Distance(mousePos, transform.position) > deactivationDist)
        {
            tooltipOpen = false;
                UIManager.toolTip.SetActive(false);
        }
    }
}

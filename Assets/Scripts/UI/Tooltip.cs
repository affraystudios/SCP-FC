using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    protected Camera Cam;
    protected AIBase AI;
    protected GameUIManager UIManager;

    [Header("Tooltip")]
    [TextArea]
    public string header;
    [TextArea]
    public string description;

    [Header("Settings")]
    public bool useContext = true;
    public float activationDist = 1.5f;
    public float deactivationDist = 5f;
    public bool requireHover = true;
    public float requiredHoverTime = 0.5f;

    [Header("Data")]
    public bool tooltipOpen = false;
    public bool contextOpen;
    protected Vector3 mousePos;
    [SerializeField]
    protected float currentHoverTime;
    Dictionary<string, string> properties;
    Dictionary<string, System.Action> commands;


    protected void Awake()
    {
        AI = GetComponent<AIBase>();
        properties = new Dictionary<string, string>();
        commands = new Dictionary<string, System.Action>();
    }

    void Start()
    {
        UIManager = GameManager.manager.UIManager;
        Cam = Camera.main;
        mousePos = Cam.ScreenToWorldPoint(GameManager.manager.inputManager.cursorPosition);
    }

    void Update()
    {
        //We put this in here so for UI we can override just that method
        CheckTooltip();
    }

    private void OnDisable()
    {
        if (tooltipOpen)
        {
            tooltipOpen = false;
            contextOpen = false;
            UIManager.CloseContextMenu();
            UIManager.CloseTooltip();
        }
    }

    protected virtual void CheckTooltip()
    {
        mousePos = Cam.ScreenToWorldPoint(GameManager.manager.inputManager.cursorPosition);

        if (Vector2.Distance(mousePos, transform.position) <= activationDist)
        {
            currentHoverTime += Time.deltaTime;

            if (!UIManager.tooltipOpen && currentHoverTime >= requiredHoverTime)
                SetTooltip();

            if (tooltipOpen && !contextOpen && GameManager.manager.inputManager.secondaryPressed && UIManager.currentTooltip == this && useContext)
                SetContext();
        }
        else if (tooltipOpen && Vector2.Distance(mousePos, transform.position) > deactivationDist)
        {
            tooltipOpen = false;
            contextOpen = false;
            UIManager.CloseContextMenu();
            UIManager.CloseTooltip();
        }
    }

    protected void SetTooltip()
    {
        string tooltipHeaderText = header;

        if (AI != null)
            tooltipHeaderText = AI.AIName + " (" + header + ")";

        UIManager.ProcessTooltip(tooltipHeaderText, description, properties, mousePos, this);
        tooltipOpen = true;
    }

    protected void SetContext()
    {
        UIManager.ProcessContextMenu(header, commands, mousePos);
        contextOpen = true;
    }

    public void SetProperty(string propertyName, string propertyValue)
    {
        if (properties == null)
            properties = new Dictionary<string, string>();

        //If the property has not been set, add it. if not, update the value
        if (properties.ContainsKey(propertyName))
        {
            properties[propertyName] = propertyValue;
        }
        else
        {
            properties.Add(propertyName, propertyValue);
        }
    }

    public void RemoveProperty(string propertyName)
    {
        properties.Remove(propertyName);
    }

    public bool GetProperty(string propertyName, out string value)
    {
        if (properties.TryGetValue(propertyName, out value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetCommand(string commandName, System.Action command)
    {
        if (commands == null)
            commands = new Dictionary<string, System.Action>();

        //If the command has not been set, add it. if not, update the value
        if (properties.ContainsKey(commandName))
        {
            commands[commandName] = command;
        }
        else
        {
            commands.Add(commandName, command);
        }

        //Debug.Log("Set command " + commandName + " to " + command.ToString());
    }

    public void Removecommand(string commandName)
    {
        commands.Remove(commandName);
    }

    public bool Getcommand(string commandName, out System.Action value)
    {
        if (commands.TryGetValue(commandName, out value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

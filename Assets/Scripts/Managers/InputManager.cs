using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public InputMaster input;

    [Header("Settings")]
    public float gamepadSensitivity = 10;

    [Header("Data")]
    public Vector2 movement;

    public bool openLayerSelect;
    public bool constrainSelection;
    public bool resetView;
    public bool cancel;
    public bool toggleTileSelect;
    public int rotateTile;
    public float zoom;

    public bool primary, primaryPressed, primaryReleased, secondary, secondaryPressed, secondaryReleased;

    public Vector2 cursorPosition,
        cursorDelta;
    Vector2 lastCursorPosition;

    void Start ()
    {
        GameManager.manager.inputManager = this;
    }

    void Update()
    {
        constrainSelection = Input.GetKey("left shift");
        resetView = Input.GetKey("left shift") && Input.GetKeyDown("0");

        cursorDelta = cursorPosition - lastCursorPosition;
        lastCursorPosition = cursorPosition;
    }

    //We have to do this because of a current bug in the new input system
    private void LateUpdate()
    {
        if (toggleTileSelect)
            toggleTileSelect = false;

        if (cancel)
            cancel = false;

        if (rotateTile != 0)
            rotateTile = 0;

        if (primaryPressed)
            primaryPressed = false;
        if (primaryReleased)
            primaryReleased = false;

        if (secondaryPressed)
            secondaryPressed = false;
        if (secondaryReleased)
            secondaryReleased = false;

    }

    private void OnEnable()
    {
        //Enable the input system
        input.Enable();

        //Subscribe events
        input.Camera.Movement.performed += ctx => movement = ctx.ReadValue<Vector2>();
        input.Camera.Movement.cancelled += ctx => movement = ctx.ReadValue<Vector2>();

        input.Building.Cancel.performed += ctx => cancel = true;
        input.Building.Cancel.cancelled += ctx => cancel = false;

        input.Building.TileSelect.performed += ctx => toggleTileSelect = true;
        input.Building.TileSelect.cancelled += ctx => toggleTileSelect = false;

        input.Building.LayerSelect.started += ctx => openLayerSelect = true;
        input.Building.LayerSelect.cancelled += ctx => openLayerSelect = false;

        input.Building.RotateTile.performed += ctx => rotateTile = (int)ctx.ReadValue<float>();
        input.Building.RotateTile.cancelled += ctx => rotateTile = (int)ctx.ReadValue<float>();

        input.Camera.Zoom.performed += ctx => zoom = ctx.ReadValue<float>();
        input.Camera.Zoom.cancelled += ctx => zoom = ctx.ReadValue<float>();

        input.Mouse.Delta.performed += ctx => cursorPosition += ctx.ReadValue<Vector2>()*Time.deltaTime*gamepadSensitivity;

        input.Mouse.Position.performed += ctx => cursorPosition = ctx.ReadValue<Vector2>();
        input.Mouse.Position.cancelled += ctx => cursorPosition = ctx.ReadValue<Vector2>();

        input.Mouse.Primary.started += ctx => primaryPressed = true;
        input.Mouse.Primary.started += ctx => primary = true;
        input.Mouse.Primary.cancelled += ctx => primary = false;
        input.Mouse.Primary.cancelled += ctx => primaryReleased = true;

        input.Mouse.Secondary.started += ctx => secondaryPressed = true;
        input.Mouse.Secondary.started += ctx => secondary = true;
        input.Mouse.Secondary.cancelled += ctx => secondary = false;
        input.Mouse.Secondary.cancelled += ctx => secondaryReleased = true;
    }

    private void OnDisable()
    {
        input.Disable();

        //Unsubscribe events
        input.Camera.Movement.performed -= ctx => movement = ctx.ReadValue<Vector2>();
        input.Camera.Movement.cancelled -= ctx => movement = ctx.ReadValue<Vector2>();

        input.Building.Cancel.performed -= ctx => cancel = true;
        input.Building.Cancel.cancelled -= ctx => cancel = false;

        input.Building.TileSelect.performed -= ctx => toggleTileSelect = true;
        input.Building.TileSelect.cancelled -= ctx => toggleTileSelect = false;

        input.Building.LayerSelect.started -= ctx => openLayerSelect = true;
        input.Building.LayerSelect.cancelled -= ctx => openLayerSelect = false;

        input.Building.RotateTile.performed -= ctx => rotateTile = (int)ctx.ReadValue<float>();
        input.Building.RotateTile.cancelled -= ctx => rotateTile = (int)ctx.ReadValue<float>();

        input.Camera.Zoom.performed -= ctx => zoom = ctx.ReadValue<float>();
        input.Camera.Zoom.cancelled -= ctx => zoom = ctx.ReadValue<float>();

        input.Mouse.Delta.performed -= ctx => cursorPosition -= ctx.ReadValue<Vector2>() * Time.deltaTime * gamepadSensitivity;

        input.Mouse.Position.performed -= ctx => cursorPosition = ctx.ReadValue<Vector2>();
        input.Mouse.Position.cancelled -= ctx => cursorPosition = ctx.ReadValue<Vector2>();

        input.Mouse.Primary.started -= ctx => primaryPressed = true;
        input.Mouse.Primary.started -= ctx => primary = true;
        input.Mouse.Primary.cancelled -= ctx => primary = false;
        input.Mouse.Primary.cancelled -= ctx => primaryReleased = true;

        input.Mouse.Secondary.started -= ctx => secondaryPressed = true;
        input.Mouse.Secondary.started -= ctx => secondary = true;
        input.Mouse.Secondary.cancelled -= ctx => secondary = false;
        input.Mouse.Secondary.cancelled -= ctx => secondaryReleased = true;
    }
}

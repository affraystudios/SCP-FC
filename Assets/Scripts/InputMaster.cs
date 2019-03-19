// GENERATED AUTOMATICALLY FROM 'Assets/InputMaster.inputactions'

using System;
using UnityEngine;
using UnityEngine.Experimental.Input;


[Serializable]
public class InputMaster : InputActionAssetReference
{
    public InputMaster()
    {
    }
    public InputMaster(InputActionAsset asset)
        : base(asset)
    {
    }
    private bool m_Initialized;
    private void Initialize()
    {
        // Camera
        m_Camera = asset.GetActionMap("Camera");
        m_Camera_Movement = m_Camera.GetAction("Movement");
        m_Camera_Zoom = m_Camera.GetAction("Zoom");
        // Building
        m_Building = asset.GetActionMap("Building");
        m_Building_TileSelect = m_Building.GetAction("Tile Select");
        m_Building_LayerSelect = m_Building.GetAction("Layer Select");
        m_Building_Cancel = m_Building.GetAction("Cancel");
        m_Building_RotateTile = m_Building.GetAction("Rotate Tile");
        // Mouse
        m_Mouse = asset.GetActionMap("Mouse");
        m_Mouse_Position = m_Mouse.GetAction("Position");
        m_Mouse_Delta = m_Mouse.GetAction("Delta");
        m_Mouse_Primary = m_Mouse.GetAction("Primary");
        m_Mouse_Secondary = m_Mouse.GetAction("Secondary");
        m_Initialized = true;
    }
    private void Uninitialize()
    {
        m_Camera = null;
        m_Camera_Movement = null;
        m_Camera_Zoom = null;
        m_Building = null;
        m_Building_TileSelect = null;
        m_Building_LayerSelect = null;
        m_Building_Cancel = null;
        m_Building_RotateTile = null;
        m_Mouse = null;
        m_Mouse_Position = null;
        m_Mouse_Delta = null;
        m_Mouse_Primary = null;
        m_Mouse_Secondary = null;
        m_Initialized = false;
    }
    public void SetAsset(InputActionAsset newAsset)
    {
        if (newAsset == asset) return;
        if (m_Initialized) Uninitialize();
        asset = newAsset;
    }
    public override void MakePrivateCopyOfActions()
    {
        SetAsset(ScriptableObject.Instantiate(asset));
    }
    // Camera
    private InputActionMap m_Camera;
    private InputAction m_Camera_Movement;
    private InputAction m_Camera_Zoom;
    public struct CameraActions
    {
        private InputMaster m_Wrapper;
        public CameraActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement { get { return m_Wrapper.m_Camera_Movement; } }
        public InputAction @Zoom { get { return m_Wrapper.m_Camera_Zoom; } }
        public InputActionMap Get() { return m_Wrapper.m_Camera; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(CameraActions set) { return set.Get(); }
    }
    public CameraActions @Camera
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new CameraActions(this);
        }
    }
    // Building
    private InputActionMap m_Building;
    private InputAction m_Building_TileSelect;
    private InputAction m_Building_LayerSelect;
    private InputAction m_Building_Cancel;
    private InputAction m_Building_RotateTile;
    public struct BuildingActions
    {
        private InputMaster m_Wrapper;
        public BuildingActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @TileSelect { get { return m_Wrapper.m_Building_TileSelect; } }
        public InputAction @LayerSelect { get { return m_Wrapper.m_Building_LayerSelect; } }
        public InputAction @Cancel { get { return m_Wrapper.m_Building_Cancel; } }
        public InputAction @RotateTile { get { return m_Wrapper.m_Building_RotateTile; } }
        public InputActionMap Get() { return m_Wrapper.m_Building; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(BuildingActions set) { return set.Get(); }
    }
    public BuildingActions @Building
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new BuildingActions(this);
        }
    }
    // Mouse
    private InputActionMap m_Mouse;
    private InputAction m_Mouse_Position;
    private InputAction m_Mouse_Delta;
    private InputAction m_Mouse_Primary;
    private InputAction m_Mouse_Secondary;
    public struct MouseActions
    {
        private InputMaster m_Wrapper;
        public MouseActions(InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Position { get { return m_Wrapper.m_Mouse_Position; } }
        public InputAction @Delta { get { return m_Wrapper.m_Mouse_Delta; } }
        public InputAction @Primary { get { return m_Wrapper.m_Mouse_Primary; } }
        public InputAction @Secondary { get { return m_Wrapper.m_Mouse_Secondary; } }
        public InputActionMap Get() { return m_Wrapper.m_Mouse; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(MouseActions set) { return set.Get(); }
    }
    public MouseActions @Mouse
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new MouseActions(this);
        }
    }
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get

        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.GetControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get

        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.GetControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
}

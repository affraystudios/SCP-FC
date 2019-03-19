using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

//This class will manage the UI for the game, it is attached to the canvas so it cannot be DDOL
public class GameUIManager : MonoBehaviour
{

    // Lore time
    // Pig said that the last programmer he hired kept putting 'fat N words' in his code
    //I've been adding to this every time I make a change
    //
    // really damn fukn bloody huge big fat enormous N word

    // YOU'VE BEEN SLACKING ON THE N WORDS ALBARNIE

    // OK ITS GAMER TIME
    // ...
    // NIGG- [REDACTED]

    // Also can I get a "Pig is gay" in chat?
    InputManager inputManager;
    MenuManager menuManager;
    TileManager tileManager;

    [Header("References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI toolTipTitle, toolTipDescription, contextTitle;
    public TextMeshProUGUI tileTitle, tilePrice, tileDescription;

    //Pause Menu Options
    [Space(10)]
    [Header("Pause Menu")]
    public bool pauseMenuOpen;


    //Radial Menu Options
    [Space(10)]
    [Header("Radial Menu")]
    public Transform radialMenu;
    public GameObject radialSegmentPrefab;

    public int radialOptions = 5;
    public string[] radialToolTips;
    public float radialTextDistance;

    public bool radialMenuOpen;
    public bool radialSelectionChanged;
    public bool placingZone = false;
    public Vector3 radialMenuPos;
    public int radialSelection;

    [Space(10)]
    [Header("Context Menu")]
    public bool contextOpen;
    public GameObject context;
    public Transform commandList;

    public GameObject commandPrefab;

    [Space(10)]
    [Header("Tooltip Menu")]
    public bool tooltipOpen;
    public GameObject toolTip;
    public Tooltip currentTooltip;

    [Space(10)]
    [Header("Tile Menu")]
    public bool tileMenuOpen;
    public Transform tileList;
    public GameObject tilePrefab;

    float segmentSize;
    Image[] radialSegments;

    private void Start()
    {
        //Store a reference to the InputManager so we don't have to go through the GameManager every time
        GameManager.manager.UIManager = this;

        inputManager = GameManager.manager.inputManager;
        menuManager = GameManager.manager.menuManager;
        tileManager = GameManager.manager.tileManager;
    }

    private void Update()
    {
        //Layer select menu
        if (!radialMenuOpen && inputManager.openLayerSelect)
        {
            radialMenuOpen = true;
            menuManager.ChangeMenu(1);
            InitialiseRadialMenu();

        }
        else if (radialMenuOpen && !inputManager.openLayerSelect)
        {
            radialMenuOpen = false;
            menuManager.ChangeMenu(-1);
            if (radialSelectionChanged)
            {
                tileManager.UpdateLayer(radialSelection);
                UpdateSelectedTile(0);
            }
        }
        if (radialMenuOpen)
        {
            radialSelection = ProcessRadialMenu();
        }

        //Tile select menu
        if (!tileMenuOpen && inputManager.toggleTileSelect)
        {
            tileMenuOpen = true;
            menuManager.ChangeMenu(2);
            InitialiseTileMenu();
            Time.timeScale = 0;
        }
        else if (tileMenuOpen && inputManager.toggleTileSelect)
        {
            tileMenuOpen = false;
            menuManager.ChangeMenu(-1);
            Time.timeScale = 1;

            // Delete all the tiles in the menu
            //this is done to fix a bug with controller selection
            for (int i = 0; i < tileList.childCount; i++)
            {
                Destroy(tileList.GetChild(i).gameObject);
            }
        }

        //Pause menu
        if (pauseMenuOpen && inputManager.cancel)
        {
            UnPause();
        }
        else if (!pauseMenuOpen && inputManager.cancel)
        {
            Pause();
        }

        moneyText.text = "$" + GameManager.manager.playerData.money.ToString();

        if (tooltipOpen)
        {
            toolTip.transform.position = Camera.main.WorldToScreenPoint(currentTooltip.transform.position);
        }

        if (contextOpen)
        {
            context.transform.position = Camera.main.WorldToScreenPoint(currentTooltip.transform.position);
        }
    }

    void InitialiseRadialMenu()
    {
        //Reset some values
        radialMenuPos = GameManager.manager.inputManager.cursorPosition;
        radialMenu.position = radialMenuPos;
        radialSelectionChanged = false;

        segmentSize = 360f / radialOptions;

        //Delete any segment from the last menu
        if (radialSegments != null)
        {
            foreach (Image radialSegment in radialSegments)
            {
                Destroy(radialSegment.gameObject);
            }
        }

        //Add in new segments
        radialSegments = new Image[radialOptions];
        for (int i = 0; i < radialOptions; i++)
        {
            GameObject radialSegment = Instantiate(radialSegmentPrefab, radialMenuPos, Quaternion.Euler(0, 0, i * segmentSize), radialMenu.GetChild(0));
            radialSegments[i] = radialSegment.GetComponent<Image>();
            radialSegments[i].fillAmount = segmentSize / 360;

            if (radialToolTips.Length > i)
                radialSegment.GetComponentInChildren<TextMeshProUGUI>().text = radialToolTips[i];

            radialSegment.transform.GetChild(0).GetComponent<RectTransform>().localPosition = MathHelper.DegreeToVector2((segmentSize / 2) + 90) * radialTextDistance;

            radialSegment.transform.GetChild(0).rotation = Quaternion.identity;
        }
    }

    int ProcessRadialMenu()
    {
        int selection = radialSelection;

        //Convert the direction from the menu to the mouse into degrees
        Vector3 dir = (Vector3)GameManager.manager.inputManager.cursorPosition - radialMenuPos;

        //Check whether the mouse is outside the cancel zone
        if (dir.magnitude > 30 && !radialSelectionChanged)
        {
            radialSelectionChanged = true;
        }
        else
        {
            float angleRad = Mathf.Atan2(dir.y, dir.x);
            float angle = angleRad * Mathf.Rad2Deg;

            angle -= 90;

            //Convert the angle to be 0 to 360, rather than -180 to 180
            if (angle < 0)
                angle = 360 - -angle;

            //Use the angle to detirmine which option to select
            for (int i = 0; i < radialOptions; i++)
            {

                if (angle > i * segmentSize && angle < (i + 1) * segmentSize)
                {
                    selection = i;
                    Debug.DrawRay(Camera.main.ScreenToWorldPoint(radialMenuPos), Quaternion.Euler(0, 0, i * segmentSize) * Vector3.up * 4, Color.green);
                }
                else
                {
                    Debug.DrawRay(Camera.main.ScreenToWorldPoint(radialMenuPos), Quaternion.Euler(0, 0, i * segmentSize) * Vector3.up * 4);
                }
            }
        }
        if (radialSelectionChanged)
        {
            //Set the colour of segments based  on whether they are selected
            Color color = Color.black;
            color.a = (175f / 255f);

            Color color2 = Color.white;
            color2.a = (115f / 255f);
            for (int i = 0; i < radialSegments.Length; i++)
            {
                if (i == radialSelection)
                {
                    radialSegments[i].color = color2;
                }
                else
                {
                    radialSegments[i].color = color;
                }
            }
        }
        return selection;
    }

    void InitialiseTileMenu()
    {
        UpdateSelectedTile(tileManager.currentTile);

        TileType[] tiles;
        tiles = tileManager.floorTiles;

        switch (tileManager.currentLayer)
        {
            //Floor
            case 0:
                tiles = tileManager.floorTiles;
                break;
            //Wall
            case 1:
                tiles = tileManager.wallTiles;
                break;
            //Object
            case 2:
                tiles = tileManager.objectTiles;
                break;
            //Background
            case 3:
                tiles = tileManager.backgroundTiles;
                break;
            //Zones
            case 4:
                tiles = tileManager.zoneTiles;
                break;
            //Utilities
            case 5:
                tiles = tileManager.utilityTiles;
                break;
            case 6:
                tiles = tileManager.placeableAI;
                break;
            default:
                break;
        }

        //Delete the previous tiles
        for (int i = 0; i < tileList.childCount; i++)
        {
            Destroy(tileList.GetChild(i).gameObject);
        }

        //Instantiate new tiles in
        int forEmulator = 0;
        foreach (TileType tile in tiles)
        {
            GameObject tileObject = Instantiate(tilePrefab, tileList);
            if (tile.tiles[0].GetType() == typeof(Tile))
            {
                tileObject.transform.GetChild(0).GetComponent<Image>().sprite = ((Tile)tile.tiles[0]).sprite;
            }
            else
            {
                tileObject.transform.GetChild(0).GetComponent<Image>().sprite = ((RuleTile)tile.tiles[0]).m_DefaultSprite;
            }

            // Zones are identified in tile menu by colour, they all have the same base sprite (for now, at least)
            if (tile.GetType() == typeof(ZoneTileType))
                tileObject.transform.GetChild(0).GetComponent<Image>().color = ((Tile)tile.tiles[0]).color;

            //Set the OnClick method
            tileObject.GetComponent<Button>().onClick.AddListener(() => UpdateSelectedTile(System.Array.IndexOf(tiles, tile)));
            forEmulator++;
        }

        GameManager.manager.eventSystem.SetSelectedGameObject(tileList.GetComponentInChildren<Selectable>().gameObject);
    }

    public void UpdateSelectedTile(int tile)
    {
        TileType tileType;
        //Tile types and the tiles that are shown are based on the current selected layer
        switch (tileManager.currentLayer)
        {
            //Floor
            case 0:
                tileType = tileManager.floorTiles[tile];
                break;
            //Wall
            case 1:
                tileType = tileManager.wallTiles[tile];
                break;
            //Object
            case 2:
                tileType = tileManager.objectTiles[tile];
                break;
            //Background
            case 3:
                tileType = tileManager.backgroundTiles[tile];
                break;
            case 4:
                tileType = tileManager.zoneTiles[tile];
                break;
            case 5:
                tileType = tileManager.utilityTiles[tile];
                break;
            //Will default to floor
            case 6:
                tileType = tileManager.placeableAI[tile];
                break;
            default:
                tileType = tileManager.floorTiles[tile];
                break;
        }
        tileTitle.text = "Name: " + tileType.name;
        tilePrice.text = "Price: " + tileType.price;
        tileDescription.text = "Description: " + tileType.description;

        tileManager.currentTile = tile;
        LayoutRebuilder.ForceRebuildLayoutImmediate(tileTitle.transform.parent.GetComponent<RectTransform>());
    }

    public void Pause()
    {
        pauseMenuOpen = true;
        menuManager.ChangeMenu(0);
        Time.timeScale = 0;
    }

    public void UnPause()
    {
        pauseMenuOpen = false;
        menuManager.ChangeMenu(-1);
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        StartCoroutine(GameManager.manager.GoToScene(0));
        UnPause();
    }

    public void Save()
    {
        GameManager.manager.saveManager.Save();
        UnPause();
    }

    public void ProcessTooltip(string title, string description, Dictionary<string, string> properties, Vector3 position, Tooltip caller)
    {
        currentTooltip = caller;

        tooltipOpen = true;
        toolTip.transform.position = position;
        toolTip.SetActive(true);
        toolTipTitle.text = title;

        foreach (KeyValuePair<string, string> property in properties)
        {
            description += "\n" + property.Key + ": " + property.Value;
        }

        toolTipDescription.text = description;
    }

    public void CloseTooltip()
    {
        tooltipOpen = false;
        toolTip.SetActive(false);

        currentTooltip.tooltipOpen = false;
        currentTooltip = null;
    }

    public void ProcessContextMenu(string title, Dictionary<string, System.Action> commands, Vector3 position)
    {
        toolTip.SetActive(false);

        contextOpen = true;
        context.transform.position = position;
        context.SetActive(true);
        contextTitle.text = title;

        for(int i = 0; i < commandList.childCount; i++)
        {
            Destroy(commandList.GetChild(i).gameObject);
        }
        
        foreach (KeyValuePair<string, System.Action> command in commands)
        {
            GameObject commandObject = Instantiate(commandPrefab, commandList);

            commandObject.GetComponent<TextMeshProUGUI>().text = command.Key;
            commandObject.GetComponent<Button>().onClick.AddListener(() => command.Value.Invoke());
        }

        if(tileList.GetComponentInChildren<Selectable>() != null)
            GameManager.manager.eventSystem.SetSelectedGameObject(tileList.GetComponentInChildren<Selectable>().gameObject);
        LayoutRebuilder.ForceRebuildLayoutImmediate(commandList.GetComponent<RectTransform>());
    }

    public void CloseContextMenu ()
    {
        contextOpen = false;
        context.SetActive(false);
    }
}

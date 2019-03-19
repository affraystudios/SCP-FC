using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class panMouse : MonoBehaviour
{
    public float mouseSensitivity,
        movementSensitivity;
    public int zoomAmount = 5;
    public int minZoom = 2;

    public int maxZoom = 15;
    float defaultSize;

    Vector3 delta;
    Vector3 lastPos;
    Vector3 lastMousePos;

    private float xMax, yMax;

    InputManager input;

    void Start()
    {
        input = GameManager.manager.inputManager;
        defaultSize = Camera.main.orthographicSize;

        var i = GameManager.manager.tileManager.tilemaps[0];
        xMax = i.size.x * i.cellSize.x * 2;
        yMax = i.size.y * i.cellSize.y * 2;

        lastMousePos = Camera.main.ScreenToWorldPoint(GameManager.manager.inputManager.cursorPosition);
    }

    void Update()
    {
        if (Time.timeScale <= 0)
            return;

        if (Mathf.Abs(transform.position.x) > xMax || Mathf.Abs(transform.position.y) > yMax)
        {
            transform.position = lastPos;
        }

        delta = Vector3.zero;
        if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.LeftControl))
            lastMousePos = Camera.main.ScreenToWorldPoint(GameManager.manager.inputManager.cursorPosition);

        if (Input.GetMouseButton(2) || Input.GetKey(KeyCode.LeftControl))
            delta = Camera.main.ScreenToWorldPoint(GameManager.manager.inputManager.cursorPosition) - lastMousePos;

        if (Input.GetMouseButtonUp(2) || Input.GetKey(KeyCode.LeftControl))
            lastPos = transform.position;

        delta -= Vector3.right * input.movement.x * Time.deltaTime * movementSensitivity;
        delta -= Vector3.up * input.movement.y * Time.deltaTime * movementSensitivity;

        transform.Translate(delta.x * -mouseSensitivity, delta.y * -mouseSensitivity, 0);

        if (Input.GetAxis("Mouse ScrollWheel") + input.zoom != 0)
        {
            float camZoom = Camera.main.orthographicSize;
            camZoom -= (Input.GetAxis("Mouse ScrollWheel")*8) + input.zoom * Time.deltaTime * zoomAmount;
            camZoom = Mathf.Clamp(camZoom, minZoom, maxZoom);

            Camera.main.orthographicSize = camZoom;
        }

        if (GameManager.manager.inputManager.resetView)
            Camera.main.orthographicSize = defaultSize;

        lastMousePos = Camera.main.ScreenToWorldPoint(GameManager.manager.inputManager.cursorPosition);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class ShipBuilder : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private ShipElementConf empty;
    [SerializeField] private ShipElementConf fullBlock;
    [SerializeField] private ShipElementConf engine;
    [SerializeField] private Image uiSprite;
    [SerializeField] private Transform orientationIndicator;

    float scroll = 0.0f;

    Dictionary<Vector2Int, IShipElement> elements;
    bool areCoordsValid;
    Vector2Int activeCoords;
    IShipElement ActiveElement { get { return elements[activeCoords]; } }

    enum Orientation
    {
        RIGHT = 0,
        UP = 1,
        LEFT = 2,
        DOWN = 3,
        NUM_OF_ORIENTATIONS = 4
    };

    Orientation orientation = Orientation.UP;


    private void Start()
    {
        GenerateGrid();
        uiSprite.sprite = GetCurrentElement().uiRepresentation;
    }

    public void ChangeActiveElement(Vector2Int coords) => activeCoords = coords;
    public void SetValidCoord(bool isValid) => areCoordsValid = isValid;

    private void GenerateGrid()
    {
        elements = new Dictionary<Vector2Int, IShipElement>();
        Build(fullBlock, Vector2Int.zero);
    }

    private void Update()
    {
        uiSprite.sprite = GetCurrentElement().uiRepresentation;
        if (Input.GetKeyDown(KeyCode.Mouse0) && ActiveElement.GetElementType() == IShipElement.ShipElementType.EMPTY && areCoordsValid)
            Build(GetCurrentElement(), activeCoords);
        if (Input.GetKeyDown(KeyCode.X))
            ToggleBuildMode();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            orientation = (Orientation)(((int)orientation + 1) % (int)(Orientation.NUM_OF_ORIENTATIONS));
            orientationIndicator.Rotate(Vector3.forward, 90);
        }
        scroll += Input.mouseScrollDelta.y * 30;
    }

    private void ToggleBuildMode()
    {
        //transform.position = Vector3.zero;
        //transform.rotation = Quaternion.identity;
        if (GetComponent<Rigidbody2D>())
            Destroy(GetComponent<Rigidbody2D>());
        else
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.mass = elements.Count / 2;
            rb.gravityScale = 0;
        }
    }

    private ShipElementConf GetCurrentElement()
    {
        switch(Mathf.Abs(scroll / 30) % 2)
        {
            case 0: return fullBlock;
            case 1: return engine;
            default: Debug.Log("WTF"); return null;
        }
    }


    private void AddNeighbour(Vector2Int coords)
    {
        if (elements.ContainsKey(coords)) return;

        Build(empty, coords);
    }

    private void AddNeighbours(Vector2Int coords)
    {
        AddNeighbour(coords + Vector2Int.right);    
        AddNeighbour(coords + Vector2Int.left);    
        AddNeighbour(coords + Vector2Int.up);    
        AddNeighbour(coords + Vector2Int.down);    
    }

    private void Build(ShipElementConf element, Vector2Int coords)
    {
        if (CheckIfValid(element, coords))
            return;

        if (elements.ContainsKey(coords) && elements[coords].GetElementType() == IShipElement.ShipElementType.EMPTY)
            Destroy(elements[coords].GetGameObject());


        var newPos = new Vector3(transform.position.x + coords.x * Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.z) - coords.y * Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.z),
                                                                        transform.position.y + coords.x * Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.z) + coords.y * Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.z));

        var spawnedElement = Instantiate(element.prefab, newPos, transform.rotation, transform);
        spawnedElement.transform.Rotate(Vector3.forward, (int)orientation * 90 - 90);
        spawnedElement.name = $"Tile {coords.x} {coords.y}";

        elements[coords] = spawnedElement.GetComponent<IShipElement>();
        elements[coords].SetBuilderRef(this);
        elements[coords].SetCoords(coords);


        switch (element.extensibility)
        {
            case ShipElementConf.Extensibility.FULL: AddNeighbours(coords); break;
            case ShipElementConf.Extensibility.NONE: break;
        }
    }

    private bool CheckIfValid(ShipElementConf element, Vector2Int coords)
    {
        if ((element.requiredBlocks & ShipElementConf.RequiredBlocks.UP) != 0)
        {
            var checkCoords = coords + new Vector2Int((int)Mathf.Cos(Mathf.Deg2Rad * (int)orientation * 90), (int)Mathf.Sin(Mathf.Deg2Rad * (int)orientation * 90));
            return (!elements.ContainsKey(checkCoords) || elements[checkCoords].GetElementType() != IShipElement.ShipElementType.FULL);
        }
        return false;
    }
}

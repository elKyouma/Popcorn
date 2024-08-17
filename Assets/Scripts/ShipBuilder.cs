using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ShipBuilder : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private ShipElementConf empty;
    [SerializeField] private ShipElementConf full;
    [SerializeField] private ShipElementConf engine;

    Dictionary<Vector2Int, IShipElement> elements;
    bool areCoordsValid;
    Vector2Int activeCoords;
    IShipElement ActiveElement { get { return elements[activeCoords]; } }


    private void Start() => GenerateGrid();
    public void ChangeActiveElement(Vector2Int coords) => activeCoords = coords;
    public void SetValidCoord(bool isValid) => areCoordsValid = isValid;

    private void GenerateGrid()
    {
        elements = new Dictionary<Vector2Int, IShipElement>();
        Build(full, Vector2Int.zero);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && ActiveElement.GetElementType() == IShipElement.ShipElementType.Empty && areCoordsValid)
            Build(full, activeCoords);
        if (Input.GetKeyDown(KeyCode.Mouse1) && ActiveElement.GetElementType() == IShipElement.ShipElementType.Empty && areCoordsValid)
            Build(engine, activeCoords);
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (GetComponent<Rigidbody2D>())
                Destroy(GetComponent<Rigidbody2D>());
            else
                gameObject.AddComponent<Rigidbody2D>();
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
        if(elements.ContainsKey(coords) && elements[coords].GetElementType() == IShipElement.ShipElementType.Empty)
            Destroy(elements[coords].GetGameObject());
        
        var spawnedElement = Instantiate(element.prefab, new Vector3(coords.x, coords.y), Quaternion.identity, transform);
        spawnedElement.name = $"Tile {coords.x} {coords.y}";
        elements[coords] = spawnedElement.GetComponent<IShipElement>();
        elements[coords].SetBuilderRef(this);

        if (elements[coords].GetElementType() == IShipElement.ShipElementType.Full)
            AddNeighbours(coords);
    }
}

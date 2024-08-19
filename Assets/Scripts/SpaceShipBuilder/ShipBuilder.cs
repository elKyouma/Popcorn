using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public enum Orientation
{
    RIGHT = 0,
    UP = 1,
    LEFT = 2,
    DOWN = 3,
    NUM_OF_ORIENTATIONS = 4
};

public class ShipBuilder : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private ShipElementConf empty;
    [SerializeField] private ShipElementConf coreBlock;
    [SerializeField] private ShipElementConf fullBlock;
    [SerializeField] private ShipElementConf engine;
    [SerializeField] private ShipElementConf weapon;
    [SerializeField] private Image uiSprite;
    [SerializeField] private Transform orientationIndicator;
    [SerializeField] private PopUpFiller popUp;

    float scroll = 0.0f;

    Dictionary<Vector2Int, ShipElement> elements;
    bool areCoordsValid;
    Vector2Int activeCoords;
    public ShipElement ActiveElement { get { return elements[activeCoords]; } }
    bool pausedInputs = false;

    Orientation orientation = Orientation.UP;

    void ForAllNeighbours(Action<Vector2Int> action, Vector2Int coords)
    {
        if(elements.ContainsKey(coords + Vector2Int.right))
            action(coords + Vector2Int.right);
        if(elements.ContainsKey(coords + Vector2Int.up))
            action(coords + Vector2Int.up);
        if(elements.ContainsKey(coords + Vector2Int.left))
            action(coords + Vector2Int.left);
        if(elements.ContainsKey(coords + Vector2Int.down))
            action(coords + Vector2Int.down);
    }

    bool CheckIfOneOfNeighbours(Func<Vector2Int, bool> action, Vector2Int coords)
    {
        bool result = false;

        if (elements.ContainsKey(coords + Vector2Int.right))
            result |= action(coords + Vector2Int.right);
        if (elements.ContainsKey(coords + Vector2Int.up))
            result |= action(coords + Vector2Int.up);
        if (elements.ContainsKey(coords + Vector2Int.left))
            result |= action(coords + Vector2Int.left);
        if (elements.ContainsKey(coords + Vector2Int.down))
            result |= action(coords + Vector2Int.down);

        return result;
    }

    bool CheckIfAllNeighbours(Func<Vector2Int, bool> action, Vector2Int coords)
    {
        bool result = true;
        
        if (elements.ContainsKey(coords + Vector2Int.right))
            result &= action(coords + Vector2Int.right);
        if (elements.ContainsKey(coords + Vector2Int.up))
            result &= action(coords + Vector2Int.up);
        if (elements.ContainsKey(coords + Vector2Int.left))
            result &= action(coords + Vector2Int.left);
        if (elements.ContainsKey(coords + Vector2Int.down))
            result &= action(coords + Vector2Int.down);
        
        return result;
    }

    bool CheckShouldEmptyBeDestroyed(Vector2Int coords)
    {
        return CheckIfAllNeighbours((Vector2Int coords) => 
        {
            switch(elements[coords].GetElementType())
            {
                case ShipElement.ShipElementType.EMPTY:    return true;
                case ShipElement.ShipElementType.CORE:     return false;
                case ShipElement.ShipElementType.FULL:     return false;
                case ShipElement.ShipElementType.WEAPON:   return true;
                case ShipElement.ShipElementType.ENGINE:   return true;
            }
            return false;
        }, coords);
    }

    private void DestroyUnconnectedEmpty()
    {
        ForAllNeighbours(
            (Vector2Int coords) => {
                if (elements[coords].GetElementType() == ShipElement.ShipElementType.EMPTY && CheckShouldEmptyBeDestroyed(coords))
                {
                    Destroy(elements[coords].gameObject);
                    elements.Remove(coords);
                }
            }, activeCoords);
    }

    private bool CheckForConnectivity()
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Queue<Vector2Int> queue;
        queue = new Queue<Vector2Int>();
        queue.Enqueue(Vector2Int.zero);

        int expected = 0;
        foreach (var element in elements)
            if (element.Value.GetElementType() != ShipElement.ShipElementType.EMPTY && element.Value.GetElementType() != ShipElement.ShipElementType.ENGINE && element.Value.GetCoords() != activeCoords)
                expected++;

        while(queue.Count != 0)
        {
            var val = queue.Dequeue();
            visited.Add(val);
            ForAllNeighbours((Vector2Int coords) => 
                {
                    if (visited.Contains(coords) || elements[coords].GetElementType() == ShipElement.ShipElementType.EMPTY || elements[coords].GetElementType() == ShipElement.ShipElementType.ENGINE || coords == activeCoords) return;

                    queue.Enqueue(coords);
                    visited.Add(coords);
                }, val);
        }

        if (visited.Count == expected)
            return true;
        else if (visited.Count < expected)
            return false;
        
        Debug.Log("WTF");
        return false;
    }

    private bool CheckIsBlockRequired()
    {
        if (elements.ContainsKey(activeCoords + Vector2Int.down))
        {
            var downElement = elements[activeCoords + Vector2Int.down];
            var config = GetConfigFromType(downElement.GetElementType());
            if ((config.requiredBlocks & ShipElementConf.RequiredBlocks.IN_FRONT_OF) != 0)
                if (downElement.GetOrientation() == Orientation.UP)
                    return true;
        }
        if (elements.ContainsKey(activeCoords + Vector2Int.left))
        {
            var leftElement = elements[activeCoords + Vector2Int.left];
            var config = GetConfigFromType(leftElement.GetElementType());
            if ((config.requiredBlocks & ShipElementConf.RequiredBlocks.IN_FRONT_OF) != 0)
                if (leftElement.GetOrientation() == Orientation.RIGHT)
                    return true;
        }
        if (elements.ContainsKey(activeCoords + Vector2Int.up))
        {
            var upElement = elements[activeCoords + Vector2Int.up];
            var config = GetConfigFromType(upElement.GetElementType());
            if ((config.requiredBlocks & ShipElementConf.RequiredBlocks.IN_FRONT_OF) != 0)
                if (upElement.GetOrientation() == Orientation.DOWN)
                    return true;
        }
        if (elements.ContainsKey(activeCoords + Vector2Int.right))
        {
            var rightElement = elements[activeCoords + Vector2Int.right];
            var config = GetConfigFromType(rightElement.GetElementType());
            if ((config.requiredBlocks & ShipElementConf.RequiredBlocks.IN_FRONT_OF) != 0)
                if (rightElement.GetOrientation() == Orientation.LEFT)
                    return true;
        }

        return false;
    }

    public void DestroyCurrentElement()
    {
        if (!CheckForConnectivity())
        {
            print("Connectivity problems");
            return;
        }
        if (CheckIsBlockRequired())
        {
            print("Block is required");
            return;
        }

        Destroy(ActiveElement.gameObject);
        elements.Remove(activeCoords);
        HidePopUp();
        DestroyUnconnectedEmpty();

        LeaveSpaceWithEmptyBlockIfHaveSolidNeighbour();

        areCoordsValid = false;
    }

    private void LeaveSpaceWithEmptyBlockIfHaveSolidNeighbour()
    {
        if (CheckIfOneOfNeighbours(
                (Vector2Int coords) =>
                {
                    switch (elements[coords].GetElementType())
                    {
                        case ShipElement.ShipElementType.EMPTY: return false;
                        case ShipElement.ShipElementType.CORE: return true;
                        case ShipElement.ShipElementType.FULL: return true;
                        case ShipElement.ShipElementType.WEAPON: return false;
                        case ShipElement.ShipElementType.ENGINE: return false;
                    }
                    return false;
                }, activeCoords))
            Build(empty, activeCoords);
    }

    private void Start()
    {
        Time.timeScale = 0;
        GenerateGrid();
        uiSprite.sprite = GetSelectedBuildingElement().uiRepresentation;
        popUp.GetComponent<PopUpImpl>().SetShipBuilderRef(this);
    }

    private void HidePopUp()
    {
        pausedInputs = false;
        popUp.GetComponent<Animator>().SetTrigger("Close");
    }

    private void ShowPopUp()
    {
        pausedInputs = true;
        ShipElementConf currentConfig = GetCurrentConfig();
        popUp.gameObject.SetActive(true);
        popUp.GetComponent<Animator>().SetTrigger("Open");
        popUp.SetShipElementConf(currentConfig, ActiveElement);
        popUp.SetUpgradePrice(20);
        popUp.SetDeletionRefund(10);

        if (currentConfig.possibleKeybindings.Count != 0)
            popUp.SetKeybindings(currentConfig.possibleKeybindings, ActiveElement.GetBindings());
        else
            popUp.HideKeyBindings();
    }

    public ShipElementConf GetCurrentConfig()
    {
        return GetConfigFromType(ActiveElement.GetElementType());
    }

    private ShipElementConf GetConfigFromType(ShipElement.ShipElementType type)
    {
        switch (type)
        {
            case ShipElement.ShipElementType.EMPTY:        return empty;
            case ShipElement.ShipElementType.ENGINE:       return engine;
            case ShipElement.ShipElementType.WEAPON:       return weapon;
            case ShipElement.ShipElementType.FULL:         return fullBlock;
            case ShipElement.ShipElementType.CORE:         return coreBlock;
        }
        return empty;
    }

    public void ChangeActiveElement(Vector2Int coords)
    {
        if (!pausedInputs) activeCoords = coords;
    }
    public void SetValidCoord(bool isValid) => areCoordsValid = isValid;

    private void GenerateGrid()
    {
        elements = new Dictionary<Vector2Int, ShipElement>();
        Build(coreBlock, Vector2Int.zero);
    }

    private void Update()
    {
        uiSprite.sprite = GetSelectedBuildingElement().uiRepresentation;
        if (Input.GetKeyDown(KeyCode.Mouse0) && areCoordsValid)
        {
            switch (ActiveElement.GetElementType())
            {
                case ShipElement.ShipElementType.EMPTY:
                    Build(GetSelectedBuildingElement(), activeCoords); break;
                case ShipElement.ShipElementType.ENGINE:
                case ShipElement.ShipElementType.WEAPON:
                case ShipElement.ShipElementType.CORE:
                case ShipElement.ShipElementType.FULL:
                    ShowPopUp(); break;
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
            ToggleBuildMode();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            orientation = (Orientation)(((int)orientation + 1) % (int)(Orientation.NUM_OF_ORIENTATIONS));
            orientationIndicator.Rotate(Vector3.forward, 90);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // pausedInputs = false; unnecessary btw
            HidePopUp();
        }
        scroll += Input.mouseScrollDelta.y * 30;
    }

    private void ToggleBuildMode()
    {
        //transform.position = Vector3.zero;
        //transform.rotation = Quaternion.identity;
        if (GetComponent<Rigidbody2D>())
        {
            Time.timeScale = 0.0f;
            Destroy(GetComponent<Rigidbody2D>());
        }
        else
        {
            Time.timeScale = 1.0f;
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.mass = elements.Count / 2;
            rb.gravityScale = 0;
        }
    }

    private ShipElementConf GetSelectedBuildingElement()
    {
        switch(Mathf.Abs(scroll / 30) % 3)
        {
            case 0: return fullBlock;
            case 1: return engine;
            case 2: return weapon;
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
        if (CheckIfNotValid(element, coords))
            return;

        if (elements.ContainsKey(coords) && elements[coords].GetElementType() == ShipElement.ShipElementType.EMPTY)
            Destroy(elements[coords].gameObject);

        var newPos = new Vector3(transform.position.x + coords.x * Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.z) - coords.y * Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.z),
                                                                        transform.position.y + coords.x * Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.z) + coords.y * Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.z));

        var spawnedElement = Instantiate(element.prefab, newPos, transform.rotation, transform);
        spawnedElement.transform.Rotate(Vector3.forward, (int)orientation * 90 - 90);
        spawnedElement.name = $"Tile {coords.x} {coords.y}";

        elements[coords] = spawnedElement.GetComponentInChildren<ShipElement>();
        var curElement = elements[coords];
        curElement.SetBuilderRef(this);
        curElement.SetCoords(coords);
        curElement.SetOrientation(orientation);

        switch (element.extensibility)
        {
            case ShipElementConf.Extensibility.FULL:
                AddNeighbours(coords); break;
            case ShipElementConf.Extensibility.NOT_IN_FRONT_OF:
                {
                    if(orientation != Orientation.RIGHT)
                        AddNeighbour(coords + Vector2Int.right);
                    if(orientation != Orientation.LEFT)
                        AddNeighbour(coords + Vector2Int.left);
                    if (orientation != Orientation.UP)
                        AddNeighbour(coords + Vector2Int.up);
                    if (orientation != Orientation.DOWN)
                        AddNeighbour(coords + Vector2Int.down);
                } break;
            case ShipElementConf.Extensibility.NONE: break;
        }
    }

    private bool CheckIfNotValid(ShipElementConf element, Vector2Int coords)
    {
        if ((element.requiredBlocks & ShipElementConf.RequiredBlocks.IN_FRONT_OF) != 0)
        {
            var checkCoords = coords + new Vector2Int((int)Mathf.Cos(Mathf.Deg2Rad * (int)orientation * 90), (int)Mathf.Sin(Mathf.Deg2Rad * (int)orientation * 90));
            return (!elements.ContainsKey(checkCoords) || elements[checkCoords].GetElementType() == ShipElement.ShipElementType.ENGINE || elements[checkCoords].GetElementType() == ShipElement.ShipElementType.EMPTY || elements[checkCoords].GetElementType() == ShipElement.ShipElementType.WEAPON);
        }
        if ((element.requiredBlocks & ShipElementConf.RequiredBlocks.AT_BACK) != 0)
        {
            var checkCoords = coords + new Vector2Int(-(int)Mathf.Cos(Mathf.Deg2Rad * (int)orientation * 90), -(int)Mathf.Sin(Mathf.Deg2Rad * (int)orientation * 90));
            return (!elements.ContainsKey(checkCoords) || elements[checkCoords].GetElementType() == ShipElement.ShipElementType.ENGINE || elements[checkCoords].GetElementType() == ShipElement.ShipElementType.EMPTY || elements[checkCoords].GetElementType() == ShipElement.ShipElementType.WEAPON);
        }

        return false;
    }
}

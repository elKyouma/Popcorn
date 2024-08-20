using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ObjectChangeEventStream;

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
    [SerializeField] private GameObject destructionParticles;
    [SerializeField] private PopUpFiller popUp;
    public bool popUpActive = false;   

    float scroll = 0.0f;

    Dictionary<Vector2Int, ShipElement> elements;
    bool areCoordsValid;
    Vector2Int activeCoords;
    public ShipElement ActiveElement { get { return elements[activeCoords]; } }
    bool pausedInputs = false;
    bool canBeBuild = true;
    private bool buildMode = true;
    public bool BuildMode => buildMode;
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

    public void DestroyElement(Vector2Int vec)
    {
        if (!elements.ContainsKey(vec)) return;
        Instantiate(destructionParticles, elements[vec].transform.position, Quaternion.identity);
        Destroy(elements[vec].gameObject);
        elements.Remove(vec);
        HashSet<Vector2Int> connected = new HashSet<Vector2Int>();
        Queue<Vector2Int> toVisit = new Queue<Vector2Int>();
        toVisit.Enqueue(Vector2Int.zero);
        connected.Add(Vector2Int.zero);
        while (toVisit.Count != 0)
        {
            ForAllNeighbours((Vector2Int coords) =>
            {
                if (connected.Contains(coords)) return;

                if (elements[coords].GetElementType() != ShipElement.ShipElementType.EMPTY && elements[coords].GetElementType() != ShipElement.ShipElementType.ENGINE && elements[coords].GetElementType() != ShipElement.ShipElementType.WEAPON)
                    toVisit.Enqueue(coords);
                connected.Add(coords);

            }, toVisit.Dequeue());

        }

        List<Vector2Int> toDelete = new List<Vector2Int>();
        foreach (var element in elements)
        {
            if (connected.Contains(element.Key)) continue;

            element.Value.transform.parent = null;
            toDelete.Add(element.Key);
        }
        foreach (var toDel in toDelete)
        {
            if (elements[toDel].GetElementType() == ShipElement.ShipElementType.EMPTY)
                Destroy(elements[toDel].gameObject);
            elements.Remove(toDel);
        }
        Build(empty, vec);
    }

    public void TryDestroyCurrentElement()
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

        RefundCurrentElement();

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
        popUpActive = false;
        pausedInputs = false;
        //BackgroundFader.Instance.FadeOut();
        popUp.GetComponent<Animator>().SetTrigger("Close");
    }

    private void ShowPopUp()
    {
        popUpActive = true;
        pausedInputs = true;
        //BackgroundFader.Instance.FadeIn();
        ShipElementConf currentConfig = GetCurrentConfig();
        popUp.GetComponent<Animator>().SetTrigger("Open");
        popUp.SetShipElementConf(this);
        popUp.SetDeletionRefund(GetRefundAmount());


        if (IsUpgradable())
            popUp.SetUpgradeCost(currentConfig.costs[ActiveElement.currentLevel + 1]);
        else
            popUp.DisableUpgrading();

        if (currentConfig.possibleKeybindings.Count != 0)
            popUp.SetKeybindings(currentConfig.possibleKeybindings, ActiveElement.GetBindings());
        else
            popUp.HideKeyBindings();
    }

    public ShipElementConf GetCurrentConfig() => GetConfigFromType(ActiveElement.GetElementType());

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
            HidePopUp();
        scroll += Input.mouseScrollDelta.y * 30;
    }

    private void ToggleBuildMode()
    {
        if (GetComponent<Rigidbody2D>())
        {
            buildMode = true;
            Time.timeScale = 0.0f;
            Destroy(GetComponent<Rigidbody2D>());
        }
        else
        {
            buildMode = false;
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
        curElement.UpdateGraphic();
        if (curElement.GetElementType() != ShipElement.ShipElementType.EMPTY)
        {
            MoneyManager.Instance.RemoveMoney(GetCurrentConfig().costs[0]);
            ValidateIfNextBlockCanBeBuild(curElement);
        }
        switch (element.extensibility)
        {
            case ShipElementConf.Extensibility.FULL:
                AddNeighbours(coords); break;
            case ShipElementConf.Extensibility.NOT_IN_FRONT_OF:
                {
                    if (orientation != Orientation.RIGHT)
                        AddNeighbour(coords + Vector2Int.right);
                    if (orientation != Orientation.LEFT)
                        AddNeighbour(coords + Vector2Int.left);
                    if (orientation != Orientation.UP)
                        AddNeighbour(coords + Vector2Int.up);
                    if (orientation != Orientation.DOWN)
                        AddNeighbour(coords + Vector2Int.down);
                }
                break;
            case ShipElementConf.Extensibility.NONE: break;
        }
    }

    private void ValidateIfNextBlockCanBeBuild(ShipElement curElement)
    {
        canBeBuild = MoneyManager.Instance.Amount > GetCurrentConfig().costs[0];
        if (!canBeBuild)
            uiSprite.color = Color.red;
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

    public void RefundCurrentElement()
    {
        int refund = GetRefundAmount();
        MoneyManager.Instance.AddMoney(refund);
    }

    private int GetRefundAmount()
    {
        int refund = 0;
        for (int i = 0; i < ActiveElement.currentLevel; i++)
            refund += GetCurrentConfig().costs[ActiveElement.currentLevel];
        refund /= 2;
        return refund;
    }

    public bool IsUpgradable()
    {
        return ActiveElement.currentLevel != 2 && MoneyManager.Instance.Amount > GetCurrentConfig().costs[ActiveElement.currentLevel];
    }

    public void UpgradeCurrentElement()
    {
        ActiveElement.currentLevel++;
        MoneyManager.Instance.RemoveMoney(GetCurrentConfig().costs[ActiveElement.currentLevel]);
        ValidateIfNextBlockCanBeBuild(ActiveElement);
        ActiveElement.UpdateGraphic();
        popUp.SetDeletionRefund(GetRefundAmount());
        if (IsUpgradable())
            popUp.SetUpgradeCost(GetCurrentConfig()
                .costs[ActiveElement.currentLevel + 1]);
        else
            popUp.DisableUpgrading();
    }
}

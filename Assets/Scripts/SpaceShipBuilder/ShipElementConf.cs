using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ship element")]
public class ShipElementConf : ScriptableObject
{
    public enum Extensibility
    {
        FULL,
        NOT_IN_FRONT_OF,
        NONE
    }

    [Flags]
    public enum RequiredBlocks
    {
        NONE = 0,
        IN_FRONT_OF = 1,
        AT_BACK = 2,
    }

    public string elementName;
    public string elementDescription;
    public List<KeyCode> possibleKeybindings;
    public GameObject prefab;
    public Sprite uiRepresentation;
    public Extensibility extensibility;
    public RequiredBlocks requiredBlocks;
    public int cost;
}

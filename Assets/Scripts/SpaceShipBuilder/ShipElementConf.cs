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
        NONE
    }

    [Flags]
    public enum RequiredBlocks
    {
        NONE = 0,
        UP = 1,
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

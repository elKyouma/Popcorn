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
        UP = 1,
        LEFT = 2,
        DOWN = 4,
        RIGHT = 8,
    }

    public string elementName;
    public GameObject prefab;
    public Sprite uiRepresentation;
    public Extensibility extensibility;
    public RequiredBlocks requiredBlocks;
    public int cost;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ship element")]
public class ShipElementConf : ScriptableObject
{
    public string elementName;
    public GameObject prefab;
    public int cost;
}

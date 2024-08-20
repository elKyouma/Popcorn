using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollisionWithPlanet : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Planet")
            GetComponent<ShipBuilder>().DestroyElement(Vector2Int.zero);
    }
}

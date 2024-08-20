using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreBox : ShipElement
{
    public GameObject Explosion;
    public override ShipElementType GetElementType() => ShipElementType.CORE;

    IEnumerator RestartTheGame()
    {
        builder.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        builder.transform.localScale = Vector3.zero;
        Instantiate(Explosion, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public override void OnDeath()
    {
        StartCoroutine(RestartTheGame());
    }
}

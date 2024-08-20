using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public Image needle;
    public float maxSpeed = 200f;
    public Transform targetObject;

    private Rigidbody2D targetRigidbody;

    void Start()
    {
        if (targetObject != null)
        {
            targetRigidbody = targetObject.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (targetRigidbody != null)
        {
            UpdateNeedleRotation();
        }
        else 
            Debug.LogWarning("Speedometer: No target Rigidbody assigned.");
    }

    private void UpdateNeedleRotation()
    {
        float speed = targetRigidbody.velocity.magnitude;
        float speedFraction = speed / maxSpeed;
        Debug.Log(speed);
        float needleAngle = Mathf.Lerp(-90, 90, speedFraction);
        needle.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, needleAngle);
    }
}

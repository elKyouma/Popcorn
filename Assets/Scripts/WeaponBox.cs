using UnityEngine;
using UnityEngine.UI;

public class WeaponBox : ShipElement
{
    public GameObject missile;
    private float nextFire = 0.0f;

    [SerializeField] float fireRate = 10f;
    [SerializeField] Image image;

    bool isShooting;

    private void Start() => bindings.Add(KeyCode.U);

    private void Update()
    {
        isShooting = false;
        foreach (KeyCode keyCode in bindings)
        {
            //if (Input.GetKeyDown(keyCode))
                //Play sound here
            if (Input.GetKey(keyCode))
            {
                isShooting = true;
                break;
            }
        }

        if (isShooting && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            image.enabled = true;
            ShootMissile();
        }
        float procentage = (nextFire - Time.time) / fireRate;
        if (procentage < 100)
        {
            image.fillAmount = procentage;
        }
        else
        {
            image.enabled = false;
        }
    }
    void ShootMissile()
    {
        var missileObj = Instantiate(missile, transform.position + transform.up * 1.5f, transform.rotation);
        missileObj.GetComponent<Bullet>().FireBullet(transform.up);
    }

    public override ShipElementType GetElementType() => ShipElementType.WEAPON;

    public override void OnDeath() { builder.DestroyElement(coord); }
}

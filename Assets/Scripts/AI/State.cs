using UnityEngine;

public abstract class State
{
    protected Transform target;
    protected Transform enemyTransform;
    protected Rigidbody2D enemyRb;
    protected float weaponsRange;
    protected BulletSource gun;
    protected State(Enemy enemy)
    {
        this.enemy = enemy;
        target = enemy.GetTarget();
        enemyTransform = enemy.transform;
        enemyRb = enemy.GetEnemyRb();
        weaponsRange = enemy.GetWeaponsRange();
        gun = enemy.GetGun();
    }

    public abstract State PlayState();

    protected Enemy enemy;
}

using UnityEngine;

public class ChaseState : State
{
    public ChaseState(Enemy enemy) : base(enemy) { }
    public override State PlayState()
    {
        if (gun.IsGunActive())
            gun.SetIsGunActive(false);
        float distanceToTarget = Vector2.Distance(enemyTransform.position, target.position);

        if (distanceToTarget < weaponsRange)
            return new AttackState(enemy);

        enemy.SetTargetPosition(target.position);

        //Vector3 direction = (target.position - enemyTransform.position).normalized;
        //float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //enemyRb.rotation = 360 - angle;
        //enemy.SetMoveDirection(direction);



        return this;
    }
}

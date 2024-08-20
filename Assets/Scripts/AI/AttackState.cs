using UnityEngine;
public class AttackState : State
{
    public AttackState(Enemy enemy) : base(enemy) { }

    public override State PlayState()
    {
        if (!gun.IsGunActive())
            gun.SetIsGunActive(true);

        float distanceToTarget = Vector2.Distance(enemyTransform.position, target.position);
        //enemy.SetMoveDirection(Vector2.zero);

        //Vector3 direction = (target.position - enemyTransform.position).normalized;
        //float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //enemyRb.rotation = 360 - angle;

        if (distanceToTarget > weaponsRange)
            return new ChaseState(enemy);

        //enemy.SetTargetPosition(target.position);


        return this;
    }
}

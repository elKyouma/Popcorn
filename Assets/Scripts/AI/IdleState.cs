public class IdleState : State
{
    public IdleState(Enemy enemy) : base(enemy) { }
    public override State PlayState()
    {
        return new ChaseState(enemy);
    }
}

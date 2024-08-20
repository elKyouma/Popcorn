using System.Collections.Generic;
using UnityEngine;
using Node = Pathfinding.Node<(float, float)>;

public class ChaseState : State
{
    public ChaseState(Enemy enemy) : base(enemy) { }

    private int frame = 5;
    Node enemyNode;
    private int currentNode = 0;

    public override State PlayState()
    {
        if (gun.IsGunActive())
            gun.SetIsGunActive(false);


        var grid = Triangulator.GetGrid();
        List<Node> path = new();

        if (frame < 5)
        {
            frame++;
        }
        else
        {
            frame = 0;
            grid = Triangulator.GetGrid();


            enemyNode = FindEnemyNode(enemy.transform.position, grid);

            path = ComputePath(enemyNode, PlayerAgent.GetPlayerNode(), grid);
            currentNode = 0;
            enemy.SetTargetPosition(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2));
        }

        if (path != null && currentNode < path.Count)
        {
            float distanceToCurrentNode = Vector2.Distance(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2), enemyTransform.position);
            if (distanceToCurrentNode < 3f)
            {
                currentNode++;
                enemy.SetTargetPosition(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2));
            }
        }


        //enemy.SetTargetPosition(target.position);
        float distanceToTarget = Vector2.Distance(enemyTransform.position, target.position);


        if (distanceToTarget < weaponsRange)
            return new AttackState(enemy);


        //Vector3 direction = (target.position - enemyTransform.position).normalized;
        //float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //enemyRb.rotation = 360 - angle;
        //enemy.SetMoveDirection(direction);



        return this;
    }

    private List<Node> ComputePath(Node startNode, Node goalNode, List<Node> grid)
    {
        var pathfinder = new Pathfinding.DStarLite<(float, float)>(startNode, goalNode, grid);

        pathfinder.Initialize();
        pathfinder.ComputeShortestPath();

        return pathfinder.GetPath();
    }

    private Node FindEnemyNode(Vector2 enemyPosition, List<Node> grid)
    {
        Node enemyNode = grid[0];
        float minDistance = Vector2.Distance(enemyPosition, new Vector2(enemyNode.Data.Item1, enemyNode.Data.Item2));
        foreach (Node node in grid)
        {
            float distance = Vector2.Distance(enemyPosition, new Vector2(node.Data.Item1, node.Data.Item2));
            if (distance < minDistance)
            {
                enemyNode = node;
                minDistance = distance;
            }
        }
        return enemyNode;

    }
}

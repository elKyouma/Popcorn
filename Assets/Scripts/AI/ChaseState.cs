using System.Collections.Generic;
using UnityEngine;
using Node = Pathfinding.Node<(float, float)>;

public class ChaseState : State
{
    public ChaseState(Enemy enemy) : base(enemy)
    {
        ////offset++; 
        //grid = Triangulator.GetGrid();
        //enemyNode = FindEnemyNode(enemy.transform.position, grid);
        //lastPlayerNode = PlayerAgent.GetPlayerNode();
        ////path.Clear();
        //path = ComputePath(enemyNode, lastPlayerNode, grid);
        //enemy.SetDebugPath(path);

    }

    //private Node currentPlayerNode;
    //private Node lastPlayerNode;
    //private int frame = 0;
    //private Node enemyNode;
    //private List<Node> grid;
    //private List<Node> path;

    //private int currentNode = 0;
    //static int offset = 0;
    public override State PlayState()
    {
        //Debug.Log("chase");
        if (gun.IsGunActive())
            gun.SetIsGunActive(false);
        /*
        grid = Triangulator.GetGrid();
        enemyNode = FindEnemyNode(enemy.transform.position, grid);

        enemy.SetDebugEnemyNode(enemyNode);

        currentPlayerNode = PlayerAgent.GetPlayerNode();
        if (currentPlayerNode != lastPlayerNode)
        {
            lastPlayerNode = currentPlayerNode;
            path = ComputePath(enemyNode, lastPlayerNode, grid);
            enemy.SetDebugPath(path);
            currentNode = 0;
        }

        if (path[currentNode] != null)
        {
            enemy.SetTargetPosition(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2));
            enemy.SetTargetRotation(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2));

        }
        if (path[currentNode] == enemyNode)
            currentNode++;

        */

        //if (frame < 10)
        //{
        //    frame++;
        //}
        //else
        //{
        //    frame = 0;
        //    path = ComputePath(enemyNode, PlayerAgent.GetPlayerNode(), grid);
        //    enemy.SetDebugPath(path);

        //}






        //var grid = Triangulator.GetGrid();
        //List<Node> path = new();

        //frame++;
        //if ((frame + offset) % 5 == 0)
        //{
        //    grid = Triangulator.GetGrid();
        //    path.Clear();
        //    frame -= 4;
        //    enemyNode = FindEnemyNode(enemy.transform.position, grid);

        //    path = ComputePath(enemyNode, PlayerAgent.GetPlayerNode(), grid);
        //    currentNode = 0;
        //    enemy.SetTargetPosition(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2));
        //}

        //if (path != null && currentNode < path.Count)
        //{
        //    float distanceToCurrentNode = Vector2.Distance(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2), enemyTransform.position);
        //    if (distanceToCurrentNode < 3f & currentNode + 1 < path.Count)
        //    {
        //        currentNode++;
        //        enemy.SetTargetPosition(new Vector2(path[currentNode].Data.Item1, path[currentNode].Data.Item2));
        //    }
        //}


        float distanceToTarget = Vector2.Distance(enemyTransform.position, target.position);


        if (distanceToTarget < weaponsRange)
            return new AttackState(enemy);

        enemy.SetTargetPosition(target.position);
        enemy.SetTargetRotation(target.position);

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

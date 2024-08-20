using System.Collections.Generic;
using UnityEngine;
using Node = Pathfinding.Node<(float, float)>;

public class PlayerAgent : MonoBehaviour
{
    private static Node playerNode;
    private List<Node> grid;
    private int frame = 0;
    // Start is called before the first frame update
    void Start()
    {
        grid = Triangulator.GetGrid();
        playerNode = findPlayerNode(transform.position, grid);
    }

    // Update is called once per frame
    void Update()
    {
        if (frame < 5)
        {
            frame++;
        }
        else
        {
            frame = 0;
            grid = Triangulator.GetGrid();
            playerNode = findPlayerNode(transform.position, grid);
        }
        Debug.Log(playerNode.Data);
    }

    public static Node GetPlayerNode() => playerNode;

    Node findPlayerNode(Vector2 playerPosition, List<Node> grid)
    {
        Node playerNode = grid[0];
        float minDistance = Vector2.Distance(playerPosition, new Vector2(playerNode.Data.Item1, playerNode.Data.Item2));
        foreach (Node node in grid)
        {
            float distance = Vector2.Distance(playerPosition, new Vector2(node.Data.Item1, node.Data.Item2));
            if (distance < minDistance)
            {
                playerNode = node;
                minDistance = distance;
            }
        }
        return playerNode;
    }
}

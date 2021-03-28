using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private LayerMask _unwalkableMask;

    [SerializeField]
    private Vector2 _gridWorldSize;
    [SerializeField]
    private float _nodeRadius;

    // This is for the grid form graph
    private Node[,] _grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    // Start is called before the first frame update
    void Start()
    {
        nodeDiameter = _nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }



    public Node GetNodeOnPosition(int x, int y)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
            return _grid[x, y];
        return null;
    }

    public Node GetNodeOnPosition(Vector3 position)
    {
        float percentX = Mathf.Clamp01((position.x + _gridWorldSize.x / 2) / _gridWorldSize.x);
        float percentY = Mathf.Clamp01((position.z + _gridWorldSize.y / 2) / _gridWorldSize.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return _grid[x, y];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.GetGridX() + x;
                int checkY = node.GetGridY() + y;

                if (checkX >= 0 && checkX < gridSizeX)
                    if (checkY >= 0 && checkY < gridSizeY)
                        neighbours.Add(_grid[checkX, checkY]);
            }
        }

        return neighbours;
    }

    public bool MovementIsValid(Vector3 position, int x, int y)
    {
        Node n = GetNodeOnPosition(position);
        
        // check the special case of the border, to make it toroidal
        if (n.GetGridY() == 14)
        {
            if (n.GetGridX() == 27)
            {
                if (x == 1)
                    return true;
            }
            else if (n.GetGridX() == 0)
            {
                if (x == -1)
                    return true;
            }
        }
        return _grid[n.GetGridX() + x, n.GetGridY() + y].GetIsWalkable();
    }

    private void CreateGrid()
    {
        _grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.forward * _gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + _nodeRadius) + Vector3.forward * (y * nodeDiameter + _nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkableMask));
                _grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }

    }

    public Vector2 GetGridSize()
    {
        return new Vector2(gridSizeX, gridSizeY);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, 1, _gridWorldSize.y));

        if (_grid != null)
        {
            foreach (Node n in _grid)
            {
                Gizmos.color = n.GetIsWalkable() ? Color.white : Color.red;
                Gizmos.DrawWireCube(n.GetPosition(), Vector3.one * (nodeDiameter - 0.05f));
            }
        }


    }
}

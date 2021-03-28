using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private bool _walkable;
    private Vector3 _position;
    private float _gCost, _hCost;
    private int _gridX, _gridY;

    public Node parent;

    public Node(bool walkable, Vector3 position, int gridX, int gridY)
    {
        _walkable = walkable;
        _position = position;
        _gridX = gridX;
        _gridY = gridY;
    }

    public Vector3 GetPosition() { return _position; }
    public bool GetIsWalkable() { return _walkable; }
    public int GetGridX() { return _gridX; }
    public int GetGridY() { return _gridY; }
    public float gCost() { return _gCost; }
    public float hCost() { return _hCost; }
    public float fCost() { return _gCost + _hCost; }

    public void SetGCost(float g) { _gCost = g; }
    public void SetHCost(float h) { _hCost = h; }
}

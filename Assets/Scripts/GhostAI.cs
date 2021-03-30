using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI : MonoBehaviourPunCallbacks
{
    public enum GhostBehaviour { Pinky, Inky };
    private Grid _grid;
    private Transform _targetTransform;
    private GhostBehaviour _behaviour;

    // To handle the moviment
    private bool _isMoving;
    private Vector3 _targetPos;
    private Vector3 _translation;
    private float _speed = 2.0f;
    private bool _start = false;

    // Start is called before the first frame update
    void Start()
    {
        _grid = FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && _start)
        {
            FindPath(transform.position, _targetTransform.position);
            ManageTranslation();
            CheckBoundary();
        }
    }


    public void SetGhostParameters(Transform playerTransform, GhostBehaviour type)
    {
        _targetTransform = playerTransform;
        _behaviour = type;
        _start = true;
    }

    private void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = _grid.GetNodeOnPosition(startPos);
        Node targetNode = _grid.GetNodeOnPosition(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Select a node from the list to explore next, based on their costs
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost() <= currentNode.fCost() && openSet[i].hCost() < currentNode.hCost())
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Found the target node
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            // Else, we add the neighbours from this node to the list
            foreach (Node neighbour in _grid.GetNeighbours(currentNode))
            {
                if (!neighbour.GetIsWalkable() || closedSet.Contains(neighbour))
                    continue;

                float newCostToNeighbour = currentNode.gCost() + Vector3.Distance(currentNode.GetPosition(), neighbour.GetPosition());
                if (newCostToNeighbour < neighbour.gCost() || !openSet.Contains(neighbour))
                {
                    neighbour.SetGCost(newCostToNeighbour);
                    neighbour.SetHCost(Vector3.Distance(neighbour.GetPosition(), targetNode.GetPosition()));
                    neighbour.parent = currentNode;
                    // if the node is not on the set yet
                    if (!closedSet.Contains(neighbour))
                        openSet.Add(neighbour);

                }

            }
        }

    }

    // Method used to return the path formed by the A*
    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(currentNode);
        path.Reverse();


        if (path.Count > 0)
        {
            Node start = path[0];
            Node finish = path[1];
            _translation = new Vector3(finish.GetGridX() - start.GetGridX(), 0, finish.GetGridY() - start.GetGridY());
        }
        //_playerController.SetPath(path);
        //graph.SetPath(path);
    }

    private void ManageTranslation()
    {

        if (_isMoving)
        {
            // check if we arrived to the target, and if yes, stop
            Vector3 difference = _targetPos - transform.position;
            if (difference.sqrMagnitude > 0.05)
                transform.Translate(_speed * Time.deltaTime * _translation);
            else
            {
                transform.position = _targetPos;
                _isMoving = false;
            }
        }
        else
        {
            Vector2 newDir = new Vector2(_translation.x, _translation.z);
            // check the new position we are going to move, and set it as the target
            if (newDir.sqrMagnitude != 0 && _grid.MovementIsValid(transform.position, (int)newDir.x, (int)newDir.y))
            {
                Node currentPos = _grid.GetNodeOnPosition(transform.position);
                Node newPos = _grid.GetNodeOnPosition(currentPos.GetGridX() + (int)newDir.x, currentPos.GetGridY() + (int)newDir.y);
                if (newPos != null)
                {
                    _isMoving = true;
                    _targetPos = newPos.GetPosition();
                }
                // we have the special case to get around the arena
                else
                {
                    Debug.Log(_targetPos);
                    _isMoving = true;
                    _targetPos += _translation;
                }
            }
        }

    }

    private void CheckBoundary()
    {
        if (transform.position.x > 14 || transform.position.x < -14)
        {
            float newX = transform.position.x * -1 + 0.1f * _translation.x;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            _targetPos = transform.position;
            _isMoving = true;
        }

    }
}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum GhostBehaviour { Pinky, Inky };
    private Grid _grid;
    [SerializeField]
    private GameObject _targetGO;
    private GhostBehaviour _behaviour;

    // To handle the moviment
    private bool _isMoving = false;
    private Vector3 _targetPos;
    private Vector3 _newTargetPos;
    private Vector3 _translation;
    private Vector3 _newTranslation;
    private float _speed = 2.8f;
    private bool _start = false;

    [SerializeField]
    private Material _inkyMaterial;


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            // We own this player: send the others our data
            stream.SendNext(_behaviour);
        }
        else
        {
            // Network player, receive data
            this._behaviour = (GhostBehaviour) stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _grid = FindObjectOfType<Grid>();
        _targetPos = transform.position + new Vector3(0, 0, -2);
        _translation = Vector3.up;
        _isMoving = true;

        if(_behaviour == GhostBehaviour.Inky)
            GetComponent<Renderer>().material = _inkyMaterial;

    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && _start)
        {
            Vector3 targetPos = SelectTarget();
            if (!Arrived2Target(targetPos))
            {
                FindPath(transform.position, targetPos);
                ManageTranslation();
                CheckBoundary();
            }
            
        }
        else
        {
            if (_behaviour == GhostBehaviour.Inky)
                GetComponent<Renderer>().material = _inkyMaterial;
        }
    }


    public void SetGhostParameters(GameObject player, GhostBehaviour type)
    {
        _targetGO = player;
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
            _newTranslation = new Vector3(finish.GetGridX() - start.GetGridX(), - (finish.GetGridY() - start.GetGridY()), 0);
            _newTargetPos = finish.GetPosition();
        }

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
                // to guarantee that it will be exactly on the centre of the tile
                transform.position = _grid.GetNodeOnPosition(_targetPos).GetPosition();
                _isMoving = false;
            }
        }
        else
        {
            _targetPos = _newTargetPos;
            if (Mathf.Abs(_newTranslation.x) < 2.0f )
                _translation = _newTranslation;
            _isMoving = true;
        }

    }

    private void CheckBoundary()
    {
        if (transform.position.x > 14 || transform.position.x < -14)
        {
            float newX = transform.position.x * -1 + 0.5f * _translation.x;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            _targetPos = transform.position;
        }

    }

    private Vector3 SelectTarget()
    {
        if (_targetGO.GetComponent<PlayerController>().IsInvincible())
        {
            Vector2 gridSize = _grid.GetGridSize();
            int randX = (int) Random.Range(0, gridSize.x);
            int randY = (int) Random.Range(0, gridSize.y);
            while (!_grid.GetNodeOnPosition(randX, randY).GetIsWalkable())
            {
                randX = (int)Random.Range(0, gridSize.x);
                randY = (int)Random.Range(0, gridSize.y);
            }

            _grid.inkyTarget = _grid.pinkyTarget = _grid.GetNodeOnPosition(randX, randY);
            return _grid.GetNodeOnPosition(randX, randY).GetPosition();
        }
        Vector3 targetPos;
        Vector3 playerPos = _targetGO.transform.position;
        switch (_behaviour)
        {
            case GhostBehaviour.Pinky:
                // Pinky will try tro predict where the player is going, and ambush him 4 or less tiles ahead.
                // If the tile he is targeting is not a valid tile to move, he will try to ambush on the tile of the direction -1
                Vector3 distanceFromPlayer = playerPos - transform.position;
                if (distanceFromPlayer.sqrMagnitude <= 4)
                {
                    targetPos = playerPos;
                    _grid.pinkyTarget = _grid.GetNodeOnPosition(playerPos);
                    break;
                }
                targetPos = CalculatePinkyTarget(playerPos);
                _grid.pinkyTarget = _grid.GetNodeOnPosition(targetPos);
                break;
            case GhostBehaviour.Inky:
                // Inky will calculate the distance between Pinky and its target, then,
                // it will proceed to set its target as 2*distance(pinky, target)
                Vector3 pinkyTarget = CalculatePinkyTarget(playerPos);
                Vector3 distance = pinkyTarget - transform.position;
                float maxDist = 2.0f;
                Node target = _grid.GetNodeOnPosition(distance *maxDist + transform.position);
                while (!target.GetIsWalkable())
                {
                    maxDist -= 0.1f;
                    target = _grid.GetNodeOnPosition(distance * maxDist + transform.position);
                }
                _grid.inkyTarget = target;
                targetPos = target.GetPosition();
                break;
            default:
                targetPos = playerPos;
                break;
        }
        return targetPos;
    }

    private Vector3 CalculatePinkyTarget(Vector3 targetPos)
    {
        Vector3 targetDirection = _targetGO.GetComponent<PlayerController>().GetPlayerDirection();
        int i = 4;
        Vector2 potentialTarget = new Vector2(targetDirection.x * i, (int)targetDirection.z * i);
        while (!_grid.MovementIsValid(targetPos, (int)potentialTarget.x, (int)potentialTarget.y))
        {
            i--;
            potentialTarget = new Vector2(targetDirection.x * i, (int)targetDirection.z * i);
        }
        Node playerOnGrid = _grid.GetNodeOnPosition(targetPos);

        int newPosX = playerOnGrid.GetGridX() + (int)potentialTarget.x;
        int newPosY = playerOnGrid.GetGridY() + (int)potentialTarget.y;
        // check the boundaries, as they work differently
        if (newPosY == 14)
        {
            if (newPosX < 4)
                newPosX = 0;
            else if (newPosX > 23)
                newPosX = 27;
        }
        return _grid.GetNodeOnPosition(newPosX, newPosY).GetPosition();
    }

    private bool Arrived2Target(Vector3 targetPos)
    {
        Node current = _grid.GetNodeOnPosition(transform.position);
        Node tgt = _grid.GetNodeOnPosition(targetPos);
        return (current.GetGridX() == tgt.GetGridX() && current.GetGridY() == tgt.GetGridY());
    }
}

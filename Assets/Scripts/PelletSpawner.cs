using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletSpawner : MonoBehaviour
{

    [SerializeField]
    private GameObject _pelletObject;
    [SerializeField]
    private GameObject _superPellectObject;
    [SerializeField]
    private Grid _grid;

    [SerializeField]
    private bool _pelletsSpawned = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_pelletsSpawned)
            SpawnPellets();
    }

    private void SpawnPellets()
    {
        _pelletsSpawned = true;
        Vector2 gridSize = _grid.GetGridSize();
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Node n = _grid.GetNodeOnPosition(i, j);
                if (n.GetIsWalkable())
                {
                    if (i >= 11 && i <= 16)
                        if (j >= 12 && j <= 15)
                            continue;
                    if (CheckSuperPelletPos(n))
                        Instantiate(_superPellectObject, n.GetPosition(), Quaternion.identity);
                    else
                        Instantiate(_pelletObject, n.GetPosition(), Quaternion.identity);
                }
            }
        }
    }

    private bool CheckSuperPelletPos(Node n)
    {
        if (n.GetGridX() == 1)
        {
            return (n.GetGridY() == 1 || n.GetGridY() == 29);
        }
        else if (n.GetGridX() == 26)
        {
            return (n.GetGridY() == 1 || n.GetGridY() == 29);
        }
        return false;
    }
}

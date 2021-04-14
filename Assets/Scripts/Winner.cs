using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Winner : MonoBehaviour
{
    public string winnerName { get; set; }
    public int score { get; set; }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}

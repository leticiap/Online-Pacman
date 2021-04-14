using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private Text _text;

    // Start is called before the first frame update
    void Start()
    {
        string playerName = GameObject.FindGameObjectWithTag("playerName").GetComponent<Winner>().winnerName;
        int score = GameObject.FindGameObjectWithTag("playerName").GetComponent<Winner>().score;
        _text.text = playerName + " won!!\n score: " + score;
        Destroy(GameObject.FindGameObjectWithTag("playerName"));
    }

}

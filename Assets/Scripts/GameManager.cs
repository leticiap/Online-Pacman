using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    [Tooltip("The prefab to use for representing the pellet")]
    [SerializeField]
    private GameObject _pelletPrefab;
    [Tooltip("The prefab to use for representing the super pellet")]
    [SerializeField]
    private GameObject _superPellectPrefab;
    [Tooltip("The prefab to use for representing the ghost")]
    [SerializeField]
    private GameObject _ghostPrefab;

    private Grid _grid;

    [SerializeField]
    private int indexScene;

    private bool _gameEnded = false;

    private bool _pelletsSpawned = false;

    void Start()
    {
        Cursor.visible = false;

        Instance = this;
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (PlayerController.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                Quaternion rotation = new Quaternion(0, 0, 0.707106829f, 0.707106829f);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0.5f, 0f, -10f), rotation, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        _grid = FindObjectOfType<Grid>();
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnPellets();
            _pelletsSpawned = true;
            StartCoroutine(SpawnGhosts());
        }
            
    }

    private void Update()
    {
        // To check if the game has ended
        GameObject[] pellets = GameObject.FindGameObjectsWithTag("superPellet");
        if (_pelletsSpawned && !_gameEnded && pellets.Length == 0)
        {
            pellets = GameObject.FindGameObjectsWithTag("pellet");
            if (pellets.Length == 0)
            {
                _gameEnded = true;
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                int highScore = players[0].GetComponent<PlayerController>().GetScore();
                string name = players[0].GetComponent<PlayerController>().GetPlayerName();
                for (int i = 1; i < players.Length; i++)
                {
                    if (players[i].GetComponent<PlayerController>().GetScore() > highScore)
                    {
                        highScore = players[i].GetComponent<PlayerController>().GetScore();
                        name = players[i].GetComponent<PlayerController>().GetPlayerName();

                    }
                }
                Winner winner = GameObject.FindGameObjectWithTag("playerName").GetComponent<Winner>();
                winner.winnerName = name;
                winner.score = highScore;
                LeaveRoom();
            }
                
        }
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            LoadArena();
        }
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        if (_gameEnded)
            SceneManager.LoadScene(3);
        else
            SceneManager.LoadScene(0);
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }


    /*
    public void ResetLevel()
    {
        // reset everything
        _grid = FindObjectOfType<Grid>();
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject[] pellets = GameObject.FindGameObjectsWithTag("pellet");
            foreach (GameObject pellet in pellets)
                PhotonNetwork.Destroy(pellet);
            pellets = GameObject.FindGameObjectsWithTag("superPellet");
            foreach (GameObject pellet in pellets)
                PhotonNetwork.Destroy(pellet);
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("ghost");
            foreach (GameObject ghost in ghosts)
                PhotonNetwork.Destroy(ghost);

            _grid = FindObjectOfType<Grid>();
            SpawnPellets();
            SpawnGhosts();
        }
    }
    */
    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            PhotonNetwork.LoadLevel("Game");
        else
        {
            PhotonNetwork.LoadLevel("WaitingRoom");
        }
    }

    private void SpawnPellets()
    {
        Vector2 gridSize = _grid.GetGridSize();
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Node n = _grid.GetNodeOnPosition(i, j);
                if (n.GetIsWalkable())
                {
                    // place where the ghosts are
                    if (i >= 11 && i <= 16)
                        if (j >= 12 && j <= 15)
                            continue;
                    // place where the players spawn
                    if (i == 14 && j == 5)
                        continue;
                    if (CheckSuperPelletPos(n))
                        PhotonNetwork.Instantiate(_superPellectPrefab.name, n.GetPosition(), Quaternion.identity);
                    else
                        PhotonNetwork.Instantiate(_pelletPrefab.name, n.GetPosition(), Quaternion.identity);
                }
            }
        }
    }

    private IEnumerator SpawnGhosts()
    {
        // wait the scene load
        yield return new WaitForSeconds(0.3f);

        // we create a total of 4 ghost, and we have two different behaviours, so there is at least one ghost of each type trying to ambush each player
        GameObject[] _ghosts = new GameObject[4];
        Vector3 ghostPosition = _grid.GetNodeOnPosition(13, 13).GetPosition();
        Quaternion rotation = new Quaternion(-0.707106829f, 0, 0, 0.707106829f);
        for (int i = 0; i < _ghosts.Length; i++)
            _ghosts[i] = PhotonNetwork.Instantiate(_ghostPrefab.name, ghostPosition + new Vector3((int) i/2, 0, i % 2), rotation);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            for (int i = 0; i < _ghosts.Length; i += 2)
            {
                _ghosts[i].GetComponent<GhostAI>().SetGhostParameters(player, GhostAI.GhostBehaviour.Inky);
                _ghosts[i + 1].GetComponent<GhostAI>().SetGhostParameters(player, GhostAI.GhostBehaviour.Pinky);
            }
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                _ghosts[i * 2].GetComponent<GhostAI>().SetGhostParameters(players[i], GhostAI.GhostBehaviour.Inky);
                _ghosts[i * 2 + 1].GetComponent<GhostAI>().SetGhostParameters(players[i], GhostAI.GhostBehaviour.Pinky);
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

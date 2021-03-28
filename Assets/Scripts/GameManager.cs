using Photon.Pun;
using Photon.Realtime;
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
    private Grid _grid;

    void Start()
    {
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
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0.5f, 0f, -10f), Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        _grid = FindObjectOfType<Grid>();
        if (PhotonNetwork.IsMasterClient)
            SpawnPellets();
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
        SceneManager.LoadScene(0);
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

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

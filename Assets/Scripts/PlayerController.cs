using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    private Grid _grid;
    private float _speed = 3.0f;
    private Vector3 _translation;
    private Vector3 _newTranslation;
    private bool _isMoving;
    private Vector3 _targetPos;
    private int _score = 0;
    private bool _invincible = false;
    private Vector3 _initialPos;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    private GameObject _uiGo = null;

    [SerializeField]
    private Material _playerMaterial;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(_score);
        }
        else
        {
            // Network player, receive data
            this._score = (int)stream.ReceiveNext();
        }
    }


    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
        _initialPos = new Vector3(0.5f, 0f, -10f);
    }

    // Start is called before the first frame update
    void Start()
    {
        _translation = _newTranslation = Vector3.zero;
        _isMoving = false;
        _targetPos = transform.position;

        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        if (PlayerUiPrefab != null && _uiGo == null)
        {
            _uiGo = Instantiate(PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        _grid = FindObjectOfType<Grid>();

        if (photonView.IsMine)
            GetComponent<Renderer>().material = _playerMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            ManageInputs();
            ManageTranslation();
            CheckBoundary();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "pellet")
        {
            Debug.Log(_score);
            if (photonView.IsMine)
                _score++;

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(other.gameObject);

        }
        else if (other.gameObject.tag == "superPellet")
        {
            if (photonView.IsMine)
            {
                _score++;
                _speed = 5.0f;
                StartCoroutine(SpeedTimer());
            }

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "ghost")
        {
            if (!_invincible)
            {
                transform.position = _initialPos;
                _translation = _newTranslation = Vector3.zero;
                _isMoving = false;
                _targetPos = transform.position;
                _invincible = true;
                StartCoroutine(InvincibleTimer());
            }
        }

    }

    private IEnumerator InvincibleTimer()
    {
        yield return new WaitForSeconds(3.0f);
        _invincible = false;
    }

    private IEnumerator SpeedTimer()
    {
        yield return new WaitForSeconds(3.0f);
        _speed = 3.0f;
    }

    private void ManageInputs()
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (_grid.MovementIsValid(transform.position, 0, -1))
                _newTranslation = Vector3.back;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            if (_grid.MovementIsValid(transform.position, 1, 0))
                _newTranslation = Vector3.right;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (_grid.MovementIsValid(transform.position, 0, 1))
                _newTranslation = Vector3.forward;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (_grid.MovementIsValid(transform.position, -1, 0))
                _newTranslation = Vector3.left;
        }
        if (!_isMoving && _newTranslation != Vector3.zero)
        {
            _translation = Vector3.up;
            if (_newTranslation == Vector3.right)
            {
                transform.rotation = new Quaternion(0.707106829f, 0.707106829f, 0, 0);
            }
            
            else if (_newTranslation == Vector3.left)
            {
                transform.rotation = new Quaternion(0, 0, 0.707106829f, 0.707106829f);
            }
            else if (_newTranslation == Vector3.forward)
            {
                transform.rotation = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
            }
            else if (_newTranslation == Vector3.back)
            {
                transform.rotation = new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f);
            }

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
                transform.position = _targetPos;
                _isMoving = false;
            }
        }
        else
        {
            Vector2 newDir = new Vector2(_newTranslation.x, _newTranslation.z);
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
                    _isMoving = true;
                    _targetPos += _newTranslation;
                }
            }
        }

    }


    private void CheckBoundary()
    {
        if (transform.position.x > 14 || transform.position.x < -14)
        {
            float newX = transform.position.x * -1 + 0.1f * _newTranslation.x;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            _targetPos = transform.position;
            _isMoving = true;
        }

    }
    public Vector3 GetPlayerDirection() { return _translation; }

    void CalledOnLevelWasLoaded(int level)
    {

        transform.position = _initialPos;
        if (PlayerUiPrefab != null && _uiGo == null)
        {
            _uiGo = Instantiate(PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        // Make sure to reset all the configurations
        _translation = _newTranslation = Vector3.zero;
        _isMoving = false;
        _targetPos = transform.position;
        _score = 0;
        _speed = 3.0f;

        _grid = FindObjectOfType<Grid>();            
    }

    public override void OnDisable()
    {
        // Always call the base to remove callbacks
        base.OnDisable();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        CalledOnLevelWasLoaded(scene.buildIndex);
    }

    public bool IsInvincible()
    {
        return _invincible;
    }

    public int GetScore()
    {
        return _score;
    }

    public string GetPlayerName()
    {
        return photonView.Owner.NickName;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Grid _grid;
    private float _speed = 3.0f;
    private Vector3 _translation;
    private Vector3 _newTranslation;
    private bool _isMoving;
    private Vector3 _targetPos;

    private int _score = 0;
    // Start is called before the first frame update
    void Start()
    {
        _translation = _newTranslation = Vector3.zero;
        _isMoving = false;
        _targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ManageInputs();
        ManageTranslation();
        CheckBoundary();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "pellet")
        {
            _score++;
            Debug.Log(_score);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "superPellet")
        {
            _score++;
            Debug.Log(_score);
            _speed = 5.0f;
            StartCoroutine(SpeedTimer());
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "ghost")
        {
            Debug.Log("Oh noes it's a ghost!");
        }
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
        if (!_isMoving)
            _translation = _newTranslation;
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

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class PlayerController : MonoBehaviour
{
    private float _acceleration;
    private const float _maxAcceleration = 0.9f;
    private const float _accelerationIncreaseFactor = 0.05f;
    private bool _spaceHeld;
    private bool _arrowHeld;
    private float _nextIncrease = 0.1f;
    private const float _increaseCooldown = 0.1f;
    private PlayerInputActions _playerInputActions;
    private RocketMovement _rocketMovement;

    private enum Direction
    {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        FORWARD_RIGHT,
        FORWARD_LEFT,
        BACKWARD_RIGHT,
        BACKWARD_LEFT,
        NONE
    }
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rocketMovement = GetComponent<RocketMovement>();
        
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Fire.performed += Fire;
    }

    private void FixedUpdate()
    {
        Move();

        if (Time.time >= _nextIncrease)
        {
            _nextIncrease = Time.time + _increaseCooldown;
            UpdateAcceleration();
        }
    }

    private void Move()
    {
        Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();
        float space = _playerInputActions.Player.MoveStraight.ReadValue<float>();
        _spaceHeld = space != 0;
        _arrowHeld = inputVector != Vector2.zero;

        if (_spaceHeld)
        {
            _rocketMovement.ApplyAcceleration(2.0f, RocketMovement.ThrusterTypes.Main);
        }

        if (_arrowHeld)
        {
            Direction dir = GetInputMovementDirection(inputVector);
            switch (dir)
            {
                case Direction.RIGHT:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustLeft);
                    break;
                case Direction.LEFT:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustRight);
                    break;
                case Direction.FORWARD:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustFront);
                    break;
                case Direction.BACKWARD:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustBack);
                    break;
                case Direction.FORWARD_RIGHT:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustFront);
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustLeft);
                    break;
                case Direction.FORWARD_LEFT:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustFront);
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustRight);
                    break;
                case Direction.BACKWARD_RIGHT:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustBack);
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustLeft);
                    break;
                case Direction.BACKWARD_LEFT:
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustBack);
                    _rocketMovement.ApplyAcceleration(_acceleration, RocketMovement.ThrusterTypes.ExhaustLeft);
                    break;
            }
        }

        _rocketMovement.UpdateOrientation();
    }

    // TODO: implement the Fire function
    private void Fire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("FIRE!!");
        }
    }
    
    private void OnEnable()
    {
        _playerInputActions.Player.Enable();
    }
    
    private void OnDisable()
    {
        _playerInputActions.Player.Disable();
    }

    private void UpdateAcceleration()
    {
        if (_spaceHeld || _arrowHeld)
        {
            if (_acceleration < _maxAcceleration)
            {
                _acceleration += _accelerationIncreaseFactor;
                _acceleration = Math.Min(_acceleration, _maxAcceleration);
            }
        }
        else
        {
            _acceleration = 0.0f;
        }
    }

    private Direction GetInputMovementDirection(Vector2 input)
    {
        if (input.x > 0)    // moving right
        {
            if (input.y == 0.0f)    // moving straight right
            {
                return Direction.RIGHT;
            }
            if (input.y < 0)        // moving backward right
            {
                return Direction.BACKWARD_RIGHT;
            }
            if (input.y > 0)        // moving forward right
            {
                return Direction.FORWARD_RIGHT;
            }
        }

        if (input.x < 0)        // moving left
        {
            if (input.y == 0.0f)    // moving straight left
            {
                return Direction.LEFT;
            }
            if (input.y < 0)        // moving backward left
            {
                return Direction.BACKWARD_LEFT;
            }
            if (input.y > 0)        // moving forward left
            {
                return Direction.FORWARD_LEFT;
            }
        }

        if (input.x == 0)       // moving either forward or backward
        {
            if (input.y < 0)    // moving backward
            {
                return Direction.BACKWARD;
            }

            if (input.y > 0)
            {
                return Direction.FORWARD;
            }
        }
        
        return Direction.NONE;
    }
}

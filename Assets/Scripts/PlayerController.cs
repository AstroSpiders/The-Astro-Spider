using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RocketMovement))]
public class PlayerController : MonoBehaviour
{
    private const float              _bias = 0.001f;

    [SerializeField]
    private       Transform          _playerInputSpace = default;

    private       PlayerInputActions _playerInputActions;
    private       RocketMovement     _rocketMovement;
        
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rocketMovement = GetComponent<RocketMovement>();
        
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Fire.performed += Fire;
    }

    private void Update()
    {
        var input = _playerInputActions.Player.Move.ReadValue<Vector2>();

        Vector3 desiredDirection;
        if (_playerInputSpace)
            desiredDirection = _playerInputSpace.TransformDirection(input.x, 0.0f, input.y);
        else
            desiredDirection = new Vector3(input.x, 0.0f, input.y);
        

        var space = _playerInputActions.Player.MoveStraight.ReadValue<float>();

        if (Math.Abs(space) >= _bias)
            _rocketMovement.ApplyAcceleration(space, RocketMovement.ThrusterTypes.Main);

        float horizontalDot = Vector3.Dot(desiredDirection, transform.right);

        var thrusterX = horizontalDot > 0 ? RocketMovement.ThrusterTypes.ExhaustLeft : RocketMovement.ThrusterTypes.ExhaustRight;
        _rocketMovement.ApplyAcceleration(Math.Abs(horizontalDot), thrusterX);

        float verticalDot = Vector3.Dot(desiredDirection, transform.up);
        
        var thrusterY = verticalDot > 0 ? RocketMovement.ThrusterTypes.ExhaustBack : RocketMovement.ThrusterTypes.ExhaustFront;
        _rocketMovement.ApplyAcceleration(Math.Abs(verticalDot), thrusterY);
        
    }

    // TODO: implement the Fire function
    private void Fire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("FIRE!!");
        }
    }
    
    private void OnEnable()  => _playerInputActions.Player.Enable();
    
    private void OnDisable() => _playerInputActions.Player.Disable();
}

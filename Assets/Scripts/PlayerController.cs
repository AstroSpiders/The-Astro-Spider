using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;
    private RocketMovement _rocketMovement;
    
    private const float _bias = 0.001f;
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rocketMovement = GetComponent<RocketMovement>();
        
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Fire.performed += Fire;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        var input = _playerInputActions.Player.Move.ReadValue<Vector2>();
        var space = _playerInputActions.Player.MoveStraight.ReadValue<float>();

        if (Math.Abs(space) >= _bias)
            _rocketMovement.ApplyAcceleration(1.0f, RocketMovement.ThrusterTypes.Main);

        var thrusterX = input.x > 0 ? RocketMovement.ThrusterTypes.ExhaustLeft : RocketMovement.ThrusterTypes.ExhaustRight;
        _rocketMovement.ApplyAcceleration(Math.Abs(input.x), thrusterX);
        
        var thrusterY = input.y > 0 ? RocketMovement.ThrusterTypes.ExhaustFront : RocketMovement.ThrusterTypes.ExhaustBack;
        _rocketMovement.ApplyAcceleration(Math.Abs(input.y), thrusterY);
        
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

}

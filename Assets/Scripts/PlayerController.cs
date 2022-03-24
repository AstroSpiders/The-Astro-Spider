using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class PlayerController : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;
    private RocketMovement _rocketMovement;
    
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
        Vector2 input = _playerInputActions.Player.Move.ReadValue<Vector2>();
        float space = _playerInputActions.Player.MoveStraight.ReadValue<float>();

        if (Math.Abs(space) >= 0.001f)
            _rocketMovement.ApplyAcceleration(1.0f, RocketMovement.ThrusterTypes.Main);

        RocketMovement.ThrusterTypes thruster;
        
        thruster = input.x > 0 ? RocketMovement.ThrusterTypes.ExhaustLeft : RocketMovement.ThrusterTypes.ExhaustRight;
        _rocketMovement.ApplyAcceleration(Math.Abs(input.x), thruster);
        thruster = input.y > 0 ? RocketMovement.ThrusterTypes.ExhaustFront : RocketMovement.ThrusterTypes.ExhaustBack;
        _rocketMovement.ApplyAcceleration(Math.Abs(input.y), thruster);
        
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

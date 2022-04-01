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
        _rocketMovement     = GetComponent<RocketMovement>();
        
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Fire.performed += Fire;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        var input = _playerInputActions.Player.Move.ReadValue<Vector2>();

        Vector3 desiredDirection;
        if (_playerInputSpace && (Math.Abs(input.x) >= _bias || Math.Abs(input.y) >= _bias))
            desiredDirection = _playerInputSpace.TransformDirection(input.x, 0.0f, input.y).normalized;
        else
            desiredDirection = new Vector3(input.x, 0.0f, input.y);

        float horizontalDot = Vector3.Dot(desiredDirection, transform.right);
        float verticalDot   = Vector3.Dot(desiredDirection, -transform.up);

        if (Math.Abs(horizontalDot) >= _bias || 
            Math.Abs(verticalDot)   >= _bias)
        {
            Vector2 dotDirection = new Vector2(horizontalDot, verticalDot).normalized * input.magnitude;

            var thrusterX = dotDirection.x > 0.0f ? RocketMovement.ThrusterTypes.ExhaustLeft : RocketMovement.ThrusterTypes.ExhaustRight;
            _rocketMovement.ApplyAcceleration(Math.Abs(dotDirection.x), thrusterX);

            var thrusterY = dotDirection.y > 0.0f ? RocketMovement.ThrusterTypes.ExhaustFront : RocketMovement.ThrusterTypes.ExhaustBack;
            _rocketMovement.ApplyAcceleration(Math.Abs(dotDirection.y), thrusterY);
        }

        var space = _playerInputActions.Player.MoveStraight.ReadValue<float>();

        if (Math.Abs(space) >= _bias)
            _rocketMovement.ApplyAcceleration(space, RocketMovement.ThrusterTypes.Main);
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

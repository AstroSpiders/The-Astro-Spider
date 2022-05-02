using System;
using UnityEngine;

[RequireComponent(typeof(RocketMovement))]
[RequireComponent(typeof(ThrustersSound))]
public class PlayerController : MonoBehaviour
{
    private const float              _bias           = 0.001f;
    private const float              _fireCooldown   = 0.2f;

    public Transform                 PlayerInputSpace;

    [SerializeField] 
    private Transform                _projectilePrefab;
    [SerializeField] 
    private Transform                _firePoint;
                  
    private PlayerInputActions       _playerInputActions;
    private RocketMovement           _rocketMovement;
    private Vector3                  _initialRotation;
    private float                    _fireTimestamp;
    private bool                     _fireButtonPressed;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rocketMovement = GetComponent<RocketMovement>();

        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Fire.performed += _ => _fireButtonPressed = true;
        _playerInputActions.Player.Fire.canceled += _ => _fireButtonPressed = false;

        _initialRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        var input = _playerInputActions.Player.Move.ReadValue<Vector2>();

        Vector3 desiredDirection;
        if (PlayerInputSpace && (Math.Abs(input.x) >= _bias || Math.Abs(input.y) >= _bias))
            desiredDirection = PlayerInputSpace.TransformDirection(input.x, 0.0f, input.y).normalized;
        else
            desiredDirection = new Vector3(input.x, 0.0f, input.y);

        float horizontalDot = Vector3.Dot(desiredDirection, transform.right);
        float verticalDot = Vector3.Dot(desiredDirection, -transform.up);

        if (Math.Abs(horizontalDot) >= _bias ||
            Math.Abs(verticalDot) >= _bias)
        {
            Vector2 dotDirection = new Vector2(horizontalDot, verticalDot).normalized * input.magnitude;

            var thrusterX = dotDirection.x > 0.0f
                ? RocketMovement.ThrusterTypes.ExhaustLeft
                : RocketMovement.ThrusterTypes.ExhaustRight;
            _rocketMovement.ApplyAcceleration(Math.Abs(dotDirection.x), thrusterX);

            var thrusterY = dotDirection.y > 0.0f
                ? RocketMovement.ThrusterTypes.ExhaustFront
                : RocketMovement.ThrusterTypes.ExhaustBack;
            _rocketMovement.ApplyAcceleration(Math.Abs(dotDirection.y), thrusterY);
        }

        var space = _playerInputActions.Player.MoveStraight.ReadValue<float>();

        if (Math.Abs(space) >= _bias)
            _rocketMovement.ApplyAcceleration(space, RocketMovement.ThrusterTypes.Main);

        if (_fireButtonPressed)
        {
            _fireTimestamp += Time.deltaTime;
            if (_fireTimestamp >= _fireCooldown)
            {
                Fire();
                _fireTimestamp -= _fireCooldown;
            }
        }
        else
        {
            _fireTimestamp = _fireCooldown;
        }
    }

    private void Fire()
    {
        if (_projectilePrefab == null || _firePoint == null)
            return;

        if (_fireTimestamp > Time.time)
            return;

        var rocketTransform = transform;
        var projectile = Instantiate(_projectilePrefab, _firePoint.position, rocketTransform.rotation);
        projectile.Rotate(-_initialRotation);
    }

    private void OnEnable() => _playerInputActions.Player.Enable();

    private void OnDisable() => _playerInputActions.Player.Disable();
}
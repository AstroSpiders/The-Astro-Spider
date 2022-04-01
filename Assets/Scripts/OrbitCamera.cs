using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    private       PlayerInputActions _playerInputActions;
    
    [SerializeField, Range(1.0f, 10.0f)] 
    private float                _distance          = 5.0f;
    
    [SerializeField, Range(1.0f, 360.0f)] 
    private float               _rotationSpeed      = 10.0f;

    [SerializeField] 
    private Transform            _focus;
    [SerializeField, Min(0.0f)] 
    private float                _focusRadius;
    [SerializeField, Range(0.0f, 1.0f)] 
    private float               _focusCentering     = 0.5f;

    [SerializeField, Range(-89.0f, 89.0f)] 
    private float               _minVerticalAngle   = -60.0f;
    [SerializeField, Range(-89.0f, 89.0f)] 
    private float               _maxVerticalAngle   = 60.0f;

    [SerializeField, Min(0.0f)] 
    private float               _alignDelay         = 5.0f;

    [SerializeField, Range(0.0f, 90.0f)] 
    private float               _alignSmoothRange   = 45.0f;

    private Vector3            _focusPoint;
    private Vector3            _previousFocusPoint;
    private Vector2            _orbitAngles        = new Vector2(0.0f, 0.0f);
    private float              _lastManualRotationTime;

    private Quaternion         _focusAlignment    = Quaternion.identity;
    private Quaternion         _orbitRotation;

    private const float        _verticalTranslate  = 0.0f;
    private const float        _bigEpsilon         = 0.01f;
    private const float        _epsilon            = 0.001f;
    private const float        _smallEpsilon       = 0.000001f;
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _focusPoint = _focus.position;
        transform.localRotation = _orbitRotation = Quaternion.Euler(_orbitAngles);
    }

    private void LateUpdate()
    {
        _focusAlignment = Quaternion.FromToRotation(_focusAlignment * Vector3.up, _focus.forward) * _focusAlignment;

        UpdateFocusPoint();
        if (ManualRotation() || AutomaticRotation())
        {
            ConstraintAngles();
            _orbitRotation = Quaternion.Euler(_orbitAngles);
        }
        Quaternion lookRotation = _focusAlignment * _orbitRotation;
        var lookDirection = lookRotation * Vector3.forward;
        var translate = new Vector3(0f, _verticalTranslate, 0f);
        var lookPosition = _focusPoint - lookDirection * _distance + translate;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    private void UpdateFocusPoint()
    {
        _previousFocusPoint = _focusPoint;
        var targetPoint = _focus.position;
        if (_focusRadius > 0.0f)
        {
            var distance = Vector3.Distance(targetPoint, _focusPoint);
            var t = 1.0f;
            if (distance > _bigEpsilon && _focusCentering > 0.0f)
            {
                t = Mathf.Pow(1.0f - _focusCentering, Time.unscaledDeltaTime);
            }
            
            if (distance > _focusRadius)
            {
                t = Mathf.Min(t, _focusRadius / distance);
            }

            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }
        else
        {
            _focusPoint = targetPoint;
        }
    }

    private bool ManualRotation()
    {
        var input = _playerInputActions.Camera.Rotation.ReadValue<Vector2>();
        
        input = new Vector2(input.y * Time.unscaledDeltaTime, input.x * Time.unscaledDeltaTime);
        if (Mathf.Abs(input.x) > _epsilon || Mathf.Abs(input.y) > _epsilon)
        {
            _orbitAngles += _rotationSpeed * new Vector2(input.x, -input.y);
            _lastManualRotationTime = Time.unscaledTime;
            return true;
        }

        return false;
    }

    private bool AutomaticRotation()
    {
        if (Time.unscaledTime - _lastManualRotationTime < _alignDelay)
            return false;

        Vector3 alignedDelta = Quaternion.Inverse(_focusAlignment) *
            (_focusPoint - _previousFocusPoint);

        var movement = new Vector2(alignedDelta.x, alignedDelta.z);
        var movementDeltaSqr = movement.sqrMagnitude;

        if (movementDeltaSqr < _smallEpsilon)
            return false;

        var headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        var deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
        var rotationChange = _rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        
        if (deltaAbs < _alignSmoothRange)
            rotationChange *= deltaAbs / _alignSmoothRange;
        else if (180.0f - deltaAbs < _alignSmoothRange)
            rotationChange *= (180.0f - deltaAbs) / _alignSmoothRange;
        
        _orbitAngles.y =  Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);

        return true;
    }

    private void ConstraintAngles()
    {
        _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVerticalAngle);

        if (_orbitAngles.y < 0.0f)
            _orbitAngles.y += 360.0f;
        else if (_orbitAngles.y >= 360.0f)
            _orbitAngles.y -= 360.0f;
    }

    private static float GetAngle(Vector2 direction)
    {
        var angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0.0f ? 360.0f - angle : angle;
    }

    private void OnEnable() => _playerInputActions.Camera.Enable();

    private void OnDisable() => _playerInputActions.Camera.Disable();

    private void OnValidate()
    {
        if (_maxVerticalAngle < _minVerticalAngle)
            _maxVerticalAngle = _minVerticalAngle;
    }
}

using UnityEngine;

// Based on https://catlikecoding.com/unity/tutorials/movement/orbit-camera/
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    private       PlayerInputActions _playerInputActions;
    
    [SerializeField, Range(1.0f, 10.0f)] 
    private float                _distance          = 5.0f;
    
    [SerializeField, Range(1.0f, 360.0f)] 
    private float               _rotationSpeed      = 15.0f;

    [SerializeField] 
    private Transform            _focus;
    [SerializeField, Min(0.0f)] 
    private float                _focusRadius       = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)] 
    private float               _focusCentering     = 0.5f;

    [SerializeField, Range(-89.0f, 89.0f)] 
    private float               _minVerticalAngle   = -89.0f;
    [SerializeField, Range(-89.0f, 89.0f)] 
    private float               _maxVerticalAngle   = 89.0f;

    [SerializeField]
    private LayerMask           _obstructionMask    = -1;

    private Vector3             _focusPoint;
    private Vector2             _orbitAngles         = new Vector2(0.0f, 0.0f);
    private Camera              _camera;

    private Vector3 _cameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
                    halfExtends.y = _camera.nearClipPlane * 
                                    Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
                    halfExtends.x = halfExtends.y * 
                                    _camera.aspect;
                    halfExtends.z = 0f;
            return halfExtends;
        }
    }
    
    private Quaternion         _focusAlignment      = Quaternion.identity;
    private Quaternion         _orbitRotation;
    private const float        _bigEpsilon          = 0.01f;
    private const float        _epsilon             = 0.001f;
    
    private void Awake()
    {
        _camera                 = GetComponent<Camera>();
        _playerInputActions     = new PlayerInputActions();
        _focusPoint             = _focus.position;
        transform.localRotation = _orbitRotation = Quaternion.Euler(_orbitAngles);
    }

    private void LateUpdate()
    {
        _focusAlignment = Quaternion.FromToRotation(_focusAlignment * Vector3.up, _focus.forward) * _focusAlignment;

        UpdateFocusPoint();
        if (ManualRotation())
        {
            ConstraintAngles();
            _orbitRotation = Quaternion.Euler(_orbitAngles);
        }
        Quaternion lookRotation = _focusAlignment * _orbitRotation;
        
        var lookDirection = lookRotation * Vector3.forward;
        var lookPosition = _focusPoint - lookDirection * _distance;

        Vector3 rectOffset    = lookDirection * _camera.nearClipPlane;
        Vector3 rectPosition  = lookPosition + rectOffset;
        Vector3 castFrom      = _focus.position;
        Vector3 castLine      = rectPosition - castFrom;
        float   castDistance  = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(
            castFrom, _cameraHalfExtends, castDirection, out RaycastHit hit,
            lookRotation, castDistance, _obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    private void UpdateFocusPoint()
    {
        var targetPoint = _focus.position;

        if (_focusRadius > 0.0f)
        {
            var distance = Vector3.Distance(targetPoint, _focusPoint);
            var t        = 1.0f;
            
            if (distance > _bigEpsilon && _focusCentering > 0.0f)
                t = Mathf.Pow(1.0f - _focusCentering, Time.unscaledDeltaTime);
            
            if (distance > _focusRadius)
                t = Mathf.Min(t, _focusRadius / distance);
            
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

        if (Mathf.Abs(input.x) > _epsilon || 
            Mathf.Abs(input.y) > _epsilon)
        {
            _orbitAngles += _rotationSpeed * new Vector2(input.x, -input.y);
            return true;
        }

        return false;
    }

    private void ConstraintAngles()
    {
        _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVerticalAngle);

        if (_orbitAngles.y < 0.0f)
            _orbitAngles.y += 360.0f;
        else if (_orbitAngles.y >= 360.0f)
            _orbitAngles.y -= 360.0f;
    }

    private void OnEnable() => _playerInputActions.Camera.Enable();

    private void OnDisable() => _playerInputActions.Camera.Disable();

    private void OnValidate()
    {
        if (_maxVerticalAngle < _minVerticalAngle)
            _maxVerticalAngle = _minVerticalAngle;
    }
}

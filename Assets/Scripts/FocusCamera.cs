using UnityEngine;

// Simple camera script.
// If attached to a camera, the camera will try to look at the 
// object provided when calling the SetFocusPoint method.
// It's useful for debugging whil training the AIs.
public class FocusCamera : MonoBehaviour
{
    private const float   _minMovementBias  = 0.01f;

    [SerializeField, Min(1.0f)]
    private       float   _distanceToTarget = 20.0f;

    [SerializeField, Min(0.0f)]
    private       float   _movementSpeed    = 50.0f;

    private       Vector3 _focusPoint;
    private       Vector3 _focusPointDirection;

    public void SetFocusPoint(Vector3 focusPoint)
    {
        if (Vector3.Distance(focusPoint, _focusPoint) >= _minMovementBias)
            _focusPointDirection = (focusPoint - _focusPoint).normalized;

        _focusPoint = focusPoint;
    }

    private void Start()
    {
        _focusPointDirection = transform.forward;
        _focusPoint          = transform.position + _focusPointDirection;
    }

    private void Update()
    {
        Vector3 desiredPosition = _focusPoint - _focusPointDirection * _distanceToTarget;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * _movementSpeed);

        transform.LookAt(_focusPoint, transform.up);
    }
}

using UnityEngine;

public static class CustomPhysics
{
    private const float _dragMultiplier  = 0.5f;
    private const float _resistanceForce = 1.0f;

    public static Vector3 GetDrag(Vector3 velocity) => -_dragMultiplier * _resistanceForce * velocity * velocity.magnitude;
}

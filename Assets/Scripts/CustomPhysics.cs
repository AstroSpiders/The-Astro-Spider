using UnityEngine;

public static class CustomPhysics
{
    private const float _dragMultiplier  = 0.1f;
    private const float _resistanceForce = 1.0f;

    public static Vector3 QuaternionToAngularVelocity(Quaternion rotation)
    {
        rotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f)
            angleInDegrees -= 360f;

        return angleInDegrees * rotationAxis.normalized * Mathf.Deg2Rad;
    }

    public static Vector3 GetDrag(Vector3 velocity) => -_dragMultiplier * _resistanceForce * velocity * velocity.magnitude;
}

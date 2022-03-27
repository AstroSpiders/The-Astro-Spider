using UnityEngine;

// Based on https://catlikecoding.com/unity/tutorials/movement/complex-gravity/
// This script is meant to be attached on the
// Planet objects.
public class GravitySphere : GravitySource
{
    [SerializeField]
    private float _gravity = 9.81f;

    [SerializeField, Min(0.0f)]
    // The radius in which the gravity has maximum effect.
    // After this radius, the gravity force starts to fade away.
    public float  OuterRadius        = 10.0f, 
    // The radius after the gravity force is completeley faded.
                  OuterFalloffRadius = 15.0f;

    private float _outerFalloffFactor;

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 vector = transform.position - position; // The object is pulled towards the center of the sphere

        float distance = vector.magnitude;

        if (distance > OuterFalloffRadius)
            return Vector3.zero;

        float g = _gravity / distance; // we divide by distance so we don't have to normalize the vector
                                       // in the final calculation (minor optimization).

        if (distance > OuterRadius)
        {
            // this formula represents a linear interpolation.
            // when the distance to the object is <= _outerRadius, then the gravity force for this sphere has a maximum value
            // when the distance is >= _outerFalloffRadius, the gravity force for this sphere is equal to 0.
            // when the _outerRadis < distance < _outerFalloffRadius, the gravity force is somewhere between the maximum value and 0.

            g *= 1.0f - (distance - OuterRadius) * _outerFalloffFactor;
        }

        return g * vector;
    }

    // Method useful for debugging purposes.
    // It draws the outer radius and falloff radius (only in the editor).
    // The outer radius is yellow, while the fallff radius is cyan.
    private void OnDrawGizmos()
    {
        Vector3 p = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, OuterRadius);

        if (OuterFalloffRadius > OuterRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, OuterFalloffRadius);
        }
    }

    private void OnValidate()
    {
        OuterFalloffRadius = Mathf.Max(OuterFalloffRadius, OuterRadius);
        _outerFalloffFactor = 1.0f / (OuterFalloffRadius - OuterRadius);
    }
}

using UnityEngine;

// Based on https://catlikecoding.com/unity/tutorials/movement/complex-gravity/
// This script is meant to be attached on the
// Planet objects.
public class GravitySphere : GravitySource
{
    [SerializeField]
    private float _gravity;

    [SerializeField, Min(0.0f)]
    // The radius in which the gravity has maximum effect.
    // After this radius, the gravity force starts to fade away.
    private float _outerRadius        = 10.0f, 
    // The radius after the gravity force is completeley faded.
                  _outerFalloffRadius = 15.0f;

    private float _outerFalloffFactor;

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 vector = transform.position - position; // The object is pulled towards the center of the sphere

        float distance = vector.magnitude;

        if (distance > _outerFalloffRadius)
            return Vector3.zero;

        float g = _gravity / distance; // we divide by distance so we don't have to normalize the vector
                                       // in the final calculation (minor optimization).

        if (distance > _outerRadius)
        {
            // this formula represents a linear interpolation.
            // when the distance to the object is <= _outerRadis, then the gravity force for this sphere has a maximum value
            // when the distance is >= _outerFalloffRadius, the gravity force for this sphere is equal to 0.
            // when the _outerRadis < distance < _outerFalloffRadius, the gravity force is somewhere between the maximum value and 0.

            g *= 1.0f - (distance - _outerRadius) * _outerFalloffFactor;
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
        Gizmos.DrawWireSphere(p, _outerRadius);

        if (_outerFalloffRadius > _outerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, _outerFalloffRadius);
        }
    }

    private void OnValidate()
    {
        _outerFalloffRadius = Mathf.Max(_outerFalloffRadius, _outerRadius);
        _outerFalloffFactor = 1.0f / (_outerFalloffRadius - _outerRadius);
    }
}

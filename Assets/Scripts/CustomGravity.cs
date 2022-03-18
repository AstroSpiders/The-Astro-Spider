using System.Collections.Generic;
using UnityEngine;

// Based on https://catlikecoding.com/unity/tutorials/movement/custom-gravity/
// Use this when querying the gravity force at a given position.
// Do not use Physics.gravity.
public static class CustomGravity
{
    // List of custom gravity objects. 
    private static List<GravitySource> _sources = new List<GravitySource>();

    // Returns the gravity force at a given point in space.
    // TODO: Improve the calculation by using Runge-Kutta Order 4: https://www.youtube.com/watch?v=hGCP6I2WisM&list=PLW3Zl3wyJwWOpdhYedlD-yCB7WQoHf-My&index=111
    // This integration method was presented at the course.
    public static Vector3 GetGravity(Vector3 position)
    {
        Vector3 result = Vector3.zero;

        foreach (var source in _sources)
            result += source.GetGravity(position);

        return result;
    }

    // Returns the gravity force at a given point in space, and also
    // the "up" axis vector for that point.
    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 gravity = GetGravity(position);

        upAxis = -gravity.normalized;

        return gravity;
    }

    // Returns the "up" axis vector for a given point.
    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 gravity = GetGravity(position);

        return -gravity.normalized;
    }

    public static void Register(GravitySource source)
    {
        Debug.Assert(!_sources.Contains(source),
                     "Duplicate registration of gravity source!", 
                     source);

        _sources.Add(source);
    }

    public static void Unregister(GravitySource source)
    {
        Debug.Assert(_sources.Contains(source),
                     "Unregistration of unknown gravity source!",
                     source);

        _sources.Remove(source);
    }
}

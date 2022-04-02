using UnityEngine;

// Based on this tutorial: https://catlikecoding.com/unity/tutorials/movement/custom-gravity/ 
// section 3.1
[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    private const float     _bias          = 0.0001f;
    private const float     _maxFloatDelay = 1.0f;

    [SerializeField]
    private       bool      _floatToSleep  = true;

    private       float     _floatDelay    = 0.0f;
    private       Rigidbody _body;

    private void Awake()
    {
        _body            = GetComponent<Rigidbody>();
        _body.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (_floatToSleep)
        {
            if (_body.IsSleeping())
                return;

            if (_body.velocity.sqrMagnitude < _bias)
            {
                _floatDelay += Time.deltaTime;
                if (_floatDelay >= _maxFloatDelay)
                    return;
            }
            else
            {
                _floatDelay = 0.0f;
            }
        }

        Vector3 velocity      = _body.velocity;

        // Runge-Kutta Order 4: https://www.youtube.com/watch?v=hGCP6I2WisM&list=PLW3Zl3wyJwWOpdhYedlD-yCB7WQoHf-My&index=111
        Vector3 k1            = VelocityField(transform.position,                              velocity, Time.deltaTime);
        Vector3 k2            = VelocityField(transform.position + Time.deltaTime / 2.0f * k1, velocity, Time.deltaTime);
        Vector3 k3            = VelocityField(transform.position + Time.deltaTime / 2.0f * k2, velocity, Time.deltaTime);
        Vector3 k4            = VelocityField(transform.position + Time.deltaTime        * k3, velocity, Time.deltaTime);

               _body.velocity = (k1 + 2.0f * k2 + 2.0f * k3 + k4) / 6.0f;
    }

    private Vector3 VelocityField(Vector3 position, Vector3 initialVelocity, float deltaTime)
    {
        Vector3 newVelocity  = initialVelocity;
                newVelocity += CustomGravity.GetGravity(position) * deltaTime;

        return newVelocity;
    }
}

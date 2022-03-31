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

        _body.AddForce(CustomGravity.GetGravity(_body.position), ForceMode.Acceleration);
    }
}

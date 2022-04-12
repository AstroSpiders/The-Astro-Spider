using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RocketState))]
public class RocketMovement : MonoBehaviour
{
    public enum ThrusterTypes
    {
        Main,

        ExhaustLeft,
        ExhaustFront,
        ExhaustBack,
        ExhaustRight,

        Count
    }

    public class Thruster
    {
        public float      Acceleration    { get; set; } = 0.0f;
        public float      FuelConsumption { get; set; } = 1.0f;

        // We keep a reference to the GameObject so we can later use it for animation,
        // or improve the GetThrusterRotation method.
        public GameObject GameObject      { get; set; } = null;
    }

    public Thruster[] Thrusters { get; private set; } = new Thruster[(int)ThrusterTypes.Count];

    [SerializeField] private GameObject _mainThruster;
    [SerializeField] private GameObject _exhaust00;
    [SerializeField] private GameObject _exhaust01;
    [SerializeField] private GameObject _exhaust02;
    [SerializeField] private GameObject _exhaust03;

    [SerializeField] private GameObject _mainThrusterParticle;
    [SerializeField] private GameObject _exhaust00Particle;
    [SerializeField] private GameObject _exhaust01Particle;
    [SerializeField] private GameObject _exhaust02Particle;
    [SerializeField] private GameObject _exhaust03Particle;

    // The maximum impact an accelerated thruster can have on the desired rotation for that frame
    [SerializeField, Range(0.0f, 90.0f)] private float _maxThrusterAngle = 90.0f;

    [SerializeField, Range(1.0f, 10.0f)] private float _angularVelocityUpdateSpeed = 10.0f;

    [SerializeField, Range(0.0f, 100.0f)] private float _accelerationMultiplier = 20.0f;

    [SerializeField, Range(0.0f, 100.0f)] private float _fuelCapacity = 100.0f;

    [SerializeField, Range(0.0f, 100.0f)] private float _mainThrusterFuelConsumption = 1.0f;

    [SerializeField, Range(0.0f, 100.0f)] private float _sideThrusterFuelConsumption = 0.5f;
    
    
    private Rigidbody   _body;
    private RocketState _state;

    private float       _currentFuelLevel;

    public void ApplyAcceleration(float acceleration,
        ThrusterTypes thruster)
    {
        if (thruster == ThrusterTypes.Count)
            return;

        Thrusters[(int) thruster].Acceleration = acceleration;
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _state = GetComponent<RocketState>();

        InitializeThrustersList();

        _currentFuelLevel = _fuelCapacity;
    }

    private void FixedUpdate()
    {
        if (_state.Dead)
            ResetThrusters();

        UpdateVelocity();
        UpdateAngularVelocity();

        ResetThrusters();
    }

    private void InitializeThrustersList()
    {
        for (int i = 0; i < Thrusters.Length; i++)
        {
            Thrusters[i] = new Thruster()
            {
                Acceleration = 0.0f,
                GameObject = null,
                FuelConsumption = _sideThrusterFuelConsumption
            };

            switch ((ThrusterTypes) i)
            {
                case ThrusterTypes.Main:
                    Thrusters[i].GameObject = _mainThruster;
                    Thrusters[i].FuelConsumption = _mainThrusterFuelConsumption;
                    break;
                case ThrusterTypes.ExhaustLeft:
                    Thrusters[i].GameObject = _exhaust00;
                    break;
                case ThrusterTypes.ExhaustFront:
                    Thrusters[i].GameObject = _exhaust01;
                    break;
                case ThrusterTypes.ExhaustBack:
                    Thrusters[i].GameObject = _exhaust02;
                    break;
                case ThrusterTypes.ExhaustRight:
                    Thrusters[i].GameObject = _exhaust03;
                    break;
            }
        }
    }

    private void UpdateAngularVelocity()
    {
        // We calculate the desired orientation based on the
        // acceleration of the thrusters.
        Quaternion desiredRotation = transform.rotation;

        for (int i = 0; i < Thrusters.Length; i++)
        {
            ThrusterTypes thruster = (ThrusterTypes) i;
            Quaternion thrusterRotation = GetThrusterRotation(thruster);
            desiredRotation *= Quaternion.Slerp(Quaternion.identity, thrusterRotation, Thrusters[i].Acceleration);
        }

        Quaternion toDesiredRotation = desiredRotation * Quaternion.Inverse(transform.rotation);

        Vector3 angularVelocity = _body.angularVelocity;
        Vector3 desiredAngularVelocity = QuaternionToAngularVelocity(toDesiredRotation);

        // We move towards the target angular velocity (desiredAngularVelocity).
        angularVelocity = Vector3.MoveTowards(angularVelocity, desiredAngularVelocity,
            Time.deltaTime * _angularVelocityUpdateSpeed);

        // We update the angular velocity of the rigidbody.
        _body.angularVelocity = angularVelocity;
        
    }

    private void UpdateVelocity()
    {
        Vector3 velocity = _body.velocity;

        Vector3 thrustersDirection = transform.forward.normalized;
        float thrustersAcceleration = 0.0f;

        // See how much impact each of the truster have on the overall velocity. 
        for (int i = 0; i < Thrusters.Length; i++)
        {
            ThrusterTypes thruster = (ThrusterTypes) i;
            float acceleration = Thrusters[i].Acceleration;
            float thrusterAngle = GetThrusterAngle(thruster);

            float fuelConsumed = acceleration * Thrusters[i].FuelConsumption * Time.deltaTime;

            if (fuelConsumed < _currentFuelLevel)
            {
                _currentFuelLevel -= fuelConsumed;
            }
            else
            {
                // If we don't have enough fuel, then we can't accelerate the thruster with
                // the desired acceleration, so we have to trim it down, or even make the acceleration
                // equal to 0 if we have no fuel.
                fuelConsumed = Mathf.Min(fuelConsumed, _currentFuelLevel);
                acceleration = fuelConsumed / Thrusters[i].FuelConsumption * Time.deltaTime;
                _currentFuelLevel = 0.0f;
            }

            float dot = Mathf.Abs(Mathf.Cos(thrusterAngle));

            thrustersAcceleration += dot * acceleration * _accelerationMultiplier;
        }

        //Runge - Kutta Order 4: https://www.youtube.com/watch?v=hGCP6I2WisM&list=PLW3Zl3wyJwWOpdhYedlD-yCB7WQoHf-My&index=111
        Vector3 k1 = VelocityField(transform.position, velocity, thrustersDirection, thrustersAcceleration,
            Time.deltaTime);
        Vector3 k2 = VelocityField(transform.position + Time.deltaTime / 2.0f * k1, velocity, thrustersDirection,
            thrustersAcceleration, Time.deltaTime);
        Vector3 k3 = VelocityField(transform.position + Time.deltaTime / 2.0f * k2, velocity, thrustersDirection,
            thrustersAcceleration, Time.deltaTime);
        Vector3 k4 = VelocityField(transform.position + Time.deltaTime * k3, velocity, thrustersDirection,
            thrustersAcceleration, Time.deltaTime);

        _body.velocity = (k1 + 2.0f * k2 + 2.0f * k3 + k4) / 6.0f;
    }

    private float GetThrusterAngle(ThrusterTypes thruster)
    {
        if (thruster == ThrusterTypes.Count || thruster == ThrusterTypes.Main)
            return 0.0f;

        float acceleration = Thrusters[(int) thruster].Acceleration;
        float angle = acceleration * _maxThrusterAngle;

        return angle;
    }

    // Returns a quaternion which represents how much the rocket must be rotated
    // based on how accelereted the thruster is.
    private Quaternion GetThrusterRotation(ThrusterTypes thruster)
    {
        if (thruster == ThrusterTypes.Count)
            return Quaternion.identity;

        float angle = GetThrusterAngle(thruster);

        // these rotations are hard-coded
        switch (thruster)
        {
            case ThrusterTypes.Main:
                return Quaternion.identity;
            case ThrusterTypes.ExhaustLeft:
                return Quaternion.Euler(0.0f, angle, 0.0f);
            case ThrusterTypes.ExhaustFront:
                return Quaternion.Euler(angle, 0.0f, 0.0f);
            case ThrusterTypes.ExhaustBack:
                return Quaternion.Euler(-angle, 0.0f, 0.0f);
            case ThrusterTypes.ExhaustRight:
                return Quaternion.Euler(0.0f, -angle, 0.0f);
        }

        return Quaternion.identity;
    }

    private Vector3 QuaternionToAngularVelocity(Quaternion rotation)
    {
        rotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f)
            angleInDegrees -= 360f;

        return angleInDegrees * rotationAxis.normalized * Mathf.Deg2Rad;
    }

    private void ResetThrusters()
    {
        // Reset the acceleration to 0 for each thruster.
        for (int i = 0; i < Thrusters.Length; i++)
            Thrusters[i].Acceleration = 0.0f;
    }

    private Vector3 VelocityField(Vector3 position, Vector3 initialVelocity, Vector3 thrustersDirection,
        float thrustersAcceleration, float deltaTime)
    {
        Vector3 newVelocity = initialVelocity;
        newVelocity += thrustersDirection * thrustersAcceleration * deltaTime;

        Vector3 gravity = CustomGravity.GetGravity(position);

        newVelocity += gravity * deltaTime;

        newVelocity += CustomPhysics.GetDrag(newVelocity) * deltaTime;

        return newVelocity;
    }
}
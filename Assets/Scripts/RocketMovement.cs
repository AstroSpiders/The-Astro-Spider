using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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

    private class Thruster
    {
        public float      Acceleration    { get; set; } = 0.0f;
        public float      FuelConsumption { get; set; } = 1.0f;

        // We keep a reference to the GameObject so we can later use it for animation,
        // or improve the GetThrusterRotation method.
        public GameObject GameObject      { get; set; } = null;
    }

    [SerializeField]
    private GameObject      _mainThruster;
    [SerializeField]        
    private GameObject      _exhaust00;
    [SerializeField]        
    private GameObject      _exhaust01;
    [SerializeField]        
    private GameObject      _exhaust02;
    [SerializeField]        
    private GameObject      _exhaust03;

    // The maximum impact an accelerated thruster can have on the desired rotation for that frame
    [SerializeField, Range(0.0f, 45.0f)]
    private float           _maxThrusterAngle            = 30.0f;

    // How fast the rocket can turn, measured in angles per second.
    [SerializeField, Range(0.0f, 90.0f)]
    private float           _maxAngle                    = 45.0f;
    
    [SerializeField, Range(0.0f, 100.0f)]
    private float           _accelerationMultiplier      = 10.0f;

    [SerializeField, Range(0.0f, 100.0f)]
    private float           _fuelCapacity                = 100.0f;

    [SerializeField, Range(0.0f, 100.0f)]
    private float           _mainThrusterFuelConsumption = 1.0f;

    [SerializeField, Range(0.0f, 100.0f)]
    private float           _sideThrusterFuelConsumption = 0.5f;

    private Thruster[]      _thrusters                   = new Thruster[(int)ThrusterTypes.Count];

    private Rigidbody       _body;

    private float           _currentFuelLevel;

    public void ApplyAcceleration(float         acceleration,
                                  ThrusterTypes thruster)
    {
        if (thruster == ThrusterTypes.Count)
            return;

        _thrusters[(int)thruster].Acceleration = acceleration;
    }

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
        InitializeThrustersList();
        _currentFuelLevel = _fuelCapacity;
    }

    void FixedUpdate()
    {
        Vector3 gravity               = CustomGravity.GetGravity(transform.position);
                                     
        Vector3 velocity              = _body.velocity;
                                     
        Vector3 thrustersDirection    = transform.forward.normalized;
        float   thrustersAcceleration = 0.0f;

        float   deltaTime             = Time.deltaTime;

        // See how much impact each of the truster have on the overall velocity. 
        for (int i = 0; i < _thrusters.Length; i++)
        {
            Vector3       forward           = transform.forward;
            ThrusterTypes thruster          = (ThrusterTypes)i;
            float         acceleration      = _thrusters[i].Acceleration;
            Quaternion    thrusterRotation  = GetThrusterRotation(thruster);
            Vector3       thrusterDirection = thrusterRotation * forward;

            float         fuelConsumed      = acceleration * _thrusters[i].FuelConsumption * deltaTime;

            if (fuelConsumed < _currentFuelLevel)
            {
                _currentFuelLevel -= fuelConsumed;
            }
            else
            {
                // If we don't have enough fuel, then we can't accelerate the thruster with
                // the desired acceleration, so we have to trim it down, or even make the acceleration
                // equal to 0 if we have no fuel.
                fuelConsumed      = Mathf.Min(fuelConsumed, _currentFuelLevel);
                acceleration      = fuelConsumed / _thrusters[i].FuelConsumption * deltaTime;
                _currentFuelLevel = 0.0f;
            }


            // We calculate the dot product between our forward direction and our desired direction
            // for the current trusters, so that the side trusters have less impact on the velocity than the main thruster.
            float         dot               = Vector3.Dot(thrusterDirection, forward);

            thrustersAcceleration          += dot * acceleration * Time.deltaTime * _accelerationMultiplier;
        }

        Vector3 newVelocity   = velocity;
                             
        newVelocity          += thrustersDirection * thrustersAcceleration;
        newVelocity          += gravity * Time.deltaTime;

        _body.velocity        = newVelocity;
        _body.angularVelocity = Vector3.zero;

        // Reset the acceleration to 0 for each thruster.
        for (int i = 0; i < _thrusters.Length; i++)
            _thrusters[i].Acceleration = 0.0f;
    }

    void InitializeThrustersList()
    {
        for (int i = 0; i < _thrusters.Length; i++)
        {
            _thrusters[i] = new Thruster()
            {
                Acceleration    = 0.0f,
                GameObject      = null,
                FuelConsumption = _sideThrusterFuelConsumption
            };

            switch ((ThrusterTypes)i)
            {
                case ThrusterTypes.Main:
                    _thrusters[i].GameObject      = _mainThruster;
                    _thrusters[i].FuelConsumption = _mainThrusterFuelConsumption;
                    break;
                case ThrusterTypes.ExhaustLeft:
                    _thrusters[i].GameObject      = _exhaust00;
                    break;
                case ThrusterTypes.ExhaustFront:
                    _thrusters[i].GameObject      = _exhaust01;
                    break;
                case ThrusterTypes.ExhaustBack:
                    _thrusters[i].GameObject      = _exhaust02;
                    break;
                case ThrusterTypes.ExhaustRight:
                    _thrusters[i].GameObject      = _exhaust03;
                    break;
            }
        }
    }

    public void UpdateOrientation()
    {
        // We calculate the desired orientation based on the
        // acceleration of the thrusters.
        Quaternion desiredRotation = transform.rotation;

        for (int i = 0; i < _thrusters.Length; i++)
        {
            ThrusterTypes thruster         = (ThrusterTypes)i;
            Quaternion    thrusterRotation = GetThrusterRotation(thruster);
            desiredRotation               *= thrusterRotation;
        }

        // We rotate our rocket towards that acceleration.
        // We want a smooth transition, that's why we are using the
        // Quaternion.RotateTowards method
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, Time.deltaTime * _maxAngle);
    }

    // Returns a quaternion which represents how much the rocket must be rotated
    // based on how accelereted the thruster is.
    Quaternion GetThrusterRotation(ThrusterTypes thruster)
    {
        if (thruster == ThrusterTypes.Count)
            return Quaternion.identity;

        float acceleration = _thrusters[(int)thruster].Acceleration;
        float angle        = acceleration * _maxThrusterAngle;

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
}

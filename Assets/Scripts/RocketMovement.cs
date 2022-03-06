using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RocketMovement : MonoBehaviour
{
    public enum ThrusterTypes
    {
        Main,

        Exhaust00,
        Exhaust01,
        Exhaust02,
        Exhaust03,

        Count
    };

    private class Thruster
    {
        public float      acceleration = 0.0f;
        // We keep a reference to the GameObject so we can later use it for animation,
        // or improve the GetThrusterRotation method.
        public GameObject gameObject   = null;
    }

    [SerializeField]
    GameObject              _mainThruster;
    [SerializeField]        
    GameObject              _exhaust00;
    [SerializeField]        
    GameObject              _exhaust01;
    [SerializeField]        
    GameObject              _exhaust02;
    [SerializeField]        
    GameObject              _exhaust03;

    // The maximum impact an accelerated thruster can have on the desired rotation for that frame
    [SerializeField, Range(0.0f, 45.0f)]
    float                   _maxThrusterAngle       = 30.0f;

    // How fast the rocket can turn, measured in angles per second.
    [SerializeField, Range(0.0f, 90.0f)]
    float                   _maxAngle               = 45.0f;
    
    [SerializeField, Range(0.0f, 100.0f)]
    float                   _accelerationMultiplier = 10.0f;

    private Thruster[]      _thrusters              = new Thruster[(int)ThrusterTypes.Count];

    private Rigidbody       _body;

    public void ApplyAcceleration(float         acceleration,
                                  ThrusterTypes thruster)
    {
        if (thruster == ThrusterTypes.Count)
            return;

        _thrusters[(int)thruster].acceleration = acceleration;
    }

    void Awake()
    {
        _body = GetComponent<Rigidbody>();
        InitializeThrustersList();
    }

    void Update()
    {
        // for testing purposes only.
        ApplyAcceleration(1.0f, ThrusterTypes.Exhaust00);

        UpdateOrientation();
    }

    void FixedUpdate()
    {
        Vector3 velocity = _body.velocity;
        float   speed    = velocity.magnitude;

        // See how much impact each of the truster have on the overall velocity. 
        for (int i = 0; i < _thrusters.Length; i++)
        {
            ThrusterTypes thruster          = (ThrusterTypes)i;
            float         acceleration      = _thrusters[i].acceleration;
            Quaternion    thrusterRotation  = GetThrusterRotation(thruster);
            Vector3       thrusterDirection = thrusterRotation * transform.forward;

            // We calculate the dot product between our forward direction and our desired direction
            // for the current trusters, so that the side trusters have less impact on the velocity than the main thruster.
            float         dot               = Vector3.Dot(thrusterDirection, transform.forward);

            speed                          += dot * acceleration * Time.deltaTime * _accelerationMultiplier;
        }

        // The rocket will always move along the forward direction, so we don't
        // care about the previous velocity vector.
        _body.velocity = transform.forward.normalized * speed;

        // Reset the acceleration to 0 for each thruster.
        for (int i = 0; i < _thrusters.Length; i++)
            _thrusters[i].acceleration = 0.0f;
    }

    void InitializeThrustersList()
    {
        for (int i = 0; i < _thrusters.Length; i++)
        {
            _thrusters[i] = new Thruster()
            {
                acceleration = 0.0f,
                gameObject = null
            };

            switch ((ThrusterTypes)i)
            {
                case ThrusterTypes.Main:
                    _thrusters[i].gameObject = _mainThruster;
                    break;
                case ThrusterTypes.Exhaust00:
                    _thrusters[i].gameObject = _exhaust00;
                    break;
                case ThrusterTypes.Exhaust01:
                    _thrusters[i].gameObject = _exhaust01;
                    break;
                case ThrusterTypes.Exhaust02:
                    _thrusters[i].gameObject = _exhaust02;
                    break;
                case ThrusterTypes.Exhaust03:
                    _thrusters[i].gameObject = _exhaust03;
                    break;
                default:
                    break;
            }
        }
    }

    void UpdateOrientation()
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

        float acceleration = _thrusters[(int)thruster].acceleration;
        float angle        = acceleration * _maxThrusterAngle;

        // these rotations are hard-coded, however
        // considering that the rocket will be contrained to
        // never spin around its' center axis, this approach
        // should be good enough.
        switch (thruster)
        {
            case ThrusterTypes.Main:
                return Quaternion.identity;
            case ThrusterTypes.Exhaust00:
                return Quaternion.Euler(0.0f, angle, 0.0f);
            case ThrusterTypes.Exhaust01:
                return Quaternion.Euler(angle, 0.0f, 0.0f);
            case ThrusterTypes.Exhaust02:
                return Quaternion.Euler(-angle, 0.0f, 0.0f);
            case ThrusterTypes.Exhaust03:
                return Quaternion.Euler(0.0f, -angle, 0.0f);
        }

        return Quaternion.identity;
    }
}

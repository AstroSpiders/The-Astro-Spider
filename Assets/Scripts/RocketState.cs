using UnityEngine;

[RequireComponent(typeof(RocketSensors))]
[RequireComponent(typeof(Rigidbody))]
// Keeps the state of the rocket object. It detects when it has
// successfully landed, or when the rocket hit an asteroid and died.
// It also keeps track of the planet that rocket needs to land on.
public class RocketState : MonoBehaviour
{
    public WorldGenerator WorldGenerator             = null;
    public  bool          Dead { get; private set; } = false;
    
    [SerializeField]
    private float         _landingDotThreshold       = 0.9f;

    [SerializeField]
    private float         _maxLandingImpact          = 1.0f;

    private RocketSensors _sensors                   = null;
    private Rigidbody     _body                      = null;
    
    private int           _currentPlanetIndex        = 0;

    private void Start()
    {
        if (WorldGenerator is null)
        {
            Debug.Log("Please provide an WorldGenerator object to the RocketState script.");
            return;
        }

        _sensors = GetComponent<RocketSensors>();
        _body    = GetComponent<Rigidbody>();

        if (_currentPlanetIndex < WorldGenerator.Planets.Length)
        { 
            _sensors.TargetPlanet = WorldGenerator.Planets[_currentPlanetIndex].transform;
            _sensors.enabled      = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var otherObject = collision.gameObject;
        if (otherObject.CompareTag("Planet"))
        {
            if (WorldGenerator.Planets[_currentPlanetIndex].gameObject != otherObject)
                return;

            var planetToObject = (transform.position - otherObject.transform.position).normalized;
            var forward        = transform.forward.normalized;
            var dot            = Vector3.Dot(planetToObject, forward);


            if (dot > _landingDotThreshold)
            {
                float impact = _body.velocity.magnitude;
                if (impact <= _maxLandingImpact)
                    ProcessLanding();
            }
        }
        else
        {
            if (otherObject.CompareTag("Obstacle"))
                Dead = true;
        }
    }

    private void ProcessLanding()
    {
        if (_currentPlanetIndex + 1 < WorldGenerator.Planets.Length)
        {
            Debug.Log("Landed on planet " + _currentPlanetIndex.ToString());
            _currentPlanetIndex++;
            _sensors.TargetPlanet = WorldGenerator.Planets[_currentPlanetIndex].transform;
        }
        else
        {
            Debug.Log("You won!!!");
        }
    }
}

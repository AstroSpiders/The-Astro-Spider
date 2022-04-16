using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RocketSensors))]
[RequireComponent(typeof(Rigidbody))]
// Keeps the state of the rocket object. It detects when it has
// successfully landed, or when the rocket hit an asteroid and died.
// It also keeps track of the planet that rocket needs to land on.
public class RocketState : MonoBehaviour
{
    public class StatsPerPlanet
    {
        public float   DistanceNavigated        { get; set; } = 0.0f;
        public float   MaxDistanceNavigated     { get; set; } = 0.0f;
        public float   DistanceFarFromPlanet    { get; set; } = 0.0f;
        public float   MaxDistanceFarFromPlanet { get; set; } = 0.0f;
        public Vector3 StartingPosition         { get; set; }
        public Vector3 TargetPosition           { get; set; }
        public bool    Landed                   { get; set; } = false;
        public float   FuelConsumed             { get; set; } = 0.0f;
        public float   InitialFuelLevel         { get; set; }
        public float   LandingDot               { get; set; } = 0.0f;
        public float   LandingImpact            { get; set; } = 1.0f;
        public float   IdealLandingImpact       { get; set; } = 1.0f;
    }

    public WorldGenerator       WorldGenerator             = null;
    public List<StatsPerPlanet> PlanetsStats               = new List<StatsPerPlanet>();

    public        float         CurrentFuelLevel { get; set; }
    public        bool          HasFuel          { get => CurrentFuelLevel > _bias; }
    public        bool          Dead             { get; private set; } = false;
    public        bool          Won                                    = false;
                                
    private const float         _bias                      = 0.01f;

    [SerializeField]            
    private       float         _landingDotThreshold       = 0.9f;
                                
    [SerializeField]            
    private       float         _deadDotThreshold          = 0.5f;
                                
    [SerializeField]            
    private       float         _maxLandingImpact          = 1.0f;

    [SerializeField]
    private       float         _maxLandingImpactTraining  = 10.0f;

    [SerializeField, Range(0.0f, 100.0f)]
    private       float         _fuelCapacity              = 100.0f;

    private       RocketSensors _sensors                   = null;
    private       Rigidbody     _body                      = null;
                                
    private       int           _currentPlanetIndex        = 0;

    private void Start()
    {
        if (WorldGenerator is null)
        {
            Debug.Log("Please provide an WorldGenerator object to the RocketState script.");
            return;
        }


        CurrentFuelLevel = _fuelCapacity;

        _sensors         = GetComponent<RocketSensors>();
        _body            = GetComponent<Rigidbody>();

        if (_currentPlanetIndex < WorldGenerator.Planets.Length)
        { 
            _sensors.TargetPlanet = WorldGenerator.Planets[_currentPlanetIndex].transform;
            _sensors.enabled      = true;
        }

        AddNewPlanetStats();
    }

    private void Update()
    {
        if (!Won)
            UpdateLatestStats();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var otherObject = collision.gameObject;
        if (otherObject.CompareTag("Planet"))
        {
            var   planetToObject = (transform.position - otherObject.transform.position).normalized;
            var   forward        = transform.forward.normalized;
            var   dot            = Vector3.Dot(planetToObject, forward);
            float impact         = _body.velocity.magnitude;

            bool isTargetPlanet = WorldGenerator.Planets[_currentPlanetIndex].gameObject == otherObject;

            if (!PlanetsStats[PlanetsStats.Count - 1].Landed)
                if (isTargetPlanet)
                    FinalizeLandingStats(dot, impact, false);

            if (dot >= _landingDotThreshold)
            {
                if (impact <= _maxLandingImpact)
                {
                    if (isTargetPlanet)
                        ProcessLanding(dot, impact);
                }
                else
                {
                    Dead = true;
                }
            }
            else if (dot < _deadDotThreshold)
            {
                Dead = true;
            }
        }
        else
        {
            if (otherObject.CompareTag("Obstacle"))
                Dead = true;
        }
    }

    private void ProcessLanding(float landingDot, float landingImpact)
    {
        if (!Won)
        {
            UpdateLatestStats();
            FinalizeLandingStats(landingDot, landingImpact, true);
        }

        if (_currentPlanetIndex + 1 < WorldGenerator.Planets.Length)
        {
            Debug.Log("Landed on planet " + _currentPlanetIndex.ToString());
            _currentPlanetIndex++;
            _sensors.TargetPlanet = WorldGenerator.Planets[_currentPlanetIndex].transform;
            AddNewPlanetStats();
        }
        else
        {
            Won = true;
            Debug.Log("You won!!!");
        }
    }

    private void AddNewPlanetStats()
    {
        var stats = new StatsPerPlanet()
        {
            StartingPosition         = transform.position,
            TargetPosition           = WorldGenerator.Planets[_currentPlanetIndex].transform.position,
            Landed                   = false,
            InitialFuelLevel         = CurrentFuelLevel,
            FuelConsumed             = 0.0f,
            LandingDot               = 0.0f,
            LandingImpact            = 1.0f,
            IdealLandingImpact       = 1.0f,
            DistanceNavigated        = 0.0f,
            MaxDistanceNavigated     = 0.0f,
            DistanceFarFromPlanet    = 0.0f,
            MaxDistanceFarFromPlanet = 0.0f
        };

        PlanetsStats.Add(stats);
    }

    private void UpdateLatestStats()
    {
        var stats                          = PlanetsStats[PlanetsStats.Count - 1];
        var distanceToTarget               = (transform.position - stats.TargetPosition).magnitude;
                                           
            stats.DistanceNavigated        = Mathf.Clamp(1.0f - (distanceToTarget / (stats.TargetPosition - stats.StartingPosition).magnitude), 0.0f, 1.0f);
            stats.MaxDistanceNavigated     = Mathf.Max(stats.MaxDistanceNavigated, stats.DistanceNavigated);

            stats.DistanceFarFromPlanet    = Mathf.Clamp((distanceToTarget / (stats.TargetPosition - stats.StartingPosition).magnitude) - 1.0f, 0.0f, 1.0f);
            stats.MaxDistanceFarFromPlanet = Mathf.Max(stats.MaxDistanceFarFromPlanet, stats.DistanceFarFromPlanet);

            stats.FuelConsumed             = (stats.InitialFuelLevel - CurrentFuelLevel) / _fuelCapacity;
    }

    private void FinalizeLandingStats(float landingDot, float landingImpact, bool landed)
    {
        var stats                    = PlanetsStats[PlanetsStats.Count - 1];
            stats.Landed             = landed;
            stats.LandingDot         = landingDot * 0.5f + 0.5f;
            stats.LandingImpact      = Mathf.Clamp(landingImpact / _maxLandingImpactTraining, 0.0f, 1.0f);
            stats.IdealLandingImpact = Mathf.Clamp(_landingDotThreshold / _maxLandingImpact, 0.0f, 1.0f);
    }
}

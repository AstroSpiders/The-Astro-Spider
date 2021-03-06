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
        public float   DistanceNavigated        { get; set; }
        public float   MaxDistanceNavigated     { get; set; }
        public float   DistanceFarFromPlanet    { get; set; }
        public float   MaxDistanceFarFromPlanet { get; set; }
        public Vector3 StartingPosition         { get; set; }
        public Vector3 TargetPosition           { get; set; }
        public bool    Landed                   { get; set; }
        public float   FuelConsumed             { get; set; }
        public float   InitialFuelLevel         { get; set; }
        public float   LandingDot               { get; set; }
        public float   LandingImpact            { get; set; } = 1.0f;
        public float   IdealLandingImpact       { get; set; } = 1.0f;
    }

    public WorldGenerator       WorldGenerator;
    public List<StatsPerPlanet> PlanetsStats               = new List<StatsPerPlanet>();

    [Range(0.0f, 100.0f)]
    public float                FuelCapacity               = 100.0f;

    public float                MaxLandingImpact = 2.0f;

    public        float         CurrentFuelLevel { get; set; }
    public        bool          HasFuel          { get => CurrentFuelLevel > _bias; }
    public        bool          Dead             { get; private set; }
    public        bool          Won;
                                
    private const float         _bias                      = 0.01f;

    [SerializeField]            
    private       float         _landingDotThreshold       = 0.9f;
                                
    [SerializeField]            
    private       float         _deadDotThreshold          = 0.5f;
    
    [SerializeField]
    private       float         _maxLandingImpactTraining  = 10.0f;

    private       RocketSensors _sensors;
    private       Rigidbody     _body;
                                
    public       int           CurrentPlanetIndex { get; private set; }

    private void Start()
    {
        if (WorldGenerator is null)
        {
            Debug.Log("Please provide an WorldGenerator object to the RocketState script.");
            return;
        }


        CurrentFuelLevel = FuelCapacity;

        _sensors         = GetComponent<RocketSensors>();
        _body            = GetComponent<Rigidbody>();

        if (CurrentPlanetIndex < WorldGenerator.Planets.Length)
        { 
            _sensors.TargetPlanet = WorldGenerator.Planets[CurrentPlanetIndex].transform;
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

        if (otherObject.CompareTag("PlanetHolder"))
        {
            var   planetToObject = (transform.position - otherObject.transform.position).normalized;
            var   forward        = transform.forward.normalized;
            var   dot              = Vector3.Dot(planetToObject, forward);
            float impact               = _body.velocity.magnitude;

            bool isTargetPlanet = WorldGenerator.Planets[CurrentPlanetIndex].gameObject == otherObject;

            if (!PlanetsStats[PlanetsStats.Count - 1].Landed)
                if (isTargetPlanet)
                    FinalizeLandingStats(dot, impact, false);

            if (dot >= _landingDotThreshold)
            {
                if (impact <= MaxLandingImpact)
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
            if (otherObject.CompareTag("Obstacle") || otherObject.CompareTag("Spaceship"))
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

        if (CurrentPlanetIndex + 1 < WorldGenerator.Planets.Length)
        {
            CurrentPlanetIndex++;
            _sensors.TargetPlanet = WorldGenerator.Planets[CurrentPlanetIndex].transform;
            AddNewPlanetStats();
        }
        else
        {
            Won = true;
        }
    }

    private void AddNewPlanetStats()
    {
        var stats = new StatsPerPlanet()
        {
            StartingPosition         = transform.position,
            TargetPosition           = WorldGenerator.Planets[CurrentPlanetIndex].transform.position,
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

            stats.FuelConsumed             = (stats.InitialFuelLevel - CurrentFuelLevel) / FuelCapacity;
    }

    private void FinalizeLandingStats(float landingDot, float landingImpact, bool landed)
    {
        var stats                    = PlanetsStats[PlanetsStats.Count - 1];
            stats.Landed             = landed;
            stats.LandingDot         = landingDot * 0.5f + 0.5f;
            stats.LandingImpact      = Mathf.Clamp(landingImpact / _maxLandingImpactTraining, 0.0f, 1.0f);
            stats.IdealLandingImpact = Mathf.Clamp(_landingDotThreshold / MaxLandingImpact, 0.0f, 1.0f);
    }
}

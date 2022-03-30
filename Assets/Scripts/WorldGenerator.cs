using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    // An array containing the planets that were generated.
    public GravitySphere[] Planets { get; private set; }

    // The prefab for a single planet. Maybe in the future it's gonna be an array of prefabs
    // but for now, it's ok.
    [SerializeField]
    private GravitySphere  _planetPrefab;
    [SerializeField]
    private Asteroid       _asteroidPrefab;

    [SerializeField, Range(1, 10)]
    private int            _planetsGenerateCount                   = 5;

    [SerializeField, Range(0, 1000)]
    private int            _minAsteroidsPerPlanetCount             = 1,
                           _maxAsteroidsPerPlanetCount             = 50;

    // The minimum and maximum radius a planet can have.
    [SerializeField, Range(10.0f, 100.0f)]
    private float          _planetMinRadius                        = 10.0f,
                           _planetMaxRadius                        = 40.0f;

    [SerializeField, Range(0.5f, 9.0f)]
    private float          _asteroidMinRadius                      = 0.5f,
                           _asteroidMaxRadus                       = 2.0f;

    // The minimum and maximum distance between planets (excluding the gravity outer radiuses).
    [SerializeField, Min(0.0f)]
    private float          _minDistanceBetweenPlanets              = 0.0f,
                           _maxDistanceBetweenPlanets              = 10.0f;

    [SerializeField, Min(1.0f)]
    private float          _planetsGravityOuterRadiusMultip        = 2.0f,
                           _planetsGravityOuterFalloutRadiusMultip = 3.0f;

    [SerializeField, Range(0.0f, 1000.0f)]
    private float          _minAsteroidInitialImpulse              = 0.0f,
                           _maxAsteroidInitialImpulse              = 1000.0f;

    private List<Asteroid> _asteroids                              = new List<Asteroid>();

    // Just some checks to make sure that the minimum values are smaller than the maximum values.
    private void OnValidate()
    {
        float min                                     = Mathf.Min(_planetMinRadius,                 _planetMaxRadius);
        float max                                     = Mathf.Max(_planetMinRadius,                 _planetMaxRadius);
                                                      
              _planetMinRadius                        = min;
              _planetMaxRadius                        = max;

              min                                     = Mathf.Min(_asteroidMinRadius,               _asteroidMaxRadus);
              max                                     = Mathf.Max(_asteroidMinRadius,               _asteroidMaxRadus);

              _asteroidMinRadius                      = min;
              _asteroidMaxRadus                       = max;
                                                      
              min                                     = Mathf.Min(_minDistanceBetweenPlanets,       _maxDistanceBetweenPlanets);
              max                                     = Mathf.Max(_minDistanceBetweenPlanets,       _maxDistanceBetweenPlanets);
                                                      
              _minDistanceBetweenPlanets              = min;
              _maxDistanceBetweenPlanets              = max;
                                                      
              min                                     = Mathf.Min(_planetsGravityOuterRadiusMultip, _planetsGravityOuterFalloutRadiusMultip);
              max                                     = Mathf.Max(_planetsGravityOuterRadiusMultip, _planetsGravityOuterFalloutRadiusMultip);

              _planetsGravityOuterRadiusMultip        = min;
              _planetsGravityOuterFalloutRadiusMultip = max;

              min                                     = Math.Min(_minAsteroidInitialImpulse,        _maxAsteroidInitialImpulse);
              max                                     = Math.Max(_minAsteroidInitialImpulse,        _maxAsteroidInitialImpulse);

              _minAsteroidInitialImpulse              = min;
              _maxAsteroidInitialImpulse              = max;

        int   minAsteroids                            = Math.Min(_minAsteroidsPerPlanetCount,       _maxAsteroidsPerPlanetCount);
        int   maxAsteroids                            = Math.Max(_minAsteroidsPerPlanetCount,       _maxAsteroidsPerPlanetCount);

              _minAsteroidsPerPlanetCount             = minAsteroids;
              _maxAsteroidsPerPlanetCount             = maxAsteroids;
    }

    // Adds a few planets separated by a distance.
    // TODO: Generate asteroids, check how close the current planet is to the previous ones.
    private void Awake()
    {
                Planets               = new GravitySphere[_planetsGenerateCount];
                                      
        Vector3 lastOrigin            = Vector3.zero;
        float   lastPlanetRadius      = Random.Range(_planetMinRadius, _planetMaxRadius);

        for (int i = 0; i < _planetsGenerateCount;)
        {
            Vector3 direction                 = Random.insideUnitSphere.normalized;
            float   radius                    = Random.Range(_planetMinRadius,           _planetMaxRadius);
            float   distanceBetween           = Random.Range(_minDistanceBetweenPlanets, _maxDistanceBetweenPlanets);

            float   lastOuterGravityRadius    = lastPlanetRadius * _planetsGravityOuterRadiusMultip;
            float   currentOuterGravityRadius = radius           * _planetsGravityOuterRadiusMultip;

            Vector3 newPosition               = lastOrigin + direction * (lastOuterGravityRadius + currentOuterGravityRadius + distanceBetween);

            if (!ValidNewPlanetPosition(newPosition, radius, i - 1))
                continue;

            var planet = SpawnPlanet(newPosition, radius);

            SpawnAsteroids(planet);

            Planets[i]       = planet;

            lastPlanetRadius = radius;
            lastOrigin       = newPosition;

            // for debugging purposes only.
            planet.gameObject.GetComponent<MeshRenderer>().material.color = new Color((float)i / (float)(_planetsGenerateCount - 1), (float)i / (float)(_planetsGenerateCount - 1), (float)i / (float)(_planetsGenerateCount - 1), 1.0f);
 
            i++;
        }
    }

    private void Update()
    {
        var toDesotry = _asteroids.Where(asteroid => asteroid.HitPlanet).ToArray();
        _asteroids.RemoveAll(asteroid => asteroid.HitPlanet);

        foreach (var asteroid in toDesotry)
            Destroy(asteroid.gameObject);
    }

    private bool ValidNewPlanetPosition(Vector3 newPosition, float newRadius, int lastPlanetIndex)
    {
        for (int i = 0; i <= lastPlanetIndex; i++)
        {
            float distBetween = Vector3.Distance(Planets[i].transform.position, newPosition);
            float outerRadius = newRadius * _planetsGravityOuterRadiusMultip;

            if (distBetween < outerRadius + Planets[i].OuterRadius)
                return false;
        }
        return true;
    }

    private GravitySphere SpawnPlanet(Vector3 position, float radius)
    {
        var planet                      = Instantiate(_planetPrefab);

            planet.transform.position   = position;
            planet.transform.localScale = Vector3.one * radius;
            planet.OuterRadius          = radius * _planetsGravityOuterRadiusMultip;
            planet.OuterFalloffRadius   = radius * _planetsGravityOuterFalloutRadiusMultip;

        return planet;
    }

    private void SpawnAsteroids(GravitySphere planet)
    {
        int asteroidsCount = Random.Range(_minAsteroidsPerPlanetCount, _maxAsteroidsPerPlanetCount + 1);
        for (int i = 0; i < asteroidsCount; i++)
        {
            Vector3 direction                               = Random.insideUnitSphere.normalized;
            float   positionRadius                          = Random.Range((planet.OuterRadius + planet.OuterFalloffRadius) * 0.5f, 
                                                                           planet.OuterFalloffRadius);
            float   radius                                  = Random.Range(_asteroidMinRadius, _asteroidMaxRadus);
            float   impulse                                 = Random.Range(_minAsteroidInitialImpulse, _maxAsteroidInitialImpulse);
                                                            
            Vector3 newPosition                             = planet.transform.position + direction * positionRadius;
                                                            
            var     asteroid                                = Instantiate(_asteroidPrefab);
                    asteroid.transform.position             = newPosition;
                    asteroid.transform.localScale           = Vector3.one * radius;
                    asteroid.GetComponent<Rigidbody>().mass = radius;

                    asteroid.InitialImpulseForce            = impulse;
            
            asteroid.ApplyInitalImpulse();

            _asteroids.Add(asteroid);
        }
    }
}

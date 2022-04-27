using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = System.Random;

public class WorldGenerator : MonoBehaviour
{
    // An array containing the planets that were generated.
    public GravitySphere[]  Planets { get; private set; }

    public Camera           Camera;
    // The prefab for a single planet. Maybe in the future it's gonna be an array of prefabs
    // but for now, it's ok.
    [SerializeField]
    private GravitySphere   _planetPrefab;
    [SerializeField]
    private Asteroid        _asteroidPrefab;
    [SerializeField]
    private Spaceship       _spaceshipPrefab;

    [SerializeField]
    private int             _randomSeed                             = 0;

    [SerializeField, Range(1, 10)]
    private int             _planetsGenerateCount                   = 5;

    [SerializeField]
    private float           _spaceshipAvoidPlanetRadius             = 20.0f;

    [SerializeField, Range(0, 1000)]
    private int             _minAsteroidsPerPlanetCount             = 1,
                            _maxAsteroidsPerPlanetCount             = 50;

    [SerializeField]
    private int             _spaceshipsCount                        = 0;

    // The minimum and maximum radius a planet can have.
    [SerializeField, Range(10.0f, 100.0f)]
    private float           _planetMinRadius                        = 10.0f,
                            _planetMaxRadius                        = 40.0f;

    [SerializeField, Range(0.5f, 9.0f)]
    private float           _asteroidMinRadius                      = 0.5f,
                            _asteroidMaxRadus                       = 2.0f;

    // The minimum and maximum distance between planets (excluding the gravity outer radiuses).
    [SerializeField, Min(0.0f)]
    private float           _minDistanceBetweenPlanets              = 0.0f,
                            _maxDistanceBetweenPlanets              = 10.0f;

    [SerializeField, Min(1.0f)]
    private float           _planetsGravityOuterRadiusMultip        = 2.0f,
                            _planetsGravityOuterFalloutRadiusMultip = 3.0f;

    [SerializeField, Range(0.0f, 1000.0f)]
    private float           _minAsteroidInitialImpulse              = 0.0f,
                            _maxAsteroidInitialImpulse              = 1000.0f;
                            
    [SerializeField]        
    private bool            _is2D                                   = false;
                            
    private List<Asteroid>  _asteroids                              = new List<Asteroid>();
    private List<Spaceship> _spaceships                             = new List<Spaceship>();

    private Random          _random;

    public void ResetWorld()
    {
        foreach (var planet in Planets)
            Destroy(planet.gameObject);

        Planets = null;

        foreach (var asteroid in _asteroids)
            Destroy(asteroid.gameObject);

        foreach (var spaceship in _spaceships)
            Destroy(spaceship.gameObject);

        _asteroids  = new List<Asteroid>();
        _spaceships = new List<Spaceship>();

        CreateWorld();
    }

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
        CreateWorld();
    }

    private void Update()
    {
        var toDesotryAsteroids = _asteroids.Where(asteroid => asteroid.HitPlanet).ToArray();
        _asteroids.RemoveAll(asteroid => asteroid.HitPlanet);

        foreach (var asteroid in toDesotryAsteroids)
            Destroy(asteroid.gameObject);

        var toDesotrySpaceships = _spaceships.Where(spaceship => spaceship.HitPlanet).ToArray();
        _spaceships.RemoveAll(spaceship => spaceship.HitPlanet);

        foreach (var spaceship in toDesotrySpaceships)
            Destroy(spaceship.gameObject);
    }

    private void CreateWorld()
    {
                _random               = new Random(_randomSeed);
                Planets               = new GravitySphere[_planetsGenerateCount];
                                      
        Vector3 lastOrigin            = Vector3.zero;
        float   lastPlanetRadius      = RandomRange(_planetMinRadius, _planetMaxRadius);

        for (int i = 0; i < _planetsGenerateCount;)
        {
            Vector3 direction                 = RandomOnUnitSphere();
            float   radius                    = RandomRange(_planetMinRadius,           _planetMaxRadius);
            float   distanceBetween           = RandomRange(_minDistanceBetweenPlanets, _maxDistanceBetweenPlanets);

            float   lastOuterGravityRadius    = lastPlanetRadius * _planetsGravityOuterRadiusMultip;
            float   currentOuterGravityRadius = radius           * _planetsGravityOuterRadiusMultip;

            Vector3 newPosition               = lastOrigin + direction * (lastOuterGravityRadius + currentOuterGravityRadius + distanceBetween);

            if (!ValidNewPlanetPosition(newPosition, radius, i - 1))
                continue;

            var planet = SpawnPlanet(newPosition, radius, i);

            SpawnAsteroids(planet);

            Planets[i]       = planet;

            lastPlanetRadius = radius;
            lastOrigin       = newPosition;

            // for debugging purposes only.
            planet.gameObject.GetComponent<MeshRenderer>().material.color = new Color((float)i / (float)(_planetsGenerateCount - 1), (float)i / (float)(_planetsGenerateCount - 1), (float)i / (float)(_planetsGenerateCount - 1), 1.0f);
 
            i++;
        }

        if (_spaceshipPrefab != null)
            SpawnSpacehsips();
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

    private GravitySphere SpawnPlanet(Vector3 position, float radius, int index)
    {
        var planet                      = Instantiate(_planetPrefab);
        var planetText                  = planet.GetComponent<PlanetText>();

            planet.transform.position   = position;
            planet.transform.localScale = Vector3.one * radius;
            planet.OuterRadius          = radius * _planetsGravityOuterRadiusMultip;
            planet.OuterFalloffRadius   = radius * _planetsGravityOuterFalloutRadiusMultip;
            planetText.Camera           = Camera;
            planetText.PlanetIndex      = index;

        return planet;
    }

    private void SpawnAsteroids(GravitySphere planet)
    {
        int asteroidsCount = _random.Next(_minAsteroidsPerPlanetCount, _maxAsteroidsPerPlanetCount + 1);
        for (int i = 0; i < asteroidsCount; i++)
        {
            Vector3 direction                               = RandomOnUnitSphere();
            float   positionRadius                          = RandomRange((planet.OuterRadius + planet.OuterFalloffRadius) * 0.5f, 
                                                                          planet.OuterFalloffRadius);
            float   radius                                  = RandomRange(_asteroidMinRadius,         _asteroidMaxRadus);
            float   impulse                                 = RandomRange(_minAsteroidInitialImpulse, _maxAsteroidInitialImpulse);
                                                            
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

    private void SpawnSpacehsips()
    {
        Vector3 minCorner = Planets[0].transform.position;
        Vector3 maxCorner = Planets[0].transform.position;

        foreach (var planet in Planets)
        {
            minCorner = Vector3.Min(minCorner, planet.transform.position);
            maxCorner = Vector3.Max(maxCorner, planet.transform.position);
        }

        for (int i = 0; i < _spaceshipsCount; i++)
        {
            Vector3 position = Vector3.zero;

            do
            {
                position = new Vector3(minCorner.x + (float)_random.NextDouble() * (maxCorner.x - minCorner.x),
                                       minCorner.y + (float)_random.NextDouble() * (maxCorner.y - minCorner.y),
                                       minCorner.z + (float)_random.NextDouble() * (maxCorner.z - minCorner.z));

            } while (!ValidSpaceshipPosition(position));

            var spaceship = Instantiate(_spaceshipPrefab);
            spaceship.transform.position = position;
            spaceship.transform.rotation = UnityEngine.Random.rotation;
            spaceship.MinBoxCorner = minCorner;
            spaceship.MaxBoxCorner = maxCorner;

            _spaceships.Add(spaceship);
        }
    }

    private float RandomRange(float minValue, float maxValue) => minValue + (float)_random.NextDouble() * maxValue;
    // https://datagenetics.com/blog/january32020/index.html
    private Vector3 RandomOnUnitSphere()
    {
        if (_is2D)
        {
            float angle = (float)_random.NextDouble() * Mathf.PI * 2.0f;
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f);
        }
        else
        {

            float theta = RandomRange(0.0f, Mathf.PI * 2.0f);
            float v = (float)_random.NextDouble();
            float phi = Mathf.Acos((2 * v) - 1);
            float r = Mathf.Pow((float)_random.NextDouble(), 1.0f / 3.0f);
            float x = r * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = r * Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = r * Mathf.Cos(phi);

            return new Vector3(x, y, z);
        }
    }

    private bool ValidSpaceshipPosition(Vector3 position) => Planets.All(planet => Vector3.Distance(planet.transform.position, position) >= planet.transform.localScale.magnitude + _spaceshipAvoidPlanetRadius);
}

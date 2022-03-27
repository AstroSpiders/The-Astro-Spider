using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    // The prefab for a single planet. Maybe in the future it's gonna be an array of prefabs
    // but for now, it's ok.
    [SerializeField]
    private GravitySphere  _planetPrefab;

    [SerializeField, Range(1, 10)]
    private int            _planetsGenerateCount      = 5;

    // The minimum and maximum radius a planet can have.
    [SerializeField, Range(10.0f, 100.0f)]
    private float          _planetMinRadius           = 10.0f,
                           _planetMaxRadius           = 40.0f;

    // The minimum and maximum distance between planets (excluding the gravity outer radiuses).
    [SerializeField, Min(0.0f)]
    private float          _minDistanceBetweenPlanets = 0.0f,
                           _maxDistanceBetweenPlanets = 10.0f;

    // An array containing the planets that were generated.
    public GravitySphere[] Planets { get; private set; }

    // Just some checks to make sure that the minimum values are smaller than the maximum values.
    private void OnValidate()
    {
        float min                        = Mathf.Min(_planetMinRadius, _planetMaxRadius);
        float max                        = Mathf.Max(_planetMinRadius, _planetMaxRadius);
                                         
              _planetMinRadius           = min;
              _planetMaxRadius           = max;
                                         
              min                        = Mathf.Min(_minDistanceBetweenPlanets, _maxDistanceBetweenPlanets);
              max                        = Mathf.Max(_minDistanceBetweenPlanets, _maxDistanceBetweenPlanets);

              _minDistanceBetweenPlanets = min;
              _maxDistanceBetweenPlanets = max;
    }

    // Adds a few planets separated by a distance.
    // TODO: Generate asteroids, check how close the current planet is to the previous ones.
    private void Start()
    {
                Planets               = new GravitySphere[_planetsGenerateCount];
                                      
        Vector3 lastOrigin            = Vector3.zero;
        float   lastPlanetRadius      = Random.Range(_planetMinRadius, _planetMaxRadius);

        for (int i = 0; i < _planetsGenerateCount; i++)
        {
            Vector3 direction                   = Random.insideUnitSphere.normalized;
            float   radius                      = Random.Range(_planetMinRadius,           _planetMaxRadius);
            float   distanceBetween             = Random.Range(_minDistanceBetweenPlanets, _maxDistanceBetweenPlanets);
            // TODO: replace these hard-coded numbers.
            Vector3 newPosition                 = lastOrigin + direction * (lastPlanetRadius * 3.0f + radius * 3.0f + distanceBetween);
                                                
            var     planet                      = Instantiate(_planetPrefab);
                    
                    planet.transform.position   = newPosition;
                    planet.transform.localScale = Vector3.one * radius;
            // TODO: replace these hard-coded numbers.
                    planet.OuterRadius          = radius * 2.0f;
                    planet.OuterFalloffRadius   = radius * 3.0f;

                    lastPlanetRadius            = radius;
                    lastOrigin                  = newPosition;
        }
    }
}

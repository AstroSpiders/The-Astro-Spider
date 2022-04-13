using UnityEngine;

public class AITrainer : MonoBehaviour
{
    [SerializeField]
    private int              _populationSize                   = 150;
    [SerializeField]                                           
    private int              _eliteCount                       = 3;
    [SerializeField]                                           
    private int              _eliteCopies                      = 2;
    [SerializeField]
    private float            _crossoverDisableConnectionChance = 0.75f;
    [SerializeField]
    private float            _addConnectionChance              = 0.05f;
    [SerializeField]
    private float            _addNodeChance                    = 0.03f;
    [SerializeField]
    private float            _mutateWeightsChance              = 0.8f;
    [SerializeField]
    private float            _perturbWeightChance              = 0.9f;
    [SerializeField]
    private float            _maxWeightPerturbation            = 0.1f;
    [SerializeField]
    private float            _previousGenMutatePercentage      = 0.25f;
    [SerializeField]
    private float            _c1                               = 1.0f;
    [SerializeField]
    private float            _c2                               = 1.0f;
    [SerializeField]
    private float            _c3                               = 0.4f;
    [SerializeField]
    private float            _n                                = 1.0f;
    [SerializeField]
    private float            _compatibilityThreshold           = 3.0f;
    [SerializeField]
    private float            _interspeciesMatingRate           = 0.001f;
    [SerializeField]
    private int              _generationsToStagnate            = 15;

    [SerializeField, Range(0.0f, 600.0f)]
    private float            _secondsPerEpoch                  = 60.0f;

    [SerializeField]
    private RocketState      _rocketPrefab                     = null;
    [SerializeField]
    private WorldGenerator   _worldGenerator                   = null;
    [SerializeField]
    private FocusCamera      _focusCamera                      = null;
                                                               
    private GeneticAlgorithm _geneticAlgorithm                 = null;

    private RocketState[]    _rockets                          = null;

    private float            _epochElapsedSeconds              = 0.0f;

    private void Start()
    {
        var prefabSensors = _rocketPrefab.GetComponent<RocketSensors>();

        GeneticAlgorithm.Parameters gaParams = new GeneticAlgorithm.Parameters()
        {
            PopulationSize                   = _populationSize,
            EliteCount                       = _eliteCount,
            EliteCopies                      = _eliteCopies,
            InNodes                          = (prefabSensors.TotalSensorsCount * 3) + 1, // 3 inputs per sensor, 1 bias sensor
            OutNodes                         = (int)RocketMovement.ThrusterTypes.Count,
            CrossoverDisableConnectionChance = _crossoverDisableConnectionChance,
            AddConnectionChance              = _addConnectionChance,
            AddNodeChance                    = _addNodeChance,
            MutateWeightsChance              = _mutateWeightsChance,
            PerturbWeightChance              = _perturbWeightChance,
            MaxWeightPerturbation            = _maxWeightPerturbation,
            PreviousGenMutatePercentage      = _previousGenMutatePercentage,
            C1                               = _c1,
            C2                               = _c2,
            C3                               = _c3,
            N                                = _n,
            CompatibilityThreshold           = _compatibilityThreshold,
            InterspeciesMatingRate           = _interspeciesMatingRate,
            GenerationsToStagnate            = _generationsToStagnate
        };

        _geneticAlgorithm = new GeneticAlgorithm(gaParams);
        _rockets          = new RocketState[_populationSize];

        for (int i = 0; i < _populationSize; i++)
            _rockets[i] = null;

        SpawnRockets();
    }

    private void Update()
    {
        Vector3 averagePosition = Vector3.zero;
        foreach (var rocket in _rockets)
            averagePosition += rocket.transform.position / _rockets.Length;

        _focusCamera.SetFocusPoint(averagePosition);

        _epochElapsedSeconds += Time.deltaTime;
        if (_epochElapsedSeconds > _secondsPerEpoch)
        {
            Epoch();
            _epochElapsedSeconds -= _secondsPerEpoch;
        }
    }

    private void Epoch()
    {
        int index = 0;
        foreach (var specie in _geneticAlgorithm.Population)
        {
            foreach (var individual in specie.Individuals)
            {
                individual.Fitness = CalculateFitness(_rockets[index]);
                index++;
            }
        }
        _geneticAlgorithm.Epoch();

        DestroyOldRockets();
        _worldGenerator.ResetWorld();
        SpawnRockets();
    }

    private void DestroyOldRockets()
    {
        for (int i = 0; i < _populationSize; i++)
        {
            if (_rockets[i] != null)
            {
                Destroy(_rockets[i].gameObject);
                _rockets[i] = null;
            }
        }
    }

    private float CalculateFitness(RocketState rocket)
    {
        float sum      = 1.0f;
        float exponent = 1.0f;

        foreach (var planetStats in rocket.PlanetsStats)
        {
            // TODO: take into account the speed, and punish the rocket for not going in the right direction.
            sum      += (planetStats.DistanceNavigated + planetStats.MaxDistanceNavigated) * 0.5f - planetStats.FuelConsumed;
            exponent += (planetStats.LandingDot + (1.0f - planetStats.LandingImpact)) * 0.5f;
        }

        Debug.Log(sum + " " + exponent);

        float fitness = Mathf.Pow(sum, exponent);

        if (rocket.Dead || !rocket.HasFuel)
            fitness /= 2.0f;

        return fitness;
    }

    private void SpawnRockets()
    {
        DestroyOldRockets();

        int index = 0;
        foreach (var specie in _geneticAlgorithm.Population)
        {
            foreach (var individual in specie.Individuals)
            { 
                var rocket                    = Instantiate(_rocketPrefab);

                    rocket.WorldGenerator     = _worldGenerator;
                    rocket.transform.position = Vector3.zero;
            
                rocket.gameObject.AddComponent(typeof(NeuralNetworkController));
                rocket.GetComponent<NeuralNetworkController>().SetNeuralNetwork(_geneticAlgorithm, individual);

                _rockets[index] = rocket;
                
                index++;
            }
        }
    }
}

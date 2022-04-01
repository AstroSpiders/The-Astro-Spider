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

    [SerializeField]
    private RocketState      _rocketPrefab                     = null;

    [SerializeField]
    private WorldGenerator   _worldGenerator                   = null;
                                                               
    private GeneticAlgorithm _geneticAlgorithm                 = null;

    private RocketState[]    _rockets                          = null;

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

        for (int i = 0; i < 10; i++)
        {
            float fitness = 1.0f;
            foreach (var specie in _geneticAlgorithm.Population)
            {
                foreach (var individual in specie.Individuals)
                {
                    individual.Fitness = fitness;
                    fitness += 1.0f;
                }
            }
            _geneticAlgorithm.Epoch();
        }

        _rockets          = new RocketState[_populationSize];

        for (int i = 0; i < _populationSize; i++)
            _rockets[i] = null;

        SpawnRockets();
    }

    private void Update()
    {
        
    }

    private void SpawnRockets()
    {
        for (int i = 0; i < _populationSize; i++)
        {
            if (_rockets[i] != null)
            {
                Destroy(_rockets[i]);
                _rockets[i] = null;
            }
        }

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

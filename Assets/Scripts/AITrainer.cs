using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class AITrainer : MonoBehaviour
{
    [Serializable]
    public class EpochStats
    {
        public int   Epoch          { get; set; }
        public float MaxFitness     { get; set; }
        public float AverageFitness { get; set; }
    }

    [Serializable]
    public class TrainingState
    {
        public GeneticAlgorithm GeneticAlgorithm { get; set; }
        public List<EpochStats> EpochStats       { get; set; }

    }

    public const string           DefaultSaveFile                   = "/stats_release.json";

    public       WorldGenerator   WorldGenerator                    = null;
    public       FocusCamera      FocusCamera                       = null;
    public       Button           SaveTrainingStateButton           = null,
                                  LoadTrainingStateButton           = null;

    public       TMP_Text         EpochTextLabel                    = null,
                                  MaxFitnessTextLabel               = null,
                                  AverageFitnessLabel               = null;

    public       float            RocketFuelMultiplier              = 2.0f;

    [SerializeField]
    private      int              _populationSize                   = 150;
    [SerializeField]
    private      int              _eliteCount                       = 3;
    [SerializeField]
    private      int              _eliteCopies                      = 2;
    [SerializeField]
    private      float            _crossoverDisableConnectionChance = 0.75f;
    [SerializeField]
    private      float            _addConnectionChance              = 0.05f;
    [SerializeField]
    private      float            _addNodeChance                    = 0.03f;
    [SerializeField]
    private      float            _mutateWeightsChance              = 0.8f;
    [SerializeField]
    private      float            _perturbWeightChance              = 0.9f;
    [SerializeField]
    private      float            _maxWeightPerturbation            = 0.1f;
    [SerializeField]
    private      float            _previousGenMutatePercentage      = 0.25f;
    [SerializeField]
    private      float            _c1                               = 1.0f;
    [SerializeField]
    private      float            _c2                               = 1.0f;
    [SerializeField]
    private      float            _c3                               = 0.4f;
    [SerializeField]
    private      float            _n                                = 1.0f;
    [SerializeField]
    private      float            _compatibilityThreshold           = 3.0f;
    [SerializeField]
    private      float            _interspeciesMatingRate           = 0.001f;
    [SerializeField]
    private      int              _generationsToStagnate            = 15;

    [SerializeField, Range(0.0f, 600.0f)]
    private      float            _secondsPerEpoch                  = 60.0f;

    [SerializeField]
    private      RocketState      _rocketPrefab                     = null;

    private      GeneticAlgorithm _geneticAlgorithm                 = null;

    private      RocketState[]    _rockets                          = null;

    private      float            _epochElapsedSeconds              = 0.0f;

    private      List<EpochStats> _epochStats                       = new List<EpochStats>();

    private      string           _saveStatePath                    = string.Empty;
    private      string           _loadStatePath                    = string.Empty;

    private void Start()
    {
        var prefabSensors = _rocketPrefab.GetComponent<RocketSensors>();

        GeneticAlgorithm.Parameters gaParams = new GeneticAlgorithm.Parameters()
        {
            PopulationSize                   = _populationSize,
            EliteCount                       = _eliteCount,
            EliteCopies                      = _eliteCopies,
            InNodes                          = (prefabSensors.TotalSensorsCount * 3) + 1, // 3 inputs per sensor, 1 bias sensor
            OutNodes                         = 3, // there are 3 thrusters required for the 2D case (left, right, main)
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

        SaveTrainingStateButton.onClick.AddListener(SaveTrainingStateCallback);
        LoadTrainingStateButton.onClick.AddListener(LoadTrainingStateCallback);

        float fixedDeltaTime = Time.fixedDeltaTime;

        //Time.timeScale = 2.0f;
        //Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
    }

    private void Update()
    {
        int     maxLanding    = 0;
        float   maxFitness    = 0.0f;
        Vector3 focusPosition = Vector3.zero;

        foreach (var rocket in _rockets)
        {
            float fitness = CalculateFitness(rocket);
            if (fitness > maxFitness)
            {
                maxFitness = fitness;
                focusPosition = rocket.transform.position;
            }

            int index = 1;
            foreach (var stats in rocket.PlanetsStats)
            {
                if (stats.Landed && index < WorldGenerator.Planets.Length - 1)
                    maxLanding = Mathf.Max(index, maxLanding);
                index++;
            }
        }


        FocusCamera.SetFocusPoint(focusPosition);

        _epochElapsedSeconds += Time.deltaTime;
        float adjustedSecondsPerEpoch = _secondsPerEpoch + maxLanding * _secondsPerEpoch;

        if (_epochElapsedSeconds > adjustedSecondsPerEpoch)
        {
            Epoch();
            _epochElapsedSeconds -= adjustedSecondsPerEpoch;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_saveStatePath != string.Empty)
        {
            SaveTrainingStateButton.GetComponentInChildren<TMP_Text>().text = "Saving training state...";
            SaveTrainingStateButton.interactable                            = false;
        }

        if (_loadStatePath != string.Empty)
        {
            LoadTrainingStateButton.GetComponentInChildren<TMP_Text>().text = "Loading training state...";
            LoadTrainingStateButton.interactable                            = false;
        }

        if (_epochStats.Count >= 1)
        {
            EpochTextLabel.text      = "Epoch: " + _epochStats.Count;
            MaxFitnessTextLabel.text = "Max Fitness: " + _epochStats[_epochStats.Count - 1].MaxFitness;
            AverageFitnessLabel.text = "Average Fitness: " + _epochStats[_epochStats.Count - 1].AverageFitness;
        }
        else
        {
            EpochTextLabel.text      = string.Empty;
            MaxFitnessTextLabel.text = string.Empty;
            AverageFitnessLabel.text = string.Empty;
        }
    }

    private void Epoch()
    {
        int index      = 0;
        var epochStats = new EpochStats
        {
            Epoch          = _epochStats.Count,
            MaxFitness     = 0.0f,
            AverageFitness = 0.0f
        };

        foreach (var specie in _geneticAlgorithm.Population)
        {
            foreach (var individual in specie.Individuals)
            {
                individual.Fitness         = CalculateFitness(_rockets[index]);
                epochStats.AverageFitness += individual.Fitness / _populationSize;
                epochStats.MaxFitness      = Mathf.Max(epochStats.MaxFitness, individual.Fitness);

                index++;
            }
        }

        _epochStats.Add(epochStats);

        if (_saveStatePath != string.Empty)
        {
            SaveTrainingState();
            _saveStatePath = string.Empty;
        }

        _geneticAlgorithm.Epoch();

        if (_loadStatePath != string.Empty)
        {
            LoadTrainingState();
            _loadStatePath = string.Empty;
        }

        DestroyOldRockets();
        WorldGenerator.ResetWorld();
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

        int   index    = 0;

        foreach (var planetStats in rocket.PlanetsStats)
        {
            float landed = planetStats.Landed ? 1.0f : 0.0f;

            sum += (planetStats.DistanceNavigated + planetStats.MaxDistanceNavigated) * (3.0f/2.0f) - (planetStats.FuelConsumed + planetStats.DistanceFarFromPlanet + planetStats.MaxDistanceFarFromPlanet) * 1.0f;
            exponent += (planetStats.LandingDot * 2.0f + ((1.0f - planetStats.LandingImpact) + landed * (1.0f - planetStats.IdealLandingImpact))) / (1 << index);


            index++;
        }

        float fitness = Mathf.Pow(sum, exponent);

        if (rocket.Dead || !rocket.HasFuel)
            fitness /= 2.0f;

        fitness = Math.Max(0.0f, fitness);

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

                    rocket.WorldGenerator     = WorldGenerator;
                    rocket.transform.position = Vector3.zero;

                rocket.gameObject.AddComponent(typeof(NeuralNetworkController));
                rocket.GetComponent<NeuralNetworkController>().SetNeuralNetwork(_geneticAlgorithm, individual);
                rocket.GetComponent<RocketState>().FuelCapacity *= RocketFuelMultiplier;

                _rockets[index] = rocket;

                index++;
            }
        }
    }
    private void SaveTrainingStateCallback()
    {
#if UNITY_EDITOR
        _saveStatePath = EditorUtility.SaveFilePanel("Save training state as JSON",
                                                     "",
                                                     "training_state.json",
                                                     "json");
#else
        _saveStatePath = Application.persistentDataPath + DefaultSaveFile;
#endif
    }

    private void SaveTrainingState()
    {
        var currentState = new TrainingState
        {
            GeneticAlgorithm = _geneticAlgorithm,
            EpochStats       = _epochStats
        };

        string json = JsonConvert.SerializeObject(currentState);

        File.WriteAllText(_saveStatePath, json);

        SaveTrainingStateButton.GetComponentInChildren<TMP_Text>().text = "Save training state";
        SaveTrainingStateButton.interactable                            = true;
    }

    private void LoadTrainingStateCallback()
    {
#if UNITY_EDITOR
        _loadStatePath = EditorUtility.OpenFilePanel("Load trading state", "", "json");
#else
        _loadStatePath = Application.persistentDataPath + DefaultSaveFile;
#endif
    }

    private void LoadTrainingState()
    {
        var json                                                         = File.ReadAllText(_loadStatePath);
        var currentState                                                 = JsonConvert.DeserializeObject<TrainingState>(json);

            _geneticAlgorithm                                            = currentState.GeneticAlgorithm;
            _epochStats                                                  = currentState.EpochStats;

           LoadTrainingStateButton.GetComponentInChildren<TMP_Text>().text = "Load training state";
           LoadTrainingStateButton.interactable                            = true;
    }
}

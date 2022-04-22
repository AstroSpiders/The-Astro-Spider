using Newtonsoft.Json;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeCreator : MonoBehaviour
{
    private enum GameState
    {
        Playing,
        Paused
    }

    [SerializeField]
    private GameParams       _gameParams;
                             
    [SerializeField]         
    private Camera           _mainCamera;
                             
    [SerializeField]         
    private LayerMask        _orbitCameraLayerMask;
                             
    [SerializeField]         
    private RocketState      _watchAIRocketPrefab;
    [SerializeField]         
    private Canvas           _watchAIUI;
                             
    [SerializeField]         
    private WorldGenerator   _trainWorldGenerator;
    [SerializeField]         
    private AITrainer        _aiTrainer;
    [SerializeField]         
    private Canvas           _trainInGameUI;
                             
    [SerializeField]         
    private WorldGenerator   _playWorldGenerator;
    [SerializeField]         
    private RocketState      _playRocketPrefab;
                             
    [SerializeField]         
    private Canvas           _pauseMenu;

    private Canvas           _gameCanvas       = null;
    private PlayerController _playerController = null;
    private OrbitCamera      _orbitCamera      = null;
    private GameState        _gameState;

    public void OnResumeButtonPressed()
    {
        _gameState = GameState.Playing;
        _pauseMenu.gameObject.SetActive(false);
        
        if (_gameCanvas != null)
            _gameCanvas.gameObject.SetActive(true);
        if (_playerController != null)
            _playerController.enabled = true;
        if (_orbitCamera != null)
            _orbitCamera.enabled = true;

        Time.timeScale = 1.0f;
    }

    public void OnMainMenuButtonPressed() => SceneManager.LoadScene(0);
    
    private void Awake()
    {
        _gameState = GameState.Playing;

        switch (_gameParams.GameMode)
        {
            case GameModes.WatchAI:
                CreateWatchAI();
                break;
            case GameModes.TrainAI:
                CreateTrainAI();
                break;
            case GameModes.Play:
                CreatePlay();
                break;
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        { 
            switch (_gameState)
            {
                case GameState.Playing:
                    _gameState = GameState.Paused;
                    if (_gameCanvas != null)
                        _gameCanvas.gameObject.SetActive(false);
                    if (_playerController != null)
                        _playerController.enabled = false;
                    if (_orbitCamera != null)
                        _orbitCamera.enabled = false;

                    _pauseMenu.gameObject.SetActive(true);
                    Time.timeScale = 0.0f;
                    break;
                case GameState.Paused:
                    OnResumeButtonPressed();
                    break;
            }
        }
    }

    private void CreateWatchAI()
    {
        var                     json           = File.ReadAllText(_gameParams.AIStateFilename);
        var                     currentState   = JsonConvert.DeserializeObject<AITrainer.TrainingState>(json);

        float                   bestFitness    = -1.0f;
        GeneticAlgorithm.Genome bestIndividual = null;

        foreach (var specie in currentState.GeneticAlgorithm.Population)
        {
            foreach (var individual in specie.Individuals)
            {
                if (individual.Fitness > bestFitness)
                {
                    bestIndividual = individual;
                    bestFitness    = individual.Fitness;
                }
            }
        }

        if (bestIndividual is null)
            return;

        var worldGenerator = Instantiate(_trainWorldGenerator);
        var player         = Instantiate(_watchAIRocketPrefab);

        _mainCamera.gameObject.AddComponent(typeof(OrbitCamera));
        var orbitCamera = _mainCamera.GetComponent<OrbitCamera>();

        orbitCamera.enabled         = true;
        orbitCamera.Focus           = player.transform;
        orbitCamera.ObstructionMask = _orbitCameraLayerMask;

        var neutalNetworkController = player.GetComponent<NeuralNetworkController>();

        player.GetComponent<RocketState>().WorldGenerator = worldGenerator;
        neutalNetworkController.SetNeuralNetwork(currentState.GeneticAlgorithm, bestIndividual);
        player.GetComponent<RocketState>().FuelCapacity *= _aiTrainer.RocketFuelMultiplier;
        player.MaxLandingImpact = player.MaxLandingImpact + player.MaxLandingImpact * 0.5f;

        var canvas = Instantiate(_watchAIUI);

        var uiNeuralNetwork = canvas.GetComponentInChildren<UINeuralNetwork>();
        uiNeuralNetwork.NeuralNetwork = neutalNetworkController.GetNeuralNetwork();

        _gameCanvas = canvas;
        _orbitCamera = orbitCamera;
    }

    private void CreateTrainAI()
    {
        var worldGenerator = Instantiate(_trainWorldGenerator);
        _mainCamera.gameObject.AddComponent(typeof(FocusCamera));
        var canvas = Instantiate(_trainInGameUI);

        var canvasButtons = canvas.GetComponentsInChildren<Button>();

        var saveTrainingButton = canvasButtons[0];
        var loadTrainingButton = canvasButtons[1];

        var canvasTextLabels        = canvas.GetComponentsInChildren<TMP_Text>().Where(x => x.GetComponentInParent<Button>() == null).ToArray();

        var epochTextLabel          = canvasTextLabels[0];
        var maxFitnessTextLabel     = canvasTextLabels[1];
        var averageFitnessTextLabel = canvasTextLabels[2];

        var aiTrainer = Instantiate(_aiTrainer);

        aiTrainer.WorldGenerator          = worldGenerator;
        aiTrainer.FocusCamera             = _mainCamera.GetComponent<FocusCamera>();

        aiTrainer.SaveTrainingStateButton = saveTrainingButton;
        aiTrainer.LoadTrainingStateButton = loadTrainingButton;
        aiTrainer.EpochTextLabel          = epochTextLabel;
        aiTrainer.MaxFitnessTextLabel     = maxFitnessTextLabel;
        aiTrainer.AverageFitnessLabel     = averageFitnessTextLabel;

        _gameCanvas = canvas;
    }

    void CreatePlay()
    {
        var worldGenerator = Instantiate(_playWorldGenerator);

        var player = Instantiate(_playRocketPrefab);
        _mainCamera.gameObject.AddComponent(typeof(OrbitCamera));
        var orbitCamera = _mainCamera.GetComponent<OrbitCamera>();

        orbitCamera.enabled         = true;
        orbitCamera.Focus           = player.transform;
        orbitCamera.ObstructionMask = _orbitCameraLayerMask;

        var playerController = player.GetComponent<PlayerController>();

        player.GetComponent<RocketState>().WorldGenerator        = worldGenerator;
        playerController.PlayerInputSpace = orbitCamera.transform;

        _orbitCamera = orbitCamera;
        _playerController = playerController;
    }
}

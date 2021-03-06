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
    public enum GameState
    {
        Playing,
        Paused,
        Ending
    }

    public  GameState        CurrentGameState { get; private set; }

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
    private Canvas           _playGameUI;

    [SerializeField]         
    private Canvas           _pauseMenu;
    [SerializeField]
    private Canvas           _endingMenu;

    [SerializeField] 
    private ProgressUI       _progressUI;

    private Canvas           _gameCanvas;
    private PlayerController _playerController;
    private RocketState      _rocketState;
    private OrbitCamera      _orbitCamera;

    private float            _defaultFixedDeltaTime;

    private float            _beforePauseTimeScale;
    private float            _beforePauseFixedDeltaTime;

    public void OnResumeButtonPressed()
    {
        CurrentGameState = GameState.Playing;
        _pauseMenu.gameObject.SetActive(false);
        
        if (_gameCanvas != null)
            _gameCanvas.gameObject.SetActive(true);
        if (_playerController != null)
            _playerController.enabled = true;
        if (_orbitCamera != null)
            _orbitCamera.enabled = true;

        Time.timeScale      = _beforePauseTimeScale;
        Time.fixedDeltaTime = _beforePauseFixedDeltaTime;
    }

    public void OnRespawnButtonPressed()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void OnMainMenuButtonPressed()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
        SceneManager.LoadScene(0);
    }
    private void Awake()
    {
        CurrentGameState = GameState.Playing;
        _defaultFixedDeltaTime = Time.fixedDeltaTime;

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
            switch (CurrentGameState)
            {
                case GameState.Playing:
                    CurrentGameState = GameState.Paused;
                    if (_gameCanvas != null)
                        _gameCanvas.gameObject.SetActive(false);
                    if (_playerController != null)
                        _playerController.enabled = false;
                    if (_orbitCamera != null)
                        _orbitCamera.enabled = false;

                    _pauseMenu.gameObject.SetActive(true);

                    _beforePauseTimeScale      = Time.timeScale;
                    _beforePauseFixedDeltaTime = Time.fixedDeltaTime;

                    Time.timeScale      = 0.0f;
                    Time.fixedDeltaTime = 0.0f;
                    break;
                case GameState.Paused:
                    OnResumeButtonPressed();
                    break;
                case GameState.Ending:
                    OnMainMenuButtonPressed();
                    break;
            }
        }

        if (_rocketState != null)
        {
            if (CurrentGameState == GameState.Playing)
            {
                if (!_rocketState.HasFuel)
                    EndGame("Out of fuel");
                else if (_rocketState.Dead)
                    EndGame("Player died");
                else if (_rocketState.Won)
                    EndGame("Player won");
            }
        }
    }

    private void EndGame(string endingMessage)
    {
        CurrentGameState = GameState.Ending;

        if (_gameCanvas != null)
            _gameCanvas.gameObject.SetActive(false);
        if (_playerController != null)
            _playerController.enabled = false;
        if (_orbitCamera != null)
            _orbitCamera.enabled = false;

        _endingMenu.gameObject.SetActive(true);

        _endingMenu.GetComponentInChildren<TMP_Text>().text = endingMessage;

        Time.timeScale = 0.0f;
    }

    private void CreateWatchAI()
    {
        var                     json      = File.ReadAllText(_gameParams.AIStateFilename);
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

        _trainWorldGenerator.Camera = _mainCamera;
        var worldGenerator          = Instantiate(_trainWorldGenerator);
        var player                  = Instantiate(_watchAIRocketPrefab);

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
        
        var fuelIndicator = canvas.GetComponentsInChildren<FuelIndicator>()[0];
        fuelIndicator.PlayerRocketState = player;
        fuelIndicator.PlayerRocketMovement = player.GetComponent<RocketMovement>();

        var uiNeuralNetwork = canvas.GetComponentInChildren<UINeuralNetwork>();
        uiNeuralNetwork.NeuralNetwork = neutalNetworkController.GetNeuralNetwork();

        var progressUI = Instantiate(_progressUI);

        progressUI.PlanetIndexTextLabel = canvas.GetComponentsInChildren<TMP_Text>()[0];
        progressUI.RocketState = player.GetComponent<RocketState>();

        _gameCanvas  = canvas;
        _orbitCamera = orbitCamera;
        _rocketState = player;
    }

    private void CreateTrainAI()
    {
        _trainWorldGenerator.Camera = _mainCamera;
        var worldGenerator          = Instantiate(_trainWorldGenerator);
        _mainCamera.gameObject.AddComponent(typeof(FocusCamera));
        var canvas = Instantiate(_trainInGameUI);

        var canvasButtons = canvas.GetComponentsInChildren<Button>();

        var saveTrainingButton = canvasButtons[0];
        var loadTrainingButton = canvasButtons[1];

        var canvasTextLabels= canvas.GetComponentsInChildren<TMP_Text>().Where(x => x.GetComponentInParent<Button>() == null).ToArray();

        var epochTextLabel          = canvasTextLabels[0];
        var maxFitnessTextLabel     = canvasTextLabels[1];
        var averageFitnessTextLabel = canvasTextLabels[2];

        var timeScaleTextLabel      = canvasTextLabels[3];

        var aiTrainer = Instantiate(_aiTrainer);

        aiTrainer.WorldGenerator          = worldGenerator;
        aiTrainer.FocusCamera             = _mainCamera.GetComponent<FocusCamera>();
        aiTrainer.GameModeCreator         = this;

        aiTrainer.SaveTrainingStateButton = saveTrainingButton;
        aiTrainer.LoadTrainingStateButton = loadTrainingButton;
        aiTrainer.EpochTextLabel          = epochTextLabel;
        aiTrainer.MaxFitnessTextLabel     = maxFitnessTextLabel;
        aiTrainer.AverageFitnessLabel     = averageFitnessTextLabel;
        aiTrainer.TimeScaleLabel          = timeScaleTextLabel;
        aiTrainer.TimeScaleSlider         = canvas.GetComponentInChildren<Slider>();

        canvas.GetComponentInChildren<UITrainingStats>().AITrainer = aiTrainer;

        _gameCanvas = canvas;
    }

    void CreatePlay()
    {
        _playWorldGenerator.Camera  = _mainCamera;
        var worldGenerator          = Instantiate(_playWorldGenerator);

        var player = Instantiate(_playRocketPrefab);
        _mainCamera.gameObject.AddComponent(typeof(OrbitCamera));
        var orbitCamera = _mainCamera.GetComponent<OrbitCamera>();

        orbitCamera.enabled         = true;
        orbitCamera.Focus           = player.transform;
        orbitCamera.ObstructionMask = _orbitCameraLayerMask;

        var playerController = player.GetComponent<PlayerController>();

        player.GetComponent<RocketState>().WorldGenerator        = worldGenerator;
        playerController.PlayerInputSpace = orbitCamera.transform;
        
        var canvas = Instantiate(_playGameUI);

        var fuelIndicator = canvas.GetComponentsInChildren<FuelIndicator>()[0];
        fuelIndicator.PlayerRocketState = player;
        fuelIndicator.PlayerRocketMovement = player.GetComponent<RocketMovement>();
        
        var progressUI = Instantiate(_progressUI);

        progressUI.PlanetIndexTextLabel = canvas.GetComponentsInChildren<TMP_Text>()[0];
        progressUI.RocketState = player.GetComponent<RocketState>();

        _gameCanvas = canvas;
        _orbitCamera = orbitCamera;
        _playerController = playerController;
        _rocketState = player;
    }
}

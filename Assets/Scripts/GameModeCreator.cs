using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeCreator : MonoBehaviour
{
    [SerializeField]
    private GameSettings   _gameSettings;

    [SerializeField]
    private Camera         _mainCamera;

    [SerializeField]
    private WorldGenerator _trainWorldGenerator;
    [SerializeField]
    private AITrainer      _aiTrainer;
    [SerializeField]
    private Canvas         _trainInGameUI;

    [SerializeField]
    private WorldGenerator _playWorldGenerator;
    [SerializeField]
    private RocketState    _playRocketPrefab;

    private void Awake()
    {

        switch (_gameSettings.GameMode)
        {
            case GameModes.WatchAI:
                break;
            case GameModes.TrainAI:
                CreateTrainAI();
                break;
            case GameModes.Play:
                CreatePlay();
                break;
        }
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
    }

    void CreatePlay()
    {
        var worldGenerator = Instantiate(_playWorldGenerator);

        var player = Instantiate(_playRocketPrefab);
        _mainCamera.gameObject.AddComponent(typeof(OrbitCamera));
        var orbitCamera = _mainCamera.GetComponent<OrbitCamera>();

        orbitCamera.enabled = true;
        orbitCamera.Focus = player.transform;

        player.GetComponent<RocketState>().WorldGenerator        = worldGenerator;
        player.GetComponent<PlayerController>().PlayerInputSpace = orbitCamera.transform;
    }
}

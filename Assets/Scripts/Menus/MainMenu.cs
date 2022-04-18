using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameSettings _gameSettings;

    public void OnWatchAIButtonPressed()
    {
        _gameSettings.GameMode = GameModes.WatchAI;
        ChangeScene();
    }

    public void OnTrainAIButtonPressed()
    {
        _gameSettings.GameMode = GameModes.TrainAI;
        ChangeScene();
    }

    public void OnPlayButtonPressed()
    {
        _gameSettings.GameMode = GameModes.Play;
        ChangeScene();
    }

    private void ChangeScene() => SceneManager.LoadScene(1);
}

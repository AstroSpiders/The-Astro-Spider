#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameParams _gameSettings;

    private string     _aiStateFilename = string.Empty;

    public void OnWatchAIButtonPressed()
    {
        _gameSettings.GameMode = GameModes.WatchAI;
#if UNITY_EDITOR
        _aiStateFilename = EditorUtility.OpenFilePanel("Load trading state", "", "json");
#else
        _aiStateFilename = Application.persistentDataPath + AITrainer.DefaultSaveFile;
#endif
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

    public void OnQuitButtonPressed() => Application.Quit();

    private void Update()
    {
        if (_aiStateFilename == string.Empty)
            return;

        _gameSettings.AIStateFilename = _aiStateFilename;
        _aiStateFilename = string.Empty;

        ChangeScene();
    }

    private void ChangeScene() => SceneManager.LoadScene(1);
}

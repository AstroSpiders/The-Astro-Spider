using UnityEngine;

public enum GameModes
{
    WatchAI,
    TrainAI,
    Play
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameParams : ScriptableObject
{
    public GameModes GameMode        = GameModes.Play;
    public string    AIStateFilename = string.Empty;
}
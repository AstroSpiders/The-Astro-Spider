using UnityEngine;

public enum GameModes
{
    WatchAI,
    TrainAI,
    Play
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public GameModes GameMode = GameModes.Play;
}
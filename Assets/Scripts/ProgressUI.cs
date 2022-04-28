using TMPro;
using UnityEngine;

public class ProgressUI : MonoBehaviour
{
    public TMP_Text PlanetIndexTextLabel;
    public RocketState RocketState;
    
    private void Update()
    {
        if (PlanetIndexTextLabel)
        {
            var planetIndex = RocketState.CurrentPlanetIndex;
            var totalPlanets = RocketState.WorldGenerator.Planets.Length;
            PlanetIndexTextLabel.text = "Landed on planet " + planetIndex + " / " + totalPlanets;
        }
    }
}
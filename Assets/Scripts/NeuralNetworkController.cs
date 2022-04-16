using UnityEngine;

[RequireComponent(typeof(RocketSensors))]
[RequireComponent(typeof(RocketMovement))]
public class NeuralNetworkController : MonoBehaviour
{
    private NeuralNetwork  _neuralNetwork  = null;
    private RocketSensors  _sensors        = null;
    private RocketMovement _rocketMovement = null;
    public void SetNeuralNetwork(GeneticAlgorithm geneticAlgorithm, GeneticAlgorithm.Genome genome) => 
        _neuralNetwork = new NeuralNetwork(geneticAlgorithm, genome);

    void Awake()
    {
        _sensors        = GetComponent<RocketSensors>();
        _rocketMovement = GetComponent<RocketMovement>();
    }

    void Update()
    {
        if (_neuralNetwork is null)
            return;

        float[] inputs = new float[_sensors.TotalSensorsCount * 3];
        int     index  = 0;

        foreach (var sensorVal in _sensors.SensorOutputs)
        {
            inputs[index] = sensorVal.ObstacleDistance;
            index++;
            inputs[index] = sensorVal.TargetDistance;
            index++;
            inputs[index] = sensorVal.TargetDirection;
            index++;
        }

        var outputs = _neuralNetwork.Forward(inputs, (int)RocketMovement.ThrusterTypes.Count);

        _rocketMovement.ApplyAcceleration(outputs[0] * 0.5f + 0.5f, RocketMovement.ThrusterTypes.Main);
        _rocketMovement.ApplyAcceleration(outputs[1] * 0.5f + 0.5f, RocketMovement.ThrusterTypes.ExhaustLeft);
        _rocketMovement.ApplyAcceleration(outputs[2] * 0.5f + 0.5f, RocketMovement.ThrusterTypes.ExhaustRight);

        //for (int i = 0; i < (int)RocketMovement.ThrusterTypes.Count; i++)
        //    _rocketMovement.ApplyAcceleration(outputs[i], (RocketMovement.ThrusterTypes)i);
    }
}

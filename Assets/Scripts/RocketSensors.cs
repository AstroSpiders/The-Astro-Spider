using UnityEngine;

public class RocketSensors : MonoBehaviour
{
    // Strucute defining the info received by a sensor.
    public class SensorOutput
    {
        // The distance to the closest obstacle that the rocket should avoid.
        // It has a value of -1 when no obstacle touches the sensor.
        // When an obstacle touches the sensor, it has a distance between
        // 0 and 1 depending on the distance to the rocket (0 when near the rocket, 1 when far).
        public float   ObstacleDistance { get; set; }

        // Same as the above, except it's used for planets instead of obstacles.
        public float   TargetDistance   { get; set; }
        
        // Info regardng how similar the direction of the sensor is to the direction
        // of the target planet. Has a value of 1 when the directions are identical
        // has a value of -1 when the direction of the sensor is the opposite of the direction
        // to the target planet.
        public float   TargetDirection  { get; set; }

        // This is the direction of the sensor. 
        // It's used only for debugging purposes.
        public Vector3 CurrentDirection { get; set; }
    }

    public Transform      TargetPlanet       = null;

    public int            TotalSensorsCount { get => (_sensorLayersCount - 2) * _sensorsPerLayer + 2; }

    public SensorOutput[] SensorOutputs { get; private set; }

    [SerializeField, Range(1, 10)]
    private int           _sensorLayersCount              = 6;
                                                          
    [SerializeField, Range(1, 20)]                        
    private int           _sensorsPerLayer                = 8;

    [SerializeField, Range(0.0f, 100.0f)]
    private float         _sensorLength                   = 40;

    [SerializeField]
    private LayerMask     _planetsLayerMask;

    [SerializeField]
    private LayerMask     _obstaclesLayerMask;

    [SerializeField]
    private float         _debugIntersectionSpheresRadius = 0.05f;

    private void Awake()  => CreateSensorsArray();

    private void Update() => UpdateSensors();

    private void OnDrawGizmos()
    {
        if (!this.enabled)
            return;

        if (SensorOutputs is null)
            CreateSensorsArray();

        UpdateSensors();

        foreach (var sensor in SensorOutputs)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + sensor.CurrentDirection * _sensorLength);

            if (sensor.ObstacleDistance >= 0.0f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position + sensor.CurrentDirection * _sensorLength * sensor.ObstacleDistance, _debugIntersectionSpheresRadius);
            }

            if (sensor.TargetDistance >= 0.0f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + sensor.CurrentDirection * _sensorLength * sensor.TargetDistance, _debugIntersectionSpheresRadius);
            }
        }
    }

    private void CreateSensorsArray()
    {
        SensorOutputs = new SensorOutput[TotalSensorsCount];
        for (int i = 0; i < SensorOutputs.Length; i++)
            SensorOutputs[i] = new SensorOutput();
    }

    private void UpdateSensors()
    {
        Quaternion layerBaseRotation  = transform.rotation;
        float      angleBetweenLayers = Mathf.PI / (_sensorLayersCount - 1);

        int        sensorIndex        = 0;
        float      phi                = 0.0f;

        for (int layer = 0; layer < _sensorLayersCount; layer++)
        {
            int sensorsPerLayer = _sensorsPerLayer;

            if (layer == 0 || layer == _sensorLayersCount - 1)
                sensorsPerLayer = 1;

            float angleBetweenSensors = (2.0f * Mathf.PI) / sensorsPerLayer;
            float theta               = 0.0f;

            for (int sensor = 0; sensor < sensorsPerLayer; sensor++)
            {
                Vector3 baseSensor       = new Vector3(Mathf.Cos(Mathf.PI * 0.5f - phi) * Mathf.Cos(theta),
                                                       Mathf.Cos(Mathf.PI * 0.5f - phi) * Mathf.Sin(theta),
                                                       Mathf.Sin(Mathf.PI * 0.5f - phi));

                Vector3 currentDirection = layerBaseRotation * baseSensor;

                SensorOutputs[sensorIndex].CurrentDirection = currentDirection;
                SensorOutputs[sensorIndex].ObstacleDistance = -1.0f;
                SensorOutputs[sensorIndex].TargetDistance   = -1.0f;
                SensorOutputs[sensorIndex].TargetDirection  = 0.0f;

                if (Physics.Raycast(transform.position, currentDirection, out RaycastHit hitInfo, _sensorLength, _obstaclesLayerMask))
                    SensorOutputs[sensorIndex].ObstacleDistance = hitInfo.distance / _sensorLength;

                if (Physics.Raycast(transform.position, currentDirection, out hitInfo, _sensorLength, _planetsLayerMask))
                {
                    if (hitInfo.transform == TargetPlanet)
                    {
                        SensorOutputs[sensorIndex].TargetDistance = hitInfo.distance / _sensorLength;
                    }
                    else
                    {
                        SensorOutputs[sensorIndex].ObstacleDistance = SensorOutputs[sensorIndex].ObstacleDistance > 0.0f ?
                                                                      Mathf.Min(SensorOutputs[sensorIndex].ObstacleDistance, hitInfo.distance / _sensorLength) :
                                                                      hitInfo.distance / _sensorLength;
                    }
                }
                if (TargetPlanet != null)
                {
                    Vector3 toTargetPlanet = (TargetPlanet.position - transform.position).normalized;
                    SensorOutputs[sensorIndex].TargetDirection = Vector3.Dot(currentDirection, toTargetPlanet);
                }

                theta += angleBetweenSensors;

                sensorIndex++;
            }
            phi += angleBetweenLayers;
        }
    }
}

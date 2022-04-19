using UnityEngine;

public class DistantSun : MonoBehaviour
{
    [SerializeField]
    private GameObject _sunPrefab;

    [SerializeField]
    private Light      _directionalLight;

    [SerializeField, Min(1.0f)]
    private float      _sunSize;

    [SerializeField, Min(1.0f)]
    private float      _sunDistance;

    private GameObject _sun;
    
    void Start()
    {
        _sun = Instantiate(_sunPrefab);
        _sun.transform.localScale = _sunSize * Vector3.one;
    }

    void Update() => _sun.transform.position = transform.position - _directionalLight.transform.forward * _sunDistance;
    
}

using UnityEngine;

public class DistantSun : MonoBehaviour
{
    [SerializeField]
    private GameObject _sunPrefab;

    [SerializeField]
    private Transform  _directionalLight;

    [SerializeField, Min(1.0f)]
    private float      _sunSize;

    [SerializeField, Min(1.0f)]
    private float      _sunDistance;

    private GameObject _sun;
    
    private void Start()
    {
        _sun = Instantiate(_sunPrefab);
        _sun.transform.localScale = _sunSize * Vector3.one;
    }

    private void Update() => _sun.transform.position = transform.position - _directionalLight.transform.forward * _sunDistance;
}

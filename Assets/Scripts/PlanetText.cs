using UnityEngine;
using TMPro;

public class PlanetText : MonoBehaviour
{
    public Camera Camera;

    public int PlanetIndex;
    [SerializeField]
    private float _distanceToCamera;
    [SerializeField]
    private float _fadingDistanceStart;
    [SerializeField]
    private float _fadingDistanceEnd;
    private float _fadingInterval;
    private TMP_Text _planetText;
    void Start()
    {
        _planetText = GetComponentInChildren<TMP_Text>();
        _planetText.SetText("#" + (PlanetIndex + 1).ToString());
        _fadingInterval = _fadingDistanceStart - _fadingDistanceEnd;
    }

    void Update()
    {
        if (Camera is null)
            return;
        Vector3 toCamera = (Camera.transform.position - transform.position).normalized;
        float radius = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        _planetText.transform.position = transform.position + toCamera * (radius + _distanceToCamera);
        _planetText.transform.LookAt(Camera.transform.position, Camera.transform.up);
        _planetText.transform.Rotate(0.0f, 180.0f, 0.0f);
        float distance = Vector3.Distance(Camera.transform.position, transform.position);
        _planetText.alpha = Mathf.Clamp((distance - _fadingDistanceEnd) / _fadingInterval, 0.0f, 1.0f);
    }
}

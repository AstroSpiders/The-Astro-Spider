using UnityEngine;
using TMPro;

public class PlanetText : MonoBehaviour
{
    public Camera         Camera;
    public int            PlanetIndex;
    
    [SerializeField]
    private float         _distanceToCamera;
    [SerializeField]
    private float         _fadingDistanceStart;
    [SerializeField]
    private float         _fadingDistanceEnd;
    
    private float         _fadingInterval;
    private TMP_Text      _planetText;

    private void Start()
    {
        _planetText = GetComponentInChildren<TMP_Text>();
        _planetText.SetText((PlanetIndex + 1).ToString());
        _fadingInterval = _fadingDistanceStart - _fadingDistanceEnd;
    }

    private void Update()
    {
        if (Camera is null)
            return;

        var textTransform = transform;
        var textPosition = textTransform.position;
        var cameraTransform = Camera.transform;
        var cameraPosition = cameraTransform.position;
        var textLocalScale = textTransform.localScale;
        
        var toCamera = (cameraPosition - textPosition).normalized;
        var radius = Mathf.Max(textLocalScale.x, textLocalScale.y, textLocalScale.z);
        _planetText.transform.position = textPosition + toCamera * (radius + _distanceToCamera);
        _planetText.transform.LookAt(cameraPosition, cameraTransform.up);
        _planetText.transform.Rotate(0.0f, 180.0f, 0.0f);
        var distance = Vector3.Distance(cameraPosition, textPosition);
        _planetText.alpha = Mathf.Clamp((distance - _fadingDistanceEnd) / _fadingInterval, 0.0f, 1.0f);
    }
}

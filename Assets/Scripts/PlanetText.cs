using UnityEngine;
using TMPro;

public class PlanetText : MonoBehaviour
{
    public Camera Camera;

    public int PlanetIndex;
    [SerializeField]
    private float _distanceToCamera;
    private TMP_Text _planetText;
    void Start()
    {
        _planetText = GetComponentInChildren<TMP_Text>();
        _planetText.SetText("#" + (PlanetIndex + 1).ToString());
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
    }
}

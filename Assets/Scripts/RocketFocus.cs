using UnityEngine;

[RequireComponent(typeof(FocusCamera))]
// This script was created for testing purposes only to make
// the camera follow a rocket.
public class RocketFocus : MonoBehaviour
{
    [SerializeField]
    private Transform   _rocket;

    private FocusCamera _camera;
    private void Start() => _camera = GetComponent<FocusCamera>();

    private void Update()
    {
        if (_rocket is null)
        {
            Debug.Log("Please provide the RocketFollow script with a reference to a rocket.");
            return;
        }

        _camera.SetFocusPoint(_rocket.transform.position);
    }
}

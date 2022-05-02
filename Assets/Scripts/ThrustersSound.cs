using UnityEngine;

[RequireComponent(typeof(RocketState))]
[RequireComponent(typeof(RocketMovement))]
public class ThrustersSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip      _thrusterSound;
    [SerializeField, Range(-3.0f, 3.0f)] 
    private float          _mainThrusterPitch       = 1.0f;
    [SerializeField, Range(-3.0f, 3.0f)] 
    private float          _secondaryThrusterPitch  = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)]
    private float          _mainThrusterVolume      = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)]
    private float          _secondaryThrusterVolume = 0.5f;
    [SerializeField, Range(0.0f, 100.0f)] 
    private float          _decreaseVolumeBy        = 10.0f;
    [SerializeField, Range(0.0f, 100.0f)]           
    private float          _increaseVolumeBy        = 10.0f;

    private RocketState    _state;
    private RocketMovement _rocketMovement;

    private AudioSource[]  _audioSources;
    
    private void Start()
    {
        _state          = GetComponent<RocketState>();
        _rocketMovement = GetComponent<RocketMovement>();
        
        for (var i = 0; i < _rocketMovement.Thrusters.Length; i++)
            gameObject.AddComponent(typeof(AudioSource));

        _audioSources = gameObject.GetComponents<AudioSource>();

        for (var i = 0; i < _rocketMovement.Thrusters.Length; i++)
        {
            var audioSource        = _audioSources[i];
                audioSource.clip   = _thrusterSound;
                audioSource.loop   = true;
                audioSource.pitch  = i == (int)RocketMovement.ThrusterTypes.Main ? _mainThrusterPitch : _secondaryThrusterPitch;
                audioSource.volume = 0.0f;

            audioSource.Play();
        }
    }

    private void LateUpdate()
    {
        for (var i = 0; i < _rocketMovement.Thrusters.Length; i++)
        {
            var   thruster               = _rocketMovement.Thrusters[i];
            var   audioSource            = _audioSources[i];
            var   acceleration       = thruster.Acceleration;

            var   targetVolume       = (i == (int)RocketMovement.ThrusterTypes.Main ? _mainThrusterVolume : _secondaryThrusterVolume) * acceleration;

            if (_state.Dead || !_state.HasFuel)
                targetVolume = 0.0f;

            var audioVolume = audioSource.volume;
            audioSource.volume = Mathf.MoveTowards(audioVolume,
                                                   targetVolume,
                                                   audioVolume < targetVolume ? _increaseVolumeBy : _decreaseVolumeBy * Time.deltaTime);
        }
    }
}
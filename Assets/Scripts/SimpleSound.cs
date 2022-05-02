using UnityEngine;

public class SimpleSound : MonoBehaviour
{
    [SerializeField] 
    private AudioClip      _sound1;
    [SerializeField] 
    private AudioClip      _sound2;
    [SerializeField, Range(0.0f, 1.0f)] 
    private float          _soundVolume  = 1.0f;

    private AudioSource    _audioSource;

    private const float    _maxDistanceSound1 = 100.0f;
    private const float    _maxDistanceSound2 = 200.0f;

    public int             SoundToPlay;
    public bool            IsExplosionSound;

    private void Start()
    {
        _audioSource = (AudioSource) gameObject.AddComponent(typeof(AudioSource));

        _audioSource.clip   = SoundToPlay == 0 ? _sound1 : _sound2;
        _audioSource.loop   = false;
        _audioSource.volume = _soundVolume;

        if (IsExplosionSound)
        {
            _audioSource.spatialBlend = 1.0f;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.maxDistance = SoundToPlay == 0 ? _maxDistanceSound1 : _maxDistanceSound2;
        }
        
        _audioSource.Play();
    }
}

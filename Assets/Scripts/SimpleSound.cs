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

    public int SoundToPlay;

    private void Start()
    {
        _audioSource = (AudioSource) gameObject.AddComponent(typeof(AudioSource));
        
        _audioSource.clip   = SoundToPlay == 0 ? _sound1 : _sound2;
        _audioSource.loop   = false;
        _audioSource.volume = _soundVolume;
        _audioSource.Play();
    }
}

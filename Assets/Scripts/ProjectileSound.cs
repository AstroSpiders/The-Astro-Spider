using UnityEngine;

public class ProjectileSound : MonoBehaviour
{
    [SerializeField] 
    private AudioClip      _projectileSound;
    [SerializeField, Range(0.0f, 1.0f)] 
    private float          _projectileSoundVolume  = 1.0f;

    private AudioSource    _audioSource;

    private void Start()
    {
        _audioSource = (AudioSource) gameObject.AddComponent(typeof(AudioSource));

        _audioSource.clip   = _projectileSound;
        _audioSource.loop   = false;
        _audioSource.volume = _projectileSoundVolume;
        _audioSource.Play();
    }
}

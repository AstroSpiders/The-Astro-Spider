using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RocketMovement))]
public class ThrustersSound : MonoBehaviour
{
    [SerializeField] private AudioSource _thrustersSound;

    [SerializeField, Range(0.0f, 3.0f)] private float _mainThrusterPitch;
    [SerializeField, Range(0.0f, 3.0f)] private float _secondaryThrusterPitch;

    [SerializeField, Range(0.0f, 10.0f)] private float _decreaseVolumeBy;
    [SerializeField, Range(0.0f, 10.0f)] private float _increaseVolumeBy;

    private List<AudioSource> _thrusterAudioSources = new List<AudioSource>();
    private List<float> _desiredVolumes = new List<float>();

    private RocketMovement _rocketMovement;

    private void Awake()
    {
        _rocketMovement = GetComponent<RocketMovement>();
        InitializeThrustersList(_rocketMovement.GetThrusters().Count);
    }

    private void InitializeThrustersList(int length)
    {
        for (int i = 0; i < length; ++i)
        {
            var audioSource = Instantiate(_thrustersSound);
            audioSource.gameObject.SetActive(true);
            if (i == (int) RocketMovement.ThrusterTypes.Main)
                audioSource.pitch = _mainThrusterPitch;
            else
                audioSource.pitch = _secondaryThrusterPitch;
            _thrusterAudioSources.Add(audioSource);
            _desiredVolumes.Add(0);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _desiredVolumes.Count; i++)
        {
            if (_thrusterAudioSources[i].volume > _desiredVolumes[i])
                _thrusterAudioSources[i].volume -= Time.deltaTime * _decreaseVolumeBy;
            else if (_thrusterAudioSources[i].volume < _desiredVolumes[i])
                _thrusterAudioSources[i].volume += Time.deltaTime * _increaseVolumeBy;
        }
    }

    private void LateUpdate()
    {
        UpdateThrusterSound(_rocketMovement.GetThrusters());
    }

    private void UpdateThrusterSound(List<RocketMovement.Thruster> thrusters)
    {
        for (int i = 0; i < thrusters.Count; i++)
            _desiredVolumes[i] = i == (int) RocketMovement.ThrusterTypes.Main ? thrusters[i].Acceleration : thrusters[i].Acceleration / 2;
    }
}
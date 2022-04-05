using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class ThrustersSound : MonoBehaviour
{
    private float _desiredThrusterVolume = 0;
    [SerializeField] private AudioSource _thrustersSound;

    private void FixedUpdate()
    {
        if (_thrustersSound.volume > _desiredThrusterVolume)
        {
            _thrustersSound.volume -= (float) 0.08;
        }
        else if (_thrustersSound.volume < _desiredThrusterVolume)
        {
            _thrustersSound.volume += (float) 0.04;
        }
    }

    public void UpdateThrusterSound(List<float> _accelerations)
    {
        float maximum = 0;
        for (int i = 0; i < _accelerations.Count; i++)
        {
            if (_accelerations[i] > maximum)
                maximum = _accelerations[i];
        }
        _desiredThrusterVolume = maximum;
    } 
}
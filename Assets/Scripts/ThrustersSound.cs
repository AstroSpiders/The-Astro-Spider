using System;
using System.Collections.Generic;
using UnityEngine;

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

    public void UpdateThrusterSound(List<float> accelerations = null)
    {
        float maximum = 0;
        bool mainThruster = accelerations[0]>0;
        if (accelerations != null)
        {
            for (int i = 0; i < accelerations.Count; i++)
            {
                if (i == 0)
                {
                    maximum = accelerations[i];
                }
                else if (accelerations[i] / 2 > maximum)
                {
                    maximum = accelerations[i] / 2;
                }
            }
        }

        _desiredThrusterVolume = maximum;
        if (mainThruster)
        {
            _thrustersSound.pitch = (float)1.5;
        }
        else
        {
            _thrustersSound.pitch = (float)0.5;
        }
    }
}
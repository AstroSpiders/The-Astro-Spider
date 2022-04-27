using UnityEngine;

[RequireComponent(typeof(RocketState))]
[RequireComponent(typeof(RocketMovement))]
public class RocketParticles : MonoBehaviour
{
    private const float            _bias                    = 0.001f;
    
    [SerializeField]
    private       ParticleSystem   _mainParticleSystem;
    [SerializeField]
    private       ParticleSystem   _secondaryParticleSystem;

    [SerializeField]
    private       float            _mainThrusterHeightOffset       = 0.2f;
    [SerializeField]
    private       float            _secondaryThrustersHeightOffset = 0.3f;

    [SerializeField]
    private float                  _mainThrusterLeftOffset;
    [SerializeField]
    private       float            _secondaryThrusterLeftOffset    = 0.2f;

    [SerializeField]
    private       float            _mainParticlesScale             = 1.0f;
    [SerializeField]                                               
    private       float            _secondaryParticlesScale        = 1.0f;

    private       RocketState      _state;
    private       RocketMovement   _rocketMovement;
    private       ParticleSystem[] _particleSystems;

    void Start()
    {
        _state           = GetComponent<RocketState>();
        _rocketMovement  = GetComponent<RocketMovement>();
        _particleSystems = new ParticleSystem[_rocketMovement.Thrusters.Length];

        for (int i = 0; i < _rocketMovement.Thrusters.Length; i++)
        {
            ParticleSystem particleSystemPrefab;
            float          heightOffset;
            float          leftOffset;

            if (i == (int)RocketMovement.ThrusterTypes.Main)
            {
                particleSystemPrefab = _mainParticleSystem;
                heightOffset         = _mainThrusterHeightOffset;
                leftOffset           = _mainThrusterLeftOffset;
            }
            else
            {
                particleSystemPrefab = _secondaryParticleSystem;
                heightOffset         = _secondaryThrustersHeightOffset;
                leftOffset           = _secondaryThrusterLeftOffset;
            }

            var particle             = Instantiate(particleSystemPrefab);

            if (_rocketMovement.Thrusters[i].GameObject is null)
            {
                Debug.Log("All gameobjects for the Rocket thrusters must be set before attaching the RocketPrefab component.");
                continue;
            }

            var particleTransform = particle.transform;

            particleTransform.parent         = _rocketMovement.Thrusters[i].GameObject.transform;
            particle.transform.localRotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            particleTransform.localPosition  = Vector3.left * leftOffset;

            particle.transform.Rotate(new Vector3(particleTransform.localEulerAngles.z, 0.0f, 0.0f));
            particle.transform.Translate(Vector3.up * heightOffset);

            particle.gameObject.SetActive(false);
            
            _particleSystems[i] = particle;
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < _rocketMovement.Thrusters.Length; i++)
        {
            var thruster       = _rocketMovement.Thrusters[i];
            var particle       = _particleSystems[i];

            if (thruster.Acceleration >= _bias && 
                _state.HasFuel                 && 
                !_state.Dead)
            {
                particle.gameObject.SetActive(true);
                particle.transform.localScale = thruster.Acceleration * 
                                                (i == (int) RocketMovement.ThrusterTypes.Main
                                                    ? _mainParticlesScale
                                                    : _secondaryParticlesScale) *
                                                Vector3.one;
            }
            else
            {
                particle.gameObject.SetActive(false);
            }
        }
    }

    public bool IsParticleSystemActive()
    {
        foreach (var p in _particleSystems)
        {
            if (p.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }
}

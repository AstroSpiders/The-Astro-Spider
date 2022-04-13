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
    private float                  _mainThrusterLeftOffset         = 0.0f;
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

            var particleSystem       = Instantiate(particleSystemPrefab);

            if (_rocketMovement.Thrusters[i].GameObject is null)
            {
                Debug.Log("All gameobjects for the Rocket thrusters must be set before attaching the RocketPrefab component.");
                continue;
            }

            particleSystem.transform.parent        = _rocketMovement.Thrusters[i].GameObject.transform;
            particleSystem.transform.localRotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            particleSystem.transform.localPosition = Vector3.left * leftOffset;

            particleSystem.transform.Rotate(new Vector3(particleSystem.transform.localEulerAngles.z, 0.0f, 0.0f));
            particleSystem.transform.Translate(Vector3.up * heightOffset);

            particleSystem.gameObject.SetActive(false);
            
            _particleSystems[i] = particleSystem;
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < _rocketMovement.Thrusters.Length; i++)
        {
            var thruster       = _rocketMovement.Thrusters[i];
            var particleSystem = _particleSystems[i];

            if (thruster.Acceleration >= _bias && 
                _state.HasFuel                 && 
                !_state.Dead)
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.transform.localScale = Vector3.one           * 
                                                      thruster.Acceleration * 
                                                      (i == (int)RocketMovement.ThrusterTypes.Main ? _mainParticlesScale : _secondaryParticlesScale);
            }
            else
            {
                particleSystem.gameObject.SetActive(false);
            }
        }
    }
}

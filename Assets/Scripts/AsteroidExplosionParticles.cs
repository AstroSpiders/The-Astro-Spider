using UnityEngine;

public class AsteroidExplosionParticles : MonoBehaviour
{
    [SerializeField] 
    private ParticleSystem        _explosionParticleSystem;

    private const float           _farDistance                = 80.0f;

    public Vector3                SpawnPosition;
    public Vector3                RocketPosition;

    private void Start()
    {
        var particle = Instantiate(_explosionParticleSystem);
        var particleTransform = particle.transform;
        particleTransform.position = SpawnPosition;
        
        var particleSound = particle.GetComponent<SimpleSound>();
        if (particleSound != null)
        {
            var dist = Distance(RocketPosition, SpawnPosition);
            particleSound.SoundToPlay = dist < _farDistance ? 0 : 1;
        }
    }
    
    private float Distance(Vector3 p1, Vector3 p2) => Vector3.Distance(p1, p2);
}

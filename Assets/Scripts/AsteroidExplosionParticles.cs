using UnityEngine;

public class AsteroidExplosionParticles : MonoBehaviour
{
    [SerializeField] 
    private ParticleSystem        _explosionParticleSystem;

    public Vector3                SpawnPosition;

    private void Start()
    {
        var particle = Instantiate(_explosionParticleSystem);
        var particleTransform = particle.transform;
        particleTransform.position = SpawnPosition;
    }
}

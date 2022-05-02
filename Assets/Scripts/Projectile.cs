using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody     _projectileRigidbody;
    
    private const float   _speed                  = 50.0f;
    private const float   _lifespan               = 5.0f;

    private void Start()
    {
        _projectileRigidbody = GetComponent<Rigidbody>();

        if (_projectileRigidbody == null)
            return;
        
        _projectileRigidbody.velocity = transform.up * _speed;
        Destroy(gameObject, _lifespan);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other == null)
            return;

        if (other.gameObject.GetComponent<Asteroid>() != null)
        {
            Destroy(other.gameObject);
        }
        
        if (!(other.gameObject.GetComponent<RocketState>() != null ||
            other.gameObject.GetComponent<RocketMovement>() != null))
        {
            Destroy(gameObject);
        }
    }
}

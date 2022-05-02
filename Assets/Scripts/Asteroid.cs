using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : MonoBehaviour
{
    public  float     InitialImpulseForce { get;         set; } = 1.0f;
            
    public  bool      HitPlanet           { get; private set; }

    private Rigidbody _body;

    public void ApplyInitalImpulse()
    {
        Vector3 gravity      = CustomGravity.GetGravity(transform.position);
        float   gravityForce = gravity.magnitude;
        Vector3 tangent      = ArbitraryOrthogonal(gravity).normalized * gravityForce;

        _body.AddForce(tangent * InitialImpulseForce, ForceMode.Acceleration);
    }

    private void Start()
    {
        var asteroidExplosionParticles = GetComponent<AsteroidExplosionParticles>();
        
        if (asteroidExplosionParticles != null)
            asteroidExplosionParticles.enabled = false;
    }

    private void Awake() => _body = GetComponent<Rigidbody>();
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Planet"))
            HitPlanet = true;
    }

    //based on https://stackoverflow.com/questions/11132681/what-is-a-formula-to-get-a-vector-perpendicular-to-another-vector
    private Vector3 ArbitraryOrthogonal(Vector3 vec)
    {
        bool b0 = (vec[0] <  vec[1]) && (vec[0] <  vec[2]);
        bool b1 = (vec[1] <= vec[0]) && (vec[1] <  vec[2]);
        bool b2 = (vec[2] <= vec[0]) && (vec[2] <= vec[1]);

        return Vector3.Cross(vec, new Vector3(b0 ? 1 : 0, b1 ? 1 : 0, b2 ? 1 : 0));
    }
}

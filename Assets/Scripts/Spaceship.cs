using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spaceship : MonoBehaviour
{
    public bool        HitPlanet { get; private set; } = false;
    public  Vector3    MinBoxCorner { get; set; }
    public  Vector3    MaxBoxCorner { get; set; }

    [SerializeField]
    private float      _speed                      = 10.0f;
                                                   
    [SerializeField]                               
    private float      _adjustVelocitySpeed        = 10.0f;

    [SerializeField]
    private float      _adjustAngularVelocitySpeed = 10.0f;
                                              
    [SerializeField]                          
    private float      _separationRadius           = 5.0f,
                       _separationIntensity        = 10.0f;
                                                   
    [SerializeField]                               
    private float      _alignmentRadius            = 20.0f,
                       _alignmentIntensity         = 5.0f;
                                                   
    [SerializeField]                               
    private float      _cohesionRadius             = 20.0f,
                       _cohesionIntensity          = 5.0f;

    [SerializeField]
    private float      _remainConstrainedIntensity = 20.0f;
                                                   
    [SerializeField]                               
    private float      _perturbRotationFactor      = 1.0f;
                                                   
    [SerializeField]                               
    private LayerMask  _spaceshipsLayerMask        = -1;
                                                   
    [SerializeField]                               
    private LayerMask  _obstaclesLayerMask         = -1;

    private Rigidbody  _body;
    private Quaternion _targetRotation;

    private void Awake() => _body = GetComponent<Rigidbody>();

    private void Update()
    {
        _targetRotation = transform.rotation;
        _targetRotation = Quaternion.RotateTowards(_targetRotation, Separation(),                        Time.deltaTime * _separationIntensity);
        _targetRotation = Quaternion.RotateTowards(_targetRotation, Alignment(),                         Time.deltaTime * _alignmentIntensity);
        _targetRotation = Quaternion.RotateTowards(_targetRotation, Cohesion(),                          Time.deltaTime * _cohesionIntensity);
        _targetRotation = Quaternion.RotateTowards(_targetRotation, Random.rotation,                     Time.deltaTime * _perturbRotationFactor);
        _targetRotation = Quaternion.RotateTowards(_targetRotation, RemainConstrainedX(_targetRotation), Time.deltaTime * _remainConstrainedIntensity);
        _targetRotation = Quaternion.RotateTowards(_targetRotation, RemainConstrainedY(_targetRotation), Time.deltaTime * _remainConstrainedIntensity);
        _targetRotation = Quaternion.RotateTowards(_targetRotation, RemainConstrainedZ(_targetRotation), Time.deltaTime * _remainConstrainedIntensity);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Planet"))
            HitPlanet = true;
    }

    void FixedUpdate()
    {
        Vector3    desiredVelocity        = transform.forward.normalized * _speed;
        Vector3    currentVelocity        = _body.velocity;
                                          
        Quaternion toDesiredRotation      = _targetRotation * Quaternion.Inverse(transform.rotation);

        Vector3    desiredAngularVelocity = CustomPhysics.QuaternionToAngularVelocity(toDesiredRotation);

        Vector3    currentAngularVelocity = _body.angularVelocity;

        _body.inertiaTensor         = Vector3.one;
        _body.inertiaTensorRotation = Quaternion.identity;

        _body.velocity              = Vector3.MoveTowards(currentVelocity,        desiredVelocity,        _adjustVelocitySpeed        * Time.deltaTime);

        if (!float.IsNaN(desiredAngularVelocity.x) && !float.IsNaN(desiredAngularVelocity.y) && !float.IsNaN(desiredAngularVelocity.z))
            _body.angularVelocity = Vector3.MoveTowards(currentAngularVelocity, desiredAngularVelocity, _adjustAngularVelocitySpeed * Time.deltaTime);
    }

    private Quaternion RemainConstrainedX(Quaternion rotation)
    {
        if (transform.position.x < MinBoxCorner.x)
            return Quaternion.LookRotation(Vector3.right);
        if (transform.position.x > MaxBoxCorner.x)
            return Quaternion.LookRotation(Vector3.left);

        return rotation;
    }

    private Quaternion RemainConstrainedY(Quaternion rotation)
    {
        if (transform.position.y < MinBoxCorner.y)
            return Quaternion.LookRotation(Vector3.up);
        if (transform.position.y > MaxBoxCorner.y)
            return Quaternion.LookRotation(Vector3.down);

        return rotation;
    }
    private Quaternion RemainConstrainedZ(Quaternion rotation)
    {
        if (transform.position.z < MinBoxCorner.z)
            return Quaternion.LookRotation(Vector3.forward);
        if (transform.position.z > MaxBoxCorner.z)
            return Quaternion.LookRotation(Vector3.back);

        return rotation;
    }

    private Quaternion Separation()
    {
        HashSet<GameObject> hitObjects = GetNearObjects(_separationRadius, _spaceshipsLayerMask | _obstaclesLayerMask);

        if (hitObjects.Count <= 0)
            return transform.rotation;

        Vector3 center = Vector3.zero;
        float averageWeight = 1.0f / hitObjects.Count;
        foreach (var hitObject in hitObjects)
            center += hitObject.transform.position * averageWeight;

        return Quaternion.Inverse(Quaternion.LookRotation(center, transform.up));
    }

    private Quaternion Alignment()
    {
        HashSet<GameObject> hitObjects = GetNearObjects(_alignmentRadius, _spaceshipsLayerMask);

        if (hitObjects.Count <= 0)
            return transform.rotation;

        Quaternion result = Quaternion.identity;

        float averageWeight = 1.0f / hitObjects.Count;
        foreach (var hitObject in hitObjects)
        {
            Quaternion q = hitObject.transform.rotation;
            result *= Quaternion.Slerp(Quaternion.identity, q, averageWeight);
        }

        return result;
    }

    private Quaternion Cohesion()
    {
        HashSet<GameObject> hitObjects = GetNearObjects(_cohesionRadius, _spaceshipsLayerMask);

        if (hitObjects.Count <= 0)
            return transform.rotation;

        Vector3 center = Vector3.zero;
        float averageWeight = 1.0f / hitObjects.Count;
        foreach (var hitObject in hitObjects)
            center += hitObject.transform.position * averageWeight;

        return Quaternion.LookRotation(center, transform.up);
    }

    private HashSet<GameObject> GetNearObjects(float radius, LayerMask layerMask) => new HashSet<GameObject>(Physics.OverlapSphere(transform.position, radius, layerMask).Where(x => x.gameObject != gameObject).Select(x => x.gameObject));
}

using UnityEngine;

public class RocketState : MonoBehaviour
{
    public bool Dead { get; private set; } = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
            Dead = true;
    }
}

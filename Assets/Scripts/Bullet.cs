using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float life = 3;

    void Awake()
    {
        Destroy(gameObject, life);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("PoopMine"))
        {
            Destroy(gameObject);
        }
    }
}

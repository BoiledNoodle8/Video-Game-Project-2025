using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int ownerId = 0;
    public int remainingBounces = 3;
    public float lifeTime = 6f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // If hit a wall or other solid object -> reflect
        if (collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            if (collision.contactCount > 0)
            {
                ContactPoint2D contact = collision.contacts[0];
                Vector2 incoming = rb.velocity.normalized;
                Vector2 reflected = Vector2.Reflect(incoming, contact.normal);
                float speed = rb.velocity.magnitude;
                rb.velocity = reflected * speed;

                remainingBounces--;
                if (remainingBounces < 0)
                {
                    Explode();
                }
            }
            return;
        }

        // If hit a tank
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            // Damage logic
            Health h = collision.gameObject.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(1, ownerId);
            }

            Explode();
            return;
        }

        // Other collisions -> destroy
        Explode();
    }

    void Explode()
    {
        // NOTE: We intentionally do NOT spawn a generic hit effect here.
        // Tank death effects should be spawned by the Health.Die(...) method,
        // so visual effects only occur on actual tank deaths.
        Destroy(gameObject);
    }
}
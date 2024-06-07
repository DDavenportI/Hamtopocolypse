using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Tank
    private Rigidbody2D rb2d;
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer for flashing
    public Animator animator; // Reference to the Animator component
    public float maxSpeed = 3; // Maximum speed of tank
    public float rotationSpeed = 200; // Turning speed of tank
    public float accelerationRate = 3f; // Rate at which the tank accelerates
    public float decelerationRate = 5f; // Rate at which the tank decelerates when no input is given
    private float currentSpeed = 0f; // Current speed of the tank
    public int hitPoints = 3; // Number of hit points for the tank
    public float invincibilityDuration = 1f; // Duration of invincibility in seconds
    public float flashInterval = 0.1f; // Interval for flashing during invincibility
    private bool isInvincible = false; // Indicates if the tank is invincible
    private Coroutine invincibilityCoroutine; // Reference to the invincibility coroutine
    [SerializeField] private int bounceX;
    [SerializeField] private int bounceY;

    // Bullet
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // Point from which bullets are fired
    public float bulletSpeed = 10f; // Speed of the bullet
    public float firingRate = 0.5f; // Rate at which bullets can be fired
    public float recoilRate = 2f; // Rate at which recoil is applied
    private float lastFireTime = 0f; // Time since last fire
    private float recoilDuration = 0f; // Duration of sustained firing

    // Input Controls
    public string horizontalAxis;
    public string verticalAxis;
    public string fireButton;

    private void Awake()
    {
        rb2d = GetComponentInParent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        // Calculate the rotation angle in degrees
        float rotationAngle = transform.eulerAngles.z;

        // Determine the animation state (30-degree segments)
        int animationState = Mathf.RoundToInt(rotationAngle / 30);

        //Debug.Log(animationState);

        // Update the animator parameters
        animator.SetFloat("Rotation", animationState);

        if (currentSpeed != 0) animator.speed = 1;
        else animator.speed = 0;
    }

    private void FixedUpdate()
    {
        // Input values
        float vertical = Input.GetAxis(verticalAxis);
        float horizontal = Input.GetAxis(horizontalAxis);
        bool isFiring = Input.GetButton(fireButton);

        // Accelerate or decelerate based on the input
        if (vertical != 0)
        {
            currentSpeed += vertical * accelerationRate * Time.deltaTime;
            // Clamp the speed to maxSpeed and -maxSpeed
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else
        {
            // Gradually decelerate to a stop when no input is given
            if (currentSpeed > 0)
            {
                currentSpeed -= decelerationRate * Time.deltaTime;
                if (currentSpeed < 0) currentSpeed = 0;
            }
            else if (currentSpeed < 0)
            {
                currentSpeed += decelerationRate * Time.deltaTime;
                if (currentSpeed > 0) currentSpeed = 0;
            }
        }

        // Move the tank based on the current speed
        rb2d.velocity = transform.up * currentSpeed;

        // Turn the tank
        rb2d.MoveRotation(rb2d.rotation + (-horizontal) * rotationSpeed * Time.deltaTime);

        // Handle bullet firing and recoil
        if (isFiring)
        {
            if (Time.time - lastFireTime >= firingRate)
            {
                lastFireTime = Time.time;
                FireBullet();
            }

            // Apply recoil
            if (((recoilDuration * recoilRate) < maxSpeed * 0.5) || ((recoilDuration * recoilRate) > -maxSpeed * 0.5))
            {
                recoilDuration += Time.deltaTime; // Increment the recoil duration
            }
            rb2d.AddForce(-transform.up * recoilDuration * recoilRate, ForceMode2D.Impulse);
        }
        else
        {
            // Reset recoil duration if not firing
            if (recoilDuration > 0) recoilDuration -= Time.deltaTime;
            else if (recoilDuration < 0) recoilDuration = 0f;
            rb2d.AddForce(-transform.up * recoilDuration * recoilRate, ForceMode2D.Impulse);
        }
    }

    private void FireBullet()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = transform.up * bulletSpeed;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !isInvincible)
        {
            TakeDamage();
        }
        else if (collision.gameObject.CompareTag("Barrier"))
        {
            if (recoilDuration > 0) recoilDuration = 0;
            //Vector2 currentVel = rb2d.velocity;
            //Vector2 surfaceNormal = collision.contacts[0].normal;
            //currentSpeed = 0;
            //StartCoroutine(WaitForABit());
            //rb2d.AddForce(new Vector2(-currentVel.x + bounceX, -currentVel.y + bounceY), ForceMode2D.Impulse);
            //currentSpeed = -rb2d.velocity.magnitude;
            //rb2d.velocity = Vector2.Reflect(currentVel, surfaceNormal);
            //rb2d.MoveRotation(rb2d.rotation * rotationSpeed);
            //Debug.Log(rb2d.velocity);

            Vector2 normal = collision.contacts[0].normal; // The normal vector of the collision
            Vector2 incomingVelocity = rb2d.velocity; // The current velocity

            // Reflect the velocity based on the collision normal, applying energy loss
            Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal);

            // Rotate the tank to face the direction of the new velocity
            float angle = Mathf.Atan2(reflectedVelocity.y, reflectedVelocity.x) * Mathf.Rad2Deg;
            rb2d.MoveRotation(90); // Offset by -90 degrees since the tank's forward is the "up" direction
            
            currentSpeed = 0;

            StartCoroutine(WaitForABit());

            rb2d.velocity = reflectedVelocity; // Set the new velocity

            // Rotate the tank to face the direction of the new velocity
            //float angle = Mathf.Atan2(reflectedVelocity.y, reflectedVelocity.x) * Mathf.Rad2Deg;
            //rb2d.MoveRotation(angle - 90); // Offset by -90 degrees since the tank's forward is the "up" direction
        }
    }

    public void TakeDamage()
    {
        hitPoints--; // Decrement hit points

        if (hitPoints <= 0)
        {
            DestroyTank(); // Destroy the tank if no hit points are left
        }
        else
        {
            StartInvincibility(); // Trigger invincibility frames
        }
    }

    private void StartInvincibility()
    {
        isInvincible = true;
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        float elapsedTime = 0f;
        bool visible = false;

        while (elapsedTime < invincibilityDuration)
        {
            // Toggle sprite visibility to create a flashing effect
            visible = !visible;
            spriteRenderer.enabled = visible;

            elapsedTime += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        // Reset sprite visibility and end invincibility
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void DestroyTank()
    {
        // Optionally, trigger any additional effects like explosions before destroying the tank
        Destroy(gameObject);
    }

    IEnumerator WaitForABit()
    {
        yield return new WaitForSeconds(0.1f);
    }
}
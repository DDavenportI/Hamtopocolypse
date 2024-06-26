using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Tank
    private Rigidbody2D rb2d;
    private CapsuleCollider2D ball;
    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer for flashing
    [SerializeField] private Animator animator; // Reference to the Animator component
    [SerializeField] private float minSpeedThreshold = 0.1f; // Minimum speed threshold for animator
    [SerializeField] private float maxSpeed = 3; // Maximum speed of tank
    [SerializeField] private float rotationSpeed = 200; // Turning speed of tank
    [SerializeField] private float accelerationRate = 3f; // Rate at which the tank accelerates
    [SerializeField] private float decelerationRate = 5f; // Rate at which the tank decelerates when no input is given
    [SerializeField] private float currentSpeed = 0f; // Current speed of the tank
    public int hitPoints = 3; // Number of hit points for the tank
    [SerializeField] private GameObject hitpointsDisplay; // Parent GameObject holding the hitpoint sprites
    private SpriteRenderer[] hitpointRenderers; // Array of SpriteRenderer components representing hitpoints
    [SerializeField] private float invincibilityDuration = 1f; // Duration of invincibility in seconds
    [SerializeField] private float flashInterval = 0.1f; // Interval for flashing during invincibility
    private bool isInvincible = false; // Indicates if the tank is invincible
    private Coroutine invincibilityCoroutine; // Reference to the invincibility coroutine
    [SerializeField] private int bounceX;
    [SerializeField] private int bounceY;

    // Bullet
    [SerializeField] private GameObject bulletPrefab; // Reference to the bullet prefab
    [SerializeField] private Transform firePoint; // Point from which bullets are fired
    [SerializeField] private float bulletSpeed = 10f; // Speed of the bullet
    [SerializeField] private float firingRate = 0.5f; // Rate at which bullets can be fired
    [SerializeField] private float recoilRate = 2f; // Rate at which recoil is applied
    private float lastFireTime = 0f; // Time since last fire
    private float recoilDuration = 0f; // Duration of sustained firing

    // Input Controls
    [SerializeField] private string horizontalAxis;
    [SerializeField] private string verticalAxis;
    [SerializeField] private string fireButton;

    // Conveyor Belt
    [SerializeField] private LayerMask conveyorBeltLayer; // LayerMask to identify speed boost panels
    private bool conveyorBeltContact;

    // Barrier
    private bool barrierContact;
    private float collisionMoveAwayForce = -1f; // Force to move away slightly from the barrier
    private Vector2 normal;

    private void Awake()
    {
        rb2d = GetComponentInParent<Rigidbody2D>();
        ball = GetComponent<CapsuleCollider2D>();
        hitpointRenderers = hitpointsDisplay.GetComponentsInChildren<SpriteRenderer>();
        UpdateHitpointDisplay();
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

        // Apply minimum speed threshold
        float displayedSpeed = Mathf.Abs(currentSpeed) > minSpeedThreshold ? currentSpeed : 0;

        // Update the animator parameters
        animator.SetFloat("Rotation", animationState);
        animator.SetFloat("Speed", displayedSpeed);

        // Set animator speed based on movement
        animator.speed = displayedSpeed != 0 ? 1 : 0;

        // Debugging
        //Debug.Log($"Animation State: {animationState}, Speed: {displayedSpeed}");
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

        if (conveyorBeltContact) CheckForSpeedBoostPanel();
        if (barrierContact) InvertVelocityOnCollision(normal);
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
            barrierContact = true;
            normal = collision.contacts[0].normal; // Collision normal
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Barrier"))
        {
            barrierContact = false;
            normal = Vector2.zero;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("ConveyorBelt"))
        {
            conveyorBeltContact = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ConveyorBelt"))
        {
            conveyorBeltContact = false;
        }
    }

    private void CheckForSpeedBoostPanel()
    {
        // Define the size and position of the overlap box
        Vector2 size = ball.size;
        Vector2 position = rb2d.position;

        // Check for collisions with speed boost panels
        Collider2D hitCollider = Physics2D.OverlapBox(position, size, 0f, conveyorBeltLayer);
        if (hitCollider != null)
        {
            ConveyorBelt boostPanel = hitCollider.GetComponent<ConveyorBelt>();
            if (boostPanel != null)
            {
                ApplySpeedBoost(boostPanel);
            }
        }
    }

    private void ApplySpeedBoost(ConveyorBelt boostPanel)
    {
        Vector2 boostDirection = boostPanel.boostDirection;
        float boostForce = boostPanel.boostForce;
        rb2d.AddForce(boostDirection.normalized * boostForce, ForceMode2D.Impulse);
    }

    private void InvertVelocityOnCollision(Vector2 normal)
    {
        //Vector2 normal = collision.contacts[0].normal; // Collision normal
        if (normal != Vector2.zero)
        {
            Vector2 moveAwayDirection = normal.normalized;

            // Apply a small force to move the tank slightly away from the barrier
            rb2d.AddForce(moveAwayDirection * collisionMoveAwayForce, ForceMode2D.Impulse);

            Vector2 currentVel = rb2d.velocity;
            currentSpeed = 0;
            StartCoroutine(WaitForABit());
            rb2d.AddForce(new Vector2(-currentVel.x + bounceX, -currentVel.y + bounceY), ForceMode2D.Impulse);
            rb2d.velocity = Vector2.Reflect(currentVel, normal);
        }
    }

    public void TakeDamage()
    {
        hitPoints--; // Decrement hit points
        UpdateHitpointDisplay();

        if (hitPoints <= 0)
        {
            DestroyTank(); // Destroy the tank if no hit points are left
        }
        else
        {
            StartInvincibility(); // Trigger invincibility frames
        }
    }

    private void UpdateHitpointDisplay()
    {
        for (int i = 0; i < hitpointRenderers.Length; i++)
        {
            if (i < hitPoints)
            {
                hitpointRenderers[i].enabled = true;
            }
            else
            {
                hitpointRenderers[i].enabled = false;
            }
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
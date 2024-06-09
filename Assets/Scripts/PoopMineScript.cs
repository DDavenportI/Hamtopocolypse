using UnityEngine;

public class PoopMineScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject blueHamster;
    public GameObject pinkHamster;
    private PlayerMovement blueHamPm;
    private PlayerMovement pinkHamPm; 
    public float timeUnhidden;
    private float timer;
    public float timeToDetonate;
    private bool poopTriggered = false;
    private bool collisionWithBlueHam = false;
    private bool collisionWithPinkHam = false;
    private CountdownTimer countdown;

    // Start is called before the first frame update
    void Start()
    {
        blueHamPm = blueHamster.GetComponent<PlayerMovement>();
        pinkHamPm = pinkHamster.GetComponent<PlayerMovement>();
        countdown = GameObject.Find("Countdown").GetComponent<CountdownTimer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (countdown.countdown == 0)
        {
            if (timer > timeUnhidden && !poopTriggered) {
                spriteRenderer.enabled = false;
            } else if (timer > timeToDetonate && poopTriggered) {
                if (collisionWithBlueHam) {
                    blueHamPm.TakeDamage();
                } else if (collisionWithPinkHam) {
                    pinkHamPm.TakeDamage();
                }
                Destroy(gameObject);
            } else {
                timer += Time.deltaTime;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject == blueHamster) {
            collisionWithBlueHam = true;
            spriteRenderer.enabled = true;
            poopTriggered = true;
            timer = 0;
        } else if (collision.gameObject == pinkHamster) {
            collisionWithPinkHam = true;
            spriteRenderer.enabled = true;
            poopTriggered = true;
            timer = 0;
        } else {
            // Do nothing
        }
    }
}

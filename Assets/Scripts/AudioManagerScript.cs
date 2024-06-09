using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    public AudioClip backgroundMusic;
    private CountdownTimer countdown;

    void Start()
    {
        countdown = GameObject.Find("Countdown").GetComponent<CountdownTimer>();
    }

    private void Update()
    {
        if (countdown.countdown == 0)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            gameObject.GetComponent<AudioManagerScript>().enabled = false;
        }
    }
}

using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    public AudioClip backgroundMusic;
    [SerializeField] private CountdownTimer countdown;

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

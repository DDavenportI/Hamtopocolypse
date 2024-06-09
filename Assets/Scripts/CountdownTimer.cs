using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TextMeshPro countdownText;
    [SerializeField] private TextMeshPro countdownTextChild;
    public float countdown = 3f;
    public float fadeOutDuration = 1f;
    private GameObject[] hamsters;

    void Awake()
    {
        hamsters = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject ham in hamsters)
        {
            ham.SetActive(false);
        }
    }

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        while (countdown > 0)
        {
            int seconds = Mathf.CeilToInt(countdown);
            countdownText.text = string.Format("{0}", seconds);
            countdownTextChild.text = countdownText.text;
            countdown -= Time.deltaTime;
            yield return null;
        }

        countdown = 0;
        countdownText.text = "Fight!";
        countdownTextChild.text = countdownText.text;

        foreach (GameObject ham in hamsters)
        {
            ham.SetActive(true);
        }

        StartCoroutine(FadeOutText());
    }

    IEnumerator FadeOutText()
    {
        float elapsedTime = 0f;
        Color originalColor = countdownText.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeOutDuration);
            countdownText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            countdownTextChild.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        countdownText.enabled = false;
        countdownTextChild.enabled = false;
    }
}

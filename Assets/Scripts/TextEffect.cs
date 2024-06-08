using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TextEffect : MonoBehaviour
{
    public TextMeshPro tmpTextParent; // Reference to the TextMeshProUGUI component
    public TextMeshPro tmpTextChild; // Reference to the TextMeshProUGUI component
    public float maxFontSize = 60f; // Maximum font size
    public float sizeIncreaseDuration = 2f; // Duration for size increase
    public float flashDuration = 1f; // Duration for flashing
    public float flashInterval = 0.1f; // Interval between flashes
    private bool alive = true;

    [SerializeField] private PlayerMovement blueHamPm;
    [SerializeField] private PlayerMovement pinkHamPm;
    [SerializeField] private Color blueFont;
    [SerializeField] private Color pinkFont;

    private void Start()
    {
        tmpTextParent.enabled = false;
        tmpTextChild.enabled = false;
    }

    void Update()
    {
        /*
        if (blueHamPm.hitPoints == 0 && alive)
        {
            SceneManager.LoadScene("Win Screen", LoadSceneMode.Single);
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("Win Screen"));
            tmpTextParent.SetText("Pink Conquers the Cage!", true);
            tmpTextChild.SetText("Pink Conquers the Cage!", true);
            tmpTextParent.color = pinkFont;
            StartCoroutine(IncreaseTextSize());
        }
        else if (pinkHamPm.hitPoints == 0 && alive)
        {
            SceneManager.LoadScene("Win Screen", LoadSceneMode.Single);
            tmpTextParent.SetText("Blue Conquers the Cage!", true);
            tmpTextChild.SetText("Blue Conquers the Cage!", true);
            tmpTextParent.color = blueFont;
            StartCoroutine(IncreaseTextSize());
        }
        */
        if ((blueHamPm.hitPoints == 0 || pinkHamPm.hitPoints == 0) && (alive))
        {
            if (blueHamPm.hitPoints == 0)
            {
                tmpTextParent.SetText("Pink Conquers the Cage!", true);
                tmpTextChild.SetText("Pink Conquers the Cage!", true);
                tmpTextParent.color = pinkFont;
            }
            else if (pinkHamPm.hitPoints == 0)
            {
                tmpTextParent.SetText("Blue Conquers the Cage!", true);
                tmpTextChild.SetText("Blue Conquers the Cage!", true);
                tmpTextParent.color = blueFont;
            }
            StartCoroutine(LoadYourAsyncScene());
            StartCoroutine(IncreaseTextSize());
        }
    }

    IEnumerator IncreaseTextSize()
    {
        alive = false;
        tmpTextParent.enabled = true;
        tmpTextChild.enabled = true;
        float startFontSize = tmpTextParent.fontSize;
        float elapsedTime = 0f;

        while (elapsedTime < sizeIncreaseDuration)
        {
            tmpTextParent.fontSize = Mathf.Lerp(startFontSize, maxFontSize, elapsedTime / sizeIncreaseDuration);
            tmpTextChild.fontSize = tmpTextParent.fontSize;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        tmpTextParent.fontSize = maxFontSize;
        tmpTextChild.fontSize = tmpTextParent.fontSize;

        // Start the coroutine to flash the text
        StartCoroutine(FlashText());
    }

    IEnumerator FlashText()
    {
        float elapsedTime = 0f;
        bool isTextVisible = true;

        while (elapsedTime < flashDuration)
        {
            tmpTextParent.enabled = isTextVisible;
            tmpTextChild.enabled = isTextVisible;
            isTextVisible = !isTextVisible;
            elapsedTime += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        // Ensure the text is displayed at the end
        tmpTextParent.enabled = true;
        tmpTextChild.enabled = true;
    }

    IEnumerator LoadYourAsyncScene()
    {
        // Set the current Scene to be able to unload it later
        Scene currentScene = SceneManager.GetActiveScene();

        // The Application loads the Scene in the background at the same time as the current Scene.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Win Screen", LoadSceneMode.Additive);

        // Wait until the last operation fully loads to return anything
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Move the GameObject (you attach this in the Inspector) to the newly loaded Scene
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByName("Win Screen"));
        // Unload the previous Scene
        SceneManager.UnloadSceneAsync(currentScene);
    }
}

using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TextEffect : MonoBehaviour
{
    [SerializeField] private TextMeshPro tmpTextParent;
    [SerializeField] private TextMeshPro tmpTextChild;
    [SerializeField] private float maxFontSize = 60f; // Maximum font size
    [SerializeField] private float sizeIncreaseDuration = 2f; // Duration for size increase
    [SerializeField] private float flashDuration = 1f; // Duration for flashing
    [SerializeField] private float flashInterval = 0.1f; // Interval between flashes
    private bool alive = true;

    [SerializeField] private PlayerMovement blueHamPm;
    [SerializeField] private PlayerMovement pinkHamPm;
    [SerializeField] private Color blueFontColor;
    [SerializeField] private Color pinkFontColor;

    private int previousScene;
    private GameObject retryButton;

    private void Start()
    {
        tmpTextParent.enabled = false;
        tmpTextChild.enabled = false;
    }

    void Update()
    {
        if ((blueHamPm.hitPoints == 0 || pinkHamPm.hitPoints == 0) && (alive))
        {
            if (blueHamPm.hitPoints == 0)
            {
                tmpTextParent.SetText("Pink Conquers the Cage!", true);
                tmpTextChild.SetText("Pink Conquers the Cage!", true);
                tmpTextParent.color = pinkFontColor;
            }
            else if (pinkHamPm.hitPoints == 0)
            {
                tmpTextParent.SetText("Blue Conquers the Cage!", true);
                tmpTextChild.SetText("Blue Conquers the Cage!", true);
                tmpTextParent.color = blueFontColor;
            }
            StartCoroutine(LoadWinScreen());
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

        // Start the coroutine to display the retry button after flashing text
        StartCoroutine(DisplayRetryButton(retryButton));
    }

    IEnumerator LoadWinScreen()
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

        // Find the RetryButton in the newly loaded scene
        retryButton = GameObject.Find("RetryButton");
        if (retryButton != null)
        {
            retryButton.SetActive(false); // Ensure it starts inactive
        }

        // Unload the previous Scene
        previousScene = currentScene.buildIndex;
        SceneManager.UnloadSceneAsync(currentScene);
    }

    IEnumerator DisplayRetryButton(GameObject retryButton)
    {
        // Wait for the flash duration to ensure the flash effect is finished
        yield return new WaitForSeconds(flashDuration);

        if (retryButton != null)
        {
            retryButton.SetActive(true);
        }
    }

    public void LoadPreviousScene()
    {
        SceneManager.LoadSceneAsync(previousScene, LoadSceneMode.Single);
    }
}

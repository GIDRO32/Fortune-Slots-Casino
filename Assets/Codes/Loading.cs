using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    public Image Filling;
    public GameObject transition;

    void Start()
    {
        transition.SetActive(true); // Set transition active at start
        StartCoroutine(FadeOutTransition());

        Application.targetFrameRate = 60;
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("Menu");

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            Filling.fillAmount = progressValue;
            yield return null;
        }
    }

    IEnumerator FadeOutTransition()
    {
        CanvasGroup transitionCanvasGroup = transition.GetComponent<CanvasGroup>();
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 5f; // Adjust the speed of the fade here
            transitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer);
            yield return null;
        }

        transition.SetActive(false); // Set transition inactive after fading out
        yield return new WaitForSeconds(0.2f); // Wait a bit before fading in

        StartCoroutine(FadeInTransition()); // Start fading in
    }

    IEnumerator FadeInTransition()
    {
        transition.SetActive(true); // Set transition active before fading in
        CanvasGroup transitionCanvasGroup = transition.GetComponent<CanvasGroup>();
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 5f; // Adjust the speed of the fade here
            transitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
            yield return null;
        }

        StartCoroutine(LoadSceneAsync()); // Start loading scene
    }
}

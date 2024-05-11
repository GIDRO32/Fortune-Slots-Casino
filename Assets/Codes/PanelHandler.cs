using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelHandler : MonoBehaviour
{
    public GameObject Interface;
    public Canvas[] panels;
    public GameObject transition;
    public AudioSource sounds;
    public AudioClip click;
    private Coroutine currentCoroutine;

void Start()
{
    transition.SetActive(true); // Set transition active at start
    CanvasGroup transitionCanvasGroup = transition.GetComponent<CanvasGroup>();
    transitionCanvasGroup.alpha = 1f; // Set alpha to 1 at start

    for (int i = 0; i < panels.Length; i++)
    {
        panels[i].gameObject.SetActive(false);
    }

    StartCoroutine(FadeOutTransition()); // Start coroutine to fade out the transition
}

IEnumerator FadeOutTransition()
{
    yield return new WaitForSeconds(0.2f); // Wait for 1 second (adjust as needed)

    CanvasGroup transitionCanvasGroup = transition.GetComponent<CanvasGroup>();
    float timer = 0f;
    while (timer < 1f)
    {
        timer += Time.deltaTime * 5f; // Adjust the speed of the fade here
        transitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer);
        yield return null;
    }

    transition.SetActive(false); // Set transition inactive after fading out
}

    public void OpenPanel(Canvas panel)
    {
        panel.gameObject.SetActive(true);
        sounds.PlayOneShot(click);

        if (panel.transform.childCount > 0)
        {
            RectTransform rectTransform = panel.transform.GetChild(0).GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (rectTransform.localScale.y == 1f)
                {
                    rectTransform.localScale = new Vector3(rectTransform.localScale.x, 0f, rectTransform.localScale.z);
                }

                currentCoroutine = StartCoroutine(AnimatePanel(panel, rectTransform, true));
            }
        }
    }

    IEnumerator AnimatePanel(Canvas panel, RectTransform rectTransform, bool open)
    {
        float targetScale = open ? 1f : 0f;
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 5f; // Adjust the speed of the animation here
            rectTransform.localScale = new Vector3(rectTransform.localScale.x, Mathf.Lerp(open ? 0f : 1f, targetScale, timer), rectTransform.localScale.z);
            yield return null;
        }

        if (!open)
        {
            panel.gameObject.SetActive(false);
        }
    }

    public void ClosePanel(Canvas panel)
    {
        sounds.PlayOneShot(click);

        if (panel.transform.childCount > 0)
        {
            RectTransform rectTransform = panel.transform.GetChild(0).GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                currentCoroutine = StartCoroutine(AnimatePanel(panel, rectTransform, false));
            }
        }
    }

    public void OpenScene(string scene)
    {
        transition.SetActive(true);
        StartCoroutine(TransitionToScene(scene));
    }

    IEnumerator TransitionToScene(string scene)
    {
        // Fade in transition
        CanvasGroup transitionCanvasGroup = transition.GetComponent<CanvasGroup>();
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f; // Adjust the speed of the fade here
            transitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
            yield return null;
        }

        // Load scene
        SceneManager.LoadScene(scene);
    }

    public void FlyLevel(int level)
    {
        float[] points_needed = { 30f, 40f, 50f, 60f, 70f, 80f, 90f, 100f, 110f, 120f };
        int[] meteor_speed = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        float[] meteor_frequency = { 2f, 1.75f, 1.5f, 1.25f, 1f, 0.75f, 0.6f, 0.5f, 0.4f, 0.3f };
        float[] bg_speed = { 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f, 5.5f, 6f, 6.5f };
        PlayerPrefs.SetFloat("Target score", points_needed[level]);
        PlayerPrefs.SetInt("Target speed", meteor_speed[level]);
        PlayerPrefs.SetFloat("Target frequency", meteor_frequency[level]);
        PlayerPrefs.SetFloat("Target scroll", bg_speed[level]);
    }
}

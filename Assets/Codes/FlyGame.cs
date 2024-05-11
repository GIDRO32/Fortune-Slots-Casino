using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FlyGame : MonoBehaviour
{
    public GameObject picture1;
    public GameObject picture2;
    public GameObject spaceship;
    public GameObject meteoritePrefab;
    public Collider2D destroyZone;
    public int[] meteorSpawnCounts; // Array to store the number of meteors to spawn simultaneously
    public float health = 100f; // Initial health of the spaceship
    public float healthDecreaseRate = 5f; // Health decrease rate when meteor is in trigger zone
    public Text health_counter;
    public Text timer;
    public GameObject[] Panels;
    private float startPosition1 = 0f;
    private float startPosition2 = 14.5f;
    private float scrollSpeed;
    private float spaceshipSpeed = 20f;
    private float meteorSpawnFrequency; // Spawn every 2 seconds
    private int meteorSpeed;
    private float time_left;
    public Slider timer_bar;
    public Slider health_bar;
    public AudioSource SFX;
    public AudioClip[] clips;
    private List<GameObject> spawnedMeteorites = new List<GameObject>();
    public SpriteRenderer bang;
    


    void Start()
    {
if(bang != null)
{
    Color spriteColor = bang.color;
        spriteColor.a = 0f; // Set alpha to 0
        bang.color = spriteColor;
}
        Panels[0].SetActive(true);
        Panels[1].SetActive(false);
        Panels[2].SetActive(false);
        CanvasGroup transitionCanvasGroup3 = Panels[0].GetComponent<CanvasGroup>();
        CanvasGroup transitionCanvasGroup = Panels[1].GetComponent<CanvasGroup>();
        CanvasGroup transitionCanvasGroup2 = Panels[2].GetComponent<CanvasGroup>();
        time_left = PlayerPrefs.GetFloat("Target score", time_left);
        scrollSpeed = PlayerPrefs.GetFloat("Target scroll", scrollSpeed);
        meteorSpawnFrequency = PlayerPrefs.GetFloat("Target frequency", meteorSpawnFrequency);
        meteorSpeed = PlayerPrefs.GetInt("Target speed", meteorSpeed);
        timer_bar.maxValue = time_left;
    }
    public void StartLaunching()
    {
        StartCoroutine(SpawnMeteorites());
        StartCoroutine(Timer());
    }
public void Pause()
    {
        spaceship.SetActive(false);
        StopCoroutine(SpawnMeteorites()); // Stop spawning new meteorites
        foreach (GameObject meteorite in spawnedMeteorites)
        {
            Destroy(meteorite); // Destroy all spawned meteorites
        }
        spawnedMeteorites.Clear(); // Clear the list of spawned meteorites
    }
        public void UnPause()
    {
        spaceship.SetActive(true);
        StartCoroutine(SpawnMeteorites());
        StartCoroutine(Timer());
    }
    void Update()
    {
        timer.text = time_left.ToString("F1");
        health_counter.text = health.ToString("F1");
        timer_bar.value = time_left;
        health_bar.value = health;
        // Move picture 1
        float newPosition1 = Mathf.Repeat(Time.time * scrollSpeed, startPosition2 - startPosition1);
        picture1.transform.position = new Vector3(picture1.transform.position.x, startPosition1 - newPosition1, picture1.transform.position.z);

        // Move picture 2
        float newPosition2 = Mathf.Repeat(Time.time * scrollSpeed, startPosition2 - startPosition1);
        picture2.transform.position = new Vector3(picture2.transform.position.x, startPosition2 - newPosition2, picture2.transform.position.z);
    }
        IEnumerator FadeOutTransition()
    {
        CanvasGroup transitionCanvasGroup3 = Panels[0].GetComponent<CanvasGroup>();
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 5f; // Adjust the speed of the fade here
            transitionCanvasGroup3.alpha = Mathf.Lerp(1f, 0f, timer);
            yield return null;
        }

        Panels[0].SetActive(false); // Set transition inactive after fading out
        yield return new WaitForSeconds(0.2f); // Wait a bit before fading in

        StartCoroutine(FadeInTransition()); // Start fading in
    }
        IEnumerator FadeInTransition()
    {
        CanvasGroup transitionCanvasGroup = Panels[1].GetComponent<CanvasGroup>();
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 5f; // Adjust the speed of the fade here
            transitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
            yield return null;
        }
    }
            IEnumerator FadeInTransition2()
    {
        CanvasGroup transitionCanvasGroup2 = Panels[2].GetComponent<CanvasGroup>();
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 5f; // Adjust the speed of the fade here
            transitionCanvasGroup2.alpha = Mathf.Lerp(0f, 1f, timer);
            yield return null;
        }
    }
public void MoveLeft()
{
    Vector3 targetPosition = new Vector3(Mathf.Max(spaceship.transform.position.x - 1f, -2f), spaceship.transform.position.y, spaceship.transform.position.z);
    spaceship.transform.position = targetPosition;
}

public void MoveRight()
{
    Vector3 targetPosition = new Vector3(Mathf.Min(spaceship.transform.position.x + 1f, 2f), spaceship.transform.position.y, spaceship.transform.position.z);
    spaceship.transform.position = targetPosition;
}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Meteorite"))
        {
            StartCoroutine(ShowBangEffect());
            health -= healthDecreaseRate;
            SFX.PlayOneShot(clips[2]);
            if (health <= 0f)
            {
                SFX.PlayOneShot(clips[1]);
                Panels[0].SetActive(false);
                Panels[1].SetActive(true);
                transform.position = new Vector3 (-10, -10, 0);
                StartCoroutine(FadeInTransition());
            }
        }
    }
    IEnumerator ShowBangEffect()
{
    SpriteRenderer bangRenderer = bang.GetComponent<SpriteRenderer>();
    Color color = bangRenderer.color;
    color.a = 1f; // Set the alpha to 1
    bangRenderer.color = color;

    // Smoothly fade out the bang effect
    float timer = 0f;
    while (timer < 1f)
    {
        timer += Time.deltaTime * 2f; // Adjust the speed of the fade here
        color.a = Mathf.Lerp(1f, 0f, timer);
        bangRenderer.color = color;
        yield return null;
    }
}


    IEnumerator Timer()
    {
        while (time_left > 0f)
        {
            yield return new WaitForSeconds(0.1f);
            time_left -= 0.1f;
            if (time_left <= 0f)
            {
                SFX.PlayOneShot(clips[0]);
                Panels[0].SetActive(false);
                Panels[2].SetActive(true);
                transform.position = new Vector3 (-10, -10, 0);
                StopCoroutine(SpawnMeteorites()); // Stop spawning new meteorites
        foreach (GameObject meteorite in spawnedMeteorites)
        {
            Destroy(meteorite); // Destroy all spawned meteorites
        }
        spawnedMeteorites.Clear(); // Clear the list of spawned meteorites
                StartCoroutine(FadeInTransition2());
            }
        }
    }

    IEnumerator SpawnMeteorites()
    {
        while (true)
        {
            // Determine how many meteorites to spawn
            int meteorCount = meteorSpawnCounts[Random.Range(0, meteorSpawnCounts.Length)];

            for (int i = 0; i < meteorCount; i++)
            {
                Vector3 spawnPosition = new Vector3(RandomXPosition(), 7f, 0f);
                GameObject meteorite = Instantiate(meteoritePrefab, spawnPosition, Quaternion.identity);
                spawnedMeteorites.Add(meteorite);
                StartCoroutine(MoveMeteorite(meteorite));
            }

            yield return new WaitForSeconds(meteorSpawnFrequency);
        }
    }

    float RandomXPosition()
    {
        float[] xPositions = { -2f, -1f, 0f, 1f, 2f };
        List<float> availableXPositions = new List<float>();

        foreach (float xPos in xPositions)
        {
            bool positionAvailable = true;

            // Check if there's a meteorite at this X position and Y position 7
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(xPos, 7f), 0.1f);
            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject.tag == "Meteorite")
                {
                    positionAvailable = false;
                    break;
                }
            }

            if (positionAvailable)
                availableXPositions.Add(xPos);
        }

        if (availableXPositions.Count > 0)
        {
            return availableXPositions[Random.Range(0, availableXPositions.Count)];
        }
        else
        {
            Debug.LogWarning("No available X positions for spawning meteorites.");
            return 0f; // Return a default position if no available positions
        }
    }

    IEnumerator MoveMeteorite(GameObject meteorite)
    {
        while (meteorite != null && !destroyZone.OverlapPoint(meteorite.transform.position))
        {
            meteorite.transform.Translate(Vector3.down * meteorSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(meteorite);
    }
}

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
    


    void Start()
    {
        Time.timeScale = 0f;
        Panels[0].SetActive(true);
        Panels[1].SetActive(false);
        Panels[2].SetActive(false);
        StartCoroutine(SpawnMeteorites());
        time_left = PlayerPrefs.GetFloat("Target score", time_left);
        scrollSpeed = PlayerPrefs.GetFloat("Target scroll", scrollSpeed);
        meteorSpawnFrequency = PlayerPrefs.GetFloat("Target frequency", meteorSpawnFrequency);
        meteorSpeed = PlayerPrefs.GetInt("Target speed", meteorSpeed);
        StartCoroutine(Timer());
        timer_bar.maxValue = time_left;
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
            // Reduce health while meteorite is in trigger zone
            health -= healthDecreaseRate;
            SFX.PlayOneShot(clips[2]);
            if (health <= 0f)
            {
                SFX.PlayOneShot(clips[1]);
                Panels[0].SetActive(false);
                Panels[1].SetActive(true);
                Time.timeScale = 0f;
            }
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
                Time.timeScale = 0f;
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

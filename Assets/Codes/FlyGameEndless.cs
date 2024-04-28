using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FlyGameEndless : MonoBehaviour
{
    public GameObject picture1;
    public GameObject picture2;
    public GameObject spaceship;
    public GameObject meteoritePrefab;
    public GameObject heartPrefab; // New heart prefab
    public Collider2D destroyZone;
    public int[] meteorSpawnCounts; // Array to store the number of meteors to spawn simultaneously
    public float health = 100f; // Initial health of the spaceship
    public float maxHealth = 100f; // Maximum health of the spaceship
    public float healthDecreaseRate = 10f; // Health decrease rate when meteor is in trigger zone
    public float healthRestoreAmount = 5f; // Health restored by heart
    public Text health_counter;
    public Text timer;
    public GameObject[] Panels;
    private float startPosition1 = 0f;
    private float startPosition2 = 14.5f;
    private float scrollSpeed = 2.5f;
    private float meteorSpawnFrequency = 1f; // Initial spawn frequency
    private float meteorSpeed = 5f; // Initial meteor speed
    private float time_passed = 0f;
    public Slider health_bar;
    private float highscore;
    public Text GameOverMessage;
    public AudioSource SFX;
    public AudioClip[] clips;

    void Start()
    {
        Time.timeScale = 0f;
        Panels[0].SetActive(true);
        Panels[1].SetActive(false);
        Panels[2].SetActive(false);
        StartCoroutine(SpawnMeteorites());
        StartCoroutine(IncreaseSpeedOverTime());
        StartCoroutine(FrequencyChange());
        highscore = PlayerPrefs.GetFloat("Fly Record", highscore);
    }

    void Update()
    {
        time_passed += Time.deltaTime;
        health_counter.text = health.ToString("F1");
        timer.text = time_passed.ToString("F0");
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
            SFX.PlayOneShot(clips[1]);
            // Reduce health while meteorite is in trigger zone
            health -= healthDecreaseRate;
            if (health <= 0f)
            {
                SFX.PlayOneShot(clips[0]);
                GameOverMessage.text = "Game Over";
                Panels[0].SetActive(false);
                Panels[1].SetActive(true);
                Time.timeScale = 0f;
                if(time_passed > highscore)
                {
                    GameOverMessage.text = "NEW RECORD!";
                    highscore = time_passed;
                    PlayerPrefs.SetFloat("Fly Record", highscore);
                }
            }
        }
        else if (other.gameObject.CompareTag("Heart")) // Check if the collision is with a heart
        {
            if (health < maxHealth) // Ensure health doesn't exceed maximum
            {
                // Restore health
                health = Mathf.Min(maxHealth, health + healthRestoreAmount);
                SFX.PlayOneShot(clips[2]);
                Destroy(other.gameObject);
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
                // Spawn either a meteorite or a heart with a 5% chance
                GameObject prefab = Random.Range(0, 100) < 5 ? heartPrefab : meteoritePrefab;
                Vector3 spawnPosition = new Vector3(RandomXPosition(), 7f, 0f);
                GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);
                StartCoroutine(MoveObject(obj));
            }

            yield return new WaitForSeconds(meteorSpawnFrequency);
        }
    }

    IEnumerator MoveObject(GameObject obj)
    {
        while (obj != null && !destroyZone.OverlapPoint(obj.transform.position))
        {
            obj.transform.Translate(Vector3.down * meteorSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(obj);
    }

    float RandomXPosition()
    {
        float[] xPositions = { -2f, -1f, 0f, 1f, 2f };
        List<float> availableXPositions = new List<float>();

        foreach (float xPos in xPositions)
        {
            bool positionAvailable = true;

            // Check if there's an object at this X position and Y position 7
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(xPos, 7f), 0.1f);
            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject.CompareTag("Meteorite") || collider.gameObject.CompareTag("Heart")) // Check if the collider is a meteorite or heart
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
            Debug.LogWarning("No available X positions for spawning objects.");
            return 0f; // Return a default position if no available positions
        }
    }

    IEnumerator IncreaseSpeedOverTime()
    {
        WaitForSeconds waitOneMinute = new WaitForSeconds(60f);

        while (true)
        {
            yield return waitOneMinute;

            // Increase background speed and meteor speed
            scrollSpeed += 0.5f;
            meteorSpeed += 1f;
        }
    }
    IEnumerator FrequencyChange()
    {
        WaitForSeconds waitThirtySeconds = new WaitForSeconds(30f);
        while (true)
        {
        yield return waitThirtySeconds;
        meteorSpawnFrequency -= 0.05f;
        meteorSpawnFrequency = Mathf.Max(0.15f, meteorSpawnFrequency);
        Debug.Log(meteorSpawnFrequency);
        }
    }
}

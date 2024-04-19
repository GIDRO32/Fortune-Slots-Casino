using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleMode : MonoBehaviour
{
    public GameObject CardTemplate; // The card prefab
    [SerializeField]
    private List<CardTypeSprites> suitsRanks = new List<CardTypeSprites>(); // Suits<Ranks>
    public CardBattle[] PlayerCards; // Array for player's cards
    public CardBattle[] EnemyCards; // Array for enemy's cards
    public GameObject[] targets;
    [SerializeField] private List<SlotRow> rows = new List<SlotRow>();
    public Sprite[] icons; // Assign in Inspector
    private bool isSpinning = false;
    private int stopCount = 0;
    public Button launchButton;
    public int initialHealth = 5;
    private int life_tokens = 10;
    public Vector2[] PlayerCardPositions;
    public Vector2[] EnemyCardPositions;
    public Text[] counters;
    private int enemy_healthbar = 50;
    public Slider healthbar_appear;
    public static BattleMode Instance;
    public GameObject[] panels;
    public int energy = 0; // Energy gained from playing BattleMode
    private int totalDamageDealt = 0; // Total damage dealt to the health bar
    public Text energy_counter;
    private int income;
    public AudioSource sounds;
    public AudioSource rolling;
    public AudioClip[] clips;

    void Start()
    {
        energy = PlayerPrefs.GetInt("energy", energy);
        panels[0].SetActive(true);
        panels[1].SetActive(false);
        panels[2].SetActive(false);
        Instance = this;
        launchButton.onClick.AddListener(ToggleSpinning);
        // Initialize each slot with a random icon at the start
        foreach (var row in rows)
        {
            foreach (var slot in row.slots)
            {
                slot.GetComponent<Image>().sprite = icons[Random.Range(0, icons.Length)];
            }
        }
        LoadEnergy();
    }
    void LoadEnergy()
    {
        energy = PlayerPrefs.GetInt("energy", 0); // Load energy from PlayerPrefs, defaulting to 0 if not found
        energy = Mathf.Min(energy, 50); // Ensure loaded energy does not exceed 50
    }
 void ToggleSpinning()
    {
        if (!isSpinning)
        {
        if (AllPlayersDefeated())
        {
            if (life_tokens > 0)
            {
                life_tokens--;
            }
        }
            rolling.Play();
            isSpinning = true;
            StartCoroutine(SpinSlots());
        }
        else
        {
            stopCount += 1;
            if (stopCount >= rows.Count)
            {
                rolling.Pause();
                stopCount = 0;
                isSpinning = false;
                ApplySlotMachineOutcome(); // Check slot machine outcome and apply effects
                if(life_tokens == 0)
                {
                sounds.PlayOneShot(clips[0]);
                panels[1].SetActive(true);
                panels[0].SetActive(false);
                return;
                }
            }
        }
    }

    IEnumerator SpinSlots()
    {
        while (isSpinning && stopCount < rows.Count)
        {
            foreach (var row in rows)
            {
                if (rows.IndexOf(row) < stopCount)
                {
                    continue; // Skip the rows that have stopped spinning
                }

                // Ensure the first slot's new icon is not the same as the current one
                Sprite currentFirstIcon = row.slots[0].GetComponent<Image>().sprite;
                Sprite newFirstIcon;
                do
                {
                    newFirstIcon = icons[Random.Range(0, icons.Length)];
                } while (newFirstIcon == currentFirstIcon);

                row.slots[0].GetComponent<Image>().sprite = newFirstIcon;

                // Cascade the previous icons down the row
                for (int slotIndex = 1; slotIndex < row.slots.Count; slotIndex++)
                {
                    Sprite nextIcon = row.slots[slotIndex].GetComponent<Image>().sprite;
                    row.slots[slotIndex].GetComponent<Image>().sprite = currentFirstIcon;
                    currentFirstIcon = nextIcon;
                }
            }

            yield return new WaitForSeconds(0.1f); // Adjust for desired spinning speed
        }
    }
    private void ApplySlotMachineOutcome()
    {
    int[] iconCounts = new int[icons.Length]; // Assuming 'icons' array contains all possible icons
    foreach (var row in rows)
    {
        Sprite firstSlotIcon = row.slots[0].GetComponent<Image>().sprite;
        int iconIndex = System.Array.IndexOf(icons, firstSlotIcon);
        if(iconIndex >= 0) iconCounts[iconIndex]++;
    }

if (iconCounts[0] == 2) // If any 2 slots have the icon for Shield effect
    {
        // Apply Shield effect on one random player card
        List<CardBattle> activeCards = GetActivePlayerCards();
        if (activeCards.Count > 0)
        {
            CardBattle selectedCard = activeCards[Random.Range(0, activeCards.Count)];
            selectedCard.Shield(); // Apply Shield() to one random active player card
        }
    }
    else if (iconCounts[0] == 3) // If all 3 slots have the icon for Shield effect
    {
        // Apply Shield effect on all player cards
        foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.Shield();
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
    }
    // Check for Nectar effect condition (icon with index 1)
    else if (iconCounts[1] == 2) // If any 2 slots have the icon for Nectar effect
    {
        List<CardBattle> activeCards = GetActivePlayerCards();
        if (activeCards.Count > 0)
        {
            CardBattle selectedCard = activeCards[Random.Range(0, activeCards.Count)];
            selectedCard.Nectar(); // Apply Nectar() to one random active player card
        }
        
    }
    else if (iconCounts[1] == 3) // If all 3 slots have the icon for Nectar effect
    {
        // Apply Nectar effect on all player cards
        foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.Nectar();
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
    }
        else if (iconCounts[2] == 2) // If any 2 slots have the icon for Double attack effect
    {
        sounds.PlayOneShot(clips[3]);
        EnemyCards[Random.Range(0, EnemyCards.Length)].AddHealth(3);
        
    }
    else if (iconCounts[2] == 3) // If all 3 slots have the icon for Double effect
    {
        sounds.PlayOneShot(clips[3]);
        foreach(CardBattle enemyCard in EnemyCards)
        {
            enemyCard.AddHealth(5);
        }
    }
            else if (iconCounts[3] == 2) // If any 2 slots have the icon for Health effect
    {
        // Apply Nectar effect on one random player card
        PlayerCards[Random.Range(0, PlayerCards.Length)].AddHealth(5);
        sounds.PlayOneShot(clips[1]);
                foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
        
    }
    else if (iconCounts[3] == 3) // If all 3 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[1]);
        // Apply Nectar effect on all player cards
        foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.AddHealth(5);
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
    }
                else if (iconCounts[4] == 2) // If any 2 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[3]);
        EnemyCards[Random.Range(0, EnemyCards.Length)].AddHealth(7);
        
    }
    else if (iconCounts[4] == 3) // If all 3 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[3]);
        foreach(CardBattle enemyCard in EnemyCards)
        {
            enemyCard.AddHealth(5);
        }
    }
    else if (iconCounts[5] == 2) // If any 2 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[1]);
        PlayerCards[Random.Range(0, PlayerCards.Length)].AddHealth(7);
                foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
        
    }
    else if (iconCounts[5] == 3) // If all 3 slots have the icon for Nectar effect
    {
        // Apply Nectar effect on all player cards
        foreach (CardBattle playerCard in PlayerCards)
        {
            sounds.PlayOneShot(clips[1]);
            playerCard.AddHealth(7);
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
    }
    else if (iconCounts[6] == 2) // If any 2 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[1]);
        PlayerCards[Random.Range(0, PlayerCards.Length)].AddHealth(10);
                foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
        
    }
    else if (iconCounts[6] == 3) // If all 3 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[1]);
        foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.AddHealth(10);
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
    }
                else if (iconCounts[7] == 2) // If any 2 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[1]);
        PlayerCards[Random.Range(0, PlayerCards.Length)].RandomAction();
                foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
        
    }
    else if (iconCounts[7] == 3) // If all 3 slots have the icon for Nectar effect
    {
        sounds.PlayOneShot(clips[1]);
        foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.RandomAction();
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
    }
    else if (iconCounts[8] == 2) // If any 2 slots have the icon for Nectar effect
    {
        life_tokens++;
    }
    else if (iconCounts[8] == 3) // If all 3 slots have the icon for Nectar effect
    {
        life_tokens += 5;
    }
    else if (iconCounts[9] == 2) // If any 2 slots have the icon for Nectar effect
    {
        List<CardBattle> activeCards = GetActivePlayerCards();
        if (activeCards.Count > 0)
        {
            CardBattle selectedCard = activeCards[Random.Range(0, activeCards.Count)];
            selectedCard.Attack(); // Apply Attack() to one random active player card
        }
    }
    else if (iconCounts[9] == 3) // If all 3 slots have the icon for Nectar effect
    {
        // Apply Nectar effect on all player cards
        foreach (CardBattle playerCard in PlayerCards)
        {
            playerCard.Attack();
            playerCard.enemyCard = targets[System.Array.IndexOf(PlayerCards, playerCard)];
        }
    }
    else
    {
        sounds.PlayOneShot(clips[2]);
        Debug.Log("Nothing...");
    }
    }
public void AttackHealthBar(int damage) {
        int previousHealth = enemy_healthbar;
        enemy_healthbar -= damage;
        enemy_healthbar = Mathf.Max(enemy_healthbar, 0); // Prevent health bar from going below 0
        int effectiveDamage = previousHealth - enemy_healthbar; // Calculate the effective damage dealt
        totalDamageDealt += effectiveDamage;
        
        UpdateEnergy(); // Update energy based on damage dealt
        if (enemy_healthbar <= 0) {
            sounds.PlayOneShot(clips[4]);
            CalculateAndSaveEnergy();
            PlayerPrefs.SetInt("energy", energy);
            panels[0].SetActive(false);
            panels[2].SetActive(true);
        }
    }

    private void UpdateEnergy()
    {
        income = totalDamageDealt / 10; // Gain 1 energy unit per 10 damage
        income = Mathf.Min(income, 5); // Cap the energy at 5 units
    }
    void CalculateAndSaveEnergy()
    {
        int energyGained = Mathf.FloorToInt(totalDamageDealt); // Assuming totalDamageDealt is tracked elsewhere
        energy += energyGained;
        energy = Mathf.Min(energy, 50); // Cap the energy at 5
        PlayerPrefs.SetInt("energy", energy); // Save the capped energy value back to PlayerPrefs
        PlayerPrefs.Save(); // Ensure data is written to disk
    }
    private List<CardBattle> GetActivePlayerCards()
    {
    List<CardBattle> activeCards = new List<CardBattle>();
    foreach (CardBattle card in PlayerCards)
    {
        if (card.health > 0)
        {
            activeCards.Add(card);
        }
    }
    return activeCards;
}

    void Update()
    {
        counters[0].text = life_tokens.ToString();
        counters[1].text = "Enemy: " +enemy_healthbar.ToString();
        healthbar_appear.value = enemy_healthbar;
        energy_counter.text = "+ " + energy.ToString() + " energy";
    }
    bool AllPlayersDefeated() {
    // Check if all player cards are defeated
    foreach (var playerCard in PlayerCards) {
        if (playerCard.health > 0) return false;
    }
    return true;
}
    [System.Serializable]
    public class CardTypeSprites
    {
        public string typeName; // Name of the card type (suit)
        public List<Sprite> rankSprites; // Sprites for each rank within this type
    }
    [System.Serializable] // This makes it visible in the Unity Inspector
public class SlotRow
{
    public List<GameObject> slots = new List<GameObject>(); // Each row is a list of slots
}

    // Additional methods and logic as needed...
}

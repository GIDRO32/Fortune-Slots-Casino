using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Explicitly define that we're using Random from UnityEngine to avoid ambiguity
using Random = UnityEngine.Random;

public class SlotMachine : MonoBehaviour
{
    [SerializeField] private List<SlotRow> rows = new List<SlotRow>();
    public Sprite[] icons; // Assign in Inspector
    private bool isSpinning = false;
    private int stopCount = 0;
    public Button launchButton;
    private int gems;
    public Text gems_counter;
    private int[] bets = {1, 5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000};
    private int bet_chosen = 0;
    private int coins;
    private int combo_multiplier = 0;
    public Text coins_counter;
    public Text bet_counter;
    public AudioSource spinning;
    public AudioSource sounds;
    public AudioClip[] sound_array;

void Start()
    {
        launchButton.onClick.AddListener(ToggleSpinning);
        gems = PlayerPrefs.GetInt("Gems", gems);
        InitializeSlots();
        // CheatInitialize();
        coins = PlayerPrefs.GetInt("Total", coins);
    }
    public void CheatInitialize()
    {
        // Example setup to test a specific pattern, adjust indices as necessary
        // Ensure rows and their slots are already assigned in the inspector
        if (rows.Count >= 5 && icons.Length >= 8)
        {
            // Setting a custom pattern to test horizontal wins for example
            rows[0].slots[0].GetComponent<Image>().sprite = icons[7];
            rows[0].slots[1].GetComponent<Image>().sprite = icons[1];
            rows[0].slots[2].GetComponent<Image>().sprite = icons[7];

            rows[1].slots[0].GetComponent<Image>().sprite = icons[5];
            rows[1].slots[1].GetComponent<Image>().sprite = icons[7];
            rows[1].slots[2].GetComponent<Image>().sprite = icons[2];

            rows[2].slots[0].GetComponent<Image>().sprite = icons[7];
            rows[2].slots[1].GetComponent<Image>().sprite = icons[1];
            rows[2].slots[2].GetComponent<Image>().sprite = icons[7];

            rows[3].slots[0].GetComponent<Image>().sprite = icons[4];
            rows[3].slots[1].GetComponent<Image>().sprite = icons[5];
            rows[3].slots[2].GetComponent<Image>().sprite = icons[1];

            rows[4].slots[0].GetComponent<Image>().sprite = icons[1];
            rows[4].slots[1].GetComponent<Image>().sprite = icons[3];
            rows[4].slots[2].GetComponent<Image>().sprite = icons[2];
        }
        else
        {
            Debug.LogError("Not enough rows or icons to initialize slots.");
        }
    }
    void InitializeSlots()
    {
        foreach (var row in rows)
        {
            foreach (var slot in row.slots)
            {
                slot.GetComponent<Image>().sprite = icons[Random.Range(0, icons.Length)];
            }
        }
    }
public void IncreaseBet()
{
    if(bet_chosen < bets.Length-1)
    {
        bet_chosen++;
        sounds.PlayOneShot(sound_array[5]);
    }
    else
    {
        bet_chosen = 0;
        sounds.PlayOneShot(sound_array[5]);
    }
}
public void DecreaseBet()
{
    if(bet_chosen > 0)
    {
        bet_chosen--;
        sounds.PlayOneShot(sound_array[5]);
    }
    else
    {
        bet_chosen = bets.Length-1;
        sounds.PlayOneShot(sound_array[5]);
    }
}
void ToggleSpinning()
    {
        if (!isSpinning && gems >= bets[bet_chosen])
        {
            gems -= bets[bet_chosen];
            PlayerPrefs.SetInt("Gems", gems);
            isSpinning = true;
            StartCoroutine(SpinSlots());
            spinning.Play();
        }
        else if(!isSpinning && gems < bets[bet_chosen])
        {
            sounds.PlayOneShot(sound_array[3]);
            Debug.Log("Not enough gems!");
        }
        else
        {
            sounds.PlayOneShot(sound_array[2]);
            stopCount += 1;
            if (stopCount >= rows.Count)
            {
                stopCount = 0;
                isSpinning = false;
                spinning.Pause();
                CheckForWins();
                PlayerPrefs.SetInt("Total", coins);
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
bool CheckForHorizontalWins()
{
    bool win = false;
    combo_multiplier = 1; // Basic multiplier for this type of win

    // Check each column for three consecutive icons
    for (int col = 0; col < 3; col++) // Assuming each row has exactly 3 slots
    {
        // Check combinations starting from each possible row that allows 3 slots downwards
        for (int startRow = 0; startRow < rows.Count - 2; startRow++) // Ensure there's room for three slots downward
        {
            // Check if the current column in three subsequent rows has the same icon
            Sprite firstIcon = rows[startRow].slots[col].GetComponent<Image>().sprite;
            Sprite secondIcon = rows[startRow + 1].slots[col].GetComponent<Image>().sprite;
            Sprite thirdIcon = rows[startRow + 2].slots[col].GetComponent<Image>().sprite;

            if (firstIcon != null && firstIcon == secondIcon && secondIcon == thirdIcon)
            {
                // Find the index of the icon
                int iconIndex = System.Array.IndexOf(icons, firstIcon);
                if (iconIndex != -1) {
                Debug.Log($"Horizontal win detected.");
                coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1); // Example calculation
                coins_counter.text = coins.ToString();
                win = true;
            }
            }
        }
    }
    return win;
    
}
bool CheckVerticalFive()
{
    bool win = false;
    int combo_multiplier = 6; // Multiplier for a vertical line of five

    // Check each column in the rows to see if all slots in the column have the same icon
    if (rows.Count >= 5) {
        for (int col = 0; col < rows[0].slots.Count; col++) { // Assuming all rows have at least as many slots as the first row
            Sprite firstIcon = rows[0].slots[col].GetComponent<Image>().sprite;
            if (firstIcon == null) continue; // Skip if the first icon in the column is null

            bool allMatch = true;
            for (int row = 1; row < rows.Count; row++) {
                if (rows[row].slots[col].GetComponent<Image>().sprite != firstIcon) {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch) {
                int iconIndex = System.Array.IndexOf(icons, firstIcon);
                Debug.Log($"Vertical line of 5 detected with icon index {iconIndex} at column {col}.");
                coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
                Debug.Log($"Coins won for Vertical Line of 5: {coins}");
                coins_counter.text = coins.ToString();
                win = true; // Exit after finding the first vertical line of five to prevent further processing
            }
        }
    }
    return win;
}


bool CheckForDiagonalWins()
{
    combo_multiplier = 2; // Multiplier for diagonal wins
    bool win = false;

    // Diagonal from top-left to bottom-right
    for (int row = 0; row <= rows.Count - 3; row++) {
        for (int col = 0; col <= 0; col++) { // Adjust column check to ensure room for diagonal
            if (rows[row].slots[col].GetComponent<Image>().sprite == rows[row + 1].slots[col + 1].GetComponent<Image>().sprite &&
                rows[row + 1].slots[col + 1].GetComponent<Image>().sprite == rows[row + 2].slots[col + 2].GetComponent<Image>().sprite &&
                rows[row].slots[col].GetComponent<Image>().sprite != null) {
                int iconIndex = System.Array.IndexOf(icons, rows[row].slots[col].GetComponent<Image>().sprite);
                Debug.Log($"Diagonal win with icon index {iconIndex} from top-left to bottom-right starting at row {row}, col {col}");
                coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
                Debug.Log($"Coins won: {coins}");
                coins_counter.text = coins.ToString();
            }
        }
    }
    return win;

    // Diagonal from bottom-left to top-right
    for (int row = 2; row < rows.Count; row++) {
        for (int col = 0; col <= 0; col++) {
            if (rows[row].slots[col].GetComponent<Image>().sprite == rows[row - 1].slots[col + 1].GetComponent<Image>().sprite &&
                rows[row - 1].slots[col + 1].GetComponent<Image>().sprite == rows[row - 2].slots[col + 2].GetComponent<Image>().sprite &&
                rows[row].slots[col].GetComponent<Image>().sprite != null) {
                int iconIndex = System.Array.IndexOf(icons, rows[row].slots[col].GetComponent<Image>().sprite);
                Debug.Log($"Diagonal win with icon index {iconIndex} from bottom-left to top-right starting at row {row}, col {col}");
                coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
                Debug.Log($"Coins won: {coins}");
                coins_counter.text = coins.ToString();
            }
        }
    }
    return win;
}

void CheckForWins()
{
    bool winDetected = false;

    // Call each function and update the winDetected flag accordingly
    winDetected |= CheckForHorizontalWins(); // Assuming these methods return true if a win is found
    winDetected |= CheckForDiagonalWins();
    winDetected |= CheckPyramidPatterns();
    winDetected |= CheckDiamondPattern();
    winDetected |= CheckCrossPattern();
    winDetected |= CheckVerticalFive();
    winDetected |= CheckInfinityPattern();
    winDetected |= CheckDoublePyramidPatterns();
    winDetected |= CheckChromosomePattern();
    winDetected |= CheckDoubleFivePattern();

    // Play sound if at least one win was detected
    if (winDetected) {
        sounds.PlayOneShot(sound_array[1]);
    }
    else
    {
        sounds.PlayOneShot(sound_array[4]);
    }
}
bool CheckCrossPattern()
{
    bool win = false;
    int combo_multiplier = 5; // Multiplier for cross wins

    // Check potential cross patterns in the central columns and rows only
    for (int row = 1; row < rows.Count - 1; row++) { // Center row where the cross can occur
        for (int col = 1; col < 2; col++) { // Center column where the cross can occur
            // Ensures all involved slots are within bounds
            if (rows[row-1].slots[col-1] && rows[row+1].slots[col+1] && 
                rows[row-1].slots[col+1] && rows[row+1].slots[col-1]) {
                // Top-left to bottom-right and bottom-left to top-right
                Sprite centerSprite = rows[row].slots[col].GetComponent<Image>().sprite;
                bool diagonal1 = (rows[row-1].slots[col-1].GetComponent<Image>().sprite == centerSprite &&
                                  rows[row+1].slots[col+1].GetComponent<Image>().sprite == centerSprite);
                bool diagonal2 = (rows[row-1].slots[col+1].GetComponent<Image>().sprite == centerSprite &&
                                  rows[row+1].slots[col-1].GetComponent<Image>().sprite == centerSprite);

                if (diagonal1 && diagonal2 && centerSprite != null) {
                    // Find the index of the icon for additional context or multiplier calculations
                    int iconIndex = System.Array.IndexOf(icons, centerSprite);
                    Debug.Log($"Cross win with icon index {iconIndex} centered at row {row}, col {col}");
                    coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
                    Debug.Log($"Coins won: {coins}");
                    coins_counter.text = coins.ToString();
                    win = true;
                }
            }
        }
    }
    return win;
}

bool CheckChromosomePattern()
{
    bool win = false;
    int combo_multiplier = 9; // Adjusted multiplier for chromosome wins

    // Check sufficient rows and slots count
    if (rows.Count >= 5 && rows[2].slots.Count > 2) {
        // Check vertical lines and horizontal line at the center
        Sprite centerSprite = rows[2].slots[1].GetComponent<Image>().sprite;
        bool isChromosome = centerSprite != null &&
                            rows[0].slots[0].GetComponent<Image>().sprite == centerSprite &&
                            rows[1].slots[0].GetComponent<Image>().sprite == centerSprite &&
                            rows[3].slots[0].GetComponent<Image>().sprite == centerSprite &&
                            rows[4].slots[0].GetComponent<Image>().sprite == centerSprite &&
                            rows[0].slots[2].GetComponent<Image>().sprite == centerSprite &&
                            rows[1].slots[2].GetComponent<Image>().sprite == centerSprite &&
                            rows[3].slots[2].GetComponent<Image>().sprite == centerSprite &&
                            rows[4].slots[2].GetComponent<Image>().sprite == centerSprite &&
                            rows[2].slots[1].GetComponent<Image>().sprite == centerSprite; // Ensure the horizontal center matches

        if (isChromosome) {
            int iconIndex = System.Array.IndexOf(icons, centerSprite);
            Debug.Log($"Chromosome pattern detected with icon index {iconIndex}.");
            coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
            Debug.Log($"Coins won for Chromosome: {coins}");
            coins_counter.text = coins.ToString();
            win = true;
        }
    }
    return win;
}


bool CheckPyramidPatterns()
{
    bool win = false;
    int combo_multiplier = 3; // Multiplier for pyramid wins

    // Check for an upright pyramid pattern
    if (rows.Count >= 3 && rows[0].slots.Count > 1 && rows[1].slots.Count > 2) { // Ensure the slots exist
        Sprite apexSprite = rows[0].slots[0].GetComponent<Image>().sprite;
        bool upright = apexSprite != null &&
                       rows[0].slots[0].GetComponent<Image>().sprite == apexSprite &&
                       rows[1].slots[1].GetComponent<Image>().sprite == apexSprite &&
                       rows[2].slots[2].GetComponent<Image>().sprite == apexSprite &&
                       rows[3].slots[1].GetComponent<Image>().sprite == apexSprite &&
                       rows[4].slots[0].GetComponent<Image>().sprite == apexSprite;

        if (upright) {
            int iconIndex = System.Array.IndexOf(icons, apexSprite);
            Debug.Log($"Upside-down Pyramid pattern detected with icon index {iconIndex} at apex.");
            coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
            Debug.Log($"Coins won: {coins}");
            coins_counter.text = coins.ToString();
            win = true;
        }
    }
    return win;

    // Check for an upside-down pyramid pattern
    if (rows.Count >= 5 && rows[4].slots.Count > 1 && rows[3].slots.Count > 2) { // Ensure the slots exist
        Sprite baseSprite = rows[0].slots[2].GetComponent<Image>().sprite;
        bool upsideDown = baseSprite != null &&
                       rows[0].slots[2].GetComponent<Image>().sprite == baseSprite &&
                       rows[1].slots[1].GetComponent<Image>().sprite == baseSprite &&
                       rows[2].slots[0].GetComponent<Image>().sprite == baseSprite &&
                       rows[3].slots[1].GetComponent<Image>().sprite == baseSprite &&
                       rows[4].slots[2].GetComponent<Image>().sprite == baseSprite;

        if (upsideDown) {
            int iconIndex = System.Array.IndexOf(icons, baseSprite);
            Debug.Log($"Pyramid pattern detected with icon index {iconIndex} at base.");
            coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
            Debug.Log($"Coins won: {coins}");
            coins_counter.text = coins.ToString();
        }
    }
    return win;
}
bool CheckDoublePyramidPatterns()
{
    bool win = false;
    int combo_multiplier = 8; // Higher multiplier for double pyramid wins

    // Check conditions for both pyramids
    bool upright = CheckUprightPyramid();
    bool upsideDown = CheckUpsideDownPyramid();

    // If both pyramid patterns are detected
    if (upright && upsideDown) {
        Debug.Log("Double Pyramid pattern detected.");
        coins += bets[bet_chosen] * combo_multiplier; // Apply the double pyramid multiplier
        Debug.Log($"Coins won for Double Pyramid: {coins}");
        coins_counter.text = coins.ToString();
        win = true;
    }
    return win;
}

bool CheckUprightPyramid()
{
    // Similar logic as provided before, but returning a boolean for detection
    if (rows.Count >= 5) {
        Sprite apexSprite = rows[0].slots[0].GetComponent<Image>().sprite;
        return apexSprite != null &&
               rows[0].slots[0].GetComponent<Image>().sprite == apexSprite &&
               rows[1].slots[1].GetComponent<Image>().sprite == apexSprite &&
               rows[2].slots[2].GetComponent<Image>().sprite == apexSprite &&
               rows[3].slots[1].GetComponent<Image>().sprite == apexSprite &&
               rows[4].slots[0].GetComponent<Image>().sprite == apexSprite;
    }
    return false;
}

bool CheckUpsideDownPyramid()
{
    // Similar logic as provided before, but returning a boolean for detection
    if (rows.Count >= 5) {
        Sprite baseSprite = rows[0].slots[2].GetComponent<Image>().sprite;
        return baseSprite != null &&
               rows[0].slots[2].GetComponent<Image>().sprite == baseSprite &&
               rows[1].slots[1].GetComponent<Image>().sprite == baseSprite &&
               rows[2].slots[0].GetComponent<Image>().sprite == baseSprite &&
               rows[3].slots[1].GetComponent<Image>().sprite == baseSprite &&
               rows[4].slots[2].GetComponent<Image>().sprite == baseSprite;
    }
    return false;
}
bool CheckDiamondPattern()
{
    bool win = false;
    int combo_multiplier = 4; // Adjusted multiplier for diamond wins

    // Start checking from the first row to the fourth row
    for (int row = 0; row < rows.Count - 2; row++) {
        if (row + 3 >= rows.Count || rows[row].slots.Count < 2 || rows[row + 1].slots.Count < 3 || rows[row + 2].slots.Count < 2) continue;

        // Check for the diamond pattern in three possible vertical alignments
        for (int r = 0; r <= 2 && row + r + 2 < rows.Count; r++) {
            Sprite centerSprite = rows[row + r].slots[1].GetComponent<Image>().sprite;
            if (centerSprite != null &&
                rows[row + r + 1].slots[0].GetComponent<Image>().sprite == centerSprite &&
                rows[row + r + 1].slots[2].GetComponent<Image>().sprite == centerSprite &&
                rows[row + r + 2].slots[1].GetComponent<Image>().sprite == centerSprite) {

                int iconIndex = System.Array.IndexOf(icons, centerSprite);
                if (iconIndex != -1) {
                    Debug.Log($"Diamond pattern detected with icon index {iconIndex} in configuration starting at row {row + r}.");
                    coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);  // Calculate the winnings
                    Debug.Log($"Coins won for Diamond: {coins}");
                    coins_counter.text = coins.ToString();  // Play the winning sound
                    win = true;
                }
            }
        }
    }
    return win;
}






bool CheckInfinityPattern()
{
    bool win = false;
    int combo_multiplier = 7; // Multiplier for infinity wins

    // Ensure that the grid has enough rows and each relevant row has enough slots
    if (rows.Count >= 5 && rows[0].slots.Count > 1 && rows[1].slots.Count > 2 && rows[3].slots.Count > 2 && rows[4].slots.Count > 1) {
        // Check the specific slots that would form an infinity pattern
        bool infinity = rows[0].slots[1].GetComponent<Image>().sprite != null &&
                        rows[0].slots[1].GetComponent<Image>().sprite == rows[4].slots[1].GetComponent<Image>().sprite &&
                        rows[1].slots[0].GetComponent<Image>().sprite == rows[3].slots[0].GetComponent<Image>().sprite &&
                        rows[1].slots[0].GetComponent<Image>().sprite == rows[1].slots[2].GetComponent<Image>().sprite &&
                        rows[1].slots[2].GetComponent<Image>().sprite == rows[3].slots[2].GetComponent<Image>().sprite &&
                        rows[1].slots[0].GetComponent<Image>().sprite == rows[0].slots[1].GetComponent<Image>().sprite;

        if (infinity) {
            // Find the index of the icon for additional context or multiplier calculations
            int iconIndex = System.Array.IndexOf(icons, rows[0].slots[1].GetComponent<Image>().sprite);
            Debug.Log($"Infinity pattern detected with icon index {iconIndex}.");
            coins += bets[bet_chosen] * combo_multiplier * (iconIndex + 1);
            Debug.Log($"Coins won for Infinity: {coins}");
            coins_counter.text = coins.ToString();
            win = true;
        }
    }
    return win;
}


bool CheckDoubleFivePattern()
{
    bool win = false;
    int combo_multiplier = 10; // Multiplier for Double Five pattern wins

    // Ensure there are enough rows and each row has at least three slots
    if (rows.Count >= 5 && rows[0].slots.Count > 2) {
        bool doubleFive = true;
        Sprite firstColumnIcon = rows[0].slots[0].GetComponent<Image>().sprite;
        Sprite thirdColumnIcon = rows[0].slots[2].GetComponent<Image>().sprite;

        if (firstColumnIcon == null || thirdColumnIcon == null) {
            doubleFive = false; // Early exit if the first or third column icons in the first row are null
        } else {
            // Check first and third columns for consistency across all five rows
            for (int i = 1; i < rows.Count; i++) {
                if (rows[i].slots[0].GetComponent<Image>().sprite != firstColumnIcon ||
                    rows[i].slots[2].GetComponent<Image>().sprite != thirdColumnIcon) {
                    doubleFive = false;
                    break;
                }
            }
        }

        if (doubleFive) {
            int iconIndex1 = System.Array.IndexOf(icons, firstColumnIcon);
            int iconIndex3 = System.Array.IndexOf(icons, thirdColumnIcon);
            Debug.Log($"Double Five pattern detected with icon indices {iconIndex1} and {iconIndex3}.");
            coins += bets[bet_chosen] * combo_multiplier * ((iconIndex1 + 1) + (iconIndex3 + 1)); // Example calculation
            Debug.Log($"Coins won for Double Five: {coins}");
            coins_counter.text = coins.ToString();
            sounds.PlayOneShot(sound_array[0]);
            win = true;
        }
    }
    return win;
}

    void Update()
    {
        gems_counter.text = gems.ToString();
        coins_counter.text = coins.ToString();
        bet_counter.text = "Bet: " + bets[bet_chosen].ToString();
    }
}

[System.Serializable] // This makes it visible in the Unity Inspector
public class SlotRow
{
    public List<GameObject> slots = new List<GameObject>(); // Each row is a list of slots
}

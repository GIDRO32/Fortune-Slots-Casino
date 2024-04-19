using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CardGame : MonoBehaviour
{
    public GameObject[] cardPrefabs; // Assign your card prefabs here in the inspector
    public Sprite unrevealedCardImage; // Assign the sprite for an unrevealed card in the inspector
    [SerializeField] private List<CardTypeSprites> cardTypeSprites = new List<CardTypeSprites>(); // This will be visible and editable in the Unity Editor

    private Dictionary<string, Sprite> cardSprites = new Dictionary<string, Sprite>();
    private List<int> drawnRanks = new List<int>();
    private List<string> drawnTypes = new List<string>();
    [SerializeField] private GameObject highCardResult;
    public GameObject pairResult; // Assign in the inspector
    public GameObject doublePairResult; // Assign in the inspector
    public GameObject trioResult;
    public GameObject fullHouseResult;
    public GameObject fourOfAKindResult;
    public GameObject flushResult;
    public GameObject straightResult; 
    public GameObject[] typeResult;
    public GameObject straightFlushResult;
    public GameObject royalFlushResult;
    private List<string> drawnCardCombinations = new List<string>();
    private int currentCardIndex = 0;
    public Slider energy_bar;
    private int energy;
    private int gems;
    public Text gems_counter;
    public AudioSource Sounds;
    public AudioClip[] sound_array;
    public GameObject NotEnough_MSG;

    void Start()
    {
    highCardResult.SetActive(false);
    pairResult.SetActive(false);
    doublePairResult.SetActive(false);
    trioResult.SetActive(false);
    fullHouseResult.SetActive(false);
    fourOfAKindResult.SetActive(false);
    straightResult.SetActive(false);
    flushResult.SetActive(false);
    straightFlushResult.SetActive(false);
    royalFlushResult.SetActive(false);
    NotEnough_MSG.SetActive(false);
    gems = PlayerPrefs.GetInt("Gems", gems);
        // Initialize cardSprites dictionary
        foreach (var cardTypeSprite in cardTypeSprites)
        {
            foreach (var rankSprite in cardTypeSprite.rankSprites)
            {
                string key = $"{cardTypeSprite.typeName}_{rankSprite.rank}";
                cardSprites[key] = rankSprite.sprite;
            }
        }

        // Set all card prefabs to the unrevealed image initially
        foreach (var cardPrefab in cardPrefabs)
        {
            cardPrefab.GetComponent<Image>().sprite = unrevealedCardImage;
        }
        for(int i = 0; i < typeResult.Length; i++)
        {
            typeResult[i].SetActive(false);
        }
    }

    // This method can be linked to a button to reveal random cards
public void RevealRandomCards()
{
    if(energy == 0)
    {
        Sounds.PlayOneShot(sound_array[0]);
        NotEnough_MSG.SetActive(true);
        StartCoroutine(MessageHang());
    }
    else
    {
    Sounds.PlayOneShot(sound_array[1]);
    energy--;
    drawnRanks.Clear();
    drawnTypes.Clear();
    drawnCardCombinations.Clear(); // Clear the list of previously drawn card combinations
    
    foreach (var cardPrefab in cardPrefabs)
    {
        string cardKey;
        int randomTypeIndex;
        int randomRankIndex;
        string typeName;
        Sprite cardSprite;
        
        // Generate a unique card combination (type and rank)
        do
        {
            randomTypeIndex = Random.Range(0, cardTypeSprites.Count);
            typeName = cardTypeSprites[randomTypeIndex].typeName;
            randomRankIndex = Random.Range(0, 13); // Assuming 13 ranks from Ace to King
            cardKey = $"{typeName}_{randomRankIndex}";
        } while (drawnCardCombinations.Contains(cardKey)); // Ensure this combination hasn't already been drawn
        
        // Once a unique combination is found, add it to the list to prevent duplicates
        drawnCardCombinations.Add(cardKey);
        drawnRanks.Add(randomRankIndex);
        drawnTypes.Add(typeName);

        // Attempt to fetch the sprite for the generated card and assign it to the prefab
        if (cardSprites.TryGetValue(cardKey, out cardSprite))
        {
            cardPrefab.GetComponent<Image>().sprite = cardSprite;
        }
        else
        {
            Debug.LogError("Card sprite not found for key: " + cardKey);
        }
    }
    PlayerPrefs.SetInt("energy", energy);
    CheckForCombinations(); // Proceed to check for any card combinations
    }
}
public void StartCheatRoll(GameObject targetResult)
    {
        StartCoroutine(CheatRoll(targetResult));
    }

    // The Coroutine for CheatRoll
    private IEnumerator CheatRoll(GameObject targetResult)
    {
        // Loop until the target result GameObject becomes active
        do
        {
            RevealRandomCards();
            yield return null; // Wait for the next frame
        } while (!targetResult.activeSelf);
    }
    private bool IsStraight(List<int> ranks)
{
    var sortedRanks = ranks.OrderBy(rank => rank).ToList();

    // Check for the presence of an Ace
    bool containsAce = sortedRanks.Contains(0);

    if (containsAce)
    {
        // Attempt to form a straight using Ace as low (Ace-2-3-4-5)
        bool isAceLowStraight = sortedRanks.SequenceEqual(new List<int> { 0, 1, 2, 3, 4 });

        // Attempt to form a straight with Ace as high (10-Jack-Queen-King-Ace)
        // Remove the Ace (0), temporarily treat it as 13 for sorting and comparison
        var highAceRanks = new List<int>(sortedRanks);
        highAceRanks.Remove(0);
        highAceRanks.Add(13); // Temporarily treat Ace as the highest card

        bool isAceHighStraight = true;
        highAceRanks = highAceRanks.OrderBy(rank => rank).ToList();
        for (int i = 0; i < highAceRanks.Count - 1; i++)
        {
            if (highAceRanks[i] + 1 != highAceRanks[i + 1])
            {
                isAceHighStraight = false;
                break;
            }
        }

        return isAceLowStraight || isAceHighStraight;
    }
    else
    {
        // Check for regular straight (no Ace or Ace not needed)
        for (int i = 0; i < sortedRanks.Count - 1; i++)
        {
            if (sortedRanks[i] + 1 != sortedRanks[i + 1])
            {
                return false; // Not a straight if any gap found
            }
        }
        return true; // No gaps found, it's a straight
    }
}
private bool IsStraightFlush(List<int> ranks, List<string> types, out bool isRoyalFlush)
{
    isRoyalFlush = false;
    if (types.Distinct().Count() != 1) return false; // Ensure all cards are of the same suit

    var sortedRanks = ranks.OrderBy(rank => rank).ToList();

    // The ranks for a Royal Flush are 10, J(11), Q(12), K(13), and A(0 or 14)
    var royalFlushRanks = new List<int> {9, 10, 11, 12, 0}; // Including Ace as '0'

    // Check if drawn ranks match the Royal Flush ranks (regardless of order)
    isRoyalFlush = royalFlushRanks.All(rank => sortedRanks.Contains(rank));

    if (isRoyalFlush)
    {
        // If it's a Royal Flush, no need to check for a standard Straight Flush
        return true;
    }

    // Continue with Straight Flush check if not a Royal Flush
    bool isStraightFlush = IsStraight(sortedRanks); // Your existing IsStraight logic

    return isStraightFlush;
}
private void CheckForCombinations()
{
    // Reset results
    highCardResult.SetActive(false);
    pairResult.SetActive(false);
    doublePairResult.SetActive(false);
    trioResult.SetActive(false);
    fullHouseResult.SetActive(false);
    fourOfAKindResult.SetActive(false); // Reset Four of a Kind result
    flushResult.SetActive(false);
    straightResult.SetActive(false);
    flushResult.SetActive(false);
    straightFlushResult.SetActive(false);
    royalFlushResult.SetActive(false);

foreach (var result in typeResult)
    {
        result.SetActive(false); // Ensure specific type results are also reset
    }

    bool hasHighCard = drawnRanks.Exists(rankIndex => rankIndex == 0 || rankIndex == 12 || rankIndex == 11 || rankIndex == 10);
    highCardResult.SetActive(hasHighCard);
    gems += 1;
    PlayerPrefs.SetInt("Gems", gems);

    // Check for Pairs, Double Pairs, Trios, Full House, and Four of a Kind
    Dictionary<int, int> rankCount = new Dictionary<int, int>();
    foreach (int rank in drawnRanks)
    {
        if (rankCount.ContainsKey(rank))
        {
            rankCount[rank]++;
        }
        else
        {
            rankCount.Add(rank, 1);
        }
    }

    int pairsFound = 0;
    bool hasTrio = false;
    bool hasFourOfAKind = false;
    foreach (var count in rankCount.Values)
    {
        if (count == 2)
        {
            pairsFound++;
        }
        else if (count == 3)
        {
            hasTrio = true;
        }
        else if (count == 4)
        {
            hasFourOfAKind = true;
            break; // Once a Four of a Kind is found, no need to check further
        }
    }
        if (drawnTypes.Distinct().Count() == 1) // All cards are of the same type
    {
        gems += 30;
        PlayerPrefs.SetInt("Gems", gems);
        flushResult.SetActive(true); // Activate the general flush result
        
        // Determine the specific type of flush and activate the corresponding result
        string flushType = drawnTypes.First();
        int typeIndex = cardTypeSprites.FindIndex(type => type.typeName == flushType);
        if (typeIndex != -1)
        {
            typeResult[typeIndex].SetActive(true);
        }
    }
    // Determine if it's a Full House (a Pair and a Trio in the same hand)
    bool hasFullHouse = pairsFound == 1 && hasTrio;

    // Activate GameObjects based on combinations found, with precedence given to Four of a Kind
    if (hasFourOfAKind)
    {
        gems += 50;
        PlayerPrefs.SetInt("Gems", gems);
        fourOfAKindResult.SetActive(true);
    }
    else if (hasFullHouse)
    {
        PlayerPrefs.SetInt("Gems", gems);
        gems += 40;
        fullHouseResult.SetActive(true);
    }
    else
    {
        pairResult.SetActive(pairsFound == 1);
        doublePairResult.SetActive(pairsFound == 2);
        trioResult.SetActive(hasTrio);
        if(pairsFound == 1 || pairsFound == 2)
        {
            gems += 3*pairsFound;
            PlayerPrefs.SetInt("Gems", gems);
        }
        else if(hasTrio)
        {
            gems += 10;
            PlayerPrefs.SetInt("Gems", gems);
        }
    }
    if (IsStraight(drawnRanks))
    {
        straightResult.SetActive(true);
        gems += 20;
        PlayerPrefs.SetInt("Gems", gems);
    }
    else
    {
        straightResult.SetActive(false);
    }
bool isRoyalFlush;
    if (IsStraightFlush(drawnRanks, drawnTypes, out isRoyalFlush))
    {
        if (isRoyalFlush)
        {
            Sounds.PlayOneShot(sound_array[2]);
            gems += 100;
            PlayerPrefs.SetInt("Gems", gems);
            royalFlushResult.SetActive(true);
            // Assuming a Royal Flush also qualifies as a Flush, activate the type result.
            string flushType = drawnTypes.First();
            int typeIndex = cardTypeSprites.FindIndex(type => type.typeName == flushType);
            if (typeIndex != -1)
            {
                typeResult[typeIndex].SetActive(true); // Show specific type, e.g., "Diamond Royal Flush!"
            }
        }
        else // Handle Straight Flush without being a Royal Flush
        {
            gems += 75;
            PlayerPrefs.SetInt("Gems", gems);
            straightFlushResult.SetActive(true);
        }
    }
    else
    {
        royalFlushResult.SetActive(false);
        straightFlushResult.SetActive(false);
        // Further combination checks...
    }
}
    IEnumerator MessageHang()
    {
            for(float f = 1f; f >= -0.05f; f -= 0.05f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            NotEnough_MSG.SetActive(false);
    }
void Update()
{
    gems_counter.text = gems.ToString();
    energy_bar.value = energy;
    energy = PlayerPrefs.GetInt("energy", energy);
    // Check for space key to cycle through cards
    if (Input.GetKeyDown(KeyCode.Space))
    {
        currentCardIndex++;
        if (currentCardIndex >= cardPrefabs.Length) // Reset index if it goes beyond the last card
        {
            currentCardIndex = 0;
        }
    }

    // Check for arrow keys to change the type or rank of the current card
    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
    {
        ChangeCardType(currentCardIndex, Input.GetKeyDown(KeyCode.RightArrow));
        CheckForCombinations();
    }
    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
    {
        ChangeCardRank(currentCardIndex, Input.GetKeyDown(KeyCode.UpArrow));
        CheckForCombinations();
    }
}

public void ChangeCardType(int cardIndex, bool next)
{
    int currentTypeIndex = cardTypeSprites.FindIndex(type => type.typeName == drawnTypes[cardIndex]);
    if (next)
    {
        currentTypeIndex = (currentTypeIndex + 1) % cardTypeSprites.Count;
    }
    else
    {
        if (--currentTypeIndex < 0) currentTypeIndex = cardTypeSprites.Count - 1;
    }
    drawnTypes[cardIndex] = cardTypeSprites[currentTypeIndex].typeName;
    UpdateCardDisplay(cardIndex);
}

public void ChangeCardRank(int cardIndex, bool increase)
{
    int rank = drawnRanks[cardIndex];
    if (increase)
    {
        rank = (rank + 1) % 13; // Assuming 13 ranks
    }
    else
    {
        if (--rank < 0) rank = 12;
    }
    drawnRanks[cardIndex] = rank;
    UpdateCardDisplay(cardIndex);
}

private void UpdateCardDisplay(int cardIndex)
{
    // Update the card's sprite based on the new type and rank
    string cardKey = $"{drawnTypes[cardIndex]}_{drawnRanks[cardIndex]}";
    if (cardSprites.TryGetValue(cardKey, out Sprite newSprite))
    {
        cardPrefabs[cardIndex].GetComponent<Image>().sprite = newSprite;
    }
    else
    {
        Debug.LogError("Card sprite not found for key: " + cardKey);
    }
}
[System.Serializable]
public class CardTypeSprites
{
    public string typeName;
    public List<RankSprite> rankSprites;
}

    [System.Serializable]
    public class RankSprite
    {
        public int rank; // From 0 (Ace) to 12 (King)
        public Sprite sprite;
    }
}

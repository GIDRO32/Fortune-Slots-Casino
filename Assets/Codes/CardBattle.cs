using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBattle : MonoBehaviour
{
    public int health = 1; // Health variable
    public Text healthText; // For displaying the card's level
    public Text enemyHealthText;
    public Image cardImage; // For displaying the card's sprite
    public float speed = 5f; // Speed at which the card moves
    public GameObject enemyCard; // The enemy card this card will attack, assign in Inspector
    [SerializeField]
    private List<CardTypeSprites> cardTypeSprites = new List<CardTypeSprites>();
    private int currentTypeIndex; // Index for the current suit
     public Image blank;

    private Vector2 startPosition; // To remember the card's starting position

 void Start()
    {
        startPosition = transform.position;
        SetRandomAppearance(); // Set random appearance at start
        UpdateHealthText(); // Update the health text
    }

    [System.Serializable]
    public class CardTypeSprites
    {
        public string typeName; // Name of the card type (suit)
        public List<Sprite> rankSprites; // Sprites for each rank within this type, used as decoration
    }

public void Attack()
{
    if (health <= 0) return;
    if (enemyCard == null || enemyCard.GetComponent<CardBattle>().health <= 0)
    {
        StartCoroutine(MoveTowardsTarget(enemyCard.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
        BattleMode.Instance.AttackHealthBar(health);
        this.ChangeHealth(0, true); // Specify it's a health bar attack
    }
    else
    {
        StartCoroutine(MoveTowardsTarget(enemyCard.transform.position, () => {
            ResolveBattle();
            StartCoroutine(MoveTowardsTarget(startPosition, null));
        }));
    }
}

    private IEnumerator MoveTowardsTarget(Vector3 targetPosition, System.Action onReached)
    {
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        onReached?.Invoke();
    }

public void ChangeHealth(int newHealth, bool isHealthBarAttack = false)
{
    int healthDifference = newHealth - health;
    health = newHealth;

    if (health <= 0)
    {
        SetAsDefeated(); // Set card as defeated when health is 0 or less
    }

    UpdateHealthText(); // Update health text regardless

    // Only adjust enemy health if this is not a health bar attack and the card has an enemy
    if (!isHealthBarAttack && healthDifference < 0 && enemyCard != null)
    {
        CardBattle enemyBattleScript = enemyCard.GetComponent<CardBattle>();
        if (enemyBattleScript != null)
        {
            enemyBattleScript.health = enemyBattleScript.health + healthDifference;
        }
    }
}

    public void RestoreCardVisuals()
    {
        // Restore card to full alpha
        var color = cardImage.color;
        color.a = 1f; // Full alpha
        cardImage.color = color;

        // Assign a random suit and rank sprite
        SetRandomAppearance();
    }
    private void SetRandomAppearance()
    {
        // Randomly select a suit and rank for decoration purposes
        int typeIndex = Random.Range(0, cardTypeSprites.Count);
        int rankIndex = Random.Range(0, cardTypeSprites[typeIndex].rankSprites.Count);
        cardImage.sprite = cardTypeSprites[typeIndex].rankSprites[rankIndex];
    }

    private void UpdateHealthText()
    {
        healthText.text = health.ToString(); // Display current health
    }

    private void ResolveBattle()
    {
        CardBattle enemyBattleScript = enemyCard.GetComponent<CardBattle>();
        if (enemyBattleScript != null && enemyBattleScript.health > 0)
        {
            if (this.health > enemyBattleScript.health)
            {
                this.health -= enemyBattleScript.health;
                enemyBattleScript.SetAsDefeated();
            }
            else
            {
                enemyBattleScript.health -= this.health;
                enemyHealthText.text = enemyBattleScript.health.ToString();
                SetAsDefeated();
            }
            UpdateHealthText(); // Ensure health text is updated after battle
        }
    }
    public void AddHealth(int value)
    {
        health += value;
        UpdateHealthText();
        // Ensure health updates don't inadvertently defeat the card
        if (health <= 0) SetAsDefeated();
    }

public void Shield()
{
    if (health <= 0) return;
    if (enemyCard == null || enemyCard.GetComponent<CardBattle>().health <= 0)
    {
        BattleMode.Instance.AttackHealthBar(health); // Attack health bar without losing health
        StartCoroutine(MoveTowardsTarget(enemyCard.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
    }
    else
    {
        ShieldAttackResolve(); // Existing logic
    }
}

private void ShieldAttackResolve()
{
    CardBattle enemyBattleScript = enemyCard.GetComponent<CardBattle>();
    if (enemyBattleScript != null && enemyBattleScript.health > 0)
    {
        enemyBattleScript.ChangeHealth(enemyBattleScript.health - this.health, false); // Enemy loses health but attacker does not
        StartCoroutine(MoveTowardsTarget(enemyCard.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
    }
}


public void Nectar()
{
    if (health <= 0) return;
    if (enemyCard == null || enemyCard.GetComponent<CardBattle>().health <= 0)
    {
        BattleMode.Instance.AttackHealthBar(2 * health); // Double damage to the health bar
        this.ChangeHealth(health - 2 * health, true); // Specify it's a health bar attack
        StartCoroutine(MoveTowardsTarget(enemyCard.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
    }
    else
    {
        NectarAttackResolve(); // Existing logic
    }
}

private void NectarAttackResolve()
{
CardBattle enemyBattleScript = enemyCard.GetComponent<CardBattle>();
        if (enemyBattleScript != null && enemyBattleScript.health > 0)
        {
            if (this.health > enemyBattleScript.health)
            {
                this.health -= enemyBattleScript.health;
                enemyBattleScript.SetAsDefeated();
            }
            else
            {
                enemyBattleScript.health -= this.health*2;
                enemyHealthText.text = enemyBattleScript.health.ToString();
                SetAsDefeated();
            }
            UpdateHealthText(); // Ensure health text is updated after battle
        }
}
    public void RandomAction()
    {
        // Chooses one of the three actions randomly
        int randomAction = UnityEngine.Random.Range(0, 3);
        switch (randomAction)
        {
            case 0:
            int[] healthValue = {3, 5, 7, 10, 15};
                AddHealth(healthValue[UnityEngine.Random.Range(0, 4)]); // Adds 1 to 10 health randomly
                break;
            case 1:
                Shield();
                break;
            case 2:
                Nectar();
                break;
        }
    }
    private void SetAsDefeated()
    {
        health = 0; // Update health to indicate defeat
        UpdateHealthText(); // Reflect the health change in text
    }
}

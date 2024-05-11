using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBattle : MonoBehaviour
{
    public int health = 1; // Health variable
    public Text healthText; // For displaying the unit's level
    public Text enemyHealthText;
    public Image unitImage; // For displaying the unit's sprite
    public float speed = 5f; // Speed at which the unit moves
    public GameObject enemyUnit; // The enemy unit this unit will attack, assign in Inspector
    [SerializeField]
    private List<UnitTypeSprites> unitTypeSprites = new List<UnitTypeSprites>();
    private int currentTypeIndex; // Index for the current suit
     public Image blank;

    private Vector2 startPosition; // To remember the unit's starting position

 void Start()
    {
        startPosition = transform.position;
        SetRandomAppearance(); // Set random appearance at start
        UpdateHealthText(); // Update the health text
    }

    [System.Serializable]
    public class UnitTypeSprites
    {
        public string typeName; // Name of the unit type (suit)
        public List<Sprite> rankSprites; // Sprites for each rank within this type, used as decoration
    }

public void Attack()
{
    if (health <= 0) return;
    if (enemyUnit == null || enemyUnit.GetComponent<UnitBattle>().health <= 0)
    {
        StartCoroutine(MoveTowardsTarget(enemyUnit.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
        BattleMode.Instance.AttackHealthBar(health);
        this.ChangeHealth(0, true); // Specify it's a health bar attack
    }
    else
    {
        StartCoroutine(MoveTowardsTarget(enemyUnit.transform.position, () => {
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
        SetAsDefeated(); // Set unit as defeated when health is 0 or less
    }

    UpdateHealthText(); // Update health text regardless

    // Only adjust enemy health if this is not a health bar attack and the unit has an enemy
    if (!isHealthBarAttack && healthDifference < 0 && enemyUnit != null)
    {
        UnitBattle enemyBattleScript = enemyUnit.GetComponent<UnitBattle>();
        if (enemyBattleScript != null)
        {
            enemyBattleScript.health = enemyBattleScript.health + healthDifference;
        }
    }
}

    public void RestoreUnitVisuals()
    {
        // Restore unit to full alpha
        var color = unitImage.color;
        color.a = 1f; // Full alpha
        unitImage.color = color;

        // Assign a random suit and rank sprite
        SetRandomAppearance();
    }
    private void SetRandomAppearance()
    {
        // Randomly select a suit and rank for decoration purposes
        int typeIndex = Random.Range(0, unitTypeSprites.Count);
        int rankIndex = Random.Range(0, unitTypeSprites[typeIndex].rankSprites.Count);
        unitImage.sprite = unitTypeSprites[typeIndex].rankSprites[rankIndex];
    }

    private void UpdateHealthText()
    {
        healthText.text = health.ToString(); // Display current health
    }

    private void ResolveBattle()
    {
        UnitBattle enemyBattleScript = enemyUnit.GetComponent<UnitBattle>();
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
        // Ensure health updates don't inadvertently defeat the unit
        if (health <= 0) SetAsDefeated();
    }

public void Shield()
{
    if (health <= 0) return;
    if (enemyUnit == null || enemyUnit.GetComponent<UnitBattle>().health <= 0)
    {
        BattleMode.Instance.AttackHealthBar(health); // Attack health bar without losing health
        StartCoroutine(MoveTowardsTarget(enemyUnit.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
    }
    else
    {
        ShieldAttackResolve(); // Existing logic
    }
}

private void ShieldAttackResolve()
{
    UnitBattle enemyBattleScript = enemyUnit.GetComponent<UnitBattle>();
    if (enemyBattleScript != null && enemyBattleScript.health > 0)
    {
        enemyBattleScript.ChangeHealth(enemyBattleScript.health - this.health, false); // Enemy loses health but attacker does not
        StartCoroutine(MoveTowardsTarget(enemyUnit.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
    }
}


public void Nectar()
{
    if (health <= 0) return;
    if (enemyUnit == null || enemyUnit.GetComponent<UnitBattle>().health <= 0)
    {
        int damageToDeal = Mathf.Min(2 * health, enemyUnit.GetComponent<UnitBattle>().health); // Calculate the damage, ensuring it doesn't exceed the enemy's health
        BattleMode.Instance.AttackHealthBar(damageToDeal); // Double damage to the health bar
        this.ChangeHealth(health - damageToDeal, true); // Specify it's a health bar attack
        StartCoroutine(MoveTowardsTarget(enemyUnit.transform.position, () => {StartCoroutine(MoveTowardsTarget(startPosition, null));}));
    }
    else
    {
        NectarAttackResolve(); // Existing logic
    }
}

private void NectarAttackResolve()
{
UnitBattle enemyBattleScript = enemyUnit.GetComponent<UnitBattle>();
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
    private void SetAsDefeated()
    {
        health = 0; // Update health to indicate defeat
        UpdateHealthText(); // Reflect the health change in text
    }
}

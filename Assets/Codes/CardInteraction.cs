using UnityEngine;

public class CardInteraction : MonoBehaviour
{
    public CardGame cardGameController; // Assign in the inspector
    public int cardIndex; // Assign each card a unique index in the inspector

    // void OnMouseDown()
    // {
    //     if (Input.GetMouseButton(0)) // Left click
    //     {
    //         Debug.Log("Card index: "+ cardIndex);
    //         PlayerPrefs.SetInt("Card num",cardIndex);
    //         cardGameController.ChangeCardType(cardIndex);
    //     }
    //     else if (Input.GetMouseButton(1)) // Right click
    //     {
    //         Debug.Log("Card index: "+ cardIndex);
    //         PlayerPrefs.SetInt("Card num",cardIndex);
    //         cardGameController.ChangeCardRank(cardIndex);
    //     }
    // }
}

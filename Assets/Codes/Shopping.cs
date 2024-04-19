using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Shopping : MonoBehaviour
{
    #region SingleTon:Shopping
    public static Shopping Instance;
    public AudioSource Sounds;
    public AudioClip Buy;
    public AudioClip CantBuy;
    void Awake()
    {
        if(Instance == null)
        Instance = this;
        else
        Destroy(gameObject);
    }
    #endregion
    [System.Serializable]
    public class ShopItem
{
    public Sprite Image;
    public int price;
    public bool isAvailable = false;
}
    public List<ShopItem> ShopItemList;
    [SerializeField]GameObject ItemTemplate;
    GameObject g;
    [SerializeField] Transform ShopScrolling;
    [SerializeField] GameObject ShopPanel;
    [SerializeField] Text Balance;
    Button buyBtn;
    public GameObject NotEnough_MSG;
    public Sprite Blank;
    void Start()
    { 
        LoadItemAvailability();
        NotEnough_MSG.SetActive(false);
        int len = ShopItemList.Count;
        for(int i = 0; i < len; i++)
        {
            g = Instantiate(ItemTemplate, ShopScrolling);
            g.transform.GetChild(0).GetComponent<Image>().sprite = Blank;
            g.transform.GetChild(1).GetComponent<Text>().text = ShopItemList[i].price.ToString();
            buyBtn = g.transform.GetChild(2).GetComponent<Button>();
            if(ShopItemList[i].isAvailable)
            {
                g.transform.GetChild(0).GetComponent<Image>().sprite = ShopItemList[i].Image;
                DisableBuyButton();
            }
            buyBtn.AddEventListener(i, OnBuyBtnClicked);
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
    void LoadItemAvailability()
    {
        for (int i = 0; i < ShopItemList.Count; i++)
        {
            ShopItemList[i].isAvailable = PlayerPrefs.GetInt("ItemAvailable_" + i, 0) == 1;
        }
    }
void OnBuyBtnClicked(int itemIndex)
{
    if(Product.Instance.HasEnoughCoins(ShopItemList[itemIndex].price))
    {
        // Deduct coins and mark the item as available
        Product.Instance.UseCoins(ShopItemList[itemIndex].price);
        ShopItemList[itemIndex].isAvailable = true;
        PlayerPrefs.SetInt("ItemAvailable_" + itemIndex, 1); // Save purchase
        PlayerPrefs.Save();

        // Directly update the UI for this item
        Transform itemTransform = ShopScrolling.GetChild(itemIndex);
        itemTransform.GetChild(0).GetComponent<Image>().sprite = ShopItemList[itemIndex].Image; // Update the image to the purchased item sprite
        Button buyButton = itemTransform.GetChild(2).GetComponent<Button>();
        if (buyButton != null)
        {
            buyButton.interactable = false;
            buyButton.transform.GetChild(0).GetComponent<Text>().text = "Owned";
        }

        Sounds.PlayOneShot(Buy);
    }
    else
    {
        Debug.Log("Not Enough!");
        NotEnough_MSG.SetActive(true);
        StartCoroutine("MessageHang");
        Sounds.PlayOneShot(CantBuy);
    }
}
public void ResetPurchases()
{
    for (int i = 0; i < ShopItemList.Count; i++) // Iterate through all items
    {
        ShopItemList[i].isAvailable = false;
        PlayerPrefs.SetInt("ItemAvailable_" + i, 0);

        // Directly update the UI for this item to revert to the "Blank" sprite
        if (ShopScrolling != null && ShopScrolling.childCount > i)
        {
            Transform itemTransform = ShopScrolling.GetChild(i);
            itemTransform.GetChild(0).GetComponent<Image>().sprite = Blank; // Set to Blank sprite

            Button buyButton = itemTransform.GetChild(2).GetComponent<Button>();
            if (buyButton != null)
            {
                buyButton.interactable = true;
                buyButton.transform.GetChild(0).GetComponent<Text>().text = "Buy";
            }
        }
    }

    PlayerPrefs.SetInt("Total", 2000); // Assuming you want to reset the coin balance
    PlayerPrefs.Save();

    Update(); // Optionally call Update to refresh the UI immediately, if necessary
}

    void DisableBuyButton()
    {
        buyBtn.interactable = false;
        buyBtn.transform.GetChild(0).GetComponent<Text>().text = "Owned";
    }
    void Update()
    {
        Balance.text = Product.Instance.Coins.ToString();
        LoadItemAvailability();
    }
    public void OpenShop()
    {
        ShopPanel.SetActive(true);
    }
    public void CloseShop()
    {
        ShopPanel.SetActive(false);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerHand : MonoBehaviour
{
    public static PlayerHand instance;
    public List<GameObject> Hand;
    public List<GameObject> DiscardPile;

    public bool isPlayerViewingTheirHand = false;
    // Start is called before the first frame update
    void Awake()
    {
        MakeInstance();
        Hand = new List<GameObject>();
        DiscardPile = new List<GameObject>();
        InitializePlayerHand();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    void InitializePlayerHand()
    {
        GameObject playerCardHandObject = GameObject.FindGameObjectWithTag("PlayerHand");
        foreach (Transform cardChild in playerCardHandObject.transform)
        {
            Hand.Add(cardChild.gameObject);
        }
        // Sort the hand based on the power?
        Hand = Hand.OrderByDescending(o => o.GetComponent<Card>().Power).ToList();
    }
    public void ShowPlayerHandOnScreen()
    {
        isPlayerViewingTheirHand = true;
        Vector3 cardLocation = new Vector3(-10f, 1.5f, 0f);
        foreach (GameObject playerCard in Hand)
        {
            if (!playerCard.activeInHierarchy)
            {
                playerCard.SetActive(true);
                playerCard.transform.position = cardLocation;
            }
            cardLocation.x += 4.5f;
        }
        // Hide land text since it displays over cards
        GameObject landHolder = GameObject.FindGameObjectWithTag("LandHolder");
        foreach (Transform landChild in landHolder.transform)
        {
            LandScript landScript = landChild.GetComponent<LandScript>();
            landScript.HideUnitText();
        }
    }
    public void HidePlayerHandOnScreen()
    {
        isPlayerViewingTheirHand = false;
        foreach (GameObject playerCard in Hand)
        {
            if (playerCard.activeInHierarchy)
            {
                playerCard.SetActive(false);
            }
        }
        GameObject landHolder = GameObject.FindGameObjectWithTag("LandHolder");
        foreach (Transform landChild in landHolder.transform)
        {
            LandScript landScript = landChild.GetComponent<LandScript>();
            landScript.UnHideUnitText();
        }
    }
}

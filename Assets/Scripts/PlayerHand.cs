using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class PlayerHand : NetworkBehaviour
{
    [SyncVar] public string ownerPlayerName;
    [SyncVar] public int ownerConnectionId;
    [SyncVar] public int ownerPlayerNumber;

    [SyncVar] public bool isHandInitialized = false;

    public List<GameObject> Hand = new List<GameObject>();
    public List<GameObject> DiscardPile = new List<GameObject>();
    public SyncList<uint> HandNetId = new SyncList<uint>();
    public SyncList<uint> DiscardPileNetId = new SyncList<uint>();

    public bool isPlayerViewingTheirHand = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializePlayerHand()
    {
        if (!isHandInitialized)
        {
            GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");
            foreach (GameObject card in allCards)
            {
                Card cardScript = card.GetComponent<Card>();
                if (cardScript.ownerConnectionId == this.ownerConnectionId)
                {
                    this.Hand.Add(card);
                }
            }
            Hand = Hand.OrderByDescending(o => o.GetComponent<Card>().Power).ToList();
            CmdInitializePlayerHand();
            Debug.Log("Hand initialized for: " + ownerPlayerName);
        }
    }
    [Command]
    void CmdInitializePlayerHand()
    {
        if (!this.isHandInitialized)
        {
            GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");
            foreach (GameObject card in allCards)
            {
                Card cardScript = card.GetComponent<Card>();
                if (cardScript.ownerConnectionId == this.ownerConnectionId)
                {
                    this.HandNetId.Add(card.GetComponent<NetworkIdentity>().netId);
                }
            }
            this.isHandInitialized = true;
            Debug.Log("Hand initialized for: " + ownerPlayerName);
        }
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

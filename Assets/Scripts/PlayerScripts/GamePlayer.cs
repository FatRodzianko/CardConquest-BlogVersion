using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;

public class GamePlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar] public string PlayerName;
    [SyncVar] public int ConnectionId;
    [SyncVar] public int playerNumber;

    [Header("Player Unit Prefabs")]
    [SerializeField] GameObject PlayerUnitHolder;
    [SerializeField] GameObject Player1Inf;
    [SerializeField] GameObject Player1Tank;
    [SerializeField] GameObject Player2Inf;
    [SerializeField] GameObject Player2Tank;

    [Header("Player Card Prefabs")]
    [SerializeField] GameObject PlayerCardHand;
    [SerializeField] GameObject[] Cards;

    [Header("Player Base/Units")]
    public GameObject myUnitHolder;
    public GameObject myPlayerCardHand;
    [SyncVar] public GameObject myPlayerBase;

    [Header("Player Statuses")]
    [SyncVar] public bool HaveSpawnedUnits = false;
    [SyncVar] public bool HaveSpawnedCards = false;
    [SyncVar] public bool GotPlayerBase = false;

    private NetworkManagerCC game;
    private NetworkManagerCC Game
    {
        get
        {
            if (game != null)
            {
                return game;
            }
            return game = NetworkManagerCC.singleton as NetworkManagerCC;
        }
    }
    public override void OnStartAuthority()
    {
        gameObject.name = "LocalGamePlayer";
        gameObject.tag = "LocalGamePlayer";
        Debug.Log("Labeling the local player: " + this.PlayerName);
    }
    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        Game.GamePlayers.Add(this);
        Debug.Log("Added to GamePlayer list: " + this.PlayerName);
    }
    public override void OnStopClient()
    {
        Debug.Log(PlayerName + " is quiting the game.");
        Game.GamePlayers.Remove(this);
        Debug.Log("Removed player from the GamePlayer list: " + this.PlayerName);
    }
    [Server]
    public void SetPlayerName(string playerName)
    {
        this.PlayerName = playerName;
    }
    [Server]
    public void SetConnectionId(int connId)
    {
        this.ConnectionId = connId;
    }
    [Server]
    public void SetPlayerNumber(int playerNum)
    {
        this.playerNumber = playerNum;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SpawnPlayerUnits()
    {
        if (!this.HaveSpawnedUnits)
        {
            Debug.Log("SpawnPlayerUnits() for: " + this.PlayerName + " with player number: " + this.playerNumber);
            CmdSpawnPlayerUnits();
        }
    }
    [Command]
    public void CmdSpawnPlayerUnits()
    {
        Debug.Log("Running CmdSpawnPlayerUnits on the server.");
        NetworkIdentity networkIdentity = connectionToClient.identity;
        GamePlayer requestingPlayer = networkIdentity.GetComponent<GamePlayer>();
        if (requestingPlayer.playerNumber == 1 && !requestingPlayer.HaveSpawnedUnits)
        {
            //Instantiate the unit holder
            GameObject playerUnitHolder = Instantiate(PlayerUnitHolder, transform.position, Quaternion.identity);
            //Get the unit holder's script to set the owner variables
            PlayerUnitHolder script = playerUnitHolder.GetComponent<PlayerUnitHolder>();
            script.ownerPlayerName = requestingPlayer.PlayerName;
            script.ownerConnectionId = requestingPlayer.ConnectionId;
            script.ownerPlayerNumber = requestingPlayer.playerNumber;
            //Spawn the unity holder on the network and assign owner/authority to the requesting client
            NetworkServer.Spawn(playerUnitHolder, connectionToClient);
            //Spawn the player1 infantry units
            for (int i = 0; i < 6; i++)
            {
                GameObject playerInfantry = Instantiate(Player1Inf, transform.position, Quaternion.identity);
                UnitScript unitScript = playerInfantry.GetComponent<UnitScript>();
                unitScript.ownerPlayerName = requestingPlayer.PlayerName;
                unitScript.ownerConnectionId = requestingPlayer.ConnectionId;
                unitScript.ownerPlayerNumber = requestingPlayer.playerNumber;
                NetworkServer.Spawn(playerInfantry, connectionToClient);
            }
            //Spawn player1 tanks
            for (int i = 0; i < 4; i++)
            {
                GameObject playerTank = Instantiate(Player1Tank, transform.position, Quaternion.identity);
                UnitScript unitScript = playerTank.GetComponent<UnitScript>();
                unitScript.ownerPlayerName = requestingPlayer.PlayerName;
                unitScript.ownerConnectionId = requestingPlayer.ConnectionId;
                unitScript.ownerPlayerNumber = requestingPlayer.playerNumber;
                NetworkServer.Spawn(playerTank, connectionToClient);
            }
            requestingPlayer.HaveSpawnedUnits = true;
            //Tell all clients to "show" the PlayerUnitHolder - set the correct parent to all the unity holders and run GameplayManager's PutUnitsInUnitBox
            RpcShowSpawnedPlayerUnits(playerUnitHolder);
            Debug.Log("Spawned Player1UnitHolder.");
        }
        else if (requestingPlayer.playerNumber == 2 && !requestingPlayer.HaveSpawnedUnits)
        {
            GameObject playerUnitHolder = Instantiate(PlayerUnitHolder, transform.position, Quaternion.identity);
            PlayerUnitHolder script = playerUnitHolder.GetComponent<PlayerUnitHolder>();
            script.ownerPlayerName = requestingPlayer.PlayerName;
            script.ownerConnectionId = requestingPlayer.ConnectionId;
            script.ownerPlayerNumber = requestingPlayer.playerNumber;
            NetworkServer.Spawn(playerUnitHolder, connectionToClient);
            for (int i = 0; i < 6; i++)
            {
                GameObject playerInfantry = Instantiate(Player2Inf, transform.position, Quaternion.identity);
                UnitScript unitScript = playerInfantry.GetComponent<UnitScript>();
                unitScript.ownerPlayerName = requestingPlayer.PlayerName;
                unitScript.ownerConnectionId = requestingPlayer.ConnectionId;
                unitScript.ownerPlayerNumber = requestingPlayer.playerNumber;
                NetworkServer.Spawn(playerInfantry, connectionToClient);
            }
            //Spawn player1 tanks
            for (int i = 0; i < 4; i++)
            {
                GameObject playerTank = Instantiate(Player2Tank, transform.position, Quaternion.identity);
                UnitScript unitScript = playerTank.GetComponent<UnitScript>();
                unitScript.ownerPlayerName = requestingPlayer.PlayerName;
                unitScript.ownerConnectionId = requestingPlayer.ConnectionId;
                unitScript.ownerPlayerNumber = requestingPlayer.playerNumber;
                NetworkServer.Spawn(playerTank, connectionToClient);
            }
            requestingPlayer.HaveSpawnedUnits = true;
            RpcShowSpawnedPlayerUnits(playerUnitHolder);
            Debug.Log("Spawned Player2UnitHolder.");
        }
        else
        {
            Debug.Log("NO PlayerUnitHolder spawned.");
        }
    }
    [ClientRpc]
    void RpcShowSpawnedPlayerUnits(GameObject playerUnitHolder)
    {
        Debug.Log("You: " + this.PlayerName + " are running RpcShowSpawnedPlayerUnits()");
        GameObject[] infantryUnits = GameObject.FindGameObjectsWithTag("infantry");
        GameObject[] tankUnits = GameObject.FindGameObjectsWithTag("tank");

        if (playerUnitHolder.GetComponent<NetworkIdentity>().hasAuthority && hasAuthority)
        {
            Debug.Log("You: " + this.PlayerName + " have authority over: " + playerUnitHolder);
            playerUnitHolder.SetActive(true);
            playerUnitHolder.transform.SetParent(gameObject.transform);
            PlayerUnitHolder unitHolderScript = playerUnitHolder.GetComponent<PlayerUnitHolder>();
            myUnitHolder = playerUnitHolder;

            foreach (GameObject inf in infantryUnits)
            {
                UnitScript infScript = inf.GetComponent<UnitScript>();
                if (unitHolderScript.ownerConnectionId == infScript.ownerConnectionId)
                {
                    inf.transform.SetParent(playerUnitHolder.transform);
                }
            }

            foreach (GameObject tank in tankUnits)
            {
                UnitScript tankScript = tank.GetComponent<UnitScript>();
                if (unitHolderScript.ownerConnectionId == tankScript.ownerConnectionId)
                {
                    tank.transform.SetParent(playerUnitHolder.transform);
                }
            }
            //GameplayManager.instance.PutUnitsInUnitBox();
        }
        else
        {
            Debug.Log("You: " + this.PlayerName + " DO NOT have authority over: " + playerUnitHolder);
        }

        GameObject[] PlayerUnitHolders = GameObject.FindGameObjectsWithTag("PlayerUnitHolder");
        foreach (GameObject unitHolder in PlayerUnitHolders)
        {
            PlayerUnitHolder unitHolderScript = unitHolder.GetComponent<PlayerUnitHolder>();
            GameObject[] gamePlayers = GameObject.FindGameObjectsWithTag("GamePlayer");
            foreach (GameObject gamePlayer in gamePlayers)
            {
                GamePlayer gamePlayerScript = gamePlayer.GetComponent<GamePlayer>();
                if (gamePlayerScript.ConnectionId == unitHolderScript.ownerConnectionId)
                {
                    foreach (GameObject inf in infantryUnits)
                    {
                        UnitScript infScript = inf.GetComponent<UnitScript>();
                        if (gamePlayerScript.ConnectionId == unitHolderScript.ownerConnectionId && unitHolderScript.ownerConnectionId == infScript.ownerConnectionId)
                        {
                            inf.transform.SetParent(unitHolder.transform);
                            inf.transform.position = new Vector3(-1000, -1000, 0);
                        }
                    }

                    foreach (GameObject tank in tankUnits)
                    {
                        UnitScript tankScript = tank.GetComponent<UnitScript>();
                        if (gamePlayerScript.ConnectionId == unitHolderScript.ownerConnectionId && unitHolderScript.ownerConnectionId == tankScript.ownerConnectionId)
                        {
                            tank.transform.SetParent(unitHolder.transform);
                            tank.transform.position = new Vector3(-1000, -1000, 0);
                        }
                    }
                    unitHolder.transform.SetParent(gamePlayer.transform);
                    gamePlayerScript.myUnitHolder = unitHolder;
                }
            }
        }
    }
    public void SpawnPlayerCards()
    {
        if (!this.HaveSpawnedCards)
        {
            Debug.Log("SpawnPlayerUnits() for: " + this.PlayerName + " with player number: " + this.playerNumber);
            CmdSpawnPlayerCards();
        }
    }
    [Command]
    void CmdSpawnPlayerCards()
    {
        Debug.Log("Running CmdSpawnPlayerCards on the server.");
        NetworkIdentity networkIdentity = connectionToClient.identity;
        GamePlayer requestingPlayer = networkIdentity.GetComponent<GamePlayer>();
        if (!requestingPlayer.HaveSpawnedCards)
        {
            //Instantiate the unit holder
            GameObject playerHand = Instantiate(PlayerCardHand, transform.position, Quaternion.identity);
            //Get the unit holder's script to set the owner variables
            PlayerHand playerHandScript = playerHand.GetComponent<PlayerHand>();
            playerHandScript.ownerPlayerName = requestingPlayer.PlayerName;
            playerHandScript.ownerConnectionId = requestingPlayer.ConnectionId;
            playerHandScript.ownerPlayerNumber = requestingPlayer.playerNumber;
            //Spawn the unity holder on the network and assign owner/authority to the requesting client
            NetworkServer.Spawn(playerHand, connectionToClient);
            foreach (GameObject card in Cards)
            {
                GameObject playerCard = Instantiate(card, transform.position, Quaternion.identity);
                Card playerCardScript = playerCard.GetComponent<Card>();
                playerCardScript.ownerPlayerName = requestingPlayer.PlayerName;
                playerCardScript.ownerConnectionId = requestingPlayer.ConnectionId;
                playerCardScript.ownerPlayerNumber = requestingPlayer.playerNumber;
                playerCard.transform.position = new Vector3(-1000, -1000, 0);
                NetworkServer.Spawn(playerCard, connectionToClient);
            }
            requestingPlayer.HaveSpawnedCards = true;
            //Tell all clients to "show" the PlayerUnitHolder - set the correct parent to all the unity holders and run GameplayManager's PutUnitsInUnitBox
            RpcSpawnPlayerCards();
            Debug.Log("Spawned PlayerHand and Player Cards.");
        }
        else
        {
            Debug.Log("NO PlayerHand or Player cards spawned.");
        }
    }
    [ClientRpc]
    void RpcSpawnPlayerCards()
    {
        GameObject[] allPlayerCardHands = GameObject.FindGameObjectsWithTag("PlayerHand");
        GameObject[] allCards = GameObject.FindGameObjectsWithTag("Card");

        GameObject LocalGamePlayer = GameObject.Find("LocalGamePlayer");
        GamePlayer LocalGamePlayerScript = LocalGamePlayer.GetComponent<GamePlayer>();

        foreach (GameObject playerCardHand in allPlayerCardHands)
        {
            PlayerHand playerCardHandScript = playerCardHand.GetComponent<PlayerHand>();
            if (playerCardHandScript.ownerConnectionId == LocalGamePlayerScript.ConnectionId)
            {
                playerCardHand.transform.SetParent(LocalGamePlayer.transform);
                foreach (GameObject card in allCards)
                {
                    Card cardScript = card.GetComponent<Card>();
                    if (cardScript.ownerConnectionId == LocalGamePlayerScript.ConnectionId)
                    {
                        card.transform.SetParent(playerCardHand.transform);
                    }
                }
                myPlayerCardHand = playerCardHand;
            }
            else
            {
                GameObject[] allGamePlayers = GameObject.FindGameObjectsWithTag("GamePlayer");
                foreach (GameObject gamePlayer in allGamePlayers)
                {
                    GamePlayer gamePlayerScript = gamePlayer.GetComponent<GamePlayer>();
                    if (playerCardHandScript.ownerConnectionId == gamePlayerScript.ConnectionId)
                    {
                        playerCardHand.transform.SetParent(gamePlayerScript.transform);
                        foreach (GameObject card in allCards)
                        {
                            Card cardScript = card.GetComponent<Card>();
                            if (cardScript.ownerConnectionId == gamePlayerScript.ConnectionId)
                            {
                                card.transform.SetParent(playerCardHand.transform);
                            }
                        }
                        gamePlayerScript.myPlayerCardHand = playerCardHand;
                    }
                }
            }
        }
    }
    public void GetPlayerBase()
    {
        if (!GotPlayerBase)
        {
            Debug.Log("Calling CmdSetPlayerBase from: " + this.PlayerName);
            CmdSetPlayerBase();
        }
    }
    [Command]
    void CmdSetPlayerBase()
    {
        NetworkIdentity networkIdentity = connectionToClient.identity;
        GamePlayer requestingPlayer = networkIdentity.GetComponent<GamePlayer>();

        Debug.Log("Running CmdSetPlayerBase on the server for: " + requestingPlayer.PlayerName);

        GameObject[] playerBases = GameObject.FindGameObjectsWithTag("PlayerBase");
        foreach (GameObject playerBase in playerBases)
        {
            PlayerBaseScript playerBaseScript = playerBase.GetComponent<PlayerBaseScript>();
            if (requestingPlayer.playerNumber == playerBaseScript.ownerPlayerNumber)
            {
                playerBaseScript.ownerPlayerName = requestingPlayer.PlayerName;
                playerBaseScript.ownerConnectionId = requestingPlayer.ConnectionId;
                CanPlayerPlaceOnLand(requestingPlayer, playerBase);
                RpcGetPlayerBase();
                break;
            }
        }
    }
    [Server]
    void CanPlayerPlaceOnLand(GamePlayer gamePlayer, GameObject playerBase)
    {
        Debug.Log("Server: Running CanPlayerPlaceOnLand for: " + gamePlayer.PlayerName + " using this base: " + playerBase);
        Vector3 playerBaseLocation = playerBase.transform.position;
        GameObject LandTileHolder = GameObject.FindGameObjectWithTag("LandHolder");

        foreach (Transform landObject in LandTileHolder.transform)
        {
            LandScript landScript = landObject.gameObject.GetComponent<LandScript>();
            float disFromBase = Vector3.Distance(landObject.transform.position, playerBaseLocation);
            Debug.Log(landScript.gameObject.name + "'s distance from player base: " + disFromBase.ToString());
            if (disFromBase <= 6.0f)
            {
                landScript.PlayerCanPlaceHere = gamePlayer.playerNumber;
            }
        }
    }
    [ClientRpc]
    void RpcGetPlayerBase()
    {
        if (!this.GotPlayerBase)
        {
            Debug.Log("Looking for playerbase for: " + this.PlayerName);
            GameObject[] playerBases = GameObject.FindGameObjectsWithTag("PlayerBase");
            foreach (GameObject playerBase in playerBases)
            {
                PlayerBaseScript playerBaseScript = playerBase.GetComponent<PlayerBaseScript>();
                Debug.Log("Playerbase: " + playerBase + " with ownerPlayerNumber: " + playerBaseScript.ownerPlayerNumber + " from player number: " + this.playerNumber);
                if (playerBaseScript.ownerPlayerNumber == this.playerNumber)
                {
                    this.myPlayerBase = playerBase;
                    this.GotPlayerBase = true;
                    Debug.Log("Found playerbase for: " + this.PlayerName);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager instance;
    public string currentGamePhase;

    private List<GameObject> infToPlace;
    private List<GameObject> tanksToPlace;

    [SerializeField]
    private Text GamePhaseText;
    [SerializeField]
    private GameObject UnitPlacementUI, endUnitPlacementButton;
    [SerializeField] Text PlayerReadyText;
    public List<string> readyPlayers = new List<string>();

    [SerializeField]
    private Text unitMovementNoUnitsMovedText;
    [SerializeField]
    private GameObject UnitMovementUI, endUnitMovementButton, resetAllMovementButton;
    [SerializeField] GameObject BattlesDetectedPanel;
    public bool haveUnitsMoved = false;

    [Header("Your Hand Buttons")]
    [SerializeField] private GameObject showPlayerHandButton;
    [SerializeField] private GameObject hidePlayerHandButton;
    [SerializeField] private GameObject showPlayerDiscardButton;

    [Header("Other Player Hand Buttons")]
    [SerializeField] private GameObject showOpponentCardButton;
    [SerializeField] private GameObject hideOpponentCardButton;
    [SerializeField] private GameObject opponentHandButtonPrefab;
    [SerializeField] private GameObject opponentDiscardButtonPrefab;
    public List<GameObject> opponentHandButtons = new List<GameObject>();

    public bool gamePlayerHandButtonsCreated = false;

    [Header("GamePlayers")]
    [SerializeField] private GameObject LocalGamePlayer;
    [SerializeField] private GamePlayer LocalGamePlayerScript;

    [Header("Player Statuses")]
    public bool isPlayerViewingOpponentHand = false;
    public GameObject playerHandBeingViewed = null;

    [Header("Player Battle Info")]
    public SyncDictionary<int, uint> battleSiteNetIds = new SyncDictionary<int, uint>();
    public bool haveBattleSitesBeenDone = false;
    [SyncVar] public int battleNumber;
    [SyncVar] public uint currentBattleSite;

    [Header("Ready Buttons")]
    [SerializeField] private GameObject startBattlesButton;

    [Header("Choose Cards Section")]
    [SerializeField] private GameObject ChooseCardsPanel;
    [SerializeField] private GameObject confirmCardButton;

    // Start is called before the first frame update
    private void Awake()
    {
        MakeInstance();
        infToPlace = new List<GameObject>();
        tanksToPlace = new List<GameObject>();
    }
    void Start()
    {      

        //currentGamePhase = "Unit Placement";
        //SetGamePhaseText();
        //ActivateUnitPlacementUI();
        //PutUnitsInUnitBox();
        //LimitUserPlacementByDistanceToBase();
        GetLocalGamePlayer();
        GetCurrentGamePhase();
        SpawnPlayerUnits();
        SpawnPlayerCards();
        GetPlayerBase();
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
    public void SetGamePhaseText()
    {
        GamePhaseText.text = currentGamePhase;
        if (currentGamePhase == "Unit Placement")
            ActivateUnitPlacementUI();
        if (currentGamePhase == "Battle(s) Detected")
            GamePhaseText.fontSize = 40;
        else
            GamePhaseText.fontSize = 50;
    }
    void ActivateUnitPlacementUI()
    {
        MouseClickManager.instance.canSelectUnitsInThisPhase = true;
        Camera.main.orthographicSize = 8f;
        Camera.main.backgroundColor = Color.gray;
        if (!UnitPlacementUI.activeInHierarchy && currentGamePhase == "Unit Placement")
            UnitPlacementUI.SetActive(true);
        if (endUnitPlacementButton.activeInHierarchy)
            endUnitPlacementButton.SetActive(false);
        if (UnitMovementUI.activeInHierarchy)
            UnitMovementUI.SetActive(false);
    }
    public void PutUnitsInUnitBox()
    {
        GameObject[] PlayerUnitHolders = GameObject.FindGameObjectsWithTag("PlayerUnitHolder");

        foreach (GameObject unitHolder in PlayerUnitHolders)
        {
            PlayerUnitHolder unitHolderScript = unitHolder.GetComponent<PlayerUnitHolder>();
            if (unitHolderScript.ownerConnectionId == LocalGamePlayerScript.ConnectionId)
            {
                foreach (Transform unitChild in unitHolder.transform)
                {
                    if (unitChild.transform.tag == "infantry")
                    {
                        infToPlace.Add(unitChild.gameObject);
                    }
                    else if (unitChild.transform.tag == "tank")
                    {
                        tanksToPlace.Add(unitChild.gameObject);
                    }

                }
                //Begin moving the units into the unit box
                for (int i = 0; i < tanksToPlace.Count; i++)
                {
                    if (i == 0)
                    {
                        Vector3 temp = new Vector3(-14.0f, 8.25f, 0f);
                        tanksToPlace[i].transform.position = temp;
                    }
                    else
                    {
                        int previousTank = i - 1;
                        Vector3 temp = tanksToPlace[previousTank].transform.position;
                        temp.x += 1.0f;
                        tanksToPlace[i].transform.position = temp;
                    }
                }
                for (int i = 0; i < infToPlace.Count; i++)
                {
                    if (i == 0)
                    {
                        Vector3 temp = new Vector3(-14.25f, 7.25f, 0f);
                        infToPlace[i].transform.position = temp;
                    }
                    else
                    {
                        int previousInf = i - 1;
                        Vector3 temp = infToPlace[previousInf].transform.position;
                        temp.x += 0.8f;
                        infToPlace[i].transform.position = temp;
                    }
                }
                //end moving units into unit box
                break;
            }
        }
    }
    public void CheckIfAllUnitsHaveBeenPlaced()
    {
        Debug.Log("Running CheckIfAllUnitsHaveBeenPlaced()");

        GameObject[] PlayerUnitHolders = GameObject.FindGameObjectsWithTag("PlayerUnitHolder");
        foreach (GameObject unitHolder in PlayerUnitHolders)
        {
            PlayerUnitHolder unitHolderScript = unitHolder.GetComponent<PlayerUnitHolder>();
            if (unitHolderScript.ownerConnectionId == LocalGamePlayerScript.ConnectionId)
            {
                bool allPlaced = false;
                foreach (Transform unitChild in unitHolder.transform)
                {
                    if (!unitChild.gameObject.GetComponent<UnitScript>().placedDuringUnitPlacement)
                    {
                        allPlaced = false;
                        break;
                    }
                    else
                        allPlaced = true;
                }
                if (allPlaced)
                {
                    endUnitPlacementButton.SetActive(true);
                }
            }
        }
    }
    public void EndUnitPlacementPhase()
    {
        Camera.main.orthographicSize = 7;
        SetGamePhaseText();
        UnitPlacementUI.SetActive(false);
        RemoveCannotPlaceHereOutlines();
        EscMenuManager.instance.GetLocalGamePlayerHand();
        GameObject[] allPlayerHands = GameObject.FindGameObjectsWithTag("PlayerHand");
        foreach (GameObject playerHand in allPlayerHands)
        {
            PlayerHand playerHandScript = playerHand.GetComponent<PlayerHand>();
            if (!playerHandScript.localHandInitialized)
            {
                playerHandScript.InitializePlayerHand();
            }
        }
        if (MouseClickManager.instance.unitsSelected.Count > 0)
            MouseClickManager.instance.ClearUnitSelection();
        StartUnitMovementPhase();
    }
    void LimitUserPlacementByDistanceToBase()
    {
        Vector3 playerBaseLocation = GameObject.FindGameObjectWithTag("PlayerBase").transform.position;
        GameObject allLand = GameObject.FindGameObjectWithTag("LandHolder");
        foreach (Transform landObject in allLand.transform)
        {
            LandScript landScript = landObject.gameObject.GetComponent<LandScript>();
            float disFromBase = Vector3.Distance(landObject.transform.position, playerBaseLocation);
            Debug.Log(landScript.gameObject.name + "'s distance from player base: " + disFromBase.ToString());
            if (disFromBase <= 6.0f)
            {
                landScript.cannotPlaceHere = false;
            }
            else
            {
                landScript.cannotPlaceHere = true;
                landScript.CreateCannotPlaceHereOutline();
            }
        }
    }
    void RemoveCannotPlaceHereOutlines()
    {
        GameObject allLand = GameObject.FindGameObjectWithTag("LandHolder");
        foreach (Transform landObject in allLand.transform)
        {
            LandScript landScript = landObject.gameObject.GetComponent<LandScript>();
            if (landScript.cannotPlaceHere)
            {
                landScript.RemoveCannotPlaceHereOutline();
                landScript.cannotPlaceHere = false;
            }
        }
    }
    public void StartUnitMovementPhase()
    {
        Debug.Log("Starting the Unit Movement Phase.");
        haveUnitsMoved = false;
        if (MouseClickManager.instance.unitsSelected.Count > 0)
            MouseClickManager.instance.ClearUnitSelection();
        ActivateUnitMovementUI();
        SaveUnitStartingLocation();
        LocalGamePlayerScript.UpdateUnitPositions();
        GameObject[] allPlayerHands = GameObject.FindGameObjectsWithTag("PlayerHand");
        foreach (GameObject playerHand in allPlayerHands)
        {
            playerHand.GetComponent<PlayerHand>().InitializePlayerHand();
        }
    }
    void ActivateUnitMovementUI()
    {
        Debug.Log("Activating the Unit Movement UI");
        if (!UnitMovementUI.activeInHierarchy && currentGamePhase == "Unit Movement")
            UnitMovementUI.SetActive(true);
        if (!unitMovementNoUnitsMovedText.gameObject.activeInHierarchy)
            unitMovementNoUnitsMovedText.gameObject.SetActive(true);
        if (!endUnitMovementButton.activeInHierarchy)
            endUnitMovementButton.SetActive(true);
        if (endUnitMovementButton.activeInHierarchy)
            endUnitMovementButton.GetComponent<Image>().color = Color.white;
        if (resetAllMovementButton.activeInHierarchy)
            resetAllMovementButton.SetActive(false);
        //if (hidePlayerHandButton.activeInHierarchy && !PlayerHand.instance.isPlayerViewingTheirHand)
        //hidePlayerHandButton.SetActive(false);
        if (hidePlayerHandButton.activeInHierarchy)
            hidePlayerHandButton.SetActive(false);
        if (!showPlayerHandButton.activeInHierarchy)
            showPlayerHandButton.SetActive(true);
        if (!showPlayerDiscardButton.activeInHierarchy)
            showPlayerDiscardButton.SetActive(true);
        if (!showOpponentCardButton.activeInHierarchy)
            showOpponentCardButton.SetActive(true);
        if (hideOpponentCardButton.activeInHierarchy)
            hideOpponentCardButton.SetActive(false);
        if (LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().isPlayerViewingTheirHand)
        {
            LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().HidePlayerHandOnScreen();
        }
        // When the movement phase begins, save the land occupied by the unit to be used in movement resets
        SaveUnitStartingLocation();
        if (!gamePlayerHandButtonsCreated)
            CreateGamePlayerHandButtons();
        if (opponentHandButtons.Count > 0)
        {
            foreach (GameObject opponentHandButton in opponentHandButtons)
            {
                opponentHandButton.SetActive(false);
            }
        }
        if (isPlayerViewingOpponentHand && playerHandBeingViewed != null)
        {
            playerHandBeingViewed.GetComponent<PlayerHand>().HidePlayerHandOnScreen();
            playerHandBeingViewed = null;
            isPlayerViewingOpponentHand = false;
        }

    }
    public void UnitsHaveMoved()
    {
        if (unitMovementNoUnitsMovedText.gameObject.activeInHierarchy)
            unitMovementNoUnitsMovedText.gameObject.SetActive(false);
        if (endUnitMovementButton.activeInHierarchy)
            endUnitMovementButton.GetComponent<Image>().color = Color.yellow;
        if (!resetAllMovementButton.activeInHierarchy)
            resetAllMovementButton.SetActive(true);
        haveUnitsMoved = true;
    }
    void SaveUnitStartingLocation()
    {
        Debug.Log("Saving unit's starting land location.");
        GameObject[] PlayerUnitHolders = GameObject.FindGameObjectsWithTag("PlayerUnitHolder");
        foreach (GameObject unitHolder in PlayerUnitHolders)
        {
            PlayerUnitHolder unitHolderScript = unitHolder.GetComponent<PlayerUnitHolder>();
            if (unitHolderScript.ownerConnectionId == LocalGamePlayerScript.ConnectionId)
            {
                foreach (Transform unitChild in unitHolder.transform)
                {
                    if (unitChild.GetComponent<NetworkIdentity>().hasAuthority)
                    {
                        UnitScript unitChildScript = unitChild.GetComponent<UnitScript>();
                        if (unitChildScript.currentLandOccupied != null)
                        {
                            unitChildScript.previouslyOccupiedLand = unitChildScript.currentLandOccupied;
                        }
                    }
                }
                break;
            }
        }
    }
    public void ResetAllUnitMovement()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen)
        {
            GameObject unitHolder = LocalGamePlayerScript.myUnitHolder;
            PlayerUnitHolder unitHolderScript = unitHolder.GetComponent<PlayerUnitHolder>();
            if (unitHolderScript.ownerConnectionId == LocalGamePlayerScript.ConnectionId)
            {
                foreach (Transform unitChild in unitHolder.transform)
                {
                    if (unitChild.GetComponent<NetworkIdentity>().hasAuthority)
                    {
                        UnitScript unitChildScript = unitChild.GetComponent<UnitScript>();
                        if (unitChildScript.newPosition != unitChildScript.startingPosition && unitChildScript.previouslyOccupiedLand != null)
                        {
                            if (MouseClickManager.instance.unitsSelected.Count > 0)
                                MouseClickManager.instance.ClearUnitSelection();
                            MouseClickManager.instance.unitsSelected.Add(unitChild.gameObject);

                            unitChildScript.CmdUpdateUnitNewPosition(unitChild.gameObject, unitChildScript.startingPosition, unitChildScript.previouslyOccupiedLand);
                            Debug.Log("Calling MoveAllUnits from GameplayManager for land  on: " + unitChildScript.previouslyOccupiedLand.transform.position);
                            MouseClickManager.instance.MoveAllUnits(unitChildScript.previouslyOccupiedLand);
                            //MouseClickManager.instance.unitsSelected.Clear();
                            unitChildScript.currentlySelected = true;
                            MouseClickManager.instance.ClearUnitSelection();
                        }
                    }
                }
            }

            if (resetAllMovementButton.activeInHierarchy)
            {
                Debug.Log("Deactivating the resetAllMovementButton");
                resetAllMovementButton.SetActive(false);
            }
            if (!unitMovementNoUnitsMovedText.gameObject.activeInHierarchy)
            {
                Debug.Log("Activating the unitMovementNoUnitsMovedText");
                unitMovementNoUnitsMovedText.gameObject.SetActive(true);
            }
            if (endUnitMovementButton.activeInHierarchy)
            {
                Debug.Log("Changing the endUnitMovementButton color to white");
                endUnitMovementButton.GetComponent<Image>().color = Color.white;
            }
            haveUnitsMoved = false;
        }
    }
    public void ShowPlayerHandPressed()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen)
        {
            endUnitMovementButton.SetActive(false);
            resetAllMovementButton.SetActive(false);
            showPlayerHandButton.SetActive(false);
            unitMovementNoUnitsMovedText.gameObject.SetActive(false);
            hidePlayerHandButton.SetActive(true);
            //PlayerHand.instance.ShowPlayerHandOnScreen();
            MouseClickManager.instance.ClearUnitSelection();
            LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().ShowPlayerHandOnScreen();
        }

    }
    public void HidePlayerHandPressed()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen)
        {
            endUnitMovementButton.SetActive(true);
            showPlayerHandButton.SetActive(true);
            if (haveUnitsMoved)
            {
                resetAllMovementButton.SetActive(true);
            }
            else if (!haveUnitsMoved)
            {
                unitMovementNoUnitsMovedText.gameObject.SetActive(true);
            }

            hidePlayerHandButton.SetActive(false);
            //PlayerHand.instance.HidePlayerHandOnScreen();
            LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().HidePlayerHandOnScreen();
        }

    }
    void GetLocalGamePlayer()
    {
        LocalGamePlayer = GameObject.Find("LocalGamePlayer");
        LocalGamePlayerScript = LocalGamePlayer.GetComponent<GamePlayer>();
    }
    void SpawnPlayerUnits()
    {
        Debug.Log("Spawn units for: " + LocalGamePlayerScript.PlayerName);
        LocalGamePlayerScript.SpawnPlayerUnits();
    }
    void SpawnPlayerCards()
    {
        Debug.Log("Spawn cards for: " + LocalGamePlayerScript.PlayerName);
        LocalGamePlayerScript.SpawnPlayerCards();
    }
    void GetPlayerBase()
    {
        Debug.Log("Finding player base for: " + LocalGamePlayerScript.PlayerName);
        LocalGamePlayerScript.GetPlayerBase();
    }
    void GetCurrentGamePhase()
    {
        LocalGamePlayerScript.SetCurrentGamePhase();
    }
    public void ChangePlayerReadyStatus()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen)
            LocalGamePlayerScript.ChangeReadyForNextPhaseStatus();
    }
    public void ChangeGamePhase(string newGamePhase)
    {
        if (currentGamePhase == "Unit Placement" && newGamePhase == "Unit Movement")
        {
            MouseClickManager.instance.canSelectUnitsInThisPhase = true;
            currentGamePhase = newGamePhase;
            EndUnitPlacementPhase();
        }
        if (currentGamePhase == "Unit Movement" && newGamePhase == "Unit Movement")
        {
            MouseClickManager.instance.canSelectUnitsInThisPhase = true;
            currentGamePhase = newGamePhase;
            StartUnitMovementPhase();
        }
        if (currentGamePhase == "Unit Movement" && newGamePhase == "Battle(s) Detected")
        {
            MouseClickManager.instance.canSelectUnitsInThisPhase = false;
            currentGamePhase = newGamePhase;
            StartBattlesDetected();
        }
        if (currentGamePhase == "Battle(s) Detected" && newGamePhase.StartsWith("Choose Cards"))
        {
            MouseClickManager.instance.canSelectUnitsInThisPhase = false;
            currentGamePhase = newGamePhase;
            StartChooseCards();
        }
    }
    public void UpdateReadyButton()
    {
        if (currentGamePhase == "Unit Placement")
        {
            if (LocalGamePlayerScript.ReadyForNextPhase)
            {
                Debug.Log("Local Player is ready to go to next phase.");
                endUnitPlacementButton.GetComponentInChildren<Text>().text = "Unready";
                if (MouseClickManager.instance.unitsSelected.Count > 0)
                    MouseClickManager.instance.ClearUnitSelection();
            }
            else
            {
                Debug.Log("Local Player IS NOT ready to go to next phase.");
                endUnitPlacementButton.GetComponentInChildren<Text>().text = "Done Placing Units";
            }
        }
        if (currentGamePhase == "Unit Movement")
        {
            if (LocalGamePlayerScript.ReadyForNextPhase)
            {
                endUnitMovementButton.GetComponentInChildren<Text>().text = "Unready";
                endUnitMovementButton.GetComponent<Image>().color = Color.white;
                if (resetAllMovementButton.activeInHierarchy)
                    resetAllMovementButton.SetActive(false);
                if (MouseClickManager.instance.unitsSelected.Count > 0)
                    MouseClickManager.instance.ClearUnitSelection();
            }
            else
            {
                endUnitMovementButton.GetComponentInChildren<Text>().text = "End Unit Movement";
                if (haveUnitsMoved)
                {
                    endUnitMovementButton.GetComponent<Image>().color = Color.yellow;
                    resetAllMovementButton.SetActive(true);
                }
            }
        }
        if (currentGamePhase == "Battle(s) Detected")
        {
            if (LocalGamePlayerScript.ReadyForNextPhase)
            {
                Debug.Log("Local Player is ready to go to next phase.");
                startBattlesButton.GetComponentInChildren<Text>().text = "Unready";
                if (MouseClickManager.instance.unitsSelected.Count > 0)
                    MouseClickManager.instance.ClearUnitSelection();
            }
            else
            {
                Debug.Log("Local Player IS NOT ready to go to next phase.");
                startBattlesButton.GetComponentInChildren<Text>().text = "Start Battles";
            }
        }
    }
    public void UpdatePlayerReadyText(string playerName, bool isPlayerReady)
    {
        if (isPlayerReady)
        {
            readyPlayers.Add(playerName);

            if (!PlayerReadyText.gameObject.activeInHierarchy)
            {
                PlayerReadyText.gameObject.SetActive(true);
            }
        }
        else
        {
            readyPlayers.Remove(playerName);
            if (readyPlayers.Count == 0)
            {
                PlayerReadyText.gameObject.SetActive(false);
                PlayerReadyText.text = "";
            }
        }
        if (readyPlayers.Count > 0)
        {
            PlayerReadyText.text = "Players Ready:";
            foreach (string player in readyPlayers)
            {
                PlayerReadyText.text += " " + player;
            }
        }
    }
    void CreateGamePlayerHandButtons()
    {
        GameObject[] allGamePlayers = GameObject.FindGameObjectsWithTag("GamePlayer");
        Vector3 buttonPos = new Vector3(-175, -25, 0);

        foreach (GameObject gamePlayer in allGamePlayers)
        {
            GamePlayer gamePlayerScript = gamePlayer.GetComponent<GamePlayer>();
            GameObject gamePlayerHandButton = Instantiate(opponentHandButtonPrefab);
            gamePlayerHandButton.transform.SetParent(UnitMovementUI.GetComponent<RectTransform>(), false);
            buttonPos.y -= 50f;
            gamePlayerHandButton.GetComponent<RectTransform>().anchoredPosition = buttonPos;
            gamePlayerHandButton.GetComponentInChildren<Text>().text = gamePlayerScript.PlayerName + " Hand";
            OpponentHandButtonScript gamePlayerHandButtonScript = gamePlayerHandButton.GetComponent<OpponentHandButtonScript>();
            gamePlayerHandButtonScript.playerHandConnId = gamePlayerScript.ConnectionId;
            gamePlayerHandButtonScript.playerHandOwnerName = gamePlayerScript.PlayerName;
            gamePlayerHandButtonScript.FindOpponentHand();

            opponentHandButtons.Add(gamePlayerHandButton);


            GameObject gamePlayerDiscardButton = Instantiate(opponentDiscardButtonPrefab);
            gamePlayerDiscardButton.transform.SetParent(UnitMovementUI.GetComponent<RectTransform>(), false);
            buttonPos.y -= 50f;
            gamePlayerDiscardButton.GetComponent<RectTransform>().anchoredPosition = buttonPos;
            gamePlayerDiscardButton.GetComponentInChildren<Text>().text = gamePlayerScript.PlayerName + " Discard";
            opponentHandButtons.Add(gamePlayerDiscardButton);

            gamePlayerHandButton.SetActive(false);
            gamePlayerDiscardButton.SetActive(false);
        }

        gamePlayerHandButtonsCreated = true;
    }
    public void ShowOpponentHandHideUI(GameObject buttonClicked)
    {
        endUnitMovementButton.SetActive(false);
        resetAllMovementButton.SetActive(false);
        showPlayerHandButton.SetActive(false);
        unitMovementNoUnitsMovedText.gameObject.SetActive(false);
        MouseClickManager.instance.ClearUnitSelection();
        foreach (GameObject opponentHandButton in opponentHandButtons)
        {
            if (opponentHandButton != buttonClicked)
                opponentHandButton.SetActive(false);
        }
        hideOpponentCardButton.SetActive(false);
    }
    public void HideOpponentHandRestoreUI()
    {

        endUnitMovementButton.SetActive(true);
        if (!LocalGamePlayerScript.ReadyForNextPhase)
        {
            if (haveUnitsMoved)
            {
                resetAllMovementButton.SetActive(true);
            }
            else if (!haveUnitsMoved)
            {
                unitMovementNoUnitsMovedText.gameObject.SetActive(true);
            }
        }

        foreach (GameObject opponentHandButton in opponentHandButtons)
        {
            opponentHandButton.SetActive(true);
        }
        hideOpponentCardButton.SetActive(true);
    }
    public void ShowOpponentCards()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen && !LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().isPlayerViewingTheirHand)
        {
            showPlayerHandButton.SetActive(false);
            showPlayerDiscardButton.SetActive(false);
            showOpponentCardButton.SetActive(false);
            hideOpponentCardButton.SetActive(true);

            foreach (GameObject opponentHandButton in opponentHandButtons)
            {
                opponentHandButton.SetActive(true);
            }
        }
    }
    public void HideOpponentCards()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen && !LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().isPlayerViewingTheirHand)
        {
            showPlayerHandButton.SetActive(true);
            showPlayerDiscardButton.SetActive(true);
            showOpponentCardButton.SetActive(true);
            hideOpponentCardButton.SetActive(false);

            foreach (GameObject opponentHandButton in opponentHandButtons)
            {
                opponentHandButton.SetActive(false);
            }
        }
    }
    void StartBattlesDetected()
    {
        Debug.Log("Starting StartBattlesDetected");
        SetGamePhaseText();
        haveUnitsMoved = false;
        if (MouseClickManager.instance.unitsSelected.Count > 0)
            MouseClickManager.instance.ClearUnitSelection();
        ActivateBattlesDetectedUI();
        SaveUnitStartingLocation();
        LocalGamePlayerScript.UpdateUnitPositions();
    }
    void ActivateBattlesDetectedUI()
    {
        if (UnitPlacementUI.activeInHierarchy)
            UnitPlacementUI.SetActive(false);
        if (UnitMovementUI.activeInHierarchy)
            UnitMovementUI.SetActive(false);
        if (!BattlesDetectedPanel.activeInHierarchy)
            BattlesDetectedPanel.SetActive(true);

        // Move buttons to the BattlesDetectedPanel

        hidePlayerHandButton.transform.SetParent(BattlesDetectedPanel.GetComponent<RectTransform>(), false);
        showPlayerHandButton.transform.SetParent(BattlesDetectedPanel.GetComponent<RectTransform>(), false);
        showPlayerDiscardButton.transform.SetParent(BattlesDetectedPanel.GetComponent<RectTransform>(), false);
        showOpponentCardButton.transform.SetParent(BattlesDetectedPanel.GetComponent<RectTransform>(), false);
        hideOpponentCardButton.transform.SetParent(BattlesDetectedPanel.GetComponent<RectTransform>(), false);

        if (hidePlayerHandButton.activeInHierarchy)
            hidePlayerHandButton.SetActive(false);
        if (!showPlayerHandButton.activeInHierarchy)
            showPlayerHandButton.SetActive(true);
        if (!showPlayerDiscardButton.activeInHierarchy)
            showPlayerDiscardButton.SetActive(true);
        if (!showOpponentCardButton.activeInHierarchy)
            showOpponentCardButton.SetActive(true);
        if (hideOpponentCardButton.activeInHierarchy)
            hideOpponentCardButton.SetActive(false);
        if (!startBattlesButton.activeInHierarchy)
            startBattlesButton.SetActive(true);

        if (LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().isPlayerViewingTheirHand)
        {
            LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().HidePlayerHandOnScreen();
        }

        if (opponentHandButtons.Count > 0)
        {
            foreach (GameObject opponentHandButton in opponentHandButtons)
            {
                opponentHandButton.transform.SetParent(BattlesDetectedPanel.GetComponent<RectTransform>(), false);
                opponentHandButton.SetActive(false);
            }
        }
        if (isPlayerViewingOpponentHand && playerHandBeingViewed != null)
        {
            playerHandBeingViewed.GetComponent<PlayerHand>().HidePlayerHandOnScreen();
            playerHandBeingViewed = null;
            isPlayerViewingOpponentHand = false;
        }
    }
    public void HighlightBattleSites()
    {
        Debug.Log("HighlightBattleSites starting. Total battle sites: " + battleSiteNetIds.Count);
        if (battleSiteNetIds.Count > 0 && !haveBattleSitesBeenDone)
        {
            foreach (KeyValuePair<int, uint> battleSiteId in battleSiteNetIds)
            {
                LandScript battleSiteIdScript = NetworkIdentity.spawned[battleSiteId.Value].gameObject.GetComponent<LandScript>();
                battleSiteIdScript.HighlightBattleSite();
                battleSiteIdScript.MoveUnitsForBattleSite();
                battleSiteIdScript.SpawnBattleNumberText(battleSiteId.Key);
            }
            haveBattleSitesBeenDone = true;
        }
    }
    public void CheckIfAllUpdatedUnitPositionsForBattleSites()
    {
        Debug.Log("Executing CheckIfAllUpdatedUnitPositionsForBattleSites");
        bool haveAllUnitsUpdated = false;
        if (!LocalGamePlayerScript.updatedUnitPositionsForBattleSites)
        {
            Debug.Log("CheckIfAllUpdatedUnitPositionsForBattleSites: LocalGamePlayer not ready");
            return;
        }
        else
            haveAllUnitsUpdated = LocalGamePlayerScript.updatedUnitPositionsForBattleSites;

        GameObject[] allGamePlayers = GameObject.FindGameObjectsWithTag("GamePlayer");
        foreach (GameObject gamePlayer in allGamePlayers)
        {
            GamePlayer gamePlayerScript = gamePlayer.GetComponent<GamePlayer>();
            if (!gamePlayerScript.updatedUnitPositionsForBattleSites)
            {
                haveAllUnitsUpdated = false;
                Debug.Log("CheckIfAllUpdatedUnitPositionsForBattleSites: " + gamePlayerScript.PlayerName + " not ready");
                break;
            }
            else
            {
                haveAllUnitsUpdated = gamePlayerScript.updatedUnitPositionsForBattleSites;
            }
        }
        if (haveAllUnitsUpdated)
        {
            Debug.Log("CheckIfAllUpdatedUnitPositionsForBattleSites: all gameplayers are ready!");
            HighlightBattleSites();
        }
    }
    public void StartChooseCards()
    {
        Debug.Log("Starting StartBattles");
        SetGamePhaseText();
        ActivateChooseCards();
    }
    void ActivateChooseCards()
    {
        if (UnitMovementUI.activeInHierarchy)
            UnitMovementUI.SetActive(false);
        if (BattlesDetectedPanel.activeInHierarchy)
            BattlesDetectedPanel.SetActive(false);
        if (!ChooseCardsPanel.activeInHierarchy)
            ChooseCardsPanel.SetActive(true);

        // Move buttons to the UnitMovementUI
        hidePlayerHandButton.transform.SetParent(ChooseCardsPanel.GetComponent<RectTransform>(), false);
        showPlayerHandButton.transform.SetParent(ChooseCardsPanel.GetComponent<RectTransform>(), false);
        showPlayerDiscardButton.transform.SetParent(ChooseCardsPanel.GetComponent<RectTransform>(), false);
        showOpponentCardButton.transform.SetParent(ChooseCardsPanel.GetComponent<RectTransform>(), false);
        hideOpponentCardButton.transform.SetParent(ChooseCardsPanel.GetComponent<RectTransform>(), false);

        if (hidePlayerHandButton.activeInHierarchy)
            hidePlayerHandButton.SetActive(false);
        if (!showPlayerHandButton.activeInHierarchy)
            showPlayerHandButton.SetActive(true);
        if (!showPlayerDiscardButton.activeInHierarchy)
            showPlayerDiscardButton.SetActive(true);
        if (!showOpponentCardButton.activeInHierarchy)
            showOpponentCardButton.SetActive(true);
        if (hideOpponentCardButton.activeInHierarchy)
            hideOpponentCardButton.SetActive(false);
        if (!startBattlesButton.activeInHierarchy)
            startBattlesButton.SetActive(true);

        if (LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().isPlayerViewingTheirHand)
        {
            LocalGamePlayerScript.myPlayerCardHand.GetComponent<PlayerHand>().HidePlayerHandOnScreen();
        }

        if (opponentHandButtons.Count > 0)
        {
            foreach (GameObject opponentHandButton in opponentHandButtons)
            {
                opponentHandButton.transform.SetParent(ChooseCardsPanel.GetComponent<RectTransform>(), false);
                opponentHandButton.SetActive(false);
            }
        }
        if (isPlayerViewingOpponentHand && playerHandBeingViewed != null)
        {
            playerHandBeingViewed.GetComponent<PlayerHand>().HidePlayerHandOnScreen();
            playerHandBeingViewed = null;
            isPlayerViewingOpponentHand = false;
        }
    }
}

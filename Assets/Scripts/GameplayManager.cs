using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
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
    public bool haveUnitsMoved = false;
    [SerializeField]
    private GameObject showPlayerHandButton, hidePlayerHandButton;

    [Header("GamePlayers")]
    [SerializeField] private GameObject LocalGamePlayer;
    [SerializeField] private GamePlayer LocalGamePlayerScript;
    // Start is called before the first frame update
    void Start()
    {
        MakeInstance();
        infToPlace = new List<GameObject>();
        tanksToPlace = new List<GameObject>();

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
    }
    void ActivateUnitPlacementUI()
    {
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
        LocalGamePlayerScript.ChangeReadyForNextPhaseStatus();
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
        if (endUnitMovementButton.activeInHierarchy)
            endUnitMovementButton.GetComponent<Image>().color = Color.white;
        if (resetAllMovementButton.activeInHierarchy)
            resetAllMovementButton.SetActive(false);
        if (hidePlayerHandButton.activeInHierarchy)
            hidePlayerHandButton.SetActive(false);
        // When the movement phase begins, save the land occupied by the unit to be used in movement resets
        SaveUnitStartingLocation();

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
        GameObject unitHolder = GameObject.FindGameObjectWithTag("PlayerUnitHolder");
        foreach (Transform unitChild in unitHolder.transform)
        {
            UnitScript unitScript = unitChild.transform.gameObject.GetComponent<UnitScript>();
            if (unitScript.currentLandOccupied != null)
            {
                unitScript.previouslyOccupiedLand = unitScript.currentLandOccupied;
            }
        }
    }
    public void ResetAllUnitMovement()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen)
        {
            if (resetAllMovementButton.activeInHierarchy)
                resetAllMovementButton.SetActive(false);
            if (!unitMovementNoUnitsMovedText.gameObject.activeInHierarchy)
                unitMovementNoUnitsMovedText.gameObject.SetActive(true);
            if (endUnitMovementButton.activeInHierarchy)
                endUnitMovementButton.GetComponent<Image>().color = Color.white;

            GameObject unitHolder = GameObject.FindGameObjectWithTag("PlayerUnitHolder");
            foreach (Transform unitChild in unitHolder.transform)
            {
                UnitScript unitScript = unitChild.transform.gameObject.GetComponent<UnitScript>();
                if (unitScript.previouslyOccupiedLand != null)
                {
                    Debug.Log("Unit was moved. Resetting unit movement.");
                    if (MouseClickManager.instance.unitsSelected.Count > 0)
                        MouseClickManager.instance.ClearUnitSelection();
                       
                    MouseClickManager.instance.unitsSelected.Add(unitChild.gameObject);
                    MouseClickManager.instance.MoveAllUnits(unitScript.previouslyOccupiedLand);
                    MouseClickManager.instance.unitsSelected.Clear();
                }
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
        currentGamePhase = newGamePhase;
        if (newGamePhase == "Unit Movement")
            EndUnitPlacementPhase();
    }
    public void UpdateReadyButton()
    {
        if (currentGamePhase == "Unit Placement")
        {
            if (LocalGamePlayerScript.ReadyForNextPhase)
            {
                Debug.Log("Local Player is ready to go to next phase.");
                endUnitPlacementButton.GetComponentInChildren<Text>().text = "Unready";
            }
            else
            {
                Debug.Log("Local Player IS NOT ready to go to next phase.");
                endUnitPlacementButton.GetComponentInChildren<Text>().text = "Done Placing Units";
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

}

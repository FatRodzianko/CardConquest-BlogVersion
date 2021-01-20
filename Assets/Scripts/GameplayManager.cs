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
    // Start is called before the first frame update
    void Awake()
    {
        MakeInstance();
        infToPlace = new List<GameObject>();
        tanksToPlace = new List<GameObject>();

        currentGamePhase = "Unit Placement";
        SetGamePhaseText();
        ActivateUnitPlacementUI();
        PutUnitsInUnitBox();
        LimitUserPlacementByDistanceToBase();
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
    void SetGamePhaseText()
    {
        GamePhaseText.text = currentGamePhase;
    }
    void ActivateUnitPlacementUI()
    {
        Camera.main.orthographicSize = 8f;
        Camera.main.backgroundColor = Color.gray;
        if (!UnitPlacementUI.activeInHierarchy && currentGamePhase == "Unit Placement")
            UnitPlacementUI.SetActive(true);
        if (endUnitPlacementButton.activeInHierarchy)
            endUnitPlacementButton.SetActive(false);
    }
    void PutUnitsInUnitBox()
    {
        GameObject unitHolder = GameObject.FindGameObjectWithTag("PlayerUnitHolder");

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
    }
    public void CheckIfAllUnitsHaveBeenPlaced()
    {
        GameObject unitHolder = GameObject.FindGameObjectWithTag("PlayerUnitHolder");
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
    public void EndUnitPlacementPhase()
    {
        if (!EscMenuManager.instance.IsMainMenuOpen)
        {
            currentGamePhase = "Unit Movement";
            Camera.main.orthographicSize = 7;
            SetGamePhaseText();
            UnitPlacementUI.SetActive(false);
            RemoveCannotPlaceHereOutlines();
        }

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

}

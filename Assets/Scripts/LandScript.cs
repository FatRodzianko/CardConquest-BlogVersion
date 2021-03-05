using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class LandScript : NetworkBehaviour
{
    public List<GameObject> infantryOnLand;
    public List<GameObject> tanksOnLand;

    public bool multipleUnitsOnLand = false;

    public GameObject infTextHolder;
    public GameObject tankTextHolder;

    private GameObject infText;
    private GameObject tankText;

    public GameObject landOutline;
    private GameObject landOutlineObject;

    public GameObject cannotPlaceHereOutline;
    private GameObject cannotPlaceHereOutlineObject;
    public bool cannotPlaceHere = false;

    [SyncVar(hook = nameof(HandlePlayerCanPlaceHereUpdate))] public int PlayerCanPlaceHere;

    // Start is called before the first frame update
    void Start()
    {
        infantryOnLand = new List<GameObject>();
        tanksOnLand = new List<GameObject>();
        if (PlayerCanPlaceHere == 0)
        {
            cannotPlaceHere = true;
            CreateCannotPlaceHereOutline();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CheckIfMultipleUnitsOnLand()
    {
        if (infantryOnLand.Count < 2 && tanksOnLand.Count < 2)
        {
            multipleUnitsOnLand = false;
        }
        else 
        {
            multipleUnitsOnLand = true;
        }
    }
    public void MultipleUnitsUIText(string unitType)
    {

        if (unitType == "infantry")
        {
            if (infText == null)
            {
                Debug.Log("Creating text box for multiple infantry");
                infText = Instantiate(infTextHolder, gameObject.transform);
                infText.transform.position = transform.position;
                infText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("x" + infantryOnLand.Count.ToString());
            }
            else
            {
                infText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("x" + infantryOnLand.Count.ToString());
            }
        }
        else if (unitType == "tank")
        {
            if (tankText == null)
            {
                Debug.Log("Creating text box for multiple tanks");
                tankText = Instantiate(tankTextHolder, gameObject.transform);
                tankText.transform.position = transform.position;
                tankText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("x" + tanksOnLand.Count.ToString());
            }
            else
            {
                tankText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("x" + tanksOnLand.Count.ToString());
            }
        }
        multipleUnitsOnLand = true;
    }
    public void UpdateUnitText()
    {
        if (infText != null)
        {
            Debug.Log("Updating inf text. Current number of infantry " + infantryOnLand.Count.ToString());
            if (infantryOnLand.Count > 1)
                infText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("x" + infantryOnLand.Count.ToString());
            else
            {
                Destroy(infText);
                CollapseUnits();
            }

        }
        if (tankText != null)
        {
            Debug.Log("Updating tank text. Current number of tanks: " + tanksOnLand.Count.ToString());
            if (tanksOnLand.Count > 1)
                tankText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("x" + tanksOnLand.Count.ToString());
            else
            {
                Destroy(tankText);
                CollapseUnits();
            }
        }

        CheckIfMultipleUnitsOnLand();
    }
    public void HighlightLandArea()
    {
        if (landOutlineObject == null)
        {
            Debug.Log("Creating land outline");
            landOutlineObject = Instantiate(landOutline, transform.position, Quaternion.identity);
            landOutlineObject.transform.SetParent(gameObject.transform);

            if (infantryOnLand.Count > 1 || tanksOnLand.Count > 1)
                ExpandUnits();
        }
    }
    public void RemoveHighlightLandArea()
    {
        if (landOutlineObject != null)
        {
            Destroy(landOutlineObject);
            if (infantryOnLand.Count > 1 || tanksOnLand.Count > 1)
                CollapseUnits();
        }
    }
    public void CheckForSelectedUnits()
    {
        bool anySelected = false;
        if (tanksOnLand.Count > 0)
        {
            foreach (GameObject unit in tanksOnLand)
            {
                UnitScript unitScript = unit.GetComponent<UnitScript>();
                if (unitScript.currentlySelected)
                {
                    anySelected = true;
                    break;
                }
            }
        }
        if (infantryOnLand.Count > 0)
        {
            foreach (GameObject unit in infantryOnLand)
            {
                UnitScript unitScript = unit.GetComponent<UnitScript>();
                if (unitScript.currentlySelected)
                {
                    anySelected = true;
                    break;
                }
            }
        }
        if (!anySelected)
        {
            RemoveHighlightLandArea();
        }

    }
    void ExpandUnits()
    {
        Vector3 temp;
        if (infantryOnLand.Count > 1)
        {
            for (int i = 1; i < infantryOnLand.Count; i++)
            {
                if (i == 1)
                {
                    temp = infantryOnLand[i].transform.position;
                    temp.x += 0.65f;
                    infantryOnLand[i].transform.position = temp;
                }
                else if (i == 2)
                {
                    temp = infantryOnLand[i].transform.position;
                    temp.x -= 0.6f;
                    infantryOnLand[i].transform.position = temp;
                }
                else if (i == 3)
                {
                    temp = infantryOnLand[i].transform.position;
                    temp.y -= 0.8f;
                    infantryOnLand[i].transform.position = temp;
                }
                else if (i == 4)
                {
                    temp = infantryOnLand[i].transform.position;
                    temp.y += 0.8f;
                    infantryOnLand[i].transform.position = temp;
                }
            }
        }
        if (tanksOnLand.Count > 1)
        {
            for (int i = 1; i < tanksOnLand.Count; i++)
            {
                if (i == 1)
                {
                    temp = tanksOnLand[i].transform.position;
                    temp.x += 0.95f;
                    tanksOnLand[i].transform.position = temp;
                }
                else if (i == 2)
                {
                    temp = tanksOnLand[i].transform.position;
                    temp.x -= 0.95f;
                    tanksOnLand[i].transform.position = temp;
                }
                else if (i == 3)
                {
                    temp = tanksOnLand[i].transform.position;
                    temp.y += 0.6f;
                    tanksOnLand[i].transform.position = temp;
                }
            }
        }
        HideUnitText();
    }
    void CollapseUnits()
    {
        Vector3 temp;
        // move units back?
        if (infantryOnLand.Count > 0)
        {

            foreach (GameObject inf in infantryOnLand)
            {
                temp = transform.position;
                temp.y -= 0.5f;
                inf.transform.position = temp;
            }
        }
        if (tanksOnLand.Count > 0)
        {
            foreach (GameObject tank in tanksOnLand)
            {
                temp = transform.position;
                temp.y += 0.5f;
                tank.transform.position = temp;
            }
        }

        UnHideUnitText();
    }
    public void HideUnitText()
    {
        if (infText != null)
        {
            infText.SetActive(false);
        }
        if (tankText != null)
        {
            tankText.SetActive(false);
        }
    }
    public void UnHideUnitText()
    {
        if (infText != null)
        {
            infText.SetActive(true);
        }
        if (tankText != null)
        {
            tankText.SetActive(true);
        }
    }
    public void CreateCannotPlaceHereOutline()
    {
        if (cannotPlaceHereOutlineObject == null && cannotPlaceHere)
        {
            cannotPlaceHereOutlineObject = Instantiate(cannotPlaceHereOutline, transform.position, Quaternion.identity);
            cannotPlaceHereOutlineObject.transform.parent = this.gameObject.transform;
        }
    }
    public void RemoveCannotPlaceHereOutline()
    {
        if (cannotPlaceHereOutlineObject != null)
        {
            Destroy(cannotPlaceHereOutlineObject);
        }
    }
    public void HandlePlayerCanPlaceHereUpdate(int oldValue, int newValue)
    {
        Debug.Log("PlayerCanPlaceHere updated to: " + PlayerCanPlaceHere);
        GameObject LocalGamePlayer = GameObject.Find("LocalGamePlayer");
        GamePlayer LocalGamePlayerScript = LocalGamePlayer.GetComponent<GamePlayer>();

        if (PlayerCanPlaceHere == LocalGamePlayerScript.playerNumber)
        {
            cannotPlaceHere = false;
            RemoveCannotPlaceHereOutline();
        }
        else
        {
            cannotPlaceHere = true;
            CreateCannotPlaceHereOutline();
        }
    }
}

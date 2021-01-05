using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    [SerializeField]
    public GameObject outline;
    public bool currentlySelected = false;

    public GameObject currentLandOccupied;

    [SerializeField]
    private LayerMask landLayer;

    // Start is called before the first frame update
    void Start()
    {
		outline = Instantiate(outline, transform.position, Quaternion.identity);
		outline.transform.SetParent(gameObject.transform);
		ClickedOn();
        GetStartingLandLocation();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
	public void ClickedOn()
	{
		if (currentlySelected)
		{
			outline.SetActive(true);
			Debug.Log("Currently selected set to true. Activating outline.");
			outline.transform.position = transform.position;

		}
		else if (!currentlySelected)
		{
			if (outline.activeInHierarchy)
			{
				Debug.Log("Currently selected set to false. Deactivating outline");
				outline.SetActive(false);
			}
		}
	}

    public void MoveUnit(GameObject LandToMoveTo)
    {
        Vector3 temp = LandToMoveTo.transform.position;
        if (gameObject.tag == "tank")
        {
            temp.y += 0.5f;
            gameObject.transform.position = temp;
        }
        else if (gameObject.tag == "infantry")
        {
            temp.y -= 0.5f;
            gameObject.transform.position = temp;
        }
        UpdateUnitLandObject(LandToMoveTo);
    }

    public void UpdateUnitLandObject(GameObject LandToMoveTo)
    {
        LandScript landScript = LandToMoveTo.GetComponent<LandScript>();

        if (currentLandOccupied != LandToMoveTo)
        {
            //Current land tile should only be null when the game is first started and the unit hasn't been "assigned" a land tile yet
            if (currentLandOccupied == null)
            {
                currentLandOccupied = LandToMoveTo;
            }
            Debug.Log("Unit moved to new land");
            if (currentLandOccupied != null)
            {
                if (gameObject.tag == "infantry")
                {
                    //Remove unit from previous land tile
                    Debug.Log("Removed infantry from previous land object at: " + currentLandOccupied.transform.position.x.ToString() + "," + currentLandOccupied.transform.position.y.ToString());
                    currentLandOccupied.GetComponent<LandScript>().infantryOnLand.Remove(gameObject);
                    currentLandOccupied.GetComponent<LandScript>().UpdateUnitText();

                    //Add Unit to new land tile
                    Debug.Log("Added infantry unit to land object at: " + LandToMoveTo.transform.position.x.ToString() + "," + LandToMoveTo.transform.position.y.ToString());
                    landScript.infantryOnLand.Add(gameObject);
                    if (landScript.infantryOnLand.Count > 1)
                    {
                        landScript.MultipleUnitsUIText("infantry");
                        Debug.Log("More than 1 infantry on land");
                    }

                }
                else if (gameObject.tag == "tank")
                {
                    //Remove unit from previous land tile
                    Debug.Log("Removed tank from previous land object at: " + currentLandOccupied.transform.position.x.ToString() + "," + currentLandOccupied.transform.position.y.ToString());
                    currentLandOccupied.GetComponent<LandScript>().tanksOnLand.Remove(gameObject);
                    currentLandOccupied.GetComponent<LandScript>().UpdateUnitText();

                    //Add unit to new land tile
                    Debug.Log("Added tank unit to land object at: " + LandToMoveTo.transform.position.x.ToString() + "," + LandToMoveTo.transform.position.y.ToString());
                    landScript.tanksOnLand.Add(gameObject);
                    if (landScript.tanksOnLand.Count > 1)
                    {
                        landScript.MultipleUnitsUIText("tank");
                        Debug.Log("More than 1 tank on land");
                    }
                }
                // Remove the land highlight when a unit moves
                currentLandOccupied.GetComponent<LandScript>().RemoveHighlightLandArea();
            }
            float disFromCurrentLocation = Vector3.Distance(LandToMoveTo.transform.position, currentLandOccupied.transform.position);
            Debug.Log("Unit moved distance of: " + disFromCurrentLocation.ToString("0.00"));

            currentLandOccupied = LandToMoveTo;
        }

    }
    public void CheckLandForRemainingSelectedUnits()
    {
        if (currentLandOccupied != null)
        {
            LandScript landScript = currentLandOccupied.GetComponent<LandScript>();
            landScript.CheckForSelectedUnits();
        }
    }
    void GetStartingLandLocation()
    {
        RaycastHit2D landBelow = Physics2D.Raycast(transform.position, Vector2.zero, Mathf.Infinity, landLayer);
        if (landBelow.collider != null)
        {
            UpdateUnitLandObject(landBelow.collider.gameObject);
        }
    }
    public bool CanAllSelectedUnitsMove(GameObject landUserClicked)
    {
        bool canMove = false;
        LandScript landScript = landUserClicked.GetComponent<LandScript>();
        int totalUnits = MouseClickManager.instance.unitsSelected.Count + landScript.tanksOnLand.Count + landScript.infantryOnLand.Count;
        if (totalUnits > 5)
        {
            Debug.Log("Too many units to move.");
            canMove = false;
            return canMove;
        }
        foreach (GameObject unit in MouseClickManager.instance.unitsSelected)
        {
            UnitScript unitScript = unit.GetComponent<UnitScript>();
            float disFromCurrentLocation = Vector3.Distance(landUserClicked.transform.position, unitScript.currentLandOccupied.transform.position);
            if (disFromCurrentLocation < 3.01f)
            {
                Debug.Log("SUCCESS: Unit movement distance of: " + disFromCurrentLocation.ToString("0.00"));
                canMove = true;
            }
            else
            {
                Debug.Log("FAILURE: Unit movement distance of: " + disFromCurrentLocation.ToString("0.00"));
                canMove = false;
                return canMove;
            }
        }
        return canMove;
    }
}

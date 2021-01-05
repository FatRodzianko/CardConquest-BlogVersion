using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickManager : MonoBehaviour
{
    [SerializeField]
    private LayerMask unitLayer;
    public List<GameObject> unitsSelected;

    [SerializeField]
    private LayerMask landLayer;

    public static MouseClickManager instance;
    // Start is called before the first frame update
    void Start()
    {
        MakeInstance();
        unitsSelected = new List<GameObject>();
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EscMenuManager.instance.IsMainMenuOpen == false)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition2d = new Vector2(mousePosition.x, mousePosition.y);
            RaycastHit2D rayHitUnit = Physics2D.Raycast(mousePosition2d, Vector2.zero, Mathf.Infinity, unitLayer);
            RaycastHit2D rayHitLand = Physics2D.Raycast(mousePosition2d, Vector2.zero, Mathf.Infinity, landLayer);

            if (rayHitUnit.collider != null)
            {
                UnitScript unitScript = rayHitUnit.collider.GetComponent<UnitScript>();
                if (!unitScript.currentlySelected)
                {
                    Debug.Log("Selecting a new unit.");
                    unitsSelected.Add(rayHitUnit.collider.gameObject);
                    unitScript.currentlySelected = !unitScript.currentlySelected;
                    unitScript.ClickedOn();

                    if (unitScript.currentLandOccupied != null)
                    {
                        LandScript landScript = unitScript.currentLandOccupied.GetComponent<LandScript>();
                        landScript.HighlightLandArea();
                    }
                }
                else 
                {
                    unitsSelected.Remove(rayHitUnit.collider.gameObject);
                    Debug.Log("Deselecting the unit unit.");
                    unitScript.currentlySelected = !unitScript.currentlySelected;
                    unitScript.ClickedOn();
                    unitScript.CheckLandForRemainingSelectedUnits();
                    if (unitScript.currentLandOccupied != null)
                    {
                        LandScript landScript = unitScript.currentLandOccupied.GetComponent<LandScript>();
                        if (landScript.multipleUnitsOnLand)
                        {
                            Debug.Log("UN-Selected unit on land with multiple units.");
                        }

                    }
                }
            }
            else if (rayHitLand.collider != null && unitsSelected.Count > 0 && rayHitUnit.collider == null) // if the player has selected units previously and clicks on a land, check if the units can be moved)
            {
                if (unitsSelected[0].GetComponent<UnitScript>().CanAllSelectedUnitsMove(rayHitLand.collider.gameObject))
                {
                    MoveAllUnits(rayHitLand.collider.gameObject);
                }                
                ClearUnitSelection();
            }
        }
        if (Input.GetMouseButtonDown(1) && EscMenuManager.instance.IsMainMenuOpen == false)
        {
            Debug.Log("Right clicked.");
            ClearUnitSelection();
        }
    }
    void ClearUnitSelection()
    {
        if (unitsSelected.Count > 0)
        {
            foreach (GameObject unit in unitsSelected)
            {
                UnitScript unitScript = unit.GetComponent<UnitScript>();
                unitScript.currentlySelected = !unitScript.currentlySelected;
                unitScript.ClickedOn();
                unitScript.CheckLandForRemainingSelectedUnits();
            }
            unitsSelected.Clear();
        }
    }

    void MoveAllUnits(GameObject landClicked)
    {        
        if (unitsSelected.Count > 0)
        {
            Debug.Log("Moving selected units.");
            foreach (GameObject unit in unitsSelected)
            {
                UnitScript unitScript = unit.GetComponent<UnitScript>();
                unitScript.MoveUnit(landClicked);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickManager : MonoBehaviour
{
    [SerializeField]
    private LayerMask unitLayer;
    public List<GameObject> unitsSelected;

    // Start is called before the first frame update
    void Start()
    {
        unitsSelected = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition2d = new Vector2(mousePosition.x, mousePosition.y);
            RaycastHit2D rayHitUnit = Physics2D.Raycast(mousePosition2d, Vector2.zero, Mathf.Infinity, unitLayer);

            if (rayHitUnit.collider != null)
            {
                UnitScript unitScript = rayHitUnit.collider.GetComponent<UnitScript>();
                if (!unitScript.currentlySelected)
                {
                    Debug.Log("Selecting a new unit.");
                    unitsSelected.Add(rayHitUnit.collider.gameObject);
                    unitScript.currentlySelected = !unitScript.currentlySelected;
                    unitScript.ClickedOn();
                }
                else 
                {
                    unitsSelected.Remove(rayHitUnit.collider.gameObject);
                    Debug.Log("Deselecting the unit unit.");
                    unitScript.currentlySelected = !unitScript.currentlySelected;
                    unitScript.ClickedOn();
                }
            }
            else
            {
                
            }
        }
        if (Input.GetMouseButtonDown(1))
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
            }
            unitsSelected.Clear();
        }
    }
}

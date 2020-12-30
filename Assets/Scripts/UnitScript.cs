using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    [SerializeField]
    public GameObject outline;
    public bool currentlySelected = false;
    // Start is called before the first frame update
    void Start()
    {
		outline = Instantiate(outline, transform.position, Quaternion.identity);
		outline.transform.SetParent(gameObject.transform);
		ClickedOn();
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
    }
}

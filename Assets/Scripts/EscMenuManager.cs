using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscMenuManager : MonoBehaviour
{
    public static EscMenuManager instance;
    public bool IsMainMenuOpen = false;
    [SerializeField]
    private GameObject escMenuPanel;

    // Start is called before the first frame update
    void Start()
    {
        MakeInstance();
        if (escMenuPanel.activeInHierarchy)
        {
            escMenuPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Opening the ESC menu");

            IsMainMenuOpen = !IsMainMenuOpen;
            escMenuPanel.SetActive(IsMainMenuOpen);
        }
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
    public void HideEscMenu()
    {
        Debug.Log("hiding the main menu");
        if (IsMainMenuOpen == true)
        {
            IsMainMenuOpen = !IsMainMenuOpen;
            escMenuPanel.SetActive(IsMainMenuOpen);
            Debug.Log("hiding the main menu");
        }
    }
    public void ExitGame()
    {
        Application.Quit();
    }

}

using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject mainMenu;   // The Canvas or parent object for the main menu
    public GameObject gameplay;   // The parent object containing tanks, walls, bullets, etc.

    [Header("Optional")]
    public bool enableGameplayAtStart = false; // If true, gameplay starts immediately

    void Start()
    {
        // Ensure menu/gameplay states are correct at start
        if (enableGameplayAtStart)
        {
            mainMenu.SetActive(false);
            gameplay.SetActive(true);
        }
        else
        {
            mainMenu.SetActive(true);
            gameplay.SetActive(false);
        }
    }

    /// <summary>
    /// Called by the Start Button
    /// </summary>
    public void StartGame()
    {
        mainMenu.SetActive(false);  // hide the menu
        gameplay.SetActive(true);   // enable all gameplay objects
    }

    /// <summary>
    /// Called by an optional Quit Button
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();  // works in build, not in editor
        Debug.Log("Quit button pressed (does nothing in editor)");
    }
}

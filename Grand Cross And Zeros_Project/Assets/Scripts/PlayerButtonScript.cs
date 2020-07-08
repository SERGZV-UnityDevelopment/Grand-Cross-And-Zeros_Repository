using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;


public class PlayerButtonScript : MonoBehaviour
{
    public byte NumberOfPlayer;                     // The number of the player for whom this drop-down list was created
    public GameManager GM;                          // Variable storing script GameManager
    public UIScript UIS;                            // Variable storing script UIScript
    public GameObject HiddenMenu;                   // This variable is for GameObject HiddenMenu
    public List<int> TriedNames = new List<int>();  // Names already suggested to the player in this cycle
    public GameObject ComputerDifficulty;           // Enemy Difficulty Dropdown
    bool TheWindowWasOffset = false;                // This variable answers whether the window was shifted upward.
    public Sprite[] PlayerAndComputer;              // Here are the player and computer icons


    public void _hiddenMenu()                               // This method opens and closes a hidden menu.
    {
        float BottonHeightPos = gameObject.GetComponent<RectTransform>().anchoredPosition.y;// The position of this button in height
        float BottonHeight = gameObject.GetComponent<RectTransform>().sizeDelta.y;          // Height of this button
        float HiddenMenuHeight = HiddenMenu.GetComponent<RectTransform>().sizeDelta.y;      // Hidden Menu Height
        float AvailableSpace = Screen.height + BottonHeightPos;                             // Available space below for expanding the hidden menu
        GameObject CaughtObject = PointerRaycast(Input.mousePosition);                      // Run the ray and save to the CaughtObject variable

        if (CaughtObject.tag == "PlayerButton")                                             // If the tag of the object we clicked on is "PlayerButton"
            UIS.LastСlickOnThePSB = Input.mousePosition.y;                                  // We save the "y" coordinates of the last click on the player settings button
        
        if (!HiddenMenu.activeSelf)                         // If the Game Object "hHidden Menu" is not active
        {
            if (UIS.LastСlickOnThePSB < Screen.height / 2)  // If the button is at the top of the screen
            {
                // Calculate a new position for the hidden menu
                Vector2 ChangedMenuPosition = new Vector2(HiddenMenu.GetComponent<RectTransform>().anchoredPosition.x,
                    HiddenMenu.GetComponent<RectTransform>().anchoredPosition.y + (BottonHeight + HiddenMenuHeight));

                HiddenMenu.GetComponent<RectTransform>().anchoredPosition = ChangedMenuPosition;    // Raise the menu above the button for easy display
                TheWindowWasOffset = true;                  // We indicate that we have shifted the hidden menu up, as it was at the bottom of the screen
            }

            HiddenMenu.SetActive(true);                     // Set it as active
            UIS.LastDDObject = UIS.ScrollContent.transform.GetChild(NumberOfPlayer).gameObject; // We put in the variable LustDDOBject the number of the last open player settings window
            StartCoroutine(_hiddenMenuContinuation());      // We sign the method "MethodDisableHiddenWindow" with a delay so that it is not called in the same click when it was signed      
        }
        else                                                // Otherwise, if the Game Object "hHidden Menu" is active
        {
            if (TheWindowWasOffset)                         // If the window was shifted up
            {
                // Calculate a old position for the hidden menu
                Vector2 ChangedMenuPosition = new Vector2(HiddenMenu.GetComponent<RectTransform>().anchoredPosition.x,
                    HiddenMenu.GetComponent<RectTransform>().anchoredPosition.y - (BottonHeight + HiddenMenuHeight));

                HiddenMenu.GetComponent<RectTransform>().anchoredPosition = ChangedMenuPosition;    // We lower the hidden menu into place
                TheWindowWasOffset = false;                 // We indicate that we have moved it down
            }

            HiddenMenu.SetActive(false);                                                        // Set it as inactive
      //    UIS.LastDDObject = UIS.ScrollContent.transform.GetChild(NumberOfPlayer).gameObject; // We put in the variable LustDDOBject the number of the last open player settings window
            UIS.MouseClick -= MethodDisableHiddenWindow;                                        // Write off the method "MethodDisableHiddenWindow" from the event "MouseClick"
        }
    }
    IEnumerator _hiddenMenuContinuation()                   // Continuation of the method "_hiddenMenu"
    {
        yield return new WaitForSeconds(0.1f);              // Make a delay
        UIS.MouseClick += MethodDisableHiddenWindow;        // Sign the method "MethodDisableHiddenWindow" to the event "MouseClick"
    }

    public void MethodDisableHiddenWindow()                 // A method that turns off a hidden window if a player clicks anywhere else
    {
        GameObject CaughtObject = PointerRaycast(Input.mousePosition);                              // Run the ray and save to the CaughtObject variable

        if (CaughtObject != null && CaughtObject.tag == "PlayerButtonHiddenMenu")                   // If the ray crashed into any UI object and its tag is equal to "PlayerButtonHiddenMenu"
        {
            for (int i = 0; i < 10; i++)                                                            // We go through a maximum of 10 cycles to find the parent button
            {
                if (CaughtObject.name.Contains("Button_Player"))                                    // If we found the parent button
                    break;                                                                          // Brake the cycle
                else                                                                                // If we have not found a parent
                    CaughtObject = CaughtObject.transform.parent.gameObject;                        // Assign the variable “CaughtObject” her parent to check it in the next cycle
            }
            if (CaughtObject.GetComponent<PlayerButtonScript>().NumberOfPlayer != NumberOfPlayer)   // If the number of the checked button does not match the number of the current opened button
                _hiddenMenu();                                                                      // Call the method to close the open hidden menu this button.

        }
        else                                                                              // Otherwise, if the beam does not crash into anything or does crash, but its tag is not "PlayerButtonHiddenMenu"
            _hiddenMenu();                  // Call the method to close the open hidden menu.
    }

    public void EndEdit(string NamePlayer)  // This method is called when the player's name was entered and the player clicked elsewhere
    {
        if (NamePlayer != "")                                                       // If we entered a non - empty player name
        {
            GM.NamesTurn.Insert(NumberOfPlayer, NamePlayer);                        // We enter in the list of player names the name of a certain player
            GM.NamesTurn.RemoveAt(NumberOfPlayer + 1);                              // Delete the old NickName
            UIS.ScrollContent.transform.GetChild(NumberOfPlayer).GetChild(2).GetComponent<Text>().text = GM.NamesTurn[NumberOfPlayer];                      // Display the new nickname in the button header
            UIS.ScrollContent.transform.GetChild(NumberOfPlayer).GetChild(3).GetChild(5).GetComponent<InputField>().text = GM.NamesTurn[NumberOfPlayer];    // Display the new nickname in the input field
        }
    }

    public void RandomName()    // This method generates and offers a random name from the "RandomNames" array of names.
    {
        string[] RandomNames = new[] { "Сергей", "Маша", "Максим", "Катя", "Олег", "Лена" };  // Array of random player names
        int RandomNomber = Random.Range(0, RandomNames.Length); // A random number representing one of the names in the "RandomNames" array

        if (TriedNames.Count < RandomNames.Length)              // If not all array names have been tried yet
        {
            for (int i = 0; i < TriedNames.Count; i++)          // We check the entire list of already used names in case this name has already been proposed
            {
                if (RandomNomber == TriedNames[i])              // If this name has already been proposed
                {
                    if (RandomNomber < RandomNames.Length - 1)  // If we have not reached the last element of the array
                    {
                        RandomNomber++;                         // Add the value to the variable "RandomNomber" to use the following name in the next loop
                    }
                    else                                        // Otherwise, if this is the last element of the array
                    {
                        RandomNomber = 0;                       // Assign it number 0
                    }
                }
            }
        }
        else                                                        // Otherwise, if all names have already been suggested
        {
            int Lastname = TriedNames[TriedNames.Count - 1];        // Remember the last name saved in the list of tried names.
            TriedNames.Clear();                                     // Clearing the list of suggested names
            TriedNames.Add(Lastname);                               // Add the last name from the end of the list to the top of the list to avoid duplicates when starting a new name cycle

            if (RandomNomber == Lastname)                           // If the name just generated was suggested the last time
            {
                if (RandomNomber < RandomNames.Length - 1)          // If we have not reached the last element of the array
                {
                    RandomNomber++;                                 // Add the value to the variable "RandomNomber" to use the following name in the next loop
                }
                else                                                // Otherwise, if this is the last element of the array
                {
                    RandomNomber = 0;                               // Assign it number 0
                }
            }
        }
        EndEdit(RandomNames[RandomNomber]);
        TriedNames.Add(RandomNomber);                               // Add the already suggested name to the list.
    }


    public void PlayerOrComputer()          // This method switches the state of a player, human or computer
    {
        if (HiddenMenu.transform.GetChild(2).GetComponent<Dropdown>().value == 0)                       // If the dropdown list returns 0 value
        {
            transform.GetChild(1).GetComponent<Image>().sprite = PlayerAndComputer[0];                  // Change the player/computer picture in the button header to - Player
            HiddenMenu.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "Player";          // Change the text of the drop-down list to "Player"
            GM.IsHeAHuman[NumberOfPlayer] = true;                                                       // We indicate that now it is listed as a Player
            ComputerDifficulty.GetComponent<Dropdown>().interactable = false;                           // Turn off the choice of difficulty
        }
        else                                                                                            // If the dropdown list returns 0 value
        {
            transform.GetChild(1).GetComponent<Image>().sprite = PlayerAndComputer[1];                  // Change the player/computer picture in the button header to - Computer
            HiddenMenu.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "Computer";        // Change the text of the drop-down list to "Computer"
            GM.IsHeAHuman[NumberOfPlayer] = false;                                                      // We indicate that now it is listed as a computer
            ComputerDifficulty.GetComponent<Dropdown>().interactable = true;                            // Turn on the choice of difficulty
        }
    }

    public void ChangeDifficulty()         // A method that changes the complexity of a particular computer adversary
    {
        byte CurrentValue = (byte)ComputerDifficulty.GetComponent<Dropdown>().value;    // We write down the number of the selected complexity in the variable "CurrentValue"

        // Until the high complexity of the computer adversary is realized, we will not let you choose the high complexity
        if (CurrentValue > 1)                                                           // If the difficulty of the game is indicated above 1
        {
            CurrentValue = 1;                                                           // Indicate complexity 1
            ComputerDifficulty.GetComponent<Dropdown>().value = CurrentValue;           // Specify the maximum value in the drop-down list
        }

        // Change the button text to the text of the current selected difficulty
        ComputerDifficulty.GetComponent<Dropdown>().transform.GetChild(0).GetComponent<Text>().text = ComputerDifficulty.GetComponent<Dropdown>().options[CurrentValue].text;
        GM.ComputerDifficulty[NumberOfPlayer] = CurrentValue;                       // We indicate a new complexity in the list of complexity of computer opponents
    }

    GameObject PointerRaycast(Vector2 Position)                                     // This method returns the object into which the ray crashed
    {
        PointerEventData PointerData = new PointerEventData(EventSystem.current);   // We put the object of the current system of events into an instance of "PointerData"
        List<RaycastResult> resultsData = new List<RaycastResult>();                // Create a list of ray collision results
        PointerData.position = Position;                                            // We lay the mouse position when pressed in PointerData.position
        EventSystem.current.RaycastAll(PointerData, resultsData);                   // Пускаем луч в сцену используя сохранённую позцию мышки, и сохраняем все объекты через которые он прошёл

        if (resultsData.Count > 0)                                                  // If the ray crashed into at least one UI object
        {
            return resultsData[0].gameObject;                                       // We return a list of all these objects
        }

        return null;                                                                // Else return null
    }



    // -------------------------------------------------------------------------------------------- Методы кнопок --------------------------------------------------------------------------------------------

    public void HiddenMenuButton()  // This method is called by the button "ButtonPlayer"
    {
        _hiddenMenu();              // Call the button open / close method
    }
}



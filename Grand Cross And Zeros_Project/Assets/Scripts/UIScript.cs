using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIScript : MonoBehaviour
{
    public delegate void MouseСlickSettingsMenu();                      // Delegate for mouse click events
    public event MouseСlickSettingsMenu MouseClick;                     // MouseClick event                        
    public Text FPSText;
    float Msec;
    int accumulator = 0;                                                // 
    int counter = 0;
    float timer = 0f;
    public GameManager GM;                                              // Переменная хранящая скрипт GameManager находящийся на определённом объекте
    public Canvas _Canvas_;                                             // Сдесь храниться канвас для более удобного нахождения нужных кнопок
    public CellsMaker CM;                                               // The "CellsMaker" script is stored here.
    public SaveLoad SL;                                                 // Here is the script "SaveLoad"
    public GameObject[] Panels;                                         // Массив главных панелей канваса
    public List<int> DeactivateOnStart;                                 // List of windows that are deactivated for convenience at startup
    public GameObject ScrollContent;                                    // Group for drop-down lists of player settings
    public GameObject NumberOfPlayersDropDown;                          // Here is a drop-down list in which the number of players is selected
    public GameObject TheFirstPlayerSettingButton;                      // Here is the first player setting button
    float HidenPanelRealPosX;                                           // Действительная позиция скрытой панели по оси "X"
    Font FontStyle;                                                     // Стиль шрифта игры
    public GameObject LastDDObject;                                     // The object on which the last open drop-down list hangs
    public Dropdown LastOpeningDD;                                      // The Dropdown class object that was called last
    public float LastСlickOnThePSB;                                     // Here will be stored the "y" coordinates of the click on the last button pressed player settings
    public RenderTexture TargetText;                                    // Target texture for taking camera image
    bool HideFlashingFourArrows = false;                                // This variable indicates whether to interrupt the blinking cycle of 4 arrows.
    float OldTick;                                                      // This variable indicates the old time for the four arrows to flash.
    float NowTick = 0;                                                  // This variable indicates the new time for the four arrows to flash.
    byte ZoomStep = 0;                                                  // This variable indicates how many steps the screen is currently magnified.
    public GameObject VerticalScrollbar;                                // Vertical scrollbar for zoom panel
    public GameObject HorizontalScrollbar;                              // Horizontal scrollbar for zoom panel
    public List<Texture> ListOfPassageMapScreenshots;                   // Screenshots of passage maps will be stored here.
    public List<Texture> ListOfCustomMapScreenshots;                    // Screenshots of custom maps will be stored here.
    public float DelayForCameraMovement = 2.17f;                        // Delay for camera movement
    public bool OnDebugPanel;                                           // This variable is responsible for enabling the debug panel
    public bool ShowFigures;                                            // This variable is true when the figures of the grock are shown at the beginning of the game
    bool JustUploaded = false;                                          // This variable indicates that the map selection window has just been loaded.
    public Image ShowingFigure;                                         // This component contains an image of the player’s figure who will be walking now.   
    public Color StandartTextColor;                                     // Standard text color for this game. 
    string StandartHexColor;                                            // Standard text color in "HEX" format

    public int SelectedPassageMap                                       // Auto property indicating the current selected passage map
    { get; set; } = 1;                                                  // Set the initial value of the selected passage map as 1

    public int SelectedCustomMap                                        // Auto property indicating the current selected user map
    { get; set; } = 1;                                                  // Set the initial value of the selected user map as 1


    void OnEnable()
    {
        GM.SetStateMenu += MethodSetStateMenu;                          // We sign the method "MethodSetStateMenu" to the event "SetStateMenu"
        GM.SetStateShceme += MethodSetStateScheme;                      // We sign the "SetStateShceme" method for the event "MethodSetStateScheme"
        GM.SetStateGame += MethodSetStateGame;                          // We sign the "SetStateGame" method for the event "MethodSetStateGame"
        GM.OneLineWasFinished += MethodOneLineWasFinished;              // We sign the "MethodOneLineWasFinished" method for the event "OneLineWasFinished"
        GM.DrawTheFollowingLine += MethodDrawTheFollowingLine;          // We sign the "MethodDrawTheFollowingLine" method for the event "DrawTheFollowingLine"
        GM.PlayGame += MethodPlayGame;                                  // We sign the "MethodPlayGame" method for the event "MethodPlayGame"
        GM.Surrender += MethodSurrender;                                // We sign the "MethodSurrender" method for the event "Surrender"
        GM.EveryoneSurrendered += MethodEveryoneSurrendered;            // We sign the "MethodEveryoneSurrendered" method for the event "EveryoneSurrendered"
        GM.ShowWalking += MethodShowWalking;                            // We sign the "MethodShowWalking" method for the event "ShowWalking"
        GM.TheFirstPointIsSet += MethodTheFirstPointIsSet;              // We sign the "MethodTheFirstPointIsSet" method for the event "TheFirstPointIsSet"
        GM.TheNextPointIsSet += MethodTheNextPointIsSet;                // We sign the "MethodTheNextPointIsSet" method for the event "TheNextPointIsSet"
        GM.LoadGameLevel += MethodLoadGameLevel;                        // We sign the method "MethodLoadGameLevel" to the event "LoadGameLevel"
        GM.PlayAgain += MethodPlayAgain;                                // We subscribe the "MethodPlayAgain" method to the "PlayAgain" event
        GM.PlayNextLevel += MethodPlayNextLevel;                        // We subscribe the "MethodPlayNextLevel" method to the "PlayNextLevel" event
        GM.StartingCellNotFound += MethodStartingCellNotFound;          // We subscribe the "MethodStartingCellNotFound" method to the "StartingCellNotFound" event
    }


    void Start()
    {
        SetPanelsStates();                                              // Call the method that sets the state of the panels.
        FontStyle = Resources.Load<Font>("Fonts/ARIAL");                // Load font style
        StandartHexColor = ToHEX(StandartTextColor);                    // We translate the standard color of the text in the format "HEX"
    }


    void Update()
    {
        MouseClickListener();                                           // Check for mouse click event
        MethodFPS();
    }


    void MouseClickListener()   // Mouse event tracking method for player settings menu
    {
        if (Panels[6].activeSelf && Input.GetMouseButtonUp(0))  // If the player settings window is open and the left mouse button is pressed
        {
            if(MouseClick != null)                              // If any method is subscribed to the Mouse Click event
                MouseClick();                                   // Call the mouse click event
        }
    }


    void SetPanelsStates()                                  // This method sets the state of the panels.
    {
        for (byte a = 0; a < DeactivateOnStart.Count; a++)  // We continue the cycle until we deactivate all panels from the deactivation list
        {
            Panels[DeactivateOnStart[a]].SetActive(false);  // Deactivate the next panel from the list
        }
    }


    void MethodFPS()                                        // Method for displaying FPS in a game on a mobile phone for optimization
    {
        accumulator++;
        timer += Time.deltaTime;

        if (timer >= 1)
        {
            timer = 0;
            counter = accumulator;
            accumulator = 0;
        }
        FPSText.text = counter.ToString();
    }


    void MoveHidenPanel()                                                           // Этот метод либо задвигает либо выдвигает скрытую панель
    {
        if (Panels[3].GetComponent<Animator>().GetBool("PanelOutside") == false)    // If the variable "Panel Outside" is false
        {
            Panels[3].GetComponent<Animator>().SetBool("PanelOutside", true);       // Then we indicate her condition as true.
            Panels[31].GetComponent<Button>().interactable = false;                 // Block block the button for displaying the elements of increasing the map
            Panels[32].GetComponent<Button>().interactable = false;                 // Block the button showing the game menu
            Panels[48].SetActive(true);                                             // Making the alert button active
            GM.BlockingMoves = true;                                                // We block the moves of the players while the panel is extended
        }
        else                                                                        // Otherwise, if the variable "Panel Outside" is true
        {
            Panels[3].GetComponent<Animator>().SetBool("PanelOutside", false);      // Then we indicate her condition as false
            Panels[31].GetComponent<Button>().interactable = true;                  // Unblock the button for displaying the elements of increasing the map
            Panels[32].GetComponent<Button>().interactable = true;                  // Unblock the button showing the game menu
            Panels[48].SetActive(false);                                            // Making the alert button inactive
            GM.BlockingMoves = false;                                               // Unlock player moves if panel is pushed back
        }
    }


    void MethodSetStateScheme()                                         // Method called event "Set game state scheme"
    {
        Panels[1].SetActive(false);                                                         // Set the main menu with all buttons in inactive mode, hiding them from the player's eyes
        Panels[6].SetActive(false);                                                         // Set the menu for setting the round to inactive mode
        Panels[27].transform.GetChild(0).GetComponent<Button>().interactable = true;        // Unlock the save map button in the main menu
        Panels[37].GetComponent<Button>().interactable = true;                              // Unlock the save map button in the End game menu
        Panels[35].GetComponent<Button>().interactable = false;                             // Lock the button to play the next level

        StartCoroutine(MethodSetStateSchemeСontinuation());             // Calling coroutine "MethodPointTheCameraAtTheMapСontinuation"
    }
    IEnumerator MethodSetStateSchemeСontinuation()                      // Continuation of the method "MethodSetStateScheme"
    {
        yield return new WaitForSeconds(DelayForCameraMovement);        // Make a delay
        Panels[10].SetActive(true);                                     // Turn on the panel "CreatingBorderField_Panel"
        Panels[2].SetActive(true);                                      // Turn on the "ButtonScheme_Panel" panel
        CM.DrawingALineIsProhibited = true;                             // We indicate that we prohibit drawing a line
        CM.TheCamHasReach = true;                                       // We indicate that the main camera arrived at the destination
    }


    void MethodTheFirstPointIsSet()                                     // This method is called an event "TheFirstPointIsSet"
    {
        Panels[41].transform.position = Camera.main.WorldToScreenPoint(CM.SelPo[0]);    // Place a flag in the place where the first point of the line was set.
        Panels[41].SetActive(true);                                     // Make the flag image active.
        Panels[42].transform.position = Camera.main.WorldToScreenPoint(CM.SelPo[0]);                // Place the image of 4 arrows in the place where the first point of the line was set.
        StartCoroutine(MethodTheFirstPointIsSetСontinuation());         // Call the coroutine "MethodTheFirstPointIsSetСontinuation"
    }
    IEnumerator MethodTheFirstPointIsSetСontinuation()                  // Continuation of the method "MethodTheFirstPointIsSet"
    {
        OldTick = 0;                                                    // Zero value of the "OldTick" variable
        Panels[42].SetActive(true);                                     // Make the image of four arrows Active
        for (; !HideFlashingFourArrows;)                                // Continue the cycle until the variable "HideFlashingFourArrows" is false
        {
            NowTick = Time.time;                                        // The time elapsed since the beginning of the game is entered into the variable "dsfsd".
            if (OldTick == 0) OldTick = NowTick;                        // If the variable "OldTick" is equal to zero, we indicate to it the value from the variable "NowTick"

            if (NowTick > (OldTick + 1))                                // If one second has passed since the last check
            {
                if (Panels[42].activeSelf == true)                      // If the image of four arrows is active
                    Panels[42].SetActive(false);                        // Make the image of four arrows inactive
                else                                                    // Else Otherwise, if the image of four arrows is active
                    Panels[42].SetActive(true);                         // Make the image of four arrows Active
                OldTick = NowTick;                                      // We update the timer
            }
            yield return new WaitForSeconds(0.1f);                      // Make a delay
        }
        if (HideFlashingFourArrows) Panels[42].SetActive(false);        // If the upper cycle is over make the flag image inactive.
    }


    void MethodTheNextPointIsSet()                                                          // This method is called an event "TheFirstPointIsSet"
    {
        HideFlashingFourArrows = true;                                                      // If the variable "HideFlashingFourArrows" is equal to false, then we make it equal to true
        if (Panels[11].transform.GetChild(0).GetComponent<Button>().interactable == false)  // If the button a step back is not interacting
        {
            Panels[11].transform.GetChild(0).GetComponent<Button>().interactable = true;    // Making the back button edit interacting
            Panels[11].transform.GetChild(2).GetComponent<Button>().interactable = true;    // Making the button "jump back" interacting
        }
        if (Panels[11].transform.GetChild(1).GetComponent<Button>().interactable == true)
        {
            Panels[11].transform.GetChild(1).GetComponent<Button>().interactable = false;
            Panels[11].transform.GetChild(3).GetComponent<Button>().interactable = false;
        }

        if (CM.SelPo.Count <= CM.MinimumLength)                   // If the length of the line is less than the variable "MinimumLength"
        {
            Panels[44].SetActive(false);                                    // Make the black dot inactive.
            Panels[43].transform.position = Camera.main.WorldToScreenPoint(CM.SelPo[CM.SelPo.Count - 1]);   // Assign a red dot to a position at the end of the line.
            Panels[43].SetActive(true);                                     // Making the red dot active.
        }
        else                                                                // Otherwise, if the length of the line is greater than or equal to the variable "MinimumLength"
        {
            Panels[43].SetActive(false);                                    // Make the red dot inactive.
            Panels[44].transform.position = Camera.main.WorldToScreenPoint(CM.SelPo[CM.SelPo.Count - 1]);   // Assign a black dot at the end of the line.
            Panels[44].SetActive(true);                                     // Make the black dot active.
        }
    }


    void TheLineLengthHasBeenChanged()                                          // This method is called to change the position of a point at the end of a line when the length of a line has been changed using the edit buttons.
    {
        if (!HideFlashingFourArrows) HideFlashingFourArrows = true;             // If the variable "HideFlashingFourArrows" is equal to false, then we make it equal to true
        if ((CM.ActiveLinePoint + 1) <= CM.MinimumLength)                       // If the length of the line is less than the variable "MinimumLength"
        {
            CM.LRs[CM.LRs.Count - 1].material = CM.LineRendererMaterials[0];    // Assign Red Material to LineRenderer
            Panels[44].SetActive(false);                                        // Make the black dot inactive.
            Panels[43].transform.position = Camera.main.WorldToScreenPoint(CM.SelPo[CM.ActiveLinePoint]);   // Assign a red dot to a position at the end of the line.
            Panels[43].SetActive(true);                                         // Making the red dot active.
        }
        else                                                                    // Otherwise, if the length of the line is greater than or equal to the variable "MinimumLength"
        {
            CM.LRs[CM.LRs.Count - 1].material = CM.LineRendererMaterials[1];    // Assign Standard Black Material to LineRenderer
            Panels[43].SetActive(false);                                        // Make the red dot inactive.
            Panels[44].transform.position = Camera.main.WorldToScreenPoint(CM.SelPo[CM.ActiveLinePoint]);   // Assign a black dot at the end of the line.
            Panels[44].SetActive(true);                                         // Make the black dot active.
        }
    }


    void MethodOneLineWasFinished()                                     // Метод вызываемый событием "Одна линия была законченна"
    {
        Panels[2].SetActive(true);                                      // Turn on the line editing panel
        Panels[43].SetActive(false);                                    // We make the red dot inactive.
        Panels[44].SetActive(false);                                    // We make the black dot inactive.
        Panels[45].SetActive(false);                                    // We make the "PushedDotCollider" icon inactive
        Panels[30].SetActive(false);                                    // Turn off the button to activate / deactivate the line editing panel
        Panels[31].SetActive(false);                                    // Turn off the button to activate / deactivate the screen zoom panel
        Panels[32].SetActive(false);                                    // Turn off the button to activate / deactivate the GameMenu button
        Panels[11].SetActive(false);                                    // Disable line editing panel
    }


    void MethodDrawTheFollowingLine()                                   // Метод вызываемый событием "Рисуем следующую линию"
    {
        Panels[2].SetActive(false);                                     // Выключаем панель кнопок схемы                   
    }


    void MethodPlayGame()                                               // Этот метод вызываеться событием "Играть текущий чертёж"
    {
        Panels[2].SetActive(false);                                     // Turn off the bottom panel of the "scheme buttons"
        Panels[2].transform.GetChild(0).gameObject.SetActive(false);    // Turn off the button "Draw another line"
        Panels[2].transform.GetChild(2).gameObject.SetActive(true);     // Turn on the "Draw the first line" button
        Panels[10].SetActive(true);                                     // Turn on the panel "CreatingBorderField"
        Panels[31].SetActive(true);                                     // Turn on the button - activate / deactivate the screen zoom panel
        Panels[32].SetActive(true);                                     // Turn on the button to activate / deactivate the GameMenu button
        Panels[27].transform.GetChild(0).GetComponent<Button>().interactable = true;   // Making the "Save Game" button in the "Game Menu" Interactable
    }


    void MethodSetStateGame()                                           // Метод вызванный событием "Установить состояние игры (Игра)"
    {
        Panels[3].GetComponentInChildren<Button>().interactable = false;// Making a hidden panel non interactable
        Panels[3].SetActive(true);                                      // Выключаем скрытую панель при старте в случае если она была не выключенна  
        Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 128);  // Making the hidden panel transparent
    }


    void MethodSetStateMenu()                                           // Method called by the event "SetStateMenu" and set the initial settings of the game settings menu
    {
        Panels[4].SetActive(false);                                     // Disable the menu at the end of the game
        Panels[5].SetActive(false);                                     // Turn off the GamePoints_Panel panel
        Panels[40].SetActive(false);                                    // Turn off the warning window with access to the main menu
        Panels[50].SetActive(false);                                    // Turn off the panel showing who won
        Panels[52].SetActive(false);                                    // Turn off the winner window at the end of the game  

        byte CC = (byte)Panels[5].transform.childCount;                 // Put the number of children of the object "GamePoints_Panel" in the variable "ChildCount"
        for (byte a = 0; a < CC; a++)                                   // Continue the cycle until we delete all the children. "GamePoints_Panel"
        {
            Destroy(Panels[5].transform.GetChild(a).gameObject);        // We destroy the next child
        }

        NumberOfPlayersDropDown.GetComponent<Dropdown>().value = 0;     // We put on the drop-down list showing the number of players, the number of players by default 0. Only 2 players left by default

        GameObject PanelTwo = Panels[6].transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject;   // Get the settings button for the second player
        
        // Fill in the information in the drop-down button of the first player
        TheFirstPlayerSettingButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().color =
            GM.ColorGridPanel.transform.GetChild(25).GetChild(0).GetComponent<Image>().color;                               // Initial color figure header

        TheFirstPlayerSettingButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite =
            Panels[8].transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite;               // Initial icon figure header

        TheFirstPlayerSettingButton.transform.GetChild(1).GetComponent<Image>().sprite =
            TheFirstPlayerSettingButton.GetComponent<PlayerButtonScript>().PlayerAndComputer[0];                            // Set the icon for the "1" player (player/computer) by default to the player

        TheFirstPlayerSettingButton.transform.GetChild(2).GetComponent<Text>().text = "Zero";                               // Initial nickname

        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(0).GetChild(0).GetComponent<Image>().color =
            GM.ColorGridPanel.transform.GetChild(25).GetChild(0).GetComponent<Image>().color;                               // Initial color icon

        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().sprite =
            Panels[8].transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite;               // Initial icon figure

        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().color =
            GM.ColorGridPanel.transform.GetChild(25).GetChild(0).GetComponent<Image>().color;                               // Initial color icon figure
            
        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(2).GetChild(0).GetComponent<Text>().text = "Player";     // Default text 1 player per button (player / computer)
        
        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(2).GetComponent<Dropdown>().value = 0;                   // We indicate to the component "Dropdown" 1 player that the player is selected
        
        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(3).GetChild(0).GetComponent<Text>().text = "Dumb";       // Specify the player 1 default button text "ComputerDifficulty"
        
        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(3).GetComponent<Dropdown>().value = 0;                   // Specify the "Dropdown" 1 player component initial computer complexity

        TheFirstPlayerSettingButton.transform.GetChild(3).GetChild(5).GetComponent<InputField>().text = "";                 // Empty the line in the input field
        

        // Fill in the information in the drop-down button of the second player
        PanelTwo.transform.GetChild(0).GetChild(0).GetComponent<Image>().color =
            GM.ColorGridPanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().color;                                // Initial color icon figure header

        PanelTwo.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite =
            Panels[8].transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().sprite;               // Initial icon figure header

        PanelTwo.transform.GetChild(1).GetComponent<Image>().sprite =
            PanelTwo.GetComponent<PlayerButtonScript>().PlayerAndComputer[0];                                               // Set the icon for the "2" player (player/computer) by default to the player

        PanelTwo.transform.GetChild(2).GetComponent<Text>().text = "Cross";                                                 // Initial nickname

        PanelTwo.transform.GetChild(3).GetChild(0).GetChild(0).GetComponent<Image>().color =
            GM.ColorGridPanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().color;                                // Initial color icon

        PanelTwo.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().sprite =
            Panels[8].transform.GetChild(2).GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().sprite;               // Initial icon figure

        PanelTwo.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().color =
            GM.ColorGridPanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().color;                                // Initial color icon figure

        PanelTwo.transform.GetChild(3).GetChild(2).GetChild(0).GetComponent<Text>().text = "Player";                        // Default text 2 player per button (player / computer)

        PanelTwo.transform.GetChild(3).GetChild(2).GetComponent<Dropdown>().value = 0;                                      // We indicate to the component "Dropdown" 2 player that the player is selected

        PanelTwo.transform.GetChild(3).GetChild(3).GetChild(0).GetComponent<Text>().text = "Dumb";                          // Specify the player 2 default button text "ComputerDifficulty"

        PanelTwo.transform.GetChild(3).GetChild(3).GetComponent<Dropdown>().value = 0;                                      // Specify the "Dropdown" 2 player component initial computer complexity

        PanelTwo.transform.GetChild(3).GetChild(5).GetComponent<InputField>().text = "";                                    // Empty the line in the input field

        StartCoroutine(MethodSetStateMenuContinuation());               // We start the method of coroutine MethodSetStateMenuContinuationOld
    }
    IEnumerator MethodSetStateMenuContinuation()
    {
        yield return new WaitForSeconds(DelayForCameraMovement);        // We make a delay
        Panels[1].SetActive(true);                                      // Turn on the main menu
    }

    void MethodSurrender()                                                          // This method subscribes to the Surrender event and triggers a hidden panel animation.
    {
        if (Panels[3].GetComponent<Animator>().GetBool("PanelOutside") == true)     // If the panel was pulled out (it was called by a live player and not by a computer)
            MoveHidenPanel();                                                       // Call the button animation method.
    }

    void MethodEveryoneSurrendered()                                    // The method triggered by the event "All surrendered"
    {
        StartCoroutine(MethodEveryoneSurrenderСontinuation());          // Вызываем корутину "MethodEveryoneSurrenderСontinuation" которая осузществляет отложенную рекацию меню
    }
    IEnumerator MethodEveryoneSurrenderСontinuation()                   // Continuation of the method "MethodEveryoneSurrendered"
    {
        if (SelectedPassageMap == 20 && GM.IsHeAHuman[GM.WinNomber] == true)  // If the player has just played the selected map & the winning player is a human)
        {
            Panels[36].GetComponent<Button>().interactable = false; // Make the "Play again" button inactive
            Panels[37].GetComponent<Button>().interactable = false; // Make the "SaveMap" button inactive
            Panels[38].GetComponent<Button>().interactable = false; // Make the "ToTheMainMenu" button inactive
            Panels[39].GetComponent<Button>().interactable = false; // Make the "QuitTheGame" button inactive
        }
    
        string _signatureСomputer = "<color=" + StandartHexColor + "> (Компьютер) </color>";// The painted nickname of the current player
        
        Panels[3].GetComponentInChildren<Button>().interactable = false;                    // Lock the button "Hidden panel"
        Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 128);            // Making the hidden panel transparent
        Panels[31].GetComponent<Button>().interactable = false;                             // Making the Open / Close the screen zoom panel button inactive
        Panels[32].GetComponent<Button>().interactable = false;                             // Making the Open / Close GameMenu button inactive

        yield return new WaitForSeconds(GM.DelayForMenu);                                   // Make a delay

        // If game mode, walkthrough & if the player is available to the next level
        if (GM.GameType == GameManager.GameTypes.Walkthrough && SL.SLO.Progress >= SelectedPassageMap)  
            if(SelectedPassageMap <20)                                                      // And if it's not the last level
                Panels[35].GetComponent<Button>().interactable = true;                      // Unlock the button to play the next level

        // If the game mode is "CustomMapGame" and if the following custom map exists
        if (GM.GameType == GameManager.GameTypes.CustomMapGame && SelectedCustomMap < ListOfCustomMapScreenshots.Count)
            Panels[35].GetComponent<Button>().interactable = true;                          // Unlock the button to play the next level

        Panels[3].SetActive(false);                                                         // Turn off the hidden panel
        Panels[31].SetActive(false);                                                        // Turn off the button - Open/Close the screen zoom panel
        Panels[32].SetActive(false);                                                        // Turn off the button - Open/Close GameMenu
        Panels[31].GetComponent<Button>().interactable = true;                              // Making the Open / Close the screen zoom panel button active
        Panels[32].GetComponent<Button>().interactable = true;                              // Making the Open / Close GameMenu button active
        Panels[4].SetActive(true);                                                          // Turn on the "GamePoints" panel
        Panels[5].SetActive(true);                                                          // Включаем панель "Очки игроков"
        float HorizontalPos = 40;                                                           // We set the text lines to a horizontal position

        List<int> QueuePoints = new List<int>();                            // Создаём список очереди очков игроков
        List<string> QueueNames = new List<string>();                       // Создаём список очереди имён игроков
        List<Color> QueueColor = new List<Color>();                         // Create a list of color queue for player text
        List<bool> QueueIsHeAHuman = new List<bool>();                      // Create a queue of states indicating the computer is this or a person

        List<GameObject> PlayerInformationHolder = new List<GameObject>();  // Create a list for objects that will be parents for objects that display player information
        List<GameObject> PlayerPoints = new List<GameObject>();             // Create a list of objects, each will have a player point
        List<GameObject> PlayerName = new List<GameObject>();               // Create a list of objects, each will have a player name
        List<GameObject> SignatureComputer = new List<GameObject>();        // Create a list of objects with a computer signature

        List<RectTransform> RectTransPIH = new List<RectTransform>();       // The list of "RectTransform" components that will be located on the "PlayerInformationHolder" objects
        List<Text> TextComponentsNames = new List<Text>();                  // Create a list of "text" components for the players' name
        List<Text> TextComponentsPoints = new List<Text>();                 // Create a list of "text" components for the players' points
        List<Vector2> RowsPositions = new List<Vector2>();                  // We create the list storing in itself positions of lines

        for (int a = 0; a < GM.GamePoints.Count; a++)                   // Продолжаем цикл столько раз сколько игроков сейчас играло
        {
            if (QueuePoints.Count == 0)                                 // Если список очков игроков пуст
            {
                QueuePoints.Add(GM.GamePoints[0]);                      // Ложим в него первое попавшееся число из списка
                QueueNames.Add(GM.NamesTurn[0]);                        // Ложим в список имён игроков первое попавшееся имя из списка NamesTurn
                QueueColor.Add(GM.ColorsTurn[0]);                       // We put the first color in the list of colors
                QueueIsHeAHuman.Add(GM.IsHeAHuman[0]);                  // We put the first value from the list "IsHeAHuman" into the list of human / computer states
                continue;                                               // Включаем принудительно следующую итерацию
            }

            for (int b = 0; ; b++)                                      // Сравниваем элемент цикла "a" списка QueuePoints c элементом b списка GM.Gamepoints 
            {
                if (QueuePoints[b] >= GM.GamePoints[a])                 // Если число набранных очков опрашиваемого элемента в массиве QueuePoints меньше или равно опрашиваемому числу в массиве GamePoints
                {
                    QueuePoints.Insert(b, GM.GamePoints[a]);            // То заносим это число из массива GamePoints на место опрашиваемое сейчас для QueuePoints
                    QueueNames.Insert(b, GM.NamesTurn[a]);              // Дублируем тоже самое для списка имён
                    QueueColor.Insert(b, GM.ColorsTurn[a]);             // Duplicate the same for the list of colors
                    QueueIsHeAHuman.Insert(b, GM.IsHeAHuman[a]);        // Do the same for the QueueIsHeAHuman list.
                    break;                                              // И преываем цикл так как мы переместили уже эту цифру из одного массива в другой
                }
                if ((b + 1) >= QueuePoints.Count)                       // Если мы перебрали весь массив QueuePoints и не нашли в нём числа больше того с которым мы сравниваем в этом цикле в массиве GamePoints
                {                                                       // Значит пришло время занести в конец списка QueuePoints новое самое старшее число
                    QueuePoints.Add(GM.GamePoints[a]);                  // Добавляем в конец списка QueuePoints новое число
                    QueueNames.Add(GM.NamesTurn[a]);                    // Дублируем тоже самое для списка имён
                    QueueColor.Add(GM.ColorsTurn[a]);                   // Duplicate the same for the list of colors
                    QueueIsHeAHuman.Add(GM.IsHeAHuman[a]);              // Do the same for the QueueIsHeAHuman list.
                    break;                                              // И прерываем цикл чтобы не опрашивать заного ту же цифру массива GamePoints
                }
            }
        }

        for (int c = 0; c < QueuePoints.Count; c++)                                             // Repeat the iteration until we display all the numbers in the list
        {
            PlayerInformationHolder.Add(new GameObject());                                      // Add a new object to the list of objects "Player Information Holder"
            PlayerPoints.Add(new GameObject());                                                 // We put on the stage a "empty" for the points player and a link to it in one of the cell's "Player Points"
            PlayerName.Add(new GameObject());                                                   // We put a empy for the name player on the stage and a link to it in one of the "PlayerName" cells

            PlayerInformationHolder[c].transform.SetParent(Panels[5].transform);                // For the next "Player Information Holder" of the PlayerName list, set the parent "GamePoints_Panel"
            PlayerPoints[c].transform.SetParent(PlayerInformationHolder[c].transform);          // For the next item in the "PlayerPoints" list, set the parent
            PlayerName[c].transform.SetParent(PlayerPoints[c].transform);                       // For the next element of the PlayerName list, set the parent

            PlayerInformationHolder[c].name = QueueNames[c] + "_InformationHolder";             // Set the name of the next "PlayerInformationHolder" object
            PlayerName[c].transform.localScale = new Vector3(1, 1, 1);                          // Specify the local text size as 1 in case the game is not running at the original screen size.
            PlayerName[c].name = QueueNames[c];                                                 // We assign to this element the name of that player the information about which he will display
            PlayerPoints[c].name = QueuePoints[c].ToString();                                   // Assign this element as the name of the points of the player whose information it will display

            RectTransPIH.Add(PlayerInformationHolder[c].AddComponent<RectTransform>()); // We hang the "RectTransform" component on this object and put the link to this component in "RectTransPIH"
            TextComponentsPoints.Add(PlayerPoints[c].AddComponent<Text>());             // We hang the "text" component on this object and put the link to this component "TextComponentsPoints"
            TextComponentsNames.Add(PlayerName[c].AddComponent<Text>());                // We hang the "text" component on this object and put the link to this component "TextComponentsNames"

            RectTransPIH[c].localScale = new Vector3(1, 1, 1);                          // Specify "Scale" 1 for the object holding the information player
            RectTransPIH[c].anchorMin = new Vector2(0, 0);                              // Set the anchors "Min" to the lower left position of the parent window
            RectTransPIH[c].anchorMax = new Vector2(0, 0);                              // Set the anchors "Max" to the lower left position of the parent window

            TextComponentsPoints[c].font = FontStyle;                                   // Specify the font style for the text displaying the player’s points
            TextComponentsNames[c].font = FontStyle;                                    // Specify the font style for the text displaying the name of the player

            TextComponentsPoints[c].fontSize = 35;                                      // Specify the font size
            TextComponentsPoints[c].resizeTextForBestFit = TextComponentsPoints[c];     // Reduce text size if it does not fit into a string
            TextComponentsNames[c].fontSize = 35;                                       // Specify the font size
            TextComponentsNames[c].resizeTextForBestFit = TextComponentsNames[c];       // Reduce text size if it does not fit into a string

            string HexColor = ToHEX(QueueColor[c]);                                                 // Convert "RGB" to "HTML" color
            string FinishedText = "<color=" + HexColor + "><b>" + QueueNames[c] + "</b></color>";   // Colorize the text
            TextComponentsPoints[c].color = new Color32(16, 104, 255, 255);                 // Indicate the color of the player's points text
            if (QueueIsHeAHuman[c] == true)                                                 // If in this cycle we create a human name
                TextComponentsNames[c].text = FinishedText;                                 // We assign a ready-made colored text displaying the player’s nickname
            else                                                                            // Otherwise, if in this loop we display the computer name
                TextComponentsNames[c].text = FinishedText + _signatureСomputer;            // Assign the finished colored text displaying the player’s nickname + signatureСomputer

            TextComponentsPoints[c].fontStyle = UnityEngine.FontStyle.Bold;                 // Specify the text style as bold

            TextComponentsPoints[c].text = QueuePoints[c].ToString() + "<color=#000000ff>:</color>";    // Specify the text of the component as text, the player's points about which the information is indicated
            TextComponentsPoints[c].alignment = TextAnchor.MiddleRight;                                 // We specify the type of alignment for the component "text" pointing player points
            TextComponentsNames[c].alignment = TextAnchor.MiddleLeft;                                   // Align the nickname of the next player in the center left

            RectTransPIH[c].anchoredPosition = new Vector2(HorizontalPos, 35);                                  // We indicate the location of the object displaying information about the player
            TextComponentsNames[c].rectTransform.anchoredPosition = new Vector2(350, 0);                        // Specify the location of the object displaying the player’s name
            RowsPositions.Add(RectTransPIH[c].anchoredPosition);                                                // Save the position "PlayerInformationHolder"

            RectTransPIH[c].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 39);                         // Specify the height of the RectTransform
            RectTransPIH[c].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 80);                       // Specify the width of the RectTransform  
            TextComponentsNames[c].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 600); // Specify the width of the RectTransform
            TextComponentsNames[c].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 39);    // Specify the height of the RectTransform
            TextComponentsPoints[c].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 80); // Specify the width of the RectTransform
            TextComponentsPoints[c].rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 39);   // Specify the height of the RectTransform

            yield return new WaitForSeconds(2);                                                     // We make a delay

            if ((c + 1) < QueuePoints.Count)                                                        // If there is another loop that should insert another line
            {
                for (int d = 0; d < RowsPositions.Count; d++)                                       // Continue the cycle until we raise all the created rows that show the results of the players in one division
                {
                    if (TextComponentsNames[d] != null)                                             // If the data in the list was not deleted by pressing the button to the main menu
                    {
                        RowsPositions[d] = new Vector2(HorizontalPos, RowsPositions[d].y + 40);     // Change the position of this line in the list of "Rows Positions" by raising it one division up in the game
                        RectTransPIH[d].anchoredPosition = RowsPositions[d];                        // Raise the object displaying player information one step higher
                    }
                    else                                                                            // Otherwise, if the player goes to the main menu
                    {
                        RowsPositions.Clear();                                                      // Clear "RowsPositions" List
                        QueuePoints.Clear();                                                        // Clear "QueuePoints" List
                        break;                                                                      // Break the cycle
                    }
                }
            }
        }

        if (QueuePoints.Count != 0) // If the length of the list of players is greater than zero
        {
            string HexColor = ToHEX(QueueColor[QueueNames.Count - 1]);                      // Convert "RGB" to "HTML" color winning player

            if (QueuePoints[QueuePoints.Count - 1] > QueuePoints[QueuePoints.Count - 2])    // If the points of the (last) player are greater than the points (before the last) player in the list of player points
            {
                string FinishedText = "<color=" + HexColor + "><b>" + QueueNames[QueueNames.Count - 1] +
                    "</b></color><color=" + StandartHexColor + "> Выиграл </color>";                                // Colorize the text

                Panels[34].GetComponent<Text>().resizeTextForBestFit = Panels[34].GetComponent<Text>();             // Turn on the "BestFIt" function for the text.

                if (QueueIsHeAHuman[QueueNames.Count - 1] == true)                                                  // If the winning player is a human
                    Panels[34].GetComponent<Text>().text = FinishedText;                                            // Write the name of the player who won
                else                                                                                                // Otherwise, If the winning player is a computer
                {
                    FinishedText = "<color=" + HexColor + "><b>" + QueueNames[QueueNames.Count - 1] +
                        "</b></color>" + _signatureСomputer + "<color=" + StandartHexColor + ">выиграл </color>";   // Color the text, add the signature
                    Panels[34].GetComponent<Text>().text = FinishedText;                                            // Assign the finished colored text displaying the winning player + signatureСomputer
                }
            }
            else                                                                            // Otherwise, if the players score equal
            {
                Panels[4].transform.GetChild(3).GetChild(0).GetComponent<Text>().text =
                    "<color=" + StandartHexColor + ">Ничья</color> ";   // If the points of the (last) player and (the last but one) are equal we write that draw
            }
        }
        

        Panels[50].SetActive(true);                                     // Turn on the window indicating who won

        if (SelectedPassageMap == 20 && GM.IsHeAHuman[GM.WinNomber] == true)  // If the player has just played the selected map & the winning player is a human)
        {
            yield return new WaitForSeconds(4);                         // We make a delay

            Panels[52].SetActive(true);                                 // Turn on the winner window at the end of the game               
        }
    }


    void NumberOfPlayers()  // This method is called from the "NumberOfPlayersButton" method and changes the number of players and rendered settings windows for them
    {
        byte TheNumberOfPlayersReceived = (byte)(NumberOfPlayersDropDown.GetComponent<Dropdown>().value + 2);    // We put the number of players in a temporary variable "TheNumberOfPlayersReceived"
        byte Difference;                    // The difference between the existing number of buttons and what should be
        byte PN = GM.NumberOfPlayers;       // How many drop-down lists exist at the moment in the list group
        byte NOPBP;                         // Number of player being processed

        if (TheNumberOfPlayersReceived > GM.NumberOfPlayers)                        // If the number of players is selected more than was before, then add new dropdown lists
        {
            Difference = (byte)(TheNumberOfPlayersReceived - GM.NumberOfPlayers);   // Calculate how many drop-down lists we need to add and write this difference to a variable "PN"
            NOPBP = (GM.NumberOfPlayers);                                           // Assign the value of the variable "NOPBP"

            for (byte a = 0; a < Difference; a++)
            {
                GameObject PLayerButton = Instantiate(TheFirstPlayerSettingButton);             // Copy the first player settings button and put it in a variable "PLayerButton"
                PLayerButton.transform.SetParent(ScrollContent.transform);                      // Adopt a PlayerButton list to a group of ScrollContent lists of player settings
                PLayerButton.transform.localScale = new Vector3(1, 1, 1);                       // Specify a forced size in case the screen size differs from the original
                PN++;                                                                           // Increase the value of the variable PN by one
                PLayerButton.name = "Button_Player_" + (PN - 1);                                // Assign a new name to the drop-down list to reflect the player's number that he describes
                PLayerButton.GetComponent<PlayerButtonScript>().NumberOfPlayer = (byte)(PN - 1);// Specify the number of the player in the script DropdownScript for which the newly created button answers
                GM.MaterialsTurn.Add(new Material(GM.Materials[NOPBP]));                        // Add a new material number to the "MaterialsTurn" list
                GM.BackMaterialsTurn.Add(new Material(GM.FigureBack));                          // Assign the background material of the figure from the list of these materials "BackMaterialsTurn"
                GM.FiguresTurn.Add(0);                                                          // Adding a new figure number to the "FiguresTurn" list
                GM.NamesTurn.Add("NoName");                                                     // Enter the player's initial name - NoName in the list of player names
                GM.ColorsTurn.Add(GM.ColorGridPanel.transform.GetChild(NOPBP).GetChild(0).GetComponent<Image>().color); // Add a starter color for this player to the list
                GM.GamePoints.Add(0);                                                           // Adding another player to the list
                GM.IsHeAHuman.Add(true);                                                        // Specify the newly created player as human
                GM.Surrendered.Add(false);                                                      // We point out that the new player has not yet surrendered
                GM.ComputerDifficulty.Add(0);                                                   // Add to the list of "ComputerDifficulty" the starting difficulty of the next player
                GM.ComputerEntityNumbers.Add(0);                                                // Add an empty computer entity number to the list.

                ScrollContent.transform.GetChild(PN -1).GetChild(2).GetComponent<Text>().text = GM.NamesTurn[PN - 1];                   // Assign the component of the drop-down list to the player's initial name
                ScrollContent.transform.GetChild(PN -1).GetChild(0).GetChild(0).GetComponent<Image>().color = GM.ColorsTurn[PN - 1];    // Assign the color of the figure in the header to the color selected for it
                ScrollContent.transform.GetChild(PN - 1).GetChild(3).GetChild(0).GetChild(0).GetComponent<Image>().color = GM.ColorsTurn[PN - 1];   // We change the selected color inside the hidden menu
                ScrollContent.transform.GetChild(PN - 1).GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().color = GM.ColorsTurn[PN - 1];   // Change the color of the figure icon inside the hidden menu
                NOPBP++;
            }
        }
        else if (TheNumberOfPlayersReceived < GM.NumberOfPlayers)                       // If the number of players is selected less than was before, then delete the dropdonw lists
        {
            Difference = (byte)(GM.NumberOfPlayers - TheNumberOfPlayersReceived);       // Calculate the difference between the number of selected players earlier and the fact that you just selected
            for (byte a = 0; a < Difference; a++)                                       // We continue the cycle until we destroy all unnecessary drop-down lists of players.
            {
                Destroy(ScrollContent.transform.GetChild(PN - 1).gameObject);           // Destroy another extra drop-down list
                PN--;                                                                   // Reduce the value of the variable "PN"
            }

            for (byte a = GM.NumberOfPlayers; a > TheNumberOfPlayersReceived; a--)
            {
                GM.NamesTurn.RemoveAt(a - 1);                       // Delete the old item from the list "NamesTurn"
                GM.ColorsTurn.RemoveAt(a - 1);                      // Delete the old item from the list "ColorsTurn"
                GM.MaterialsTurn.RemoveAt(a - 1);                   // Delete the old item from the list "MaterialsTurn"
                GM.BackMaterialsTurn.RemoveAt(a - 1);               // Delete the old item from the list "BackMaterialsTurn"
                GM.FiguresTurn.RemoveAt(a - 1);                     // Delete the old item from the list "FiguresTurn"
                GM.GamePoints.RemoveAt(a - 1);                      // Delete the old item from the list "GamePoints"
                GM.IsHeAHuman.RemoveAt(a - 1);                      // Delete the old item from the list "IsHeAHuman"
                GM.Surrendered.RemoveAt(a - 1);                     // Delete the old item from the list "Surrendered"
                GM.ComputerDifficulty.RemoveAt(a - 1);              // Delete the old item from the list "ComputerDifficulty"
                GM.ComputerEntityNumbers.RemoveAt(a - 1);           // Delete the old item from the list "ComputerEntityNumbers"
            }
        }
        GM.NumberOfPlayers = TheNumberOfPlayersReceived;            // We update the number of players in the main variable indicating the number of players
    }


    void MethodShowWalking()        // The method triggered by the event "ShowWalking". At the beginning of the game level, it shows information about the player who goes to the first time.
    {
        Panels[9].SetActive(true);                                                                              // Turn on the panel of the turn of the players

        string NamePlayer = "";                                                                                 // Here will be the name of the player + pointer if it is a computer

        if (GM.FirstCallShowWalking)                                                                            // If event "ShowWalking" was triggered for the first time.
        {
            if (GM.IsHeAHuman[GM.Turn])                                                                         // If ahuman walks
            {
                NamePlayer = "<color=" + StandartHexColor + ">" + GM.NamesTurn[GM.Turn] + "</color>";                   // Enter the player’s colored nickname in the “NamePlayer” variable
                Panels[9].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = NamePlayer;                     // We display the nickname of the player whose turn came
            }
            else                                                                                                        // Otherwise, if the computer is walks now
            {
                NamePlayer = "<color=" + StandartHexColor + ">" + GM.NamesTurn[GM.Turn] + "\n (Компьютер)</color>";     // Add the inscription computer to the player’s name
                Panels[9].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = NamePlayer;                     // We pass the name of the player in a string showing his name
            }

            // We load a picture of a figure of the corresponding player
            ShowingFigure.sprite = Panels[8].transform.GetChild(2).GetChild(0).GetChild(GM.FiguresTurn[GM.Turn]).GetChild(0).GetComponent<Image>().sprite;  
            ShowingFigure.color = GM.ColorsTurn[GM.Turn];                                                               // Color the picture in the corresponding color of the player
        }
        else                                                                                                            // Otherwise if we show not the first player
        {
            if (GM.IsHeAHuman[GM.Turn + 1])                                                                             // If a human walks
            {
                NamePlayer = "<color=" + StandartHexColor + ">" + GM.NamesTurn[GM.Turn + 1] + "</color>";               // Enter the player’s colored nickname in the “NamePlayer” variable
                Panels[9].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = NamePlayer;                     // We display the nickname of the player whose turn came
            }
            else                                                                                                        // Otherwise, if the computer is walks now
            {
                NamePlayer = "<color=" + StandartHexColor + ">" + GM.NamesTurn[GM.Turn + 1] + "\n (Компьютер)</color>"; // Add the inscription computer to the player’s name
                Panels[9].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = NamePlayer;                     // We display the nickname of the player whose turn came
            }

            // We load a picture of a figure of the corresponding player
            ShowingFigure.sprite = Panels[8].transform.GetChild(2).GetChild(0).GetChild(GM.FiguresTurn[GM.Turn + 1]).GetChild(0).GetComponent<Image>().sprite;  
            ShowingFigure.color = GM.ColorsTurn[GM.Turn + 1];                                                                           // Color the picture in the corresponding color of the player
        }

        Panels[9].GetComponent<Animator>().SetBool("ScreenOutput", false);                                          // Specify that "SequenceOfPlayers_Panel" should go to the screen
        StartCoroutine(MethodShowWalkingСontinuation());                                                            // Call the coroutine "MethodShowWalkingContinuation" 
    }
    IEnumerator MethodShowWalkingСontinuation()                                                                     // Continuation of the method "MethodShowWalking"
    {
        yield return new WaitForSeconds(2);                                                                     // Make a delay
        Panels[9].GetComponent<Animator>().SetBool("ScreenOutput", true);                                       // Specify that "SequenceOfPlayers_Panel" should leave the screen
    }


    void ZoomRollback(bool Zoom)                                                            // Method called from the "ZoomRollbackButtons" method. It is called when you need to increase or decrease the screen of the game.
    {
        Vector3 FlagScale = Panels[41].transform.localScale;                                // Icon flag scale
        Vector3 ArrowsScale = Panels[10].transform.GetChild(1).transform.localScale;        // Icon arrows scale
        Vector3 RedDotScale = Panels[10].transform.GetChild(2).transform.localScale;        // RedDotScale
        Vector3 BlackDotScale = Panels[10].transform.GetChild(3).transform.localScale;      // BlackDotScale
        float ISF = 0.2f;                                                                   // Icons Scale Factor
        float SSF = 0.1f;                                                                   // Sliders Scale Factor                    
        byte ScaleStep = 5;                                                                 // The angle at which the field of view of the camera decreases or increases.

        if (Zoom)                                                                           // If this is a screen zoom button
        {
            if (Camera.main.fieldOfView > 25)                                               // If the field of view of the camera is more than 25 degrees
            {
                Camera.main.fieldOfView -= ScaleStep;                                       // Reduce the field of view of the camera on the "ScaleStep"
                Panels[41].transform.localScale = new Vector3(FlagScale.x + ISF, FlagScale.y + ISF, FlagScale.z + ISF);                         // Increase the flag icon
                Panels[10].transform.GetChild(1).transform.localScale = new Vector3(FlagScale.x + ISF, FlagScale.y + ISF, FlagScale.z + ISF);   // Increase the icon of four arrows
                Panels[10].transform.GetChild(2).transform.localScale = new Vector3(FlagScale.x + ISF, FlagScale.y + ISF, FlagScale.z + ISF);   // Increase the red dot icon
                Panels[10].transform.GetChild(3).transform.localScale = new Vector3(FlagScale.x + ISF, FlagScale.y + ISF, FlagScale.z + ISF);   // Increase the black dot icon
                Panels[12].transform.GetChild(0).GetComponent<Scrollbar>().size -= SSF;     // Reduce the size of the vertical scroll bar
                Panels[12].transform.GetChild(1).GetComponent<Scrollbar>().size -= SSF;     // Reduce the size of the horizontal scroll bar
                ZoomStep += 1;                                                              // Increase the value of the variable "ZoomStep" by one unit.
                VerticalHorizontalScrollbarButtons(VerticalScrollbar);                             // Call the "VerticalHorizontalScrollbarButtons" method to update the position of the camera and slider along the Y axis
                VerticalHorizontalScrollbarButtons(HorizontalScrollbar);                           // Call the "VerticalHorizontalScrollbarButtons" method to update the position of the camera and slider along the X axis
            }
        }
        else                                                                                // Otherwise, if it is a screen down button
        {
            if (Camera.main.fieldOfView < 60)                                               // If the field of view of the camera is less than 60 degrees
            {
                Camera.main.fieldOfView += ScaleStep;                                       // Increase the field of view of the camera on "ScaleStep"
                Panels[41].transform.localScale = new Vector3(FlagScale.x - ISF, FlagScale.y - ISF, FlagScale.z - ISF);  // Reduce the flag icon
                Panels[10].transform.GetChild(1).transform.localScale = new Vector3(FlagScale.x - ISF, FlagScale.y - ISF, FlagScale.z - ISF);  // Reduce the icon of four arrows
                Panels[10].transform.GetChild(2).transform.localScale = new Vector3(FlagScale.x - ISF, FlagScale.y - ISF, FlagScale.z - ISF);  // Reduce the red dot icon
                Panels[10].transform.GetChild(3).transform.localScale = new Vector3(FlagScale.x - ISF, FlagScale.y - ISF, FlagScale.z - ISF);  // Reduce the black dot icon
                Panels[12].transform.GetChild(0).GetComponent<Scrollbar>().size += SSF;     // Increase the size of the vertical scroll bar
                Panels[12].transform.GetChild(1).GetComponent<Scrollbar>().size += SSF;     // Increasing the size of the horizontal scroll bar
                ZoomStep -= 1;                                                              // Decrease the value of the variable "ZoomStep" by one unit.
                VerticalHorizontalScrollbarButtons(VerticalScrollbar);              // Call the "VerticalHorizontalScrollbarButtons" method to update the position of the camera and slider along the Y axis
                VerticalHorizontalScrollbarButtons(HorizontalScrollbar);            // Call the "VerticalHorizontalScrollbarButtons" method to update the position of the camera and slider along the X axis
            }
        }
        if (CM.SelPo.Count != 0) UpdateIconsPosition();                   // If the line has the first point, call the method to update the position of the icons
    }


    void VerticalHorizontalScrollbar(GameObject Scrollbar)                          // This method calls from the "VerticalHorizontalScrollbarButtons" method and moves the screen from side to side.
    {
        float IncreaseByStepX = (60f / 7f);                                         // IncreaseByStepX
        float IncreaseByStepY = (108f / 7f);                                        // IncreaseByStepY
        float CMPx = ((IncreaseByStepX * ZoomStep) * 2);                            // X-axis camera motion potential
        float CMPy = ((IncreaseByStepY * ZoomStep) * 2);                            // Y-axis camera motion potential
        float CameraPosition;                                                       // Calculated camera position

        if (Scrollbar.name == "Scrollbar_Up-Down")                                  // If this is a vertical slider
        {
            CameraPosition = (CMPy * Scrollbar.GetComponent<Scrollbar>().value);    // Потенциал движения камеры умножаем на значение скроллбара получая относительное положение камеры в своём потенциале
            CameraPosition = (CameraPosition - (CMPy / 2));                         // Получаем положение камеры учитывая что она может быть как в плюсовоп положении так и в минусовом
            Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, CameraPosition, Camera.main.transform.localPosition.z); // Присваиваем камере высчитанное положение
        }
        else                                                                        // Otherwise, if it is a horizontal slider
        {
            CameraPosition = (CMPx * Scrollbar.GetComponent<Scrollbar>().value);    // Потенциал движения камеры умножаем на значение скроллбара получая относительное положение камеры в своём потенциале
            CameraPosition = (CameraPosition - (CMPx / 2));                         // Получаем положение камеры учитывая что она может быть как в плюсовоп положении так и в минусовом
            Camera.main.transform.localPosition = new Vector3(CameraPosition, Camera.main.transform.localPosition.y, Camera.main.transform.localPosition.z); // Присваиваем камере высчитанное положение
        }
        if (CM.SelPo.Count != 0) UpdateIconsPosition();                   // If the line has the first point, call the method to update the position of the icons
    }


    void EditLine(int State)                                                        // Method called from the "EditLineButton" method. He is needed to edit the line in the game.
    {
        if (State == 1)                                                             // If the "StepBack" button was pressed
        {
            if (CM.ActiveLinePoint > 0)                                             // If the line still has somewhere to shorten back
            {
                CM.ActiveLinePoint--;                                                                               // Decrease the active point by one
                CM.LRs[CM.NumberLine].GetComponent<LineRenderer>().positionCount--;                                 // Remove the last point from the component of Linerenderer
                GameObject.Destroy(CM.Empty[CM.Empty.Count - 1]);                                                   // Remove empties with line segment collider
                CM.Empty.RemoveAt(CM.Empty.Count - 1);                                                              // Remove a empties with a line segment collider from the list of empties
                TheLineLengthHasBeenChanged();                                                                      // Call the method that changes the position of the point at the end of the line.

                if (Panels[11].transform.GetChild(1).GetComponent<Button>().interactable == false)                  // If the "Step Forward" button was inactive
                {
                    Panels[11].transform.GetChild(1).GetComponent<Button>().interactable = true;                    // Making the button step forward again interactble
                    Panels[11].transform.GetChild(3).GetComponent<Button>().interactable = true;                    // Making the button step forward interactble
                }
                if (CM.ActiveLinePoint == 0)                                                                        // If the value of the variable "Active Point" is zero
                {
                    Panels[10].transform.GetChild(2).gameObject.SetActive(false);                                   // Make the red dot inactive.
                    HideFlashingFourArrows = false;                                                                 // Set the value of the HideFlashingFourArrows variable to false so that the cross appears immediately when calling the CallTheFirstPointIsSet method
                    GM.CallTheFirstPointIsSet();                                                                    // Call the event "CallTheFirstPointIsSet"
                }
            }
            else                                                                                                    // If this is the very first point in the line
            {
                LastStepBack();                 // Call the method to take the last step back in line editing.
            }
        }
        else if (State == 2)                    // If the "StepForward" button was pressed
        {
            StepForward();                      // Call the line editing method one step further.

            if (CM.ActiveLinePoint == CM.SelPo.Count - 1)                                                 // If this is the last existing point in the line
            {
                Panels[11].transform.GetChild(1).GetComponent<Button>().interactable = false;                       // Making the button a "StepForward" not interacting
                Panels[11].transform.GetChild(3).GetComponent<Button>().interactable = false;                       // Making the button a "JumpForward" not interacting
            }
        }
        else if (State == 3)                    // If the "JumpBack" button was pressed
        {
            if (GameObject.FindGameObjectWithTag("LineRendererGroup").transform.Find("BorderLine " + CM.NumberLine) != null)   // If current line exists
            {
                if (CM.ActiveLinePoint > 0)     // If the line still has somewhere to shorten back
                {
                    CM.ActiveLinePoint = 0;                                                     // Set value "ActiveLinePoint" as zero
                    CM.LRs[CM.NumberLine].positionCount = 1;                                    // Remove all points from the "Linerenderer" component except the first

                    for (; 0 < CM.Empty.Count;)                                                 // Continue the cycle until we remove all the dummies on which the colliders of the outlined line hang
                    {
                        GameObject.Destroy(CM.Empty[CM.Empty.Count - 1]);                       // Remove empties with line segment collider
                        CM.Empty.RemoveAt(CM.Empty.Count - 1);                                  // Remove a empties with a line segment collider from the list of empties
                    }
                    Panels[11].transform.GetChild(1).GetComponent<Button>().interactable = true;// Making the button step forward again interactble
                    Panels[11].transform.GetChild(3).GetComponent<Button>().interactable = true;// Making the button step forward interactble
                    Panels[43].SetActive(false);                                                // Make the red dot inactive.
                    Panels[44].SetActive(false);                                                // Make the black dot inactive.
                    HideFlashingFourArrows = false;                                             // Set the value of the HideFlashingFourArrows variable to false so that the cross appears immediately when calling the CallTheFirstPointIsSet method
                    GM.CallTheFirstPointIsSet();                                                // Call the event "CallTheFirstPointIsSet"
                }
                else                                                                            // If this is the very first point in the line
                {
                    LastStepBack();                                                             // Call the method to take the last step back in line editing.
                }
            }
            else                                                                                // Otherwise, if the current line does not exist
            {
                Panels[11].SetActive(false);                                                    // Turn off Editing Line Panel
                Panels[12].SetActive(false);                                                    // Turn off Zoom Panel
                Panels[30].SetActive(false);                                                    // Turn off Open/Close_LineEditingPanel_Button
                Panels[31].SetActive(false);                                                    // Turn off Open/Close_ZoomPanel_Button

                Panels[30].GetComponent<Button>().interactable = true;                          // Making the button interactble Open/Close_LineEditingPanel_Button
                Panels[31].GetComponent<Button>().interactable = true;                          // Making the button interactble Open/Close_ZoomPanel_Button
                Panels[32].GetComponent<Button>().interactable = true;                          // Making the button interactble Open/Close_GameMenu_Button

                Panels[2].transform.gameObject.SetActive(true);                                 // Activate the panel "ButtonScheme"
                if (CM.NumberLines == 0)                                                        // If there is not a single line
                {
                    Panels[2].transform.GetChild(0).gameObject.SetActive(false);                // Disable the "ContinueDrawingButton" button
                    Panels[2].transform.GetChild(2).gameObject.SetActive(true);                 // Turn on the button "Draw a limiter"
                }
                CM.LineHasBeenDelineated = false;                                               // We note that this line was not finished.
            }
        }
        else if (State == 4)                                                                     // If the "JumpForward" button was pressed
        {
            for (; CM.ActiveLinePoint < CM.SelPo.Count - 1;)
            {
                StepForward();                                                                  // Call the line editing method one step further.
            }
            Panels[11].transform.GetChild(1).GetComponent<Button>().interactable = false;       // Making the button a step forward not interacting
            Panels[11].transform.GetChild(3).GetComponent<Button>().interactable = false;       // Making the button a "JumpForward" not interacting
        }
    }


    void LastStepBack() // The method is called when the last step is made backwards in editing the line destroying it. (It is part of the "EditLine" method)
    {
        Debug.Log("Метод был вызван");
        for (byte a = 0; a < 4; a++)                                                                        // Continue the cycle until we make all the line editing buttons except the jump back non-interacting
        {
            if (a != 2) Panels[11].transform.GetChild(a).GetComponent<Button>().interactable = false;       // If this is not a button, jump back make it non interactble.                                                            
        }

        Panels[41].SetActive(false);                                                                        // Turn off the flag
        Panels[43].SetActive(false);                                                                        // Make the red dot inactive.
        Panels[45].SetActive(false);                                                                        // Turn off the icon "PushedDotCollider"
        Panels[45].GetComponent<RectTransform>().position = new Vector3(-20, -25, 0);                       // Set the "PushedDotCollider" icon to its original place.
        HideFlashingFourArrows = true;                                                                      // Turn off the crosshair
        Destroy(GameObject.FindGameObjectWithTag("GhostLine"));                                             // Remove the ghost line
        Destroy(GameObject.FindGameObjectWithTag("LineRendererGroup").transform.Find("BorderLine " + (CM.NumberLines - 1)).gameObject);  // Destroy the last line we tried to draw
        CM.LineRenderersGOs.RemoveAt(CM.LineRenderersGOs.Count - 1);                                        // Remove the object with a line from the List "LineRenderersGOs"
        CM.SelPo.Clear();                                                                                   // Clear the "SelPo" List
        CM.LRs.RemoveAt(CM.LRs.Count - 1);                                                                  // Remove the last component "LineRenderer" from the list "LRs"
        CM.NumberLines--;                                                                                   // Indicate that the existing number of lines has decreased by 1
    }


    void StepForward()              // This method is called when you need to edit a line a step further. (It is part of the "EditLine" method)
    {
        BoxCollider BoxCol;                                                                                     // Переменная для коллайдера
        Ray raypoz = new Ray();                                                                                 // Создаём новый луч
        CM.ActiveLinePoint++;                                                                                   // Increase the active point by one
        CM.LRs[CM.NumberLine].positionCount++;                                                                  // We extend the number of points in LineRenderer by one
        CM.LRs[CM.NumberLine].SetPosition(CM.ActiveLinePoint, CM.SelPo[CM.ActiveLinePoint]);          // The last point from the list is entered into the last point of the LineRenderer.
        CM.Empty.Add(new GameObject());                                                                         // Create a GameObject "Empty" for the BoxCollider component
        BoxCol = CM.Empty[CM.Empty.Count - 1].AddComponent<BoxCollider>();                                      // Create a BoxCollider and assign it to a empty
        CM.Empty[CM.Empty.Count - 1].transform.SetParent(CM.BorderLine.transform);                              // We adopt the next empty LineRenderer object
        CM.Empty[CM.Empty.Count - 1].name = "BorderLineCollider";                                               // We give this empty the name "BorderLineCollider"

        if (CM.NumberLines == 1) CM.Empty[CM.Empty.Count - 1].tag = "OutsideLine";                              // If this is the first line Then we give it to the collider the tag "Outside line"
        else CM.Empty[CM.Empty.Count - 1].tag = "InsideLine";                                                   // Otherwise, if this is not the first line Give her a name "InsideLine"    

        raypoz.origin = CM.SelPo[CM.ActiveLinePoint];                                                                     // Take the active point from the list of points and assign it as the center of the beam
        raypoz.direction = Vector3.Normalize(CM.SelPo[CM.ActiveLinePoint - 1] - CM.SelPo[CM.ActiveLinePoint]);  // Calculate the direction of the beam
        CM.Empty[CM.Empty.Count - 1].transform.position = raypoz.GetPoint(5);                                   // Calculate the location for the new collider

        if (CM.SelPo[CM.ActiveLinePoint].x != CM.SelPo[CM.ActiveLinePoint - 1].x)           // If the active point and the point behind it in the "SelPo" list are different along the "X" axis.
            BoxCol.size = new Vector3(11, 1, 1);                                                                // Then we make the collider long on the axis "X"
        else                                                                                                    // Otherwise, if the active point and the point behind it in the "SelPo" list are different along the "Z" axis.
            BoxCol.size = new Vector3(1, 1, 11);                                                                // Then we make the collider long along the "Z" axis
        TheLineLengthHasBeenChanged();                                                                          // Call the method that changes the position of the point at the end of the line.
    }


    void UpdateIconsPosition()  // This method updates the icon positions in the zoom menu.
    {
        Panels[41].transform.position = Camera.main.WorldToScreenPoint(CM.SelPo[0]);                                // Update the position of the flag on the screen
        Panels[10].transform.GetChild(1).position = Camera.main.WorldToScreenPoint(CM.SelPo[0]);                    // Update the position of the four arrows on the screen
        Panels[10].transform.GetChild(2).position = Camera.main.WorldToScreenPoint(CM.SelPo[CM.SelPo.Count - 1]);   // Assign a red dot to a position at the end of the line.
        Panels[10].transform.GetChild(3).position = Camera.main.WorldToScreenPoint(CM.SelPo[CM.SelPo.Count - 1]);   // Assign a black dot to a position at the end of the line.
    }


    public void LoadScreenshotsOfMaps()    // This method loads custom screenshots of maps. Screenshots viewing windows in the map selection menu.
    {
        Panels[13].SetActive(true);                                                                                     // Turn on the map selection panel

        if (ListOfPassageMapScreenshots.Count > 0)                                                                      // If there are maps in the list of screenshots user maps
        {
            SelectedPassageMap = 1;                                                                                     // We indicate the number of the "SelectedPassageMap" as 1
            Panels[19].GetComponent<Image>().color = new Color32(255, 255, 255, 0);                                     // Making the button image clean

            Panels[22].transform.GetChild(1).GetComponent<Text>().text = SelectedPassageMap.ToString();                 // Specify the number of the active passage map
            Panels[22].transform.GetChild(3).GetComponent<Text>().text = ListOfPassageMapScreenshots.Count.ToString();  // Specify the number of the active passage map
            Panels[21].GetComponent<RawImage>().texture = ListOfPassageMapScreenshots[SelectedPassageMap - 1];          // Set the passage map screenshot number corresponding to the number of the active map

            if (ListOfPassageMapScreenshots.Count > 1 && SelectedPassageMap < ListOfPassageMapScreenshots.Count)        // If we have more than one passage map and not the last map is selected
                Panels[13].transform.GetChild(3).GetChild(0).GetComponent<Button>().interactable = true;                // We turn on the button chose passage map to the right
            else if(SelectedPassageMap == ListOfPassageMapScreenshots.Count)                                            // If the number of the active passage map is equal to the last map
                Panels[13].transform.GetChild(3).GetChild(0).GetComponent<Button>().interactable = false;               // Turn off the button passage map to the right

            if (SelectedPassageMap == 1)                                                                                // If the first map was selected
                Panels[13].transform.GetChild(3).GetChild(1).GetComponent<Button>().interactable = false;               // Disable for interaction - backward button

            Panels[19].GetComponent<Button>().interactable = true;                                                      // Turn on the button "DownloadThePassageMap_Button" 
        }

        if (SL.GetNumMapScreen.Count > 0)                                                                               // If there is a save object and in it a list of maps
        {
            if (JustUploaded)               // If the map selection window was just loaded
                SelectedCustomMap = 1;      // We indicate the number of the selected map as 1

            Panels[20].transform.GetChild(0).gameObject.SetActive(false);                                               // Disable text stating that screenshots of custom maps were not found.
            Panels[15].transform.GetChild(1).GetComponent<Text>().text = SelectedCustomMap.ToString();                  // Specify the number of the active custom map                                                                         //            Panels[22].transform.GetChild(1).GetComponent<Text>().text = SelectedPassageMap.ToString();         // Specify the number of the active passage map
            Panels[15].transform.GetChild(3).GetComponent<Text>().text = ListOfCustomMapScreenshots.Count.ToString();   // Specify the total number of custom maps
            Panels[20].GetComponent<RawImage>().texture = ListOfCustomMapScreenshots[SelectedCustomMap - 1];    // Set the custom map screenshot number corresponding to the number of the active map

            if (SL.GetNumMapScreen.Count > 1 && SelectedCustomMap < ListOfCustomMapScreenshots.Count)           // If we have more than one custom map and not the last map is selected
                Panels[13].transform.GetChild(3).GetChild(2).GetComponent<Button>().interactable = true;        // We turn on the button custom map to the right
            else if(SelectedCustomMap == ListOfCustomMapScreenshots.Count)                                      // If the number of the active custom map is equal to the last map
                Panels[13].transform.GetChild(3).GetChild(2).GetComponent<Button>().interactable = false;       // Turn off the button custom map to the right

            if (SelectedCustomMap == 1)                                                                         // If the first map was selected
                Panels[13].transform.GetChild(3).GetChild(3).GetComponent<Button>().interactable = false;       // Disable for interaction - backward button

            Panels[16].GetComponent<Button>().interactable = true;                                              // Turn on the "DownloadTheCustomMapButton" 
        }
        else                                                                                                            // If we don’t have more than one saved map
        {
            SelectedCustomMap = 0;                                                                                      // We indicate the number of the selected user map 0                                
            Panels[15].transform.GetChild(1).GetComponent<Text>().text = SelectedCustomMap.ToString();                  // Specify the number of the active map
            Panels[15].transform.GetChild(3).GetComponent<Text>().text = ListOfCustomMapScreenshots.Count.ToString();   // Specify the total number of custom maps
            Panels[20].GetComponent<RawImage>().texture = null;                                                         // Zeroing the image containing the photo of the selected map
            Panels[13].transform.GetChild(2).GetChild(0).GetChild(0).gameObject.SetActive(true);                        // Include text indicating that no maps were found 
            Panels[16].GetComponent<Button>().interactable = false;                                                     // Turn off the "DownloadTheCustomMapButton" 
            Panels[25].GetComponent<Button>().interactable = false;                                                     // Turn off the delete map button
        }
    }


    void ContinueDrawing()                                              // This method starts line drawing.
    {
        GM.CallDrawTheFollowingLine();                                  // Call the method causing the event "draw the next line"
        Panels[2].SetActive(false);                                     // Выключаем панель кнопок схемы    
        Panels[11].GetComponentInChildren<Button>().interactable = false;
        Panels[30].SetActive(true);                                     // Turn on the button to activate / deactivate the line editing panel
        Panels[31].SetActive(true);                                     // Turn on the button to activate / deactivate the screen zoom panel
        Panels[32].SetActive(true);                                     // Turn on the button to activate / deactivate GameMenu
        CM.DrawingALineIsProhibited = false;                            // We indicate that we allow drawing a line
        Panels[2].transform.GetChild(0).gameObject.SetActive(true);                     // Turn on the button "ContinueDrawingButton" 
        Panels[2].transform.GetChild(2).gameObject.SetActive(false);                    // Disable the "Draw a limiter" button
        Panels[11].transform.GetChild(2).GetComponent<Button>().interactable = true;    // Make the button "Jump back" interactable
        Panels[27].transform.GetChild(0).GetComponent<Button>().interactable = false;   // Making the "Save Game" button in the "Game Menu" Non interactable
    }


    void NextAndPreviousPassageMap(int Button)                          // This method switches the active passage map.
    {
        if (Button == 0)                                                // If the right forward was pressed
        {
            SelectedPassageMap++;                                                                                                   // Increment the active passage map number by one.
            Panels[22].transform.GetChild(1).GetComponent<Text>().text = (SelectedPassageMap).ToString();                           // Update the displayed number of the active passage map
            Panels[21].GetComponent<RawImage>().texture = ListOfPassageMapScreenshots[SelectedPassageMap - 1];                      // Update the displayed active passage map screenshot

            if (SelectedPassageMap > (SL.SLO.Progress + 1))                                 // If this map has not yet been passed
            {
                Panels[19].GetComponent<Image>().color = new Color32(255, 255, 255, 255);   // Making the button image blurred
                Panels[19].GetComponent<Button>().interactable = false;                     // Disable map download button
            }
            else
            {
                Panels[19].GetComponent<Image>().color = new Color32(255, 255, 255, 0);     // Making the button image clean again
                Panels[19].GetComponent<Button>().interactable = true;                      // Enable map download button
            }

            Panels[13].transform.GetChild(3).GetChild(1).GetComponent<Button>().interactable = true;                                // Enable for interaction - backward passage map button

            if (SelectedPassageMap == ListOfPassageMapScreenshots.Count)                                                            // If this is the last passage map in the list
                Panels[13].transform.GetChild(3).GetChild(0).GetComponent<Button>().interactable = false;                           // Disable for interaction - forward passage map button
        }
        else if (Button == 1)               // If the backward button was pressed      
        {
            SelectedPassageMap--;                                                                                                   // Reduce the number of the active passage map by 1
            Panels[22].transform.GetChild(1).GetComponent<Text>().text = (SelectedPassageMap).ToString();                           // Update the displayed number of the active passage map
            Panels[21].GetComponent<RawImage>().texture = ListOfPassageMapScreenshots[SelectedPassageMap - 1];                      // Update the displayed active passage map screenshot

            if (SelectedPassageMap > (SL.SLO.Progress + 1))                                      // If this map has not yet been passed
            {
                Panels[19].GetComponent<Image>().color = new Color32(255, 255, 255, 255);   // Making the button image blurred
                Panels[19].GetComponent<Button>().interactable = false;                     // Disable map download button                                                                          
            }
            else
            {
                Panels[19].GetComponent<Image>().color = new Color32(255, 255, 255, 0);     // Making the button image clean again
                Panels[19].GetComponent<Button>().interactable = true;                      // Enable map download button
            }

            Panels[13].transform.GetChild(3).GetChild(0).GetComponent<Button>().interactable = true;                                // Enable for interaction - forward passage map button

            if (SelectedPassageMap == 1)                                                                                            // If this is the first passage map in the list   
                Panels[13].transform.GetChild(3).GetChild(1).GetComponent<Button>().interactable = false;                           // Disable for interaction - backward passage map button
        }
    }

    void NextAndPreviousСustomMap(int Button)                       // This method switches the custom active user map.
    {
        if (Button == 0)                                                                                                            // If the right forward was pressed
        {
            SelectedCustomMap++;                                                                                                    // Increment the active custom map number by one.
            Panels[15].transform.GetChild(1).GetComponent<Text>().text = (SelectedCustomMap).ToString();                            // Update the displayed number of the active custom map
            Panels[20].GetComponent<RawImage>().texture = ListOfCustomMapScreenshots[SelectedCustomMap - 1];                        // Update the displayed active custom map screenshot
            Panels[13].transform.GetChild(3).GetChild(3).GetComponent<Button>().interactable = true;                                // Enable for interaction - backward custom map button

            if (SelectedCustomMap == ListOfCustomMapScreenshots.Count)                                                              // If this is the last custom map in the list
                Panels[13].transform.GetChild(3).GetChild(2).GetComponent<Button>().interactable = false;                           // Disable for interaction - forward custom map button
        }
        else if (Button == 1)                                                                                                       // If the backward button was pressed        
        {
            SelectedCustomMap--;                                                                                                    // Reduce the number of the active custom map by 1
            Panels[15].transform.GetChild(1).GetComponent<Text>().text = (SelectedCustomMap).ToString();                            // Update the displayed number of the active custom map
            Panels[20].GetComponent<RawImage>().texture = ListOfCustomMapScreenshots[SelectedCustomMap - 1];                        // Update the displayed active custom map screenshot
            Panels[13].transform.GetChild(3).GetChild(2).GetComponent<Button>().interactable = true;                                // Enable for interaction - forward custom map button

            if (SelectedCustomMap == 1)                                                                                             // If this is the first custom map in the list
                Panels[13].transform.GetChild(3).GetChild(3).GetComponent<Button>().interactable = false;                           // Disable for interaction - backward custom map button
        }
    }

    void MethodLoadGameLevel()                                  // This method is called by the load game level event.
    {
        if (SL.LoadPassageMap)                                  // If a player loads a passing level
        {
            SL.DownloadableLevelNumber = SelectedPassageMap;    // Specify the number of the loading level from the selected level of passage
        }
        else if (!SL.LoadPassageMap)                            // Otherwise, if the player loads the user level
        {
            SL.DownloadableLevelNumber = SelectedCustomMap;     // Specify the number of the loading level from the selected user level
            Panels[13].SetActive(false);                        // Turn off the level selection window
            Panels[6].SetActive(false);                         // Turn off the level setting window
        }
    }

    void RunPassageMap()                                        // This method is called by clicking on the passage map.
    {
        bool HaveAPlayer = false;               // The variable indicates whether the player
        bool HaveAComputerAdversary = false;    // The variable indicates whether there is a computer adversary

        for (int i = 0; i < 2; i++)             // We go through the cycle two times
        {
            if (GM.IsHeAHuman[i] == true)       // If we find a human player
                HaveAPlayer = true;             // We indicate in the variable that the human player exists

            if (GM.IsHeAHuman[i] == false)      // If we found a computer Adversary
                HaveAComputerAdversary = true;  // We indicate in the variable that we found a computer adversary
        }

        if (GM.NumberOfPlayers == 2)                    // If the number of players is two
        {
            if (HaveAPlayer && HaveAComputerAdversary)  // If there is a player and a computer opponent                             
            {
                Panels[18].SetActive(true);             // Call the passage map download confirmation window
            }
            else if (!HaveAPlayer)                      // If there is no player
            {
                Panels[26].SetActive(true);             // Call the warning window
                Panels[26].transform.GetChild(2).GetComponent<Text>().text = "У вас в качестве обоих игроков выбранны компьютерные противники. Для прохождения игры один из игроков должен быть человеком.";   // Display message
            }
            else if (!HaveAComputerAdversary)           // If there is no computer opponent
            {
                Panels[26].SetActive(true);             // Call the warning window
                Panels[26].transform.GetChild(2).GetComponent<Text>().text = "У вас в качестве обоих игроков выбранны люди. Для прохождения игры противник должен быть компьютером.";     // Display message
            }
        }
        else if (GM.NumberOfPlayers > 2)                // If the number of players is more than two
        {
            Panels[26].SetActive(true);                 // Call the warning window
            Panels[26].transform.GetChild(2).GetComponent<Text>().text = "Для режима прохождения допускается только 2 игрока: \n 1 - Человек, \n 2 - Компьютерный противник."; // Display message
        }
    }

    IEnumerator CallLoadLevel()                                         // This coroutine triggers a map download event, and later a game start event.
    {
        Panels[18].SetActive(false);                                    // Turn off the map confirmation window for passing the map
        Panels[17].SetActive(false);                                    // Turn off the user map loading confirmation window

        GM.CallLoadGameLevel();                                         // Call the caller method load level event
        yield return new WaitForSeconds(DelayForCameraMovement);        // We make a delay

        GM.CallPlayGame();                                              // Call the method causing the event "Play the current drawing"

        Panels[2].SetActive(false);                                     // Turn off the button panel scheme  
        Panels[10].SetActive(true);                                     // Turn on the panel "CreatingBorderField"
        Panels[35].GetComponent<Button>().interactable = false;         // Lock the button to play the next level
    }


    public void OpenASelectionOfColors()                // This method is called from the button method "OpenASelectionOfColorsButton"
    {
        Panels[7].SetActive(true);                      // Turn on the color picker panel

        for (int i = 0; i < GM.NumberOfPlayers; i++)    // We continue the cycle until we sort through all the existing buttons of the players
        {
            Panels[33].transform.GetChild(i).GetComponent<GraphicRaycaster>().enabled = false;  // Turn off the "GraphicRaycaster" component at the next player button
        }
    }


    IEnumerator ColorSelected()                         // This method is called upon pressing any of the buttons that select the color of the player's figure
    {
        Panels[7].SetActive(false);                     // Deactivate the color picker panel

        yield return new WaitForSeconds(0.1f);          // We are waiting for 0.1 second

        for (int i = 0; i < GM.NumberOfPlayers; i++)    // We continue the cycle until we sort through all the existing buttons of the players
        {
            // We turn off the “GraphicRaycaster” component on the next player settings button so that after choosing the figure “accidentally” the button under the mouse would not be activated
            Panels[33].transform.GetChild(i).GetComponent<GraphicRaycaster>().enabled = true;
        }
    }


    void OpenASelectionOfFigures()                      // This method is called from the button method "OpenASelectionOfFiguresButton"
    {
        byte NoP;                                                                       // Variable "Nomber of player"

        NoP = LastDDObject.GetComponent<PlayerButtonScript>().NumberOfPlayer;           // We put into the variable "Nop" the player number for which we select the color
        GameObject PlayerButton = ScrollContent.transform.GetChild(NoP).gameObject;     // We put a sample of the player’s button in the "PlayerButton" variable

        for (byte i = 0; i < 8; i++)                    // We continue the cycle until we go through all the buttons with figures
        {
            Panels[51].transform.GetChild(i).GetChild(0).GetComponent<Image>().color = PlayerButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().color;
        }

        Panels[8].SetActive(true);                      // Activate the figure selection bar

        for (int i = 0; i < GM.NumberOfPlayers; i++)    // We continue the cycle until we sort through all the existing buttons of the players
        {
            Panels[33].transform.GetChild(i).GetComponent<GraphicRaycaster>().enabled = false;  // Turn off the "GraphicRaycaster" component at the next player button
        }
    }

    IEnumerator FigureSelected()                            // Make a move with a simple algorithm
    {
        Panels[8].SetActive(false);                         // Deactivate the figure selection panel

        yield return new WaitForSeconds(0.1f);              // We are waiting for 0.1 second

        for (int i = 0; i < GM.NumberOfPlayers; i++)        // We continue the cycle until we sort through all the existing buttons of the players
        {
            // We turn off the “GraphicRaycaster” component on the next player settings button so that after choosing the figure “accidentally” the button under the mouse would not be activated
            Panels[33].transform.GetChild(i).GetComponent<GraphicRaycaster>().enabled = true;  
        }
    }


    public IEnumerator MapSavedNotice()         // This coroutine is called when the map is saved.
    {
        Panels[47].SetActive(true);             // Turn on the notification that the card was saved
        yield return new WaitForSeconds(3);     // Awaiting
        Panels[47].SetActive(false);            // Turn on the notification that the card was saved
    }


    public IEnumerator GameBlockNotice()        // This coroutine shows a warning "close all extraneous windows to continue the game"
    {
        Panels[49].SetActive(true);             // Turn on the window warning about blocking the game
        yield return new WaitForSeconds(3);     // Awaiting
        Panels[49].SetActive(false);            // Turn off the window warning about blocking the game
    }


    void OpenCloseEditingPanel()                // This method is called from the button method "OpenCloseEditingPanelButton"
    {
        if (Panels[11].activeSelf == false)     // If the line editing panel has been disabled and we clicked to turn it on
        {
            Panels[11].SetActive(true);         // Turn on "EditingPanel"
            Panels[31].GetComponent<Button>().interactable = false; // Bock the button showing line editing elements
            Panels[32].GetComponent<Button>().interactable = false; // Block the button showing the game menu
            Panels[48].SetActive(true);         // Making the alert button active
            CM.DrawingALineIsProhibited = true; // We indicate that we prohibit drawing a line
        }
        else                                    // Otherwise, if it was enabled
        {
            Panels[11].SetActive(false);        // Turn it off
            Panels[31].GetComponent<Button>().interactable = true; // Unbock the button showing line editing elements
            Panels[32].GetComponent<Button>().interactable = true; // Unblock the button showing the game menu
            Panels[48].SetActive(false);        // Making the alert button inactive
            CM.DrawingALineIsProhibited = false;// We indicate that we allow drawing a line
        }
    }
    

    void OpenCloseZoomPanel()                   // This method is called from the button method "OpenCloseZoomPanelButton"
    {
        if (Panels[12].activeSelf == false)     // If the screen zoom panel is disabled and we clicked to turn it on
        {
            Panels[12].SetActive(true);         // Turn on "ZoomPanel"
            Panels[30].GetComponent<Button>().interactable = false; // Block the button for displaying line editing elements
            Panels[32].GetComponent<Button>().interactable = false; // Block the button showing the game menu
            Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 128);  // Making the hidden panel transparent
            Panels[3].transform.GetChild(0).GetComponent<Button>().interactable = false; // Block the button: (show / hide the "give up" button)
            Panels[48].SetActive(true);         // Making the alert button active
            CM.DrawingALineIsProhibited = true; // We indicate that we prohibit drawing a line
            GM.BlockingMoves = true;            // Block player moves
        }
        else                                    // Otherwise, if it was enabled
        {
            Panels[12].SetActive(false);        // Turn it off
            Panels[30].GetComponent<Button>().interactable = true; // Unblock the button for displaying line editing elements
            Panels[32].GetComponent<Button>().interactable = true; // Unblock the button showing the game menu
            Panels[48].SetActive(false);        // Making the alert button inactive

            if (GM.FirstRoundOfMovesEnded)      // If the first round of moves has been completed
            {
                Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 255);  // Making the hidden panel not transparent
                Panels[3].transform.GetChild(0).GetComponent<Button>().interactable = true; // Unblock the button: (show / hide the "give up" button)
            }

            if (GM.Gamestate == GameManager.GameStates.Scheme) CM.DrawingALineIsProhibited = false;// If the game state scheme we indicate that we allow drawing a line
            GM.BlockingMoves = false;           // Again allow the player to walk
        }
    }

    
    void OpenCloseGameMenu()                    // This method is called from the button method "OpenCloseGameMenuButton"
    {
        if (Panels[27].activeSelf == false)     // If the game menu window is turned off
        {
            Panels[27].SetActive(true);         // Turn it on
            Panels[30].GetComponent<Button>().interactable = false; // Block the button for displaying line editing elements
            Panels[31].GetComponent<Button>().interactable = false; // Block the button for displaying the elements of increasing the map
            Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 128);    // Making the hidden panel transparent
            Panels[3].transform.GetChild(0).GetComponent<Button>().interactable = false;// Block the button: (show / hide the "give up" button)
            Panels[48].SetActive(true);         // Making the alert button active
            GM.BlockingMoves = true;            // Block player moves
        }
        else                                    // Otherwise if it is off
        {
            Panels[27].SetActive(false);        // Turn it on
            Panels[30].GetComponent<Button>().interactable = true;  // Unblock the button for displaying line editing elements
            Panels[31].GetComponent<Button>().interactable = true;  // Unblock the button for displaying the elements of increasing the map
            Panels[48].SetActive(false);        // Making the alert button inactive

            if (GM.FirstRoundOfMovesEnded)      // If the first round of moves has been completed
            {
                Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 255);    // Making the hidden panel non transparent
                Panels[3].transform.GetChild(0).GetComponent<Button>().interactable = true; // Unblock the button: (show / hide the "give up" button)
            }

            GM.BlockingMoves = false;           // Again allow the player to walk
        }
    }


    void MethodPlayAgain()                                  // This method is called the "Play Current Drawing" event.
    {
        Panels[4].SetActive(false);                         // Turn off "EndOfGameMenu_Panel"

        for (int a = 0; a < Panels[5].transform.childCount; a++)                        // We continue the cycle until we sort through all the children of the "GamePoints_Panel [5]" panel
        {
            Destroy(Panels[5].transform.GetChild(a).gameObject);                        // Delete the next information element corresponding to the next player
        }

        Panels[3].SetActive(true);                                                      // Making the "hidden_panel" panel visible    
        Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 128);        // Making the hidden panel transparent
        Panels[3].transform.GetChild(0).GetComponent<Button>().interactable = false;    // Block the "Show / hide hidden panel" button
        Panels[31].SetActive(true);                                                     // Make the "show / hide enlargement menu" button visible
        Panels[32].SetActive(true);                                                     // Make the "show / hide game menu" button visible
        Panels[50].SetActive(false);                                                    // Turn off the panel showing who won
    }


    void MethodPlayNextLevel()                                                      // This method is called the "PlayNextLevel" event.
    {
        Panels[4].SetActive(false);                                                 // Turn off "EndOfGameMenu_Panel"
        Panels[35].GetComponent<Button>().interactable = false;                     // Turn off the button "Next Level"

        for (int a = 0; a < Panels[5].transform.childCount; a++)                    // We continue the cycle until we sort through all the children of the "GamePoints_Panel [5]" panel
        {
            Destroy(Panels[5].transform.GetChild(a).gameObject);                    // Delete the next information element corresponding to the next player
        }

        Panels[3].SetActive(true);                                                  // Making the "hidden_panel" panel visible  
        Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 128);    // Making the hidden panel transparent
        Panels[3].transform.GetChild(0).GetComponent<Button>().interactable = false;// Block the "Show / hide hidden panel" button
        Panels[31].SetActive(true);                                                 // Make the "show / hide enlargement menu" button visible
        Panels[32].SetActive(true);                                                 // Make the "show / hide game menu" button visible
        Panels[50].SetActive(false);                                                // Turn off the panel showing who won
    }


    private void MethodStartingCellNotFound()                       // This method is called by the "StartingCellNotFound" event.
    {
        Panels[3].SetActive(false);                                 // Turn off HiddenPanel
        Panels[31].SetActive(false);                                // Turn off the object with the "show / hide" button of the zoom menu
        Panels[32].SetActive(false);                                // Turn off the object with the "show / hide" button of the game menu
        Panels[27].SetActive(false);                                // Turn off "GameMenu_Panel"
        Panels[12].SetActive(false);                                // Turn off "Zoom_Panel"
        Panels[29].SetActive(false);                                // Turn off "GameExitWarningWindow" panel        
        Panels[40].SetActive(true);                                 // Activate the warning window with access to the main menu
    }


    string ToHEX(Color color)                                       // This method converts the color from rgb to "hex" and returns it in the "hex" format
    {
        Color32 c32 = color;
        string htmlColor = "#" + c32.r.ToString("X2") + c32.g.ToString("X2") + c32.b.ToString("X2");
        return htmlColor;
    }

    void OnDisable()
    {
        GM.SetStateMenu -= MethodSetStateMenu;                      // We write off the method "MethodSetStateMenu" from the event "SetStateMenu"
        GM.SetStateShceme -= MethodSetStateScheme;                  // Unsubscribe the method "MethodSetStateScheme" from the event "Set game state scheme"
        GM.PlayGame -= MethodPlayGame;                              // Unsubscribe the method MethodPlayGame from the event PlayGameButton
        GM.OneLineWasFinished -= MethodOneLineWasFinished;          // Отписываем метод "MethodOneLineWasFinished" от события "Одна линия была закончена"
        GM.DrawTheFollowingLine -= MethodDrawTheFollowingLine;      // Unsubscribe the "MethodDrawTheFollowingLine" method from the "DrawTheFollowingLine" event
        GM.SetStateGame -= MethodSetStateGame;                      // Отписываем метод "MethodSetStateGame" от события "SetStateGame"
        GM.Surrender -= MethodSurrender;                            // Отписываем метод "MethodSurrender" от события "Surrender"
        GM.EveryoneSurrendered -= MethodEveryoneSurrendered;        // Отписываем метод "MethodEveryoneSurrendered" от события "EveryoneSurrendered"
        GM.ShowWalking -= MethodShowWalking;                        // Unsubscribe the "MethodShowWalking" method from the "ShowWalking" event
        GM.TheFirstPointIsSet -= MethodTheFirstPointIsSet;          // Unsubscribe the "MethodTheFirstPointIsSet" method from the "TheFirstPointIsSet" event
        GM.TheNextPointIsSet -= MethodTheNextPointIsSet;            // Unsubscribe the "MethodTheNextPointIsSet" method from the "TheNextPointIsSet" event
        GM.LoadGameLevel -= MethodLoadGameLevel;                    // We write off the method "MethodLoadGameLevel" from the event "LoadGameLevel"
        GM.PlayAgain -= MethodPlayAgain;                            // We write off the method "MethodPlayAgain" from the event "PlayAgain"
        GM.PlayNextLevel -= MethodPlayNextLevel;                    // We write off the method "MethodPlayNextLevel" from the event "PlayNextLevel"
        GM.StartingCellNotFound -= MethodStartingCellNotFound;      // We write off the method "MethodStartingCellNotFound" from the event "StartingCellNotFound"
    }


    // -------------------------------------------------------------------------------------------- Методы кнопок --------------------------------------------------------------------------------------------


    public void CreateADrawing()                // This method is called when you need to create a drawing
    {
        GM.CallSetStateScheme();                // Вызываем метод вызывающий событие "Установить состояние игры схема"
    }


    public void MapSelectionButton()            // This method is triggered by pressing the "Select drawing" button and takes the player from the main menu to the drawing selection window
    {
        JustUploaded = true;                    // We indicate that we just loaded the download window
        LoadScreenshotsOfMaps();                // Call the method of uploading custom map screenshots
    }


    public void NextAndPreviousСustomMapButton(int Button)      // These buttons toggle the selected active user map.
    {
        NextAndPreviousСustomMap(Button);       // Call the switching method of the active user map
    }


    public void NextAndPreviousPassageMapButton(int Button)
    {
        NextAndPreviousPassageMap(Button);      // Call the switching method of the active passage map
    }


    public void NumberOfPlayersButton()         // This method is called when the number of players was selected
    {
        NumberOfPlayers();                      // Call the "NumberOfPlayers" method
    }


    public void OpenASelectionOfColorsButton()  // This method is called when the player presses the color selection button
    {
        OpenASelectionOfColors();               // We call the executing method
    }


    public void ColorSelectedButton()           // This method is called upon pressing any of the buttons that select the color of the player's figure
    {
        StartCoroutine(ColorSelected());        // We call the executing coroutine
    }


    public void OpenASelectionOfFiguresButton() // This method is called when the player presses the figure selection button
    {
        OpenASelectionOfFigures();              // We call the executing method
    }


    public void FigureSelectedButton()          // Deactivate the figure selection panel
    {
        StartCoroutine(FigureSelected());       // We call the executing coroutine
    }
    
    public void CustomizeTheGame()          // This method is called before the game starts to set up the round
    {
        Panels[1].SetActive(false);         // Turn off the main menu panel
        Panels[6].SetActive(true);          // Turn on the options panel of the round
    }


    public void GoToMainMenu()              // Method called when you need to go to the main menu
    {
        GM.CallSetStateMenu();              // We call the method that calls the event "SetStateMenu"
    }


    public void BeginDrawingButton()        // This method is called with the "BeginDrawing" button.
    {
        ContinueDrawing();                  // Call the line drawing start method.
    }


    public void ContinueDrawingButton()     // Method called "Continue to Draw"
    {
        ContinueDrawing();                  // Call the line drawing start method.
    }


    public void PlayGameButton()            // Играть текущий терёж
    {
        GM.CallPlayGame();                  // Call the method causing the event "Play the current drawing"
    }


    public void Moving()                    // Выехать
    {
        MoveHidenPanel();                   // Call the button animation method.
    }


    public void SurrenderButton()           // Method called by button Surrender
    {
        GM.CallSurrender();                 // Call the method that causes the event to surrender.
    }


    public void OpenCloseEditingPanelButton()   // Method called "OpenCloseEditingPanelButton" button
    {
        OpenCloseEditingPanel();                // We call the executing method
    }


    public void OpenCloseZoomPanelButton()      // Method called "OpenCloseScreenMagnifier" button
    {
        OpenCloseZoomPanel();                   // We call the executing method
    }
    
    public void OpenCloseGameMenuButton()       // This method is called by pressing the "Open/Close_GameMenu_Button" button and opens the game menu
    {
        OpenCloseGameMenu();                    // We call the executing method
    }

    public void ZoomRollbackButtons(bool Zoom)  // This method is called up and down with the buttons.
    {
        ZoomRollback(Zoom);                     // Call the method of reducing and increasing the screen
    }


    public void VerticalHorizontalScrollbarButtons(GameObject Scrollbar)    // This method moves the camera to the sides.
    {
        VerticalHorizontalScrollbar(Scrollbar); // Call the method moving the screen from side to side
    }


    public void EditLineButton(int State)       // The method called by the buttons "Step/Jump, Back/Forward"
    {
        EditLine(State);                        // Call the line editing method
    }

    public void SaveMapButton()                                                         // This method is called with the save CustomMap button.
    {
        SL.SavingType = SaveLoad.WhatWeSaving.CustomMap;                                // We indicate that we want to save the user map
        Panels[27].transform.GetChild(0).GetComponent<Button>().interactable = false;   // Lock the save button in the main menu
        Panels[37].GetComponent<Button>().interactable = false;                         // Lock the save button in the menu at the end of the game
        GM.CallSaveGameEvent();                                                         // Call the method that causes the event to save the map.
    }

    public void SavePassageLevel()                              // This method is called by the SavePassageLevel button and is needed to save the levels created by the developer.
    {
        SL.SavingType = SaveLoad.WhatWeSaving.SavingPassageMap; // We indicate that we want to save the map of passage
        GM.CallSaveGameEvent();                                 // Call the method that causes the event to save the map.
    }

    public void GoToRoundSettingsButton()       // Exit button back to main menu from map selection window
    {
        Panels[13].SetActive(false);            // Turn off the map selection window
    }

    public void GoToMainMenuButton()            // This method is called by the back button from the level settings menu.
    {
        Panels[6].SetActive(false);             // Turn off the game settings window
        Panels[1].SetActive(true);              // Turn on the main menu window
    }

    public void RunPassageMapButton()           // This button launches a custom map.
    {
        RunPassageMap();                        // Call the method that activates this button.
    }

    public void RunCustomMapButton()            // This button launches a custom map.
    {
        Panels[17].SetActive(true);             // Call the custom map download confirmation window
    }

    public void CloseWarningWindowButton()      // This method closes the window - warning that the game cannot be started until the player and computer are selected
    {
        Panels[26].SetActive(false);
    }

    public void DeleteMapConformationWindowButton()         // This method is called by pressing the delete card button and displays a card deletion confirmation window.
    {
        JustUploaded = false;                               // We indicate that we have not just loaded the download window
        Panels[24].SetActive(true);                         // Call the delete map confirmation window
    }

    public void NoAnswerCustomButton()                      // This button closes the window asking whether to load chosen custom map.
    {
        Panels[17].transform.gameObject.SetActive(false);   // Close the chosen custom map confirmation window without any consequences.
    }

    public void NoAnswerPassageButton()                     // This button closes the window asking whether to load chosen passage map.
    {
        Panels[18].transform.gameObject.SetActive(false);   // Close the chosen passage map confirmation window without any consequences.
    }

    public void YesAnswerDeleteMapButton()                  // This method deletes the map and closes the card deletion confirmation window.
    {
        Panels[24].transform.gameObject.SetActive(false);   // Close the chosen delete map confirmation window 
        SL.DeleteCustomMap();                               // Call the map removal method
    }

    public void NoAnswerDeleteMapButton()                   // This button closes the window asking whether to load chosen passage map.
    {
        Panels[24].transform.gameObject.SetActive(false);   // Close the delete map confirmation window without any consequences.
    }

    public void YesLoadPassageMapButton()                   // This button loads the selected passage map with the configured parameters.
    {
        GM.GameType = GameManager.GameTypes.Walkthrough;    // We indicate that we play the passage mode
        StartCoroutine(CallLoadLevel());                    // Call coroutine processing this button.
    }

    public void YesLoadCustomMapButton()                    // This button loads the selected custom map with the configured parameters.
    {
        GM.GameType = GameManager.GameTypes.CustomMapGame;  // Specify that we are playing custom map mode
        StartCoroutine(CallLoadLevel());                    // Call coroutine processing this button.
    }

    public void LogoutConfirmation()                    // This method is called when you need to confirm exit from the game.
    {
//        Panels[28].SetActive(true);                   // Turn on the confirmation window panel
        Panels[29].SetActive(true);                     // Turn on the exit warning window 
    }

    public void NoAnswerExitGameButton()                // This method is called by the no button in the exit confirmation window.
    {
//        Panels[28].SetActive(false);                  // Turn off the confirmation window panel
        Panels[29].SetActive(false);                    // Turn off the exit warning window 
    }

    public void PlayAgainButton()                       // This method is called by the "Play Again" button.
    {
        GM.CallPlayAgain();                             // We call the method "GM.CallPlayAgain" causing the event "PlayAgain"
    }

    public void PlayNextLevelButton()                   // This method is called by the "PlayNextLevel" button.
    {
        GM.CallPlayNextLevel();                         // We call the method "CallPlayNextLevel" causing the event "PlayNextLevel"
    }

    public void QuestionButton()                        // This method is called by the question button in the player settings menu.
    {
        Panels[46].SetActive(true);                     // Activate the help window
    }

    public void HideHelpWindow()                        // This method is called by clicking on the information screen.
    {
        Panels[46].SetActive(false);                    // Deactivate help window
    }

    public void GameBlockNoticeButton()                 // This method is called by the warning button that the game is blocked by an open menu.
    {
        StartCoroutine(GameBlockNotice());              // Call the coroutine warning
    }

    public void QuitTheGameButton()                     // The method called by the button "Quit the game"
    {
        Application.Quit();                             // Quit the game
    }
}

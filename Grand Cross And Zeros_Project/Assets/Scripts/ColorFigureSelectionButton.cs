using UnityEngine;
using UnityEngine.UI;

public class ColorFigureSelectionButton : MonoBehaviour
{
    public UIScript UIS;                    // Variable storing script UIScript
    public GameManager GM;                  // Variable GM storing object of class GameManager
    byte NoP;                               // Variable "Nomber of player"
    GameObject PlayerButton;                // The specific player button with which the script is currently running
    PlayerButtonScript PBS;                 // Here will be the script of the player’s settings button, the color or shape of which we are currently changing


    public void ChangeColor(int NomberOfColor)               // Here is stored the number of the player for whom we select the color
    {
        NoP = UIS.LastDDObject.GetComponent<PlayerButtonScript>().NumberOfPlayer;           // We put into the variable "Nop" the player number for which we select the color
        Color SelectedColor = gameObject.GetComponent<Image>().color;                       // We put the selected color in the variable "SelectedColor"
        PlayerButton = UIS.ScrollContent.transform.GetChild(NoP).gameObject;                // We put a sample of the player’s button in the "PlayerButton" variable
        PBS = PlayerButton.GetComponent<PlayerButtonScript>();                              // Put in the variable "PBS" the button script of this player

        GM.MaterialsTurn.Insert(NoP, new Material(GM.Materials[NomberOfColor]));            // We put the new number of the selected material in the list "MaterialsTurn"
        GM.MaterialsTurn.RemoveAt(NoP+1);                                                   // Remove the old material number from the list "MaterialsTurn"
        GM.ColorsTurn.Insert(NoP, SelectedColor);                                           // We put the new selected color in the list "ColorsTurn"
        GM.ColorsTurn.RemoveAt(NoP+1);                                                      // Delete the old color for this player from the list "ColorsTurn"

        PlayerButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = GM.ColorsTurn[NoP];                // We change the selected color in the button header
        PlayerButton.transform.GetChild(3).GetChild(0).GetChild(0).GetComponent<Image>().color = GM.ColorsTurn[NoP];    // We change the selected color inside the drop-down list
        PlayerButton.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().color = GM.ColorsTurn[NoP];    // We also change the color in the picture showing the figure itself
        PBS._hiddenMenu();                                      // We call the button open / close method to reopen the window and sign it for the closing event          
    }


    public void ChangeFigure(int NomberFigure)                                              // In this method, we change the player's figure to the selected one and open the dropdown-list
    {
        NoP = UIS.LastDDObject.GetComponent<PlayerButtonScript>().NumberOfPlayer;           // We put into the variable "Nop" the player number for which we select the color
        PlayerButton = UIS.ScrollContent.transform.GetChild(NoP).gameObject;                // We put a sample of the player’s button in the "PlayerButton" variable
        PBS = PlayerButton.GetComponent<PlayerButtonScript>();                              // Put in the variable "PBS" the button script of this player

        GM.FiguresTurn.Insert(NoP, (byte)NomberFigure);         // We put the new number of the selected figure in the list "FiguresTurn"
        GM.FiguresTurn.RemoveAt(NoP+1);                         // Remove the old figure number from the list "FiguresTurn"
        GM.FiguresTurn.Insert(NoP, (byte)NomberFigure);         // We put the new selected figure in the list "FiguresTurn"
        GM.FiguresTurn.RemoveAt(NoP+1);                         // Delete the old figure for this player from the list "FiguresTurn"

        // We change the selected image in the header of the drop-down list
        PlayerButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = UIS.Panels[8].transform.GetChild(2).GetChild(0).GetChild(GM.FiguresTurn[NoP]).
        GetChild(0).GetComponent<Image>().sprite;

        // We change the selected image from the drop-down list
        PlayerButton.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().sprite = UIS.Panels[8].transform.GetChild(2).GetChild(0).GetChild(GM.FiguresTurn[NoP]).
        GetChild(0).GetComponent<Image>().sprite;

        PBS._hiddenMenu();                                      // We call the button open / close method to reopen the window and sign it for the closing event
    }
 }

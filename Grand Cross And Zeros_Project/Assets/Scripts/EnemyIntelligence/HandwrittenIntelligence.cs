// It is planned 4 levels of complexity, the two easiest will be implemented using programming. The next two are using neural networks. 
// Abbreviations in the object name: N - Player number, D - Difficulty level
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandwrittenIntelligence : MonoBehaviour
{
    public byte PlayerNomber;                               // Here the player number will be stored in the general list of players
    public byte ComputerDifficulty;                         // The complexity of the computer opponent will be indicated here.
    public bool ImpossibleToGo = false;                     // This variable will become "true" if the computer does not find a further move and it needs to give up
    GameManager GM;                                         // Here will be an instance of the object "Gamemanager"
    List<Vector3> AllowableMove = new List<Vector3>();      // List of cells where a computer player can go
    public List<Vector3> MyСells { get; set; }              // Auto property for the list of cells occupied by this computer
    Vector3[] RayDirections = new Vector3[8];               // Array, with 8 directions of rays
    Vector3[] RayDirFirsFig = new Vector3[4];               // Here are the rays to search for the first cell of the computer, clockwise
    GameObject Finder;                                      // This object “Finder” will check 4 cells around the selected cell and decide whether they are free to move or not.
    GameObject TicTacToeCategory;                           // Here will be the category "TicTacToe"
    Vector3 ClosestEnemyFigure;                             // The figure standing closest to the player’s starting cell
    Vector3 ClosestComputerFigure;                          // Nearest computer figure to the nearest enemy figure
    Vector3 TargetСell;                                     // This is the cell to which the computer will move until it reaches it and receives a new target cell
    List<Vector3> TargetPath;                               // A list of cells on which the computer will go
    bool TheNextMoveIsAvailable = false;                    // This variable indicates whether there are still free cells for the move of this player or he will lose now
    int LayerCellDetecor = 1 << 10;                         // Mask for the ray that determines that it will collide with colliders from the "CellsDetectors" layer
    int LayerFogCell = 1 << 11;                             // Mask for the ray determines that it will collide with the colliders of the FogCell layer
    int LayerFigure = 1 << 12;                              // A mask for the ray determining that it will also collide with the "PlayerFigure" colliders


    void OnEnable()
    {
        GM = GameObject.FindGameObjectWithTag("MainGameObject").GetComponent<GameManager>();    // Put in the variable "GM" Script "Gamemanager"
        GM.PlayNextLevel += MethodPlayNextLevel;               // We subscribe the "MethodPlayNextLevel" method to the "PlayNextLevel" event
    }


    private void Start()
    {
        RayDirections[0] = new Vector3(1, 0, 0);            // The ray launched into the upper border of the board
        RayDirections[1] = new Vector3(1, 0, -1);           // Ray, launced Up & Right on diagonal
        RayDirections[2] = new Vector3(0, 0, -1);           // The ray launched into the right border of the board
        RayDirections[3] = new Vector3(-1, 0, -1);          // Ray, launced Down & Right on diagonal
        RayDirections[4] = new Vector3(-1, 0, 0);           // The ray launched into the lower border of the board
        RayDirections[5] = new Vector3(-1, 0, 1);           // Ray, launced Down & Left on diagonal
        RayDirections[6] = new Vector3(0, 0, 1);            // The ray launched into the left border of the board
        RayDirections[7] = new Vector3(1, 0, 1);            // Ray, launced Up & Left on diagonal

        RayDirFirsFig[0] = new Vector3(1, 0, 0);            // The ray launched into the upper border of the board
        RayDirFirsFig[1] = new Vector3(0, 0, -1);           // The ray launched into the right border of the board
        RayDirFirsFig[2] = new Vector3(-1, 0, 0);           // The ray launched into the lower border of the board
        RayDirFirsFig[3] = new Vector3(0, 0, 1);            // The ray launched into the left border of the board

        MyСells = new List<Vector3>();                      // Create an empty list to be used by the "MyCells" auto-property

        TicTacToeCategory = GameObject.FindWithTag("TicTacToe");                                // We find the category "TicTacToe" and put it in the corresponding variable

        TargetPath = new List<Vector3>();                   // Create a new list of paths to the target cell
        CreateAFinder();                                    // Create a "finder"
    }


    public void MakeAMove()                                 // The main method determining which intelligence to activate
    {
        switch (ComputerDifficulty)
        {
            case 0:                                         // If 1 difficulty level was selected
                FirstLevel();                               // We call method 1 of difficulty level
                break;
            case 1:                                         // If 2 difficulty level was selected
                SecondLevel();                              // We call method 2 of difficulty level
                break;
        }
    }


    public void FirstLevel()                                            // first level of intelligence (the simplest)
    {
        if (MyСells.Count == 0)                                         // If the first move of this player has not yet been made
            TheNextMoveIsAvailable = FreeCellSearch();                  // We indicate whether we have free cells for the first move and p1ut all free cells in the "FreeCells" list
        else                                                            // Otherwise, if this is not the first move of the computer
            TheNextMoveIsAvailable = FindСellsAvailableToMove();        // Find cells available to move

        if (MyСells.Count == 0)                                         // If the first move of this player has not yet been made
        {
            if (TheNextMoveIsAvailable)                                 // If there are free cells to move
                FirstLevelMove();                                       // We call the standard move of AI, the first level
            else                                                        // Otherwise, if there are no more free cells
            {
                GM.FirstMoveNotFound = true;                                        // We indicate that this kmoputer did not find a cage for the first move
            //    ImpossibleToGo = true;                                  // We indicate that the computer did not find cells for the move
            }
        }
        else                                                            // Otherwise, if this is not the first move
        {
            if (TheNextMoveIsAvailable)                                 // If there are free cells to move
                FirstLevelMove();                                       // We call the standard move of AI, the first level
            else                                                        // If there are no more free squares to move
            {
                ImpossibleToGo = true;                                  // We indicate that the computer did not find cells for the move
            }
        }
    }

    public void SecondLevel()                                               // Second level of intelligence (the simplest)
    {
        int NomberSelectedCell;                                             // The cell number in the "AllowableMove" list selected for the move
        Vector3 SelectedСell = Vector3.zero;                                // Movement specific cell

        switch (MyСells.Count)
        {
            case 0:                                                                     // If the first move of this player has not yet been made (First Phase)
                SelectedСell = FindFirstCell(out TheNextMoveIsAvailable);               // Find the cell for the first move of the computer

                if (TheNextMoveIsAvailable)                                             // If a free cell was found to move   
                {
                    MyСells.Add(SelectedСell);                                          // Put in the list of cells occupied by this computer player cell
                    GM.PlaceTheFigureOnTheField(SelectedСell);                          // Put the computer figure on the field
                    SelectedСell = Vector3.zero;                                        // Zeroing the values of "SelectedCell" to avoid errors
                    AllowableMove.Clear();                                              // Сlear the list of free cells
                    GM.GamePoints[GM.Turn]++;                                           // We add one to the number of figures for this player. 
                }
                else                                                                    // Otherwise, if there are no more free cells
                {
                    GM.FirstMoveNotFound = true;                                        // We indicate that this kmoputer did not find a cage for the first move
                }
                break;
            case 1:                                                                     // If the computer made only one move (Second Phase)
                ClosestEnemyFigure = FindTheClosestEnemyFigure(MyСells[0]);             // Find the position of the nearest enemy figure and put it in the variable "ClosestEnemyFigure"

                if (Vector3.Distance(ClosestEnemyFigure, MyСells[0]) >= 15.1f)          // If the distance between the starting computer figure and the nearest enemy figure is more than 15.1 units
                {
                    TargetСell = FindTheClosestFreeCell(MyСells[0], ClosestEnemyFigure);// Find the nearest empty cell to the nearest enemy figure and enter it into the "TargetCell" variable
                    MoveToTheSpecifiedPoint(MyСells[0], TargetСell);                    // We put a new figure on the way to the empty target cell of the enemy
                    GM.GamePoints[GM.Turn]++;                                           // We add one to the number of figures for this player. 
                }
                else                                                                    // Otherwise, if the distance is less
                {
                    goto DefaultMove;                                                   // Then go to the default
                }
                break;
            default:                                                                    // If he has more than one figure
                DefaultMove:
                if (TargetPath.Count != 0)                                              // If the computer has not reached the nearest enemy cell
                {
                    MoveToTheSpecifiedPoint(MyСells[0], TargetСell);                    // We put a new figure on the way to the empty target cell of the enemy
                    GM.GamePoints[GM.Turn]++;                                           // We add one to the number of figures for this player. 
                }
                else                                                                    // If the computer has reached the nearest enemy figure (Third Phase)
                {
                    if (FindСellsAvailableToMove())                                     // We are looking for cells available for the move, and if we find at least one
                    {
                        SelectedСell = FindACellAdjacentToTheEnemyFigure(AllowableMove);// We find the first cell adjacent to the enemy figure
                        if (SelectedСell != Vector3.zero)                               // If the method found such a cell
                        {
                            MyСells.Add(SelectedСell);                                  // Put in the list of cells occupied by this computer player cell
                            GM.PlaceTheFigureOnTheField(SelectedСell);                  // Put the computer figure on the field
                            AllowableMove.Clear();                                      // Сlear the list of free cells
                            GM.GamePoints[GM.Turn]++;                                   // We add one to the number of figures for this player. 
                        }
                        else                                                            // If the method (returned a null value) did not find any suitable cells
                        {
                            NomberSelectedCell = Random.Range(0, AllowableMove.Count);  // Select a random cell number in the "FreeCells" list
                            SelectedСell = AllowableMove[NomberSelectedCell];           // Put in the variable "SelectedСell" сell selected for the first move
                            MyСells.Add(SelectedСell);                                  // Put in the list of cells occupied by this computer player cell
                            GM.PlaceTheFigureOnTheField(SelectedСell);                  // Put the computer figure on the field
                            AllowableMove.Clear();                                      // Сlear the list of free cells
                            GM.GamePoints[GM.Turn]++;                                   // We add one to the number of figures for this player. 
                        }
                    }
                    else                                                                // Otherwise, if at least one cell available for the move was not found 
                        ImpossibleToGo = true;                                          // We indicate that the computer did not find cells for the move
                }
                break;
        }
    }

    void FirstLevelMove()                                       // Standard move AI, first level
    {
        int NomberSelectedCell = Random.Range(0, AllowableMove.Count);  // Select a random cell number in the "FreeCells" list
        Vector3 SelectedСell = AllowableMove[NomberSelectedCell];       // Put in the variable "StartCell" сell selected for the first move
        MyСells.Add(SelectedСell);                                      // Put in the list of cells occupied by this computer player cell
        GM.PlaceTheFigureOnTheField(SelectedСell);                      // Put the computer figure on the field
        AllowableMove.Clear();                                          // Сlear the list of free cells
        GM.GamePoints[GM.Turn]++;                                       // We add one to the number of figures for this player. 
    }

    bool FreeCellSearch()                                       // This method searches for free cells and puts them in the "FreeCells" list.
    {
        Vector3 FinderStartPos = new Vector3(325, 5, -25);      // Finder Starting Position
        Vector3 NewFinderPos = Vector3.zero;                    // The new highlighted position for the Finder is indicated here.
        RaycastHit FinderRayHit;                                // Here there will be data about the object into which the beam crashed
        bool CellFound = false;                                 // This variable indicates whether at least one cell was found.

        GameObject Finder = new GameObject();                                                           // This object will find all empty cells and put their coordinates in the array "AllowableMove"
        Finder.transform.SetParent(GameObject.FindGameObjectWithTag("AnotherObjects").transform);       // Adopt the object "finder" in the category "AnotherObjects"
        Finder.name = "Finder";                                                                         // Give the object its name "Finder"
        Finder.transform.position = FinderStartPos;                                                     // Place the finder over the first checked cell

        for (int i = 0; i < 600; i++)                                                                   // This cycle will continue until we check all the cells in the field.
        {
            if (Physics.Raycast(Finder.transform.position, Vector3.down, out FinderRayHit, 6))          // If the ray crashed into some kind of object
            {
                if (FinderRayHit.collider.tag == "FreeCell")                                            // If the "tag" of the object into which the beam crashed is "FreeCell"   
                {
                    AllowableMove.Add(FinderRayHit.transform.position);                                 // Enter the coordinates of this cell as available for the move
                    CellFound = true;
                }
            }

            NewFinderPos = FinderNewPosition(FinderStartPos, Finder.transform.position);                // Calculate the new position of the "Finder" and put it into the variable "NewFinderPos"

            if (NewFinderPos != Vector3.zero)                                                           // If the method did not return us a null value
                Finder.transform.position = NewFinderPos;                                               // We move the "Finder" to his new position
            else                                                                                        // Otherwise, if the method returned a null value
                break;                                                                                  // Finish the work of the method
        }

        return CellFound;                           // We return the value of the variable "CellFound"
    }

    Vector3 FinderNewPosition(Vector3 StartPosition, Vector3 CurrentPos) // This method returns the next position for the Finder.
    {
        if (CurrentPos.z > -175)                    // If the "Finder" has not yet reached the right edge
            CurrentPos = new Vector3(CurrentPos.x, CurrentPos.y, CurrentPos.z - 10); // We move the "Finder" by 10 units to the right side of the field
        else                                        // Otherwise, if the position of the seeker along the "Z" axis is -175
        {
            if (CurrentPos.x > 16)                  // If the "Finder" has not yet left the bottom of the playing field
                CurrentPos = new Vector3(CurrentPos.x - 10, CurrentPos.y, StartPosition.z);
            else                                    // Otherwise, if the "Finder" reaches the bottom of the field
                CurrentPos = Vector3.zero;          // Return the zero coordinates to complete the search for empty cells 
        }

        return CurrentPos;                          // Returning a new "Finder" position
    }

    void CreateAFinder()                                    // Create a "finder"
    {
        Finder = new GameObject();                          // Create the "qualifier" object
        Finder.transform.SetParent(transform);              // We adopt the "Finder" object to the object on which this script hangs
        Finder.name = "FinderPlayer_" + PlayerNomber;       // Set the name for the "Finder"
    }

    bool FindСellsAvailableToMove()     // When the first move was also done, this method searches for a cell free for the next move
    {
        for (int a = 0; a < MyСells.Count; a++)                         // We continue the cycle until we sort through all the cells in the array "MyCell"
            AllowableMove.AddRange(FindFreeCellsArround(MyСells[a]));   // Find new cells around the next figure and add them to the "AllowableMove" list

        if (AllowableMove.Count != 0)                           // If at least one available move has been put on
            return true;
        else                                                    // If there is no where else to go
            return false;
    }

    void MoveToTheSpecifiedPoint(Vector3 StartingCell, Vector3 TargetCell)  // This method puts the figure in the direction of the selected cell.
    {
        if (TargetPath.Count == 0)                                          // If the list of paths followed by the computer is empty
        {
            CreateANewPathToTheTargetCell(StartingCell, TargetСell);        // Create a new path to the target cell.
        }
        else                                                                // Otherwise, if the path is already paved
        {
            Vector3 NewClosestEnemyFigure = FindTheClosestEnemyFigure(MyСells[MyСells.Count - 1]);     // Find the position of the nearest enemy figure and put it in the variable "ClosestEnemyFigure"

            ClosestEnemyFigure = NewClosestEnemyFigure;                                             // Updating the nearest enemy figure
            ClosestComputerFigure = FindTheClosestCellInTheList(MyСells, ClosestEnemyFigure);       // Find the nearest computer figure to the nearest enemy figure
            TargetСell = FindTheClosestFreeCell(ClosestComputerFigure, ClosestEnemyFigure);         // Find the nearest empty cell to the nearest enemy figure and enter it into the "TargetCell" variable
            CreateANewPathToTheTargetCell(ClosestComputerFigure, TargetСell);                       // Create a new path to the target cell.    
        }

        GM.PlaceTheFigureOnTheField(TargetPath[0]);                         // Put the computer figure on the field
        MyСells.Add(TargetPath[0]);                                         // Put in the list of cells occupied by this computer player cell
        TargetPath.RemoveAt(0);                                             // Delete a position from the "TargetPath" list
    }

    Vector3 FindTheClosestEnemyFigure(Vector3 FirstAIFigure)    // This method finds the closest enemy figure and returns it.
    {
        GameObject SelectedEnemyFigure;                         // Here will be the current processed cell of the computer player
        Vector3 _сlosestEnemyFigure = Vector3.zero;              // Here will be the closest computer cell to the starting cell of this computer player

        for (int a = 0; a < GM.NumberOfPlayers; a++)            // We continue the cycle until we sort through all categories of figures of players
        {
            if (a != PlayerNomber)                              // If we are not working with our category of figures
            {
                for (int b = 0; b < GM.GamePoints[a]; b++)      // We continue the cycle until we sort through all the figures in the current category
                {
                    SelectedEnemyFigure = TicTacToeCategory.transform.GetChild(a).GetChild(b).gameObject; // We find the next figure of the enemy player and put it into the variable "SelectedEnemyFigure"

                    if (_сlosestEnemyFigure == Vector3.zero)                                // If the coordinates of the "ClosestEnemyFigure" match are equal to the "starting value"
                        _сlosestEnemyFigure = SelectedEnemyFigure.transform.position;       // Then we put in it the coordinates of the first figure
                    else                                                                    // Otherwise, if any nearby figure has already been detected
                    {
                        float OldDistance = Vector3.Distance(FirstAIFigure, _сlosestEnemyFigure);                   // Calculate the distance to the old figure
                        float NewDistance = Vector3.Distance(FirstAIFigure, SelectedEnemyFigure.transform.position);// Calculate the distance to the new figure

                        if (NewDistance < OldDistance)                                      // And if the new figure in this cycle is closer than the last
                            _сlosestEnemyFigure = SelectedEnemyFigure.transform.position;   // We indicate that now she is the closest figure
                    }
                }
            }
        }

        return _сlosestEnemyFigure;
    }


    void CreateANewPathToTheTargetCell(Vector3 StartingCell, Vector3 TargetCell)    // This method creates a new path to the target cell.
    {
        Ray RayTowards;                                                             // Here the ray is fired from the figure in the target direction
        Ray RayFinder;                                                              // Here is stored the ray launched from the "Finder" to the bottom
        RaycastHit Hit;                                                             // Here the object into which the beam crashed is stored
        Vector3 RayOrigin;                                                          // There will be a point from where we will draw the beam towards the target cell
        Vector3 VectorDirrection;                                                   // Here will be the direction of the vector from the next figure in the loop, to "TargetCell"
        Vector3 FinderPosition;                                                     // The position of the point from which the next cell is probed
        Vector3 ClosestCell;                                                        // The cell closest to the enemy in this cycle

        RayOrigin = StartingCell;                                                   // We lay the starting position in "RayOrigin"
        TargetPath.Clear();                                                         // If there were old cells along the way, we erase them

        for (int i = 0; i < 60; i++)                                                // We continue the cycle until we add the target points to the "TargetPath" list
        {
            VectorDirrection = (TargetCell - RayOrigin);                            // We get the direction of the vector
            RayTowards = new Ray(RayOrigin, VectorDirrection);                      // Create a ray from the starting position directed to the target position
            Vector3 InFigPos = RayTowards.GetPoint(10);                             // We find the approximate position where the new figure will stand and put in the variable "InaccurateFigurePosition"

            FinderPosition = new Vector3(InFigPos.x, 5, InFigPos.z);     // We put "Finder" over this position.
            RayFinder = new Ray(FinderPosition, Vector3.down);           // Save the ray launched down from the "Finder" to the bottom

            if (Physics.SphereCast(RayFinder, 2, out Hit, 6, LayerCellDetecor | LayerFogCell)) // If a beam launched from top to bottom crashed into something
            {
                if (Hit.transform.tag == "FreeCell")                // If the finder’s beam crashes into a free cell
                {
                    if (!FindCellInThePath(Hit.transform.position, TargetPath)) // If there is no such cell in the list
                    {
                        TargetPath.Add(Hit.transform.position);         // Add a new cell to the path to the target cell.

                        if (Hit.transform.position == TargetСell)       // If we reached the target cell
                            break;                                      // Interrupt the cycle
                        else                                            // Otherwise, if we have not yet reached the target cell
                            RayOrigin = Hit.transform.position;         // Specify a new cell to build the path
                    }
                    else                                            // If such a cell is already on the path list
                    {
                        ClosestCell = FindTheClosestFreeCellWithAWall(TargetPath[TargetPath.Count -1], TargetCell);

                        if (ClosestCell != Vector3.zero)            // If the nearest empty cell was found
                        {
                            TargetPath.Add(ClosestCell);            // Add a new cell to the path to the target cell.
                            RayOrigin = ClosestCell;                // Specify a new cell to build the path
                        }
                        else                                        // Otherwise, if no free cells were found near the wall
                        {// Find any available cell to move

                            List<Vector3> AllCells = new List<Vector3>();   // In this list there will be busy computer cells and only planned
                            AllCells = TargetPath;                          // Put in the list of "AllCells" the planned computer cells

                            for (int a = 0; a < MyСells.Count; a++)           // We continue the cycle until we add the existing computer cells to the "AllCells" list
                            {
                                AllCells.Add(MyСells[a]);                   // Add another cell occupied by the computer to the "AllCells" list
                            }

                            ClosestCell = FindClosestFreeCellArround(TargetPath, TargetCell); // Find the nearest free cell in the list of occupied positions.
                            TargetPath.Add(ClosestCell);            // Add a new cell to the path to the target cell.
                            RayOrigin = ClosestCell;                // Specify a new cell to build the path
                        }
                    }
                }
                else if (Hit.transform.tag == "FogCell")                                                    // Otherwise, if the finder’s ray crashes into the fog cell
                {
                    if (TargetPath.Count != 0)                                                              // If the "TargetPath" list already has a cell
                    {  
                        ClosestCell = FindClosestCellArround(TargetPath[TargetPath.Count - 1], TargetCell); // We find a free cell closest to the enemy around the last recorded in the list

                        if (!FindCellInThePath(ClosestCell, TargetPath)) // If there is no such cell in the list
                        {
                            if (ClosestCell != Vector3.zero)            // If the nearest empty cell was found
                            {
                                TargetPath.Add(ClosestCell);            // Add a new cell to the path to the target cell.
                                RayOrigin = ClosestCell;                // Specify a new cell to build the path
                            }
                            else                                        // If the cell for the move was not found
                            {// Find any available cell to move

                                List<Vector3> AllCells = new List<Vector3>();   // In this list there will be busy computer cells and only planned
                                AllCells = TargetPath;                          // Put in the list of "AllCells" the planned computer cells

                                for (int a = 0; a<MyСells.Count; a++)           // We continue the cycle until we add the existing computer cells to the "AllCells" list
                                {
                                    AllCells.Add(MyСells[a]);                   // Add another cell occupied by the computer to the "AllCells" list
                                }

                                ClosestCell = FindClosestFreeCellArround(TargetPath, TargetCell); // Find the nearest free cell in the list of occupied positions.
                                TargetPath.Add(ClosestCell);            // Add a new cell to the path to the target cell.
                                RayOrigin = ClosestCell;                // Specify a new cell to build the path
                            }
                        }
                        else                                                                                                // If the nearest cell is the cell the last listed cell in the path list
                        {
                            TargetPath.Add(FindTheClosestFreeCellWithAWall(TargetPath[TargetPath.Count - 1], TargetСell));  // We put in the list of paths the closest cell located next to the wall
                            RayOrigin = TargetPath[TargetPath.Count - 1];                                                   // Specify a new cell to build the path
                        }
                    }
                    else                                                                // Otherwise, if this list is empty
                    {
                        ClosestCell = FindClosestCellArround(StartingCell, TargetCell); // Then we search around the first cell

                        if (ClosestCell != StartingCell)                               // If the first cell is not closest to the opponent 
                        {
                            if (ClosestCell != Vector3.zero)                // If the nearest empty cell was found
                            {
                                TargetPath.Add(ClosestCell);                // Add a new cell to the path to the target cell.
                                RayOrigin = ClosestCell;                    // Specify a new cell to build the path
                            }
                            else                                            // If the cell for the move was not found
                            {// Find any available cell to move
                                ClosestCell = FindClosestFreeCellArround(MyСells, TargetCell); // Find the nearest free cell in the list of occupied positions.
                                TargetPath.Add(ClosestCell);            // Add a new cell to the path to the target cell.
                                RayOrigin = ClosestCell;                // Specify a new cell to build the path
                            }
                        }
                        else                                                // Else if the first cell is the closest cell
                        {
                            ClosestCell = FindTheClosestFreeCellWithAWall(ClosestCell, TargetCell);

                            if (ClosestCell != Vector3.zero)            // If the nearest empty cell was found
                            {
                                TargetPath.Add(ClosestCell);            // Add a new cell to the path to the target cell.
                                RayOrigin = ClosestCell;                // Specify a new cell to build the path
                            }
                            else                                        // If the cell for the move was not found
                            {// Find any available cell to move
                                ClosestCell = FindClosestFreeCellArround(MyСells, TargetCell); // Find the nearest free cell in the list of occupied positions.
                                TargetPath.Add(ClosestCell);            // Add a new cell to the path to the target cell.
                                RayOrigin = ClosestCell;                // Specify a new cell to build the path
                            }
                        }
                    }
                }
            }
        }
    }


    Vector3 FindClosestCellArround(Vector3 OriginCell, Vector3 FinalCell)               // This method finds the closest free cell around the specified cell to the final cell in the path.
    {
        Vector3 NearestСell = Vector3.zero;                     // Set the value of the variable "NearestСell" to zero
        Vector3 CurrentOrigin;                                  // CurrentOrigin "Finder" transform position
        List<Vector3> CellsСhallengers = new List<Vector3>();   // List of cells of candidates for the title of the closest cell to the target cell

        Finder.transform.position = new Vector3(OriginCell.x, 5, OriginCell.z); // We put "Finder" over this position.
        CurrentOrigin = Finder.transform.position;                              // Put the current position of the "Finder" in the variable "CurrentOrigin"
        CellsСhallengers.Add(OriginCell);                                       // Add the central cell to the list for comparison.
        CellsСhallengers.AddRange(FindFreeCellsArround(OriginCell));            // Place all free cells around OriginCell and store them in CellsChallengers
        NearestСell = FindTheClosestCellInTheList(CellsСhallengers, FinalCell); // Find the closest cell to the target cell
        return NearestСell;                                                     // Return the cell closest to the target
    }


    Vector3 FindTheClosestFreeCell(Vector3 FirstAIFigure, Vector3 EnemyFigure)      // This method finds the closest empty cell from the selected enemy cell.
    {
        List<Vector3> Applicants = new List<Vector3>();                             // Here there will be cells claiming the title of the closest of the cells
        Vector3 ClosestApplicant = Vector3.zero;                                    // Here will be the closest cell of the applicants

        Applicants = FindFreeCellsArround(EnemyFigure);                             // Find empty cells around the nearest enemy figure
        ClosestApplicant = FindTheClosestCellInTheList(Applicants, FirstAIFigure);  // Find the closest cell in the list and put it in the variable "ClosestApplicant"

        return ClosestApplicant;                            // Return the closest cell from the method
    }


    Vector3 FindTheClosestFreeCellWithAWall(Vector3 OriginCell, Vector3 FinalCell)  // This method finds the closest cell to the target cell, but with the condition that it is next to the wall
    {
        List<Vector3> Applicants = new List<Vector3>();                         // Here there will be cells claiming the closest cell to the enemy but located next to the wall
        Vector3 ClosestApplicant = Vector3.zero;                                // Here will be the closest cell to the target cell located next to the wall

        Applicants = FindFreeCellsArround(OriginCell);                          // Find all the free cells around

        for (int a = 0; a < TargetPath.Count; a++)                              // Weed out the cells that are already in the "TargetPath" list
            for (int b = 0; b < Applicants.Count; b++)
                if (TargetPath[a] == Applicants[b])                             // If such a cell is already in the "TargetPath" list
                    Applicants.RemoveAt(b);                                     // We remove it from the list of applicants

        Applicants = CellsNearTheWall(Applicants, FinalCell);                   // We select only those cells around which are next to the fog cell
        ClosestApplicant = FindTheClosestCellInTheList(Applicants, FinalCell);  // The closest cell to the target cell, and located next to the wall, lay in the variable "ClosestApplicant"

        return ClosestApplicant;                                                // We return the cell corresponding to the requirements
    }


    Vector3 FindTheClosestCellInTheList(List<Vector3> CellList, Vector3 TargetCell)       // This method finds the closest cell from the list to the target cell.
    {
        Vector3 ClosestApplicant = Vector3.zero;            // Here will be the closest cell of the applicants

        for (int i = 0; i < CellList.Count; i++)            // We continue the cycle until we sort through the entire list of applicants
        {
            if (ClosestApplicant == Vector3.zero)           // If we do not have a single applicant
                ClosestApplicant = CellList[0];             // We bring the first applicant as the closest
            else                                            // Otherwise, if we already have any applicant
            {
                float OldDistance = Vector3.Distance(ClosestApplicant, TargetCell); // We measure the distance from the "starting cell AI " until the last candidate cell
                float NewDistance = Vector3.Distance(CellList[i], TargetCell);      // We measure the distance from the "starting cell AI "to the current candidate cell

                if (NewDistance < OldDistance)              // If a new, treated cell is closer than the last
                    ClosestApplicant = CellList[i];         // Put it in the variable "ClosestApplicant"
            }
        }
        return ClosestApplicant;                            // Return the closest cell from the method
    }


    List<Vector3> CellsNearTheWall(List<Vector3> Applicants, Vector3 TargetCell)   // This method filters out cells that are not near the wall. (FogCell)
    {
        Vector3 Current;                                    // The point from which we will shoot a ray to probe the next cell
        List<Vector3> _applicants = new List<Vector3>();    // List of cells next to the fog cell
        RaycastHit FinderRayHit;                            // Here there will be data about the object into which the ray crashed

        for (int a = 0; a < Applicants.Count; a++)          // We continue the cycle until we have passed all the cells in the list of applicants
        {
            Current = new Vector3(Applicants[a].x, 5, Applicants[a].z);  // We place the point at the position from which the ray will shoot

            for (int b = 0; b < 8; b++)                     // We continue the cycle until we check all the cells from the list
            {
                Current = new Vector3(Current.x + (RayDirections[b].x * 10), Current.y, Current.z + (RayDirections[b].z * 10));   // We place the finder on the cell that needs to be checked
                if (Physics.Raycast(Current, Vector3.down, out FinderRayHit)) // If the ray launched into the next cell around the checked cell crashed into an any collider
                {

                    if (FinderRayHit.collider.tag == "FogCell")                                 // If the "tag" of the object into which the beam crashed is "FogCell"
                    {
                        _applicants.Add(Applicants[a]);     // We put in the list another free cell located next to the fog cell
                        break;
                    }
                }
            }
        }

        return _applicants;
    }


    Vector3 FindACellAdjacentToTheEnemyFigure(List<Vector3> Cells)              // This method finds the first cell from the list adjacent to the enemy figure, and returns it from the method
    {
        Vector3 SelectedСell = Vector3.zero;    // Set the initial values of the variable "SelectedCell"
        Vector3 Current;                        // CurrentOrigin "Finder" transform position
        RaycastHit FinderRayHit;                // Here there will be data about the object into which the ray crashed

        for (int a = 0; a < Cells.Count; a++)   // We continue the list until we sort through all the cells from the "Cells" list
        {
            Finder.transform.position = new Vector3(Cells[a].x, 5, Cells[a].z);   // We place the finder directly above the cell of this cycle
            Current = Finder.transform.position;    // Put the current position of the "Finder" in the variable "CurrentOrigin"
            for (int b = 0; b < 8; b++)             // We continue the cycle until we check all the cells around the next cell in the "Cells" list
            {
                Finder.transform.position = new Vector3(Current.x + (RayDirections[b].x * 10), Current.y, Current.z + (RayDirections[b].z * 10));   // We place the finder on the cell that needs to be checked
                if (Physics.Raycast(Finder.transform.position, Vector3.down, out FinderRayHit)) // If the ray launched into the next cell around the checked cell crashed into an any collider
                    if (FinderRayHit.collider.tag == "PlayerFigure")                            // If the "tag" of the object into which the beam crashed is "PlayerFigure"
                        // We ask which player this figure belongs to, and if it is not attached to this computer player
                        if (FinderRayHit.transform.gameObject.GetComponent<PlayerFigure>().NomberPlayer != PlayerNomber)
                            return Cells[a];    // Then we return this suitable cell from the method 
            }
        }
        return SelectedСell;                    // Return the cell selected for the move from the method
    }


    List<Vector3> FindFreeCellsArround(Vector3 OriginCell)  // This method finds free cells around the specified figure.
    {
        Vector3 FinderOrigin;                               // The point from which we will shoot a ray to probe the next cell
        RaycastHit FinderRayHit;                            // Here there will be data about the object into which the ray crashed
        List<Vector3> FreeCells = new List<Vector3>();      // List of found free cells

        FinderOrigin = new Vector3(OriginCell.x, 5, OriginCell.z);  // We position the position "Current" above the central cell
        for (int i = 0; i < 8; i++)                                 // We continue the cycle until we check all the cells around
        {
            FinderOrigin = new Vector3(OriginCell.x + (RayDirections[i].x * 10), FinderOrigin.y, OriginCell.z + (RayDirections[i].z * 10));
            if (Physics.Raycast(FinderOrigin, Vector3.down, out FinderRayHit))  // If the ray launched into the next cell around the checked cell crashed into an any collider
                if (FinderRayHit.collider.tag == "FreeCell")                    // If the "tag" of the object into which the beam crashed is "FreeCell"
                    FreeCells.Add(FinderRayHit.transform.position);             // Add another empty candidate cell   
        }

        return FreeCells;                                   // We return a list of found free cells around the specified figure.
    }


    Vector3 FindClosestFreeCellArround(List<Vector3> ListOfPositions, Vector3 TargetPosition)        // This method finds the closest free figure among the list of occupied positions.
    {
        Vector3 ClosestFreeCell = Vector3.zero;                 // Here will be the nearest empty cell to the enemy
        List<Vector3> ListOfApplicants = new List<Vector3>();   // List of empty cells of applicants for the closest cell to the enemy
        Vector3 FinderOrigin;                                   // The point from which we will shoot a ray to probe the next cell
        RaycastHit FinderRayHit;                                // Here there will be data about the object into which the ray crashed

        for (int a = 0; a < ListOfPositions.Count; a++)     // We continue the cycle until we sort through all the points in the "ListOfPositions" list
        {
            FinderOrigin = new Vector3(ListOfPositions[0].x, 5, ListOfPositions[0].y);  // We put FinderOrigin position above the next position in the list.
            for (int b = 0; b < RayDirections.Length; b++)  // We continue the cycle until we check all the points for "accessibility" around the point being checked
            {   // place "FinderOrigin" over the next checked cell
                FinderOrigin = new Vector3(ListOfPositions[a].x + (RayDirections[b].x * 10), FinderOrigin.y, ListOfPositions[a].z + (RayDirections[b].z * 10));
                if (Physics.Raycast(FinderOrigin, Vector3.down, out FinderRayHit))       // If the ray launched into the next cell around the checked cell crashed into an any collider
                {
                    if (FinderRayHit.collider.tag == "FreeCell")                         // If the "tag" of the object into which the beam crashed is "FreeCell"
                    {
                        if (!ListOfPositions.Contains(FinderRayHit.transform.position)) // If this point is not in the list of positions planned for the move
                        {
                            if (!ListOfApplicants.Contains(FinderRayHit.transform.position))    // If this point has not yet been listed "ListOfApplicants"
                            {
                                ListOfApplicants.Add(FinderRayHit.transform.position);      // Add the appropriate applicant cell to the "ListOfApplicants" list.
                            }
                        }
                    }
                }
            }
        }

        ClosestFreeCell = FindTheClosestCellInTheList(ListOfApplicants, TargetPosition);

        return ClosestFreeCell;
    }


    bool FindCellInThePath(Vector3 Sample, List<Vector3> Path)  // This method indicates whether such a cell is already in the path list.
    {
        bool Finded = false;                    // This variable indicates: whether a match was found.

        for (int i = 0; i < Path.Count; i++)    // We go through the cycle until we go through the entire list of paths
        {
            if (Sample == Path[i])              // If a match was found
                Finded = true;                  // We indicate that such a cell is already in the list. 
        }

        return Finded;
    }


    Vector3 FindFirstCell(out bool FreeCellFound)   // This method tries to find the central cell in the field (the most profitable for the first move)
    {
        GameObject ExternalRendererGroup;           // An external group of colliders will be stored here.
        int ChildCount;                             // The number of external colliders will be stored here.
        Vector3 Sum = Vector3.zero;                 // Here, the sum of the numbers of collider positions for each axis will be stored separately.
        Vector3 CentralPoint = Vector3.zero;        // Here the point located in the center of this field will be stored
        RaycastHit Hit;                             // Here the object into which the beam crashed is stored
        Vector3 FinderPosition;                     // The position of the point from which the next cell is probed
        Ray RayFinder;                              // Here is stored the ray launched from the "FinderPos" to the bottom
        Vector3 CalculatedPosition = Vector3.zero;  // This will store the calculated position for the first move.            

        ExternalRendererGroup = GameObject.FindWithTag("LineRendererGroup").transform.GetChild(0).gameObject;   // Find the outer group of colliders
        ChildCount = ExternalRendererGroup.transform.childCount;                                                // We put in the "ChildCount" the number of colliders in the external line

        for (int i = 0; i < ChildCount; i++)        // We continue the cycle until we sort through all the colliders of the external Linerenderer
            Sum += ExternalRendererGroup.transform.GetChild(i).transform.position;  // Find the sum of all numbers

        CentralPoint = new Vector3(Sum.x / ChildCount, 0, Sum.z / ChildCount);      // We calculate the best position for the starting figure (Based on all points of the line averaging them)

        char[] ChCPX = CentralPoint.x.ToString().ToCharArray();     // Convert "CentralPoint.x" to an array of characters
        char[] ChCPZ = CentralPoint.z.ToString().ToCharArray();     // Convert "CentralPoint.z" to an array of characters

        float IncriminateX = Random.Range(0, 2);                    // Assign the variable "IncriminateX" either 0 or 1
        float IncriminateZ = Random.Range(0, 2);                    // Assign the variable "IncriminateZ" either 0 or 1

        if (ChCPX[ChCPX.Length - 1] == '0')                         // If "CentralPoint.x" stands, it is not in the center of the cell along the x axis
        {
            if (IncriminateX == 0)                                                                  // If "IncriminateX" has a random value of 0
                CentralPoint = new Vector3(CentralPoint.x + 5 , CentralPoint.y, CentralPoint.z);    // Then we shift the value of "CentralPoint.x" to plus by 5 units
            else                                                                                    // Otherwise, if "IncriminateX" has a random value 1
                CentralPoint = new Vector3(CentralPoint.x - 5, CentralPoint.y, CentralPoint.z);     // Then we shift the value of "CentralPoint.x" to minus 5 units
        }

        if (ChCPZ[ChCPZ.Length - 1] == '0')                                                         // If "CentralPoint.z" stands, it is not in the center of the cell along the z axis
        {
            if (IncriminateZ == 0)                                                                  // If "IncriminateZ" has a random value of 0
                CentralPoint = new Vector3(CentralPoint.x, CentralPoint.y, CentralPoint.z + 5);     // Then we shift the value of "CentralPoint.z" to plus by 5 units
            else                                                                                    // Otherwise, if "IncriminateZ" has a random value 1
                CentralPoint = new Vector3(CentralPoint.x, CentralPoint.y, CentralPoint.z - 5);     // Then we shift the value of "CentralPoint.z" to minus 5 units  
        }

        FinderPosition = new Vector3(CentralPoint.x, 5, CentralPoint.z);            // We put "Finder" over "CentralPoint" position.
        RayFinder = new Ray(FinderPosition, Vector3.down);                          // Save the ray launched down from the "Finder" to the bottom

        if (Physics.SphereCast(RayFinder, 2, out Hit, 6, LayerCellDetecor | LayerFogCell | LayerFigure))// We shoot with a spherical ray at this point and if the ray crashed into a collider
        {
            if (Hit.collider.tag == "FreeCell")                                                         // If the "tag" of the object into which the ray crashed is "FreeCell"
            {
                CalculatedPosition = Hit.transform.position;                                            // Here will be the best position for the first computer figure
                FreeCellFound = true;                                                                   // We indicate that the cell for the first move was not found
                return CalculatedPosition;                                                              // Return the calculated position
            }
            else                                                                                        // Otherwise, if the ray crashed into a fog cell or an enemy cage
            {
                int TickCounter = 0;                // The tick counter at the beginning is 0
                int LengthСounter = 1;              // This counter counts how many steps you need to take before turning. Every two cycles it increases its length

                for (int a = 0; a < 17; a++)        // We continue the cycle until we find an empty cell as close as possible to the center of the playing field
                {
                    for (int b = 0; b < 4; b++)     // Continue the cycle until we make a full circle clockwise (Until we go through 4 directions)
                    {
                        for (int c = 0; c < LengthСounter; c++) // We continue the cycle until we look through all the cells assigned in this direction
                        {
                            // Update position: "FinderPosition"
                            FinderPosition = new Vector3(FinderPosition.x + (RayDirFirsFig[b].x * 10), FinderPosition.y, FinderPosition.z + (RayDirFirsFig[b].z * 10));
                            RayFinder.origin = FinderPosition;  // Updating the beam position

                            // We shoot with a spherical ray at this point and if the ray crashed into a collider
                            if (Physics.SphereCast(RayFinder, 2, out Hit, 6, LayerCellDetecor | LayerFogCell | LayerFigure))
                            {
                                if (Hit.collider.tag == "FreeCell")                 // If the "tag" of the object into which the ray crashed is "FreeCell"
                                {
                                    CalculatedPosition = Hit.transform.position;    // Here will be the best position for the first computer figure
                                    FreeCellFound = true;                           // We indicate that the cell for the first move was not found
                                    return CalculatedPosition;                      // Return the calculated position
                                }
                            }
                        }

                        TickCounter++;          // Increase the tick counter by 1
                        if (TickCounter == 2)   // If the tick counter becomes equal to two
                        {
                            TickCounter = 0;    // Zero tick counter
                            LengthСounter++;    // We increase by 1 counter long
                        }
                    }
                }
            }
        }

        FreeCellFound = false;                  // We indicate that the cell for the first move was not found
        return CalculatedPosition;              // Return the calculated position
    }


    void MethodPlayNextLevel()                                  // This method is called the "PlayNextLevel" event.
    {
        MyСells.Clear();                                        // We clear the list of computer cells on the field so that he knows that his cells are not on the field
        TheNextMoveIsAvailable = true;                          // We indicate that by default the next move is available
    }

    void OnDisable()
    {
        GM.PlayNextLevel -= MethodPlayNextLevel;               // We write off the method "MethodPlayNextLevel" from the event "PlayNextLevel"
    }
}


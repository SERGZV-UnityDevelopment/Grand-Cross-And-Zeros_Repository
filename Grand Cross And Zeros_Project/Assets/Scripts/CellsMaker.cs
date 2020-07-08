using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Этот скрипт чертит линию ограждающую игровую площадь и на основе её расставляет клетки тумана


public class CellsMaker : MonoBehaviour
{
    public GameObject Registrator;                              // This object will pass through the colliders and register the passage through each of them.
    Camera Cam;
    bool Saved = true;                                          // This variable says whether "DotDetectors" were added to the array of detectors.
//    float NomberSavedPoints;
    List<Vector3> DotCol = new List<Vector3>();                 // "DotColliders" This list will contain the positions of those "sdfsfd" on which the player swiped his finger             



    public SaveLoad SL;                                         // Here is the script "SaveLoad"
    public GameObject[] FogCells = new GameObject[4];           // Array of Prefab Fog Cells
    public GameManager GM;                                      // Variable storing object of class GameManager
    public UIScript UIS;                                        // Variable for object of class "UIScript"
    public Material[] LineRendererMaterials;                    // Материал для линии
    int LayerMask = 1 << 9;                                     // Маска для луча которая определяет что он будет сталькиваться только с коллайдерами со слоя "DotDetectors"
//    int FogCellMask = 1 << 11;                                // The mask for the beam determines that it will collide with the cells of the fog
    public bool TheCamHasReach = false;                         // This variable indicates whether the main camera has reached the destination by playing the animation and hanging over the line drawing table.
    public byte MinimumLength;                                  // Minimum length of the main line in points
    //----------------------------------------------------------------------------------- Переменные для текущей линии -----------------------------------------------------------------------------------
    public byte NumberLine = 0;                                         // Line number we are working with now
    public int ActiveLinePoint = 0;                                     // Active point (active line), the point with which we are currently working
    public List<Vector3> SelPo = new List<Vector3> { };                 // "Selected Points" Список точек из которых состоит текущая линия
    public List<GameObject> LineRenderersGOs = new List<GameObject> { };// LineRenderers "GameObjects" В этом списке будут храниться объекты группы "LineRendererGroup"
    LineRenderer GLR;                                                   // Variable for component "_lineRenderer", ghost line
    Ray CheckLineCollider;                                              // Ray for checking the presence of a line collider for a given point
    RaycastHit HitCheckLineCol;                                         // Ray Object Information "CheckLineCol"
    Ray ray;                                                    // Луч пущенный из начала камеры к пальцу
    RaycastHit hit;                                             // Эта переменная нужна чтобы определять детекторы на которые указала стрелка или палец
    Vector3 HTP;                                                // Сдесь будет храниться лишь позиция того объекта в который ударилься луч, чтобы можно было короче обращаться к этой позиции
    bool XdotDiffers;                                           // Эта переменная определяет различаються ли последняя точка в списке по оси "X" и точка объекта по оси "X" в переменной HTP
    bool ZdotDiffers;                                           // Эта переменная определяет различаються ли последняя точка в списке по оси "Z" и точка объекта по оси "Z" в переменной HTP
    bool XdotNotDiffersTen;                                     // Эта переменная указывает правда если нет разницы более чем на 10 юнитов между точкой по оси "X" в списке и точкой в HTP по оси "X"
    bool ZdotNotDiffersTen;                                     // Эта переменная указывает правда если нет разницы более чем на 10 юнитов между точкой по оси "Z" в списке и точкой в HTP по оси "Z"
    public GameObject BorderLine;                               // Переменная для последнего созданного объекта на котором весит последний созданый LineRendrer
    public List<GameObject> Empty = new List<GameObject>();     // The list of empties to which the colliders will be attached
    public bool LineHasBeenDelineated;                          // This variable determines whether the current line is drawn.
    public bool DrawingALineIsProhibited = false;               // Variable indicating whether to draw a line
    //---------------------------------------------------------------- Переменные для всех линий ---------------------------------------------------------------------------------------------------------

    public List<LineRenderer> LRs = new List<LineRenderer> { };         // Список компонентов "_lineRenderer" для линии рисующей поле
    public byte NumberLines = 0;                                        // This variable indicates how many lines now exist.
    public List<Line> LineList = new List<Line>();                      // The coordinates of points of all lines at the level will be stored here.
    public List<CellGroup> CellGroupList = new List<CellGroup>();       // This list contains all the groups of cells on the map.

    Vector3[] RayDirections = new Vector3[4];                           // Массив с 4 направлениями лучей
    Vector3[] RayDiagonallyDirections = new Vector3[4];                 // Массив с 4 направлениями лучей но по диагонали  


    void OnEnable()
    {
        GM.SetStateMenu += MethodSetStateMenu;                          // We sign the method "MethodSetStateMenu" to the event "SetStateMenu"
        GM.OneLineWasFinished += MethodOneLinWasFinished;               // Подписываем метод "MethodOneLineWasFinished" на событие "Одна линия была закончена"
        GM.PlayGame += MethodPlayGame;                                  // Подписываем метод "MethodPlayGame" на событие "PlayGame"
        GM.DrawTheFollowingLine += MethodDrawTheFollowingLine;          // We sign the "MethodDrawTheFollowingLine" method for the event "DrawTheFollowingLine"
        GM.LoadGameLevel += MethodLoadGameLevel;                        // We sign the method "MethodLoadGameLevel" to the event "LoadGameLevel"
    }


    private void Start()
    {
        Cam = Camera.main;
        RayDirections[0] = new Vector3(1, 0, 0);                        // Луч пускаемый в верхнюю границу доски
        RayDirections[1] = new Vector3(0, 0, -1);                       // Луч пускаемый в правую границу доски
        RayDirections[2] = new Vector3(-1, 0, 0);                       // Луч пускаемый в нижнюю границу доски
        RayDirections[3] = new Vector3(0, 0, 1);                        // Луч пускаемый в левую границу доски

        RayDiagonallyDirections[0] = new Vector3(1, 0, -1);             // Луч пускаемый в верх и право наискоски
        RayDiagonallyDirections[1] = new Vector3(-1, 0, -1);            // Луч пускаемый в низ и в право наискоски
        RayDiagonallyDirections[2] = new Vector3(-1, 0, 1);             // Луч пускаемый в низ и в лево наискоски   
        RayDiagonallyDirections[3] = new Vector3(1, 0, 1);              // Луч пускаемый в верх и в лево наискоски
    }



    void Update()
    {
        CreateLines();                                                  // This method draws a line
    }


    void MethodSetStateMenu()                                               // The method called by the event "SetStateMenu"
    {
        LineHasBeenDelineated = false;                                      // We give permission to draw a line   
        SelPo.Clear();                                                      // Сlear the list "SelPo"
        NumberLines = 0;                                                    // Zeroing the value of a variable that counts the number of lines in a game
    }


    void CreateLines()                                                      // Method of drawing lines
    {
        if (Input.GetMouseButton(0))
        {
            if (TheCamHasReach && !GM.BlockingMoves && !DrawingALineIsProhibited)
            {
                ray = Cam.ScreenPointToRay(Input.mousePosition);                        // Ложим в "ray" начальные координаты луча и его направление из камеры к стрелке
                UIS.Panels[45].SetActive(true);                                         // We activate the icon indicator that the ray hit the "DotCollider"

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask))           // Let the beam go and if it hits the object on the layer “LayerMask” or “FogCellMask”,
                {
                    DotCol.Add(hit.collider.transform.position);                        // Add the position of the next collider in the loop to the "DotCol" list
                    UIS.Panels[45].transform.position = Camera.main.WorldToScreenPoint(hit.collider.transform.position);   // Assign a red dot to a position at the end of the line.
                }

                Saved = false;                                                      // We indicate that the line has not yet been saved.
            }
        }
        else if(!Input.GetMouseButton(0) && Saved == false)
        {
            BoxCollider BoxCol;                                                     // Variable for the _lineRenderer collider with which we are currently working
            bool DotMatchFound = false;                                             // This variable stores the state (are we trying to put a new point on the drawn line)

            for (int a = 0; a < DotCol.Count; a++)
            {
                if (SelPo.Count == 0)                                               // If there are no dots in the "SelPo" list
                {
                    DotMatchFound = false;                                                          // This variable holds the state (whether a match is detected or not)
                    Vector3 TestV = DotCol[a];                                                      // Place the position of the object in which the ray "ray" crashed into the variable "TestV"
                    TestV = new Vector3(TestV.x, TestV.y + 2, TestV.z);                             // Update the "TestV" position by shifting it two units higher.
                    CheckLineCollider = new Ray(TestV, Vector3.down);                               // Determine that the "CheckLineCollider" ray is from their "TestV" point and is pointing down

                    if (Physics.Raycast(CheckLineCollider, out HitCheckLineCol, Mathf.Infinity))    // If the “CheckLineCollider” beam crashed into a drawn line collider
                        if (HitCheckLineCol.collider.name == "BorderLineCollider")                  // If the checking ray crashed into an object with the name "BorderLineCollider"
                            DotMatchFound = true;                                                   // Then we set that a match was found

                    if (HitCheckLineCol.collider.tag != "FogCell")                                  // If the beam hit the "DotCollider" and not the "FogCell"
                    {
                        if (!DotMatchFound)                                                 // If the beam does not crash into the collider of the drawn line when checking
                        {
                            SelPo.Add(DotCol[a]);                                           // Then we put in it the first point
                            BorderLine = new GameObject();                                  // Create a new object for the variable "BorderLine"
                            GameObject GhostLine = new GameObject();                        // Create a new object for the variable "GhostLine"
                            BorderLine.transform.SetParent(GameObject.FindGameObjectWithTag("LineRendererGroup").transform);    // Adding this object to the group with LineRenderers
                            GhostLine.transform.SetParent(GameObject.FindGameObjectWithTag("LineRendererGroup").transform);     // Adding this object to the group with LineRenderers
                            BorderLine.name = "BorderLine " + NumberLines;                  // Call it "BorderLine" and specify the number of this object in the name
                            GhostLine.name = "GhostLine";                                   // Call it "GhostLine"
                            GhostLine.tag = "GhostLine";                                    // Give a ghost line tag "GhostLine"
                            LineRenderer LR = BorderLine.AddComponent<LineRenderer>();      // We hang the component "_lineRenderer" on this object and put it into the variable "LR"
                            GLR = GhostLine.AddComponent<LineRenderer>();                   // We hang the component "_lineRenderer" on this object and put it into the variable "GLR"
                            LRs.Add(LR);                                                    // Add this component _lineRenderer to the list with LineRenders
                            LR.positionCount = 1;                                           // Set the length of the list of points of the component "_lineRenderer" from the variable "LR"
                            GLR.positionCount = 1;                                          // Set the length of the list of points of the component "_lineRenderer" from the variable "GLR"
                            LR.SetPosition(0, DotCol[a]);                                   // The first point is also entered into the zero slot of the _lineRenderer points.
                            GLR.SetPosition(0, new Vector3(DotCol[a].x, DotCol[a].y - 0.1f, DotCol[a].z));// We transfer the first point to the beginning of the ghost line. But we make it lower by 0.1 units.
                            LR.startWidth = 1.5f;                                           // Line thickness in the variable "LR" at the beginning
                            LR.endWidth = 1.5f;                                             // Line thickness in the variable "LR" at the end
                            GLR.startWidth = 1.5f;                                          // Line thickness in the variable "GLR" at the beginning
                            GLR.endWidth = 1.5f;                                            // Line thickness in the variable "GLR" at the end
                            LR.material = LineRendererMaterials[0];                         // Assign the red material to the short "_lineRenderer"
                            GLR.material = LineRendererMaterials[2];                        // Assign the gray material to the "Ghost line"
                            LineRenderersGOs.Add(BorderLine);                               // Put a new GameObject - "BorderLine" in the list of LineRenderersGOs
                            NumberLines++;                                                  // We indicate that there was a "born" new line
                            GM.CallTheFirstPointIsSet();                                    // Call the method to trigger the event "TheFirstPointIsSet"
                        }
                    }
                }
                else                                                                // Otherwise, if the list contains one or more points, then it's time to draw a line
                {
                    if (SelPo[ActiveLinePoint].x != DotCol[a].x)                    // If the last point in the line along the x axis is different from the next collider along the x axis
                        XdotDiffers = true;                                         // Then we set the value of the variable "XdotDiffers" true
                    else                                                            // Else
                        XdotDiffers = false;                                        // XdotDiffers equals false

                    if (SelPo[ActiveLinePoint].z != DotCol[a].z)                    // If the points of the detected object along the "Z" axis and the last point in the list differ along the "Z" axis
                        ZdotDiffers = true;                                         // Then we set the value of the variable "ZdotDiffers" true
                    else                                                            // Else
                        ZdotDiffers = false;                                        // "ZdotDiffers" equals false

                    XdotNotDiffersTen = ComparisonOfNumbers(SelPo[ActiveLinePoint].x, DotCol[a].x, 10.1f);   // Find out if these two values are more than 10 units
                    ZdotNotDiffersTen = ComparisonOfNumbers(SelPo[ActiveLinePoint].z, DotCol[a].z, 10.1f);   // Find out if these two values are more than 10 units

                    if (XdotDiffers && !ZdotDiffers || !XdotDiffers && ZdotDiffers)     //  If the last point in the line differs in only one axis
                    {
                        if (!XdotNotDiffersTen && !ZdotNotDiffersTen)
                        {
                            DotMatchFound = false;                                                          // This variable holds the state (whether a match is detected or not)
                            Vector3 TestV = DotCol[a];                                                      // Place the position of the object in which the ray "ray" crashed into the variable "TestV"
                            TestV = new Vector3(TestV.x, TestV.y + 2, TestV.z);                             // Update the "TestV" position by shifting it two units higher.
                            CheckLineCollider = new Ray(TestV, Vector3.down);                               // Determine that the "CheckLineCollider" ray is from their "TestV" point and is pointing down

                            if (Physics.Raycast(CheckLineCollider, out HitCheckLineCol, Mathf.Infinity))    // If the “CheckLineCollider” beam crashed into a drawn line collider
                                if (HitCheckLineCol.collider.name == "BorderLineCollider")                  // If the checking ray crashed into an object with the name "BorderLineCollider"
                                    DotMatchFound = true;                                                   // Then we set that a match was found

                            if (MinimumLength <= ActiveLinePoint + 1)                                       // If the line is longer than the variable "MinimumLength"
                                LRs[LRs.Count - 1].material = LineRendererMaterials[1];                     // Assign Standard Black Material to "_lineRenderer"

                            if (SelPo[0] != DotCol[a])                                                      // If the starting point of the line and the next in the cycle are different
                            {
                                if (!DotMatchFound)                                                         // If the beam does not crash into the collider of the drawn line when checking
                                {
                                    DeleteGhostLine();                                                                          // We call the method shortening the line
                                    SelPo.Add(DotCol[a]);                                                                       // List a new point in the list.
                                    ActiveLinePoint = SelPo.Count - 1;                                                          // Set the last point in the line as active.
                                    LRs[LRs.Count - 1].positionCount++;                                                         // We extend the number of points in _lineRenderer by one
                                    LRs[LRs.Count - 1].SetPosition(SelPo.Count - 1, DotCol[a]);                                 // The last point from the list is entered into the last point of the _lineRenderer.
                                    GLR.positionCount++;                                                                        // Expand the list of points of the ghost line by one point
                                    GLR.SetPosition(SelPo.Count - 1, new Vector3(DotCol[a].x, DotCol[a].y - 0.1f, DotCol[a].z));// SaveGame the selected point to the last point of the ghost line but 0.1 unit below

                                    Ray raypoz = new Ray();                                                                     // Create a new beam
                                    raypoz.origin = SelPo[SelPo.Count - 1];                                                     // Take the last point from the list of points and assign it as the center of the beam
                                    raypoz.direction = Vector3.Normalize((SelPo[SelPo.Count - 2]) - (SelPo[SelPo.Count - 1]));  // Calculate the direction of the beam
                                    Empty.Add(new GameObject());                                                                // Create a GameObject "Empty" for the BoxCollider component
                                    Empty[Empty.Count - 1].transform.SetParent(BorderLine.transform);                           // We adopt the next empty _lineRenderer object
                                    Empty[Empty.Count - 1].name = "BorderLineCollider";                                         // We give this empty the name "BorderLineCollider"
                                    Empty[Empty.Count - 1].layer = 14;                                                          // Specify the line layer as "LineCollider"

                                    if (NumberLines == 1) Empty[Empty.Count - 1].tag = "OutsideLine";                           // If this is the first line Then we give it to the collider the tag "Outside line"
                                    else Empty[Empty.Count - 1].tag = "InsideLine";                                             // Otherwise, if this is not the first line Give her a name "InsideLine"  

                                    Empty[Empty.Count - 1].transform.position = raypoz.GetPoint(5);                     // Calculate the position for the new collider and put it there
                                    BoxCol = Empty[Empty.Count - 1].AddComponent<BoxCollider>();                        // Create a BoxCollider and assign it to a empty
                                    if (SelPo[SelPo.Count - 1].x != SelPo[SelPo.Count - 2].x)                           // If the last and the penultimate point in the list differs along the "X" axis
                                        BoxCol.size = new Vector3(11, 1, 1);                                            // Then we make the collider long on the axis "X"
                                    else                                                                                // Otherwise, if the last and the last but one point in the list is different along the "Z" axis.
                                        BoxCol.size = new Vector3(1, 1, 11);                                            // Then we make the collider long along the "Z" axis
                                    GM.CallTheNextPointIsSet();                                                         // Call the method to trigger the event "TheNextPointIsSet"
                                }
                            }
                            else                                    // Otherwise, if the starting point of the line and the next in the cycle are the same
                            {
                                if (SelPo.Count >= MinimumLength)   // And if the line is longer than the "minimum length", then the edges of the field were outlined
                                {
                                    DeleteGhostLine();              // We call the method shortening the line
                                    SelPo.Add(SelPo[0]);                                            // Write the last point in the "Selection Points" list
                                    LRs[LRs.Count - 1].positionCount++;                                                 // We extend the number of points in _lineRenderer by one
                                    LRs[LRs.Count - 1].SetPosition(SelPo.Count - 1, DotCol[a]);                         // The last point from the list is entered into the last point of the _lineRenderer.

                                    Ray raypoz = new Ray();                                                             // Создаём новый луч
                                    raypoz.origin = SelPo[SelPo.Count - 1];                                             // Берём прошлую точку из списка точек и назначаем её как центр луча
                                    raypoz.direction = Vector3.Normalize((SelPo[SelPo.Count - 2]) - (SelPo[SelPo.Count - 1]));  // Calculate the direction of the beam
                                    Empty[Empty.Count - 1] = new GameObject();                                          // Создаём GameObject "Empty" для компонента BoxCollider
                                    Empty[Empty.Count - 1].transform.SetParent(BorderLine.transform);                   // Удочеряем очередную Empty объекту _lineRenderer
                                    Empty[Empty.Count - 1].name = "BorderLineCollider";                                 // Даём ей имя "КоллайдерЛинии"
                                    Empty[Empty.Count - 1].layer = 14;                                                  // Specify the line layer as "LineCollider"

                                    if (NumberLines == 1) Empty[Empty.Count - 1].tag = "OutsideLine";                   // If this is the first line Then we give it to the collider the tag "Outside line"
                                    else Empty[Empty.Count - 1].tag = "InsideLine";                                     // Otherwise, if this is not the first line Give her a name "InsideLine"   

                                    Empty[Empty.Count - 1].transform.position = raypoz.GetPoint(5);                     // Вычисляем местоположение для нового коллайдера
                                    BoxCol = Empty[Empty.Count - 1].AddComponent<BoxCollider>();                        // Создаём кубичесский коллайдер и присваеваем его пустышке
                                    if (SelPo[SelPo.Count - 1].x != SelPo[SelPo.Count - 2].x)                           // If the last and the penultimate point in the list differs along the "X" axis
                                        BoxCol.size = new Vector3(11, 1, 1);                                            // То делаем коллайдер длинным по оси "X"
                                    else                                                                                // Иначе если последняя и предпоследняя точка в списке различаються по оси "Z"
                                        BoxCol.size = new Vector3(1, 1, 11);                                            // То делаем коллайдер длинным по оси "Z"

                                    Line NewLine = new Line();                                                          // Create a new object to save the line "line"  
                                    NewLine.AddRange(SelPo);                                                            // Fill the new line with dots from the just drawn line "_lineRenderer"
                                    LineList.Add(NewLine);                                                              // Add a new line to the line list at the "LineList" level

                                    LineHasBeenDelineated = true;                                                       // We note that this line was completed.
                                    DrawingALineIsProhibited = true;                                                    // We indicate that we prohibit drawing a line
                                    UIS.Panels[41].SetActive(false);                                                    // Make the flag image active.
                                    GM.CallOneLinWasFinished();                                                         // We call the method that raises the event "One line was finished"
                                    break;                                                                              // "Break the DotColliders" bust cycle
                                }
                            }
                        }
                    }
                }
            }

            DotCol.Clear();                                         // Clearing the "DotCol" List
            Saved = true;                                           // We indicate that the line has been saved
        }
    }


    void DeleteGhostLine()  // The method shortens the line by removing the ghost line, if we decided to draw it in the other direction.
    {
        // If the number of active points and the size of the list of points do not match delete in the loop all points in the list "SelPo" after the active point
        for (int a = ActiveLinePoint; a + 1 < SelPo.Count;)
        {
            SelPo.RemoveAt(SelPo.Count - 1);        // Delete another point
            GLR.positionCount--;                    // Remove extra points in the ghost line
        }
    }


    public void LoadMap()
    {
        Level lvl = new Level();                    // Here we will store the level that we will load

        if (GM.GameType == GameManager.GameTypes.CustomMapGame)                 // If we are going to Load the custom level
        {
            lvl = SL.SLO.CustomLevels.LevelList[UIS.SelectedCustomMap - 1];     // Load the selected level from the save in the variable "lvl" class "Level"  
        }
        else if (GM.GameType == GameManager.GameTypes.Walkthrough)              // Else if we are going to Load the level of passage
        {
            SL.LoadPassageLevel();                  // load the level from the binary file into the variable SL.CurrentPassageLevel
            lvl = SL.CurrentPassageLevel;           // We put in the variable "lvl" the current loaded level of passage
        }

        GameObject FCO;                                                         // Fog cell object
        BoxCollider FCCol;                                                      // Fog cell collider
        LineRenderer LR;                                                        // Variable for the _lineRenderer component processed in this cycle
        BoxCollider BoxCol;                                                     // Variable for the _lineRenderer collider with which we are currently working

        for (int a = 0; a < lvl.Lines.Count; a++)                               // Each iteration of this cycle creates a new line.
        {
            BorderLine = new GameObject();                                                                      // Create a new object for the variable "BorderLine"
            BorderLine.transform.SetParent(GameObject.FindGameObjectWithTag("LineRendererGroup").transform);    // Adding this object to the group with LineRenderers
            BorderLine.name = "BorderLine " + NumberLines;                                                      // Call it "BorderLine" and specify the number of this object in the name
            LR = BorderLine.AddComponent<LineRenderer>();                                                       // We hang the component "_lineRenderer" on this object and put it into the variable "LR"
            LRs.Add(LR);                                                                                        // Add this component _lineRenderer to the list with LineRenders
            LR.positionCount = lvl.Lines[a].Points.Count;                                                       // Set the component "_lineRenderer" the total number of points in the line

            for (int b = 0; b < lvl.Lines[a].Points.Count; b++)                                                 // In this cycle, stored points are loaded for the next line.
            {
                LR.SetPosition(b, lvl.Lines[a].Points[b]);                                                      // Install the newly loaded line the next point from the saved line of the saved level
                if (b > 0)
                {
                    Ray raypoz = new Ray();                                                                     // Create a new beam
                    raypoz.origin = LR.GetPosition(b - 1);                                                      // ???
                    raypoz.direction = Vector3.Normalize(LR.GetPosition(b) - LR.GetPosition(b - 1));            // Calculate the direction of the beam
                    Empty.Add(new GameObject());                                                                // Create a GameObject "Empty" for the BoxCollider component
                    Empty[Empty.Count - 1].transform.SetParent(BorderLine.transform);                           // We adopt the next empty _lineRenderer object
                    Empty[Empty.Count - 1].name = "BorderLineCollider";                                         // We give this empty the name "BorderLineCollider"
                    Empty[Empty.Count - 1].layer = 14;                                                          // Specify the line layer as "LineCollider"

                    if (NumberLines == 0) Empty[Empty.Count - 1].tag = "OutsideLine";                           // If now in the process of loading is the first line, then we give it to the collider the tag "Outside line"
                    else Empty[Empty.Count - 1].tag = "InsideLine";                                             // Otherwise, if this is not the first line Give her a name "InsideLine"  

                    Empty[Empty.Count - 1].transform.position = raypoz.GetPoint(5);                             // Calculate the position for the new collider and put it there
                    BoxCol = Empty[Empty.Count - 1].AddComponent<BoxCollider>();                                // Create a BoxCollider and assign it to a empty
                    if (LR.GetPosition(b).x != LR.GetPosition(b - 1).x)                                         // If the last and the penultimate point in the list differs along the "X" axis
                        BoxCol.size = new Vector3(11, 1, 1);                                                    // Then we make the collider long on the axis "X"
                    else                                                                                        // Otherwise, if the last and the last but one point in the list is different along the "Z" axis.
                        BoxCol.size = new Vector3(1, 1, 11);                                                    // Then we make the collider long along the "Z" axis
                }

            }
            LR.material = LineRendererMaterials[1];                                                             // Assign Standard Black Material to "_lineRenderer"
            LineRenderersGOs.Add(BorderLine);                                                                   // Put a new GameObject - "BorderLine" in the list of LineRenderersGOs
            NumberLines++;                                                                                      // We indicate that there was a "born" new line
        }


        for (byte c = 0; c < lvl.CellGroup.Count; c++)                                                          // In this cycle we load different groups of cells.
        {
            if (lvl.CellGroup[c].GroupType == GameManager.CellGroupTypes.OutsideCellGroup)
            {
                for (int d = 0; d < lvl.CellGroup[c].Cells.Count; d++)                                              // In this cycle, cells of the fog group of different types are arranged.
                {
                    if (lvl.CellGroup[c].Cells[d].FogType == GameManager.FogCellTypes.FullCell)                     // If the type of the loaded cell fog is "Whole cell"
                    {
                        FCO = Instantiate(FogCells[0]);                                                             // Copy the whole cell onto the stage
                        FCO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);           // We adopt it in the external panel group.
                        FCO.name = "FogCell";                                                                       // Give her the name "Fog Cell"
                        FCO.transform.position = lvl.CellGroup[c].Cells[d].Point;                                   // We assign the saved position
                    }
                    else if (lvl.CellGroup[c].Cells[d].FogType == GameManager.FogCellTypes.CircumcisedFogCellZ)     // If the type of load cell mist "Circumcised Fog Cell Z"
                    {
                        FCO = Instantiate(FogCells[1]);                                                             // Copy the Circumcised Fog Cell Z onto the stage
                        FCO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);           // We adopt it in the external panel group.
                        FCO.name = "CircumcisedFogCellZ";                                                           // Give her the name "Circumcised Fog Cell Z"
                        FCO.transform.position = lvl.CellGroup[c].Cells[d].Point;                                   // We assign the saved position
                    }
                    else if (lvl.CellGroup[c].Cells[d].FogType == GameManager.FogCellTypes.CircumcisedFogCellX)     // If the type of load cell mist "Circumcised Fog Cell X"
                    {
                        FCO = Instantiate(FogCells[2]);                                                             // Copy the Circumcised Fog Cell X onto the stage
                        FCO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);           // We adopt it in the external panel group.
                        FCO.name = "CircumcisedFogCellX";                                                           // Give her the name "Circumcised Fog Cell X"
                        FCO.transform.position = lvl.CellGroup[c].Cells[d].Point;                                   // We assign the saved position
                    }
                    else if (lvl.CellGroup[c].Cells[d].FogType == GameManager.FogCellTypes.CircumcisedFogCellXZ)    // If the type of load cell mist "Circumcised Fog Cell X Z"
                    {
                        FCO = Instantiate(FogCells[3]);                                                             // Copy the whole cell onto the stage
                        FCO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);           // We adopt it in the external panel group.
                        FCO.name = "CircumcisedFogCellXZ";                                                          // Give her the name "Circumcised Fog Cell X Z"
                        FCO.transform.position = lvl.CellGroup[c].Cells[d].Point;                                   // We assign the saved position
                    }
                    else                                                                                            // Otherwise, if the type of loaded cells "FogCellSpecial"
                    {
                        FCO = Instantiate(FogCells[0]);                                                             // Copy the whole cell onto the stage
                        FCO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);           // We adopt it in the external panel group.
                        FCO.name = "FogCellSpecial";                                                                // Give her the name "Fog Cell"
                        FCO.transform.position = lvl.CellGroup[c].Cells[d].Point;                                   // We assign the saved position
                    }
                }
            }
            else
            {
                GameObject InnerCellGroup = new GameObject();                                                   // Create a group for inner fog cells.
                InnerCellGroup.name = "InsideGroup_" + (c);                                                     // Give her name
                InnerCellGroup.transform.SetParent(GameObject.FindWithTag("InsideFogCellGroup").transform);     // We adopt it in a group of interior panels.

                for (int d = 0; d < lvl.CellGroup[c].Cells.Count; d++)                                          // In this cycle, cells of the fog group of different types are arranged.
                {
                    FCO = Instantiate(FogCells[0]);                                                             // Copy the whole cell onto the stage
                    FCO.transform.SetParent(InnerCellGroup.transform);                                          // We adopt a cage of fog in the group of internal panels
                    FCO.name = "FogCell_" + (c);                                                                // Give her the name
                    FCO.tag = "FogCell";                                                                        // Specify the tag for the fog cell
                    FCO.transform.localScale = new Vector3(10, 1, 10);                                          // Specify the size of the "Fog Panel"
                    FCO.transform.position = lvl.CellGroup[c].Cells[d].Point;                                   // We assign the saved position
                    FCO.layer = 11;                                                                             // Give it a FogCell layer
                    FCO.GetComponent<Renderer>().material = GM.SheetMaterial;                                   // Assign it a material from a variable "SheetMaterial"
                    FCCol = FCO.AddComponent<BoxCollider>();                                                    // Create a collider for the game object and place it in the FCCol variable.
                    FCCol.size = new Vector3(1, 1, 1);                                                          // We give him dimensions suitable for the form of the panel.
                }
            }
        }
    }


    void MethodPlayGame()                                               // Этот метод вызываеться событием "Играть текущий чертёж"
    {
        DoINeedToCreateALine();                                         // If there is more than one line on the field, create it
        FillTheFieldWithExternalOrInternalCells();                      // Call the field fill method with internal or external fog cells.
        GM.CallSetStateGame();                                          // Call the event triggering method. The game has started.
    }


    void DoINeedToCreateALine()     // If there are no lines on the field, this method draws a line along the edges of the field. In order for the lightweight AI to walk normally
    {
        GameObject LineRendererGroup = GameObject.FindGameObjectWithTag("LineRendererGroup");    // Put in the variable "LineRendererGroup" the object in which the lines are usually stored
        CellGroup PreservedGroup = new CellGroup();                             // This object will store the outer group of fog cells.

        if (LineRendererGroup.transform.childCount == 0)                        // If at the start of the game there was no one line drawn
        {
            GameObject OriginalLine = GameObject.FindGameObjectWithTag("AnotherObjects").transform.Find("BorderLine 0").gameObject; // Put the spare line in the "OriginalLine" variable
            GameObject LineClone = Instantiate(OriginalLine, Vector3.zero, Quaternion.identity);    // Put the line clone in the variable "LineClone"
            LineClone.transform.SetParent(LineRendererGroup.transform);         // We adopt a clone of the line in the corresponding group
            LineClone.name = "BorderLine 0";                                    // Give her the proper name
            LineClone.SetActive(true);                                          // And make her active

            LineRenderer LR = LineClone.GetComponent<LineRenderer>();           // Save the component "LineRenderer" of the line "LineClone" to the variable "LR"
            List<Vector3> ListOFPoints = new List<Vector3>();                   // We declare a list in which line points will be stored to save

            for (int i = 0; i < LR.positionCount; i++)                          // We continue the cycle until we pass all the points in the clone line
            {
                ListOFPoints.Add(LR.GetPosition(i));                            // Add another clone line point to the "ListOfPoints" list.
            }

            Line NewLine = new Line();                                          // Create a new object to save the line "line"  
            NewLine.AddRange(ListOFPoints);                                     // Fill the new line with dots from the just drawn line "_lineRenderer"
            LineList.Add(NewLine);                                              // Add a new line to the line list at the "LineList" level

            PreservedGroup.GroupType = GameManager.CellGroupTypes.OutsideCellGroup; // Define the type of group of fog cells as "external"
            CellGroupList.Add(PreservedGroup);                                      // Add a saved group of cells to the list of groups of cells
        }
    }


    void FillTheEmptySpace()                                            // This method fills non-player outer cells of the outer line with white panels hiding them from the player.
    {
        GameObject GO;                                                  // Fog panel object
        BoxCollider GOCol;                                              // Fog Cell collider
        Vector3 StartPosition = new Vector3(330, 0.2f, -20);            // The starting position of the first panel
        Vector3 CurrentPosition = StartPosition;                        // The current position is calculated by adjusting the cycle number without offset.
        GameObject Judge = new GameObject();                            // This “Judge” object will throw rays in different directions and decide whether to be inside or outside the outlined edges.
        Judge.transform.SetParent(GameObject.FindGameObjectWithTag("AnotherObjects").transform);    // We adopt "FigureJudge" to the object "Other categories"
        Judge.name = "Judge";                                           // Give the object its name "Judge"
        FogCell PreservedСell;                                          // Variable for retained cell
        CellGroup PreservedGroup = new CellGroup();                     // This object will store the outer group of fog cells.
        RaycastHit JudgeRayHit;                                         // Сдесь данные об объекте в который был пущен луч

        int LineDetectorMask = 1 << 14;                                 // We specify 14 layer as a layer with which the beam will contact
        int DotDetectorMask = 1 << 9;                                   // Specify 9 layer as a layer with which the beam will contact
        DotDetectorMask = ~DotDetectorMask;                             // Invert the layer so that only this layer is ignored.
        List<Vector3> Chosens = new List<Vector3>();                    // Список позиций панелей которые должны поработать ещё после первого цикла расстановки панелей на карте
        Ray RayPanel;                                                   // The beam that is needed to determine the position of the next panel

        for (int a = 0; a < 600; a++)                                   // This is where the first panel creation cycle occurs 600 times.
        {
            bool OutsideTouching = false;                               // This variable indicates whether the beam touched the field border or the fog panel collider.
            bool OutsideLineColTouching = false;                        // This variable indicates whether the beam of the outer line collider is in contact.
            Judge.transform.position = new Vector3(CurrentPosition.x - 5, 0.3f, CurrentPosition.z - 5);    // Assign the judge a place in the center of the cell at the line collider level

            for (byte b = 0; b < 4; b++)                                     // We continue the iteration 4 times by throwing rays in 4 directions.
            {
                if (Physics.Raycast(Judge.transform.position, RayDirections[b], out JudgeRayHit, 5.4f, DotDetectorMask))    // If the beam (long a little more than the panel) crashed into the collider
                {
                    if (JudgeRayHit.collider.name == "FogCell")                     // And this is a fog cell 
                        OutsideTouching = true;                                     // Specify that you can put a fog cell in these coordinates
                    else if (JudgeRayHit.collider.tag == "Border")                  // Otherwise, if the beam touched the board of the game
                        OutsideTouching = true;                                     // Значит панель которую мы хотим поставить будет находиться вне очерченной игроком линии ставим "OutsideTouching" правда
                    else if (JudgeRayHit.collider.tag == "OutsideLine")             // Otherwise, if the beam touched the outer line collider
                        OutsideLineColTouching = true;                              // We indicate that this position should be added to favorites
                }

                if (Physics.Raycast(Judge.transform.position, RayDiagonallyDirections[b], out JudgeRayHit, 6.4f, DotDetectorMask))    // If the diagonal beam that is (slightly longer than the mist panel) crashes into the outer line collider
                {
                    if (JudgeRayHit.collider.tag == "OutsideLine")                  // If the beam touched the outer line collider
                        OutsideLineColTouching = true;                              // Указываем что эта позиция должна быть занесена в избранные
                }
            }

            if (OutsideTouching && OutsideLineColTouching)                          // Если эта панель находиться рядом с бордюром игрового стола или рядом с другой панелью и так же рядом находиться коллайдер линии
                Chosens.Add(CurrentPosition);                                       // We put the current checked position in the list of favorites
            
            if (OutsideTouching)                                                                                        // If the beam touched the boundaries of the playing field or fog panel
            {
                PreservedСell = new FogCell();                                                                          // Create a new object for the saved cell.

                if (CurrentPosition.x > 10)                                                                             // Если координаты для текущей панели не опустились ниже стандартной позиции по оси X
                {
                    if (CurrentPosition.z > -171)                                                                       // Если панель которую мы собираемся поставить меньше -171 по оси Z
                    {
                        GO = Instantiate(FogCells[0]);                                                                  // Copy the whole cell onto the stage

                        GO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);                // We adopt it in the external panel group.
                        GO.name = "FogCell";                                                                            // Give her the name "Fog Cell"
                        GO.transform.position = CurrentPosition;                                                        // Assign current position
                        CurrentPosition = new Vector3(CurrentPosition.x, StartPosition.y, CurrentPosition.z - 10);      // И уменьшаем на 10 юнитов позицию для следующей панели по оси "Z"

                        PreservedСell.Point = GO.transform.position;                                                    // Save cell position
                        PreservedСell.FogType = GameManager.FogCellTypes.FullCell;                                      // Save cell type
                        PreservedGroup.Cells.Add(PreservedСell);                                                        // Save the fog cell to the fog cell list
                    }
                    else if (CurrentPosition.z <= -170 && CurrentPosition.z >= -180)                                    // Если панель которую мы собираемся поставить между указанными координтами
                    {
                        GO = Instantiate(FogCells[1]);                                                                  // Copy the cut cell on the Z-axis onto the scene.
                        GO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);                // We adopt it in the external panel group.
                        GO.name = "CircumcisedFogCellZ";                                                                // Give her the name "Circumcised Fog Cell Z"
                        GO.transform.position = CurrentPosition;                                                        // Assign current position
                        CurrentPosition = new Vector3(CurrentPosition.x - 10, StartPosition.y, StartPosition.z);        // И уменьшаем на 10 юнитов позицию для следующей панели по оси "X" и ставим по оси "Z" на старт

                        PreservedСell.Point = GO.transform.position;                                                    // Save cell position
                        PreservedСell.FogType = GameManager.FogCellTypes.CircumcisedFogCellZ;                           // Save cell type
                        PreservedGroup.Cells.Add(PreservedСell);                                                        // Save the fog cell to the fog cell list
                    }
                }
                else if (CurrentPosition.x <= 10)                                                                       // Иначе если опустились то пришло время ставить последние обрезанные панели
                {
                    if (CurrentPosition.z > -171)                                                                       // Если панель которую мы собираемся поставить меньше -171 по оси Z
                    {
                        GO = Instantiate(FogCells[2]);                                                                  // Копируем на сцену клетку обрезанную по оси X
                        GO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);                // We adopt it in the external panel group.
                        GO.name = "CircumcisedFogCellX";                                                                // Give her the name "Circumcised Fog Cell X"
                        GO.transform.position = CurrentPosition;                                                        // Assign current position
                        CurrentPosition = new Vector3(CurrentPosition.x, StartPosition.y, CurrentPosition.z - 10);      // И уменьшаем на 10 юнитов позицию для следующей панели по оси "Z"

                        PreservedСell.Point = GO.transform.position;                                                    // Save cell position
                        PreservedСell.FogType = GameManager.FogCellTypes.CircumcisedFogCellX;                           // Save cell type
                        PreservedGroup.Cells.Add(PreservedСell);                                                        // Save the fog cell to the fog cell list
                    }
                    else if (CurrentPosition.z <= -171 && CurrentPosition.z >= -180)                                    // Если панель которую мы собираемся поставить между указанными координтами
                    {
                        Vector3 PositionOverTheCell = new Vector3(CurrentPosition.x, CurrentPosition.y +1, CurrentPosition.z); // Create a new position for the test beam and assign it to the variable "position over cell"

                        if (!Physics.Raycast(PositionOverTheCell, new Vector3(0,-1,0), out JudgeRayHit, 5.4f, LineDetectorMask))    // We let the beam on top of the trimmed cell and if the beam does not hit the line ...
                        {
                            GO = Instantiate(FogCells[3]);                                                                  // Копируем на сцену клетку обрезанную по осям "X" и "Z"
                            GO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);                // We adopt it in the external panel group.
                            GO.name = "CircumcisedFogCellXZ";                                                               // Give her the name "Circumcised Fog Cell X Z"
                            GO.transform.position = CurrentPosition;                                                        // Assign current position

                            PreservedСell.Point = GO.transform.position;                                                    // Save cell position
                            PreservedСell.FogType = GameManager.FogCellTypes.CircumcisedFogCellXZ;                          // Save cell type
                            PreservedGroup.Cells.Add(PreservedСell);                                                        // Save the fog cell to the fog cell list
                        }
                        break;                                                                                          // И прерываем цикл
                    }
                }
            }
            else                                                                                                    // Otherwise, if no contact of the beam was detected
            {
                if (CurrentPosition.x > 10)                                                                         // If the coordinates for the current panel have not dropped below the standard position on the X axis
                {
                    if (CurrentPosition.z > -171)                                                                   // If the panel we are going to put is less than -171 on the Z axis
                        CurrentPosition = new Vector3(CurrentPosition.x, StartPosition.y, CurrentPosition.z - 10);  // Then decrease the position for the next panel along the "Z" axis by 10 units.
                    else if (CurrentPosition.z <= -170 && CurrentPosition.z >= -180)                                // If the panel we are going to put is between the specified coordinates
                        CurrentPosition = new Vector3(CurrentPosition.x - 10, StartPosition.y, StartPosition.z);    // And reduce the position for the next panel by 10 "X" axes by 10 units and put them on the "Z" axis at the start.
                }
                else if (CurrentPosition.x <= 10)                                                                   // Иначе если опустились то пришло время ставить последние обрезанные панели
                {
                    if (CurrentPosition.z > -171)                                                                   // Если панель которую мы собираемся поставить меньше -171 по оси Z
                        CurrentPosition = new Vector3(CurrentPosition.x, StartPosition.y, CurrentPosition.z - 10);  // И уменьшаем на 10 юнитов позицию для следующей панели по оси "Z"
                    else if (CurrentPosition.z <= -170 && CurrentPosition.z >= -180)                                // Если панель которую мы собираемся поставить между указанными координтами
                        break;                                                                                      // И прерываем цикл
                }
            }
        }

        if (Chosens.Count != 0)                                             // If there are chozens cells that need to be placed
        {
            for (int a = 0; a < 600; a++)                                   // This is where the second panel creation cycle occurs 600 times.
            {
                Judge.transform.position = new Vector3(Chosens[0].x - 5, 0.3f, Chosens[0].z - 5);                       // Assign the judge a place in the center of the cell at the line collider level

                for (byte b = 0; b < 4; b++)                                // Продолжаем цикл 4 раза пуская лучи длинной в одну игровую клетку в 4 стороны
                {
                    if (!Physics.Raycast(Judge.transform.position, RayDirections[b], out JudgeRayHit, 5.4f, DotDetectorMask))     // Если луч (длинной чуть более панели) не во что не врезалься 
                    {
                        RayPanel = new Ray(Chosens[0], RayDirections[b]);                                       // Записываем в луч стартовые координаты первые в списке chosen а направление зависит от номера цикла        

                        if (RayPanel.GetPoint(10).z > -171 && RayPanel.GetPoint(10).x > 10)                     // Если панель которую мы собираемся поставить меньше -171 по оси "Z" и больше 10 по оси "X"
                        {
                            Chosens.Add(RayPanel.GetPoint(10));                                                 // We let the beam 10 units long, find out the place where you need to put a new panel and write its position at the end of the list

                            GO = Instantiate(FogCells[0]);                                                      // Копируем на сцену целую клетку
                            GO.transform.SetParent(GameObject.FindWithTag("OutsideFogCellGroup").transform);    // Удочеряем её в группу панелей
                            GO.name = "FogCellSpecial";                                                         // Даём ей имя "Специальная клетка тумана"
                            GO.transform.position = Chosens[Chosens.Count - 1];                                 // Assign the current position of the new panel
                            GO.transform.localScale = new Vector3(10, 1, 10);                                   // Указываем размер "Панели тумана"
                            GO.layer = 11;                                                                      // Присваиваем ей слой FogCell
                            GO.GetComponent<Renderer>().material = GM.SheetMaterial;                            // Assign it a material from a variable "SheetMaterial"
                            GOCol = GO.AddComponent<BoxCollider>();                                             // Создаём коллайдер для игрового объекта и ложим его в переменную FCCol
                            GOCol.size = new Vector3(1, 1, 1);                                                  // Задаём ему размеры подходящие под форму панели

                            PreservedСell = new FogCell();                                                      // Create a new object for the saved cell.
                            PreservedСell.Point = GO.transform.position;                                        // Save cell position
                            PreservedСell.FogType = GameManager.FogCellTypes.FogSpecial;                        // Save cell type
                            PreservedGroup.Cells.Add(PreservedСell);                                            // Save the fog cell to the fog cell list
                        }
                    }
                }
                Chosens.RemoveAt(0);                                            // Remove the waste coordinates from the top of the list.
                if (Chosens.Count == 0)                                         // Если список пуст
                {
                    GameObject.Destroy(Judge);                                  // Удаляем судью
                    break;                                                      // прерываем цикл
                }
            }
        }
        for (byte a = 0; a < 4; a++) GM.SideLimiter[a].SetActive(true);         // Continue the cycle until we activate 4 side collider limiters

        PreservedGroup.GroupType = GameManager.CellGroupTypes.OutsideCellGroup; // Define the type of group of fog cells as "external"
        CellGroupList.Add(PreservedGroup);                                      // Add a saved group of cells to the list of groups of cells
    }


    void FillTheInnerSpace()                                                // This method fills non-game internal cells of the internal line with white panels hiding them from the player.
    {
        GameObject GO;                                                      // Fog panel object
        BoxCollider GOCol;                                                  // Mist cage collider
        List<GameObject> PossibleGameZoneCells = new List<GameObject>();    // Fog cells placed on the field as internal but possibly external
        List<GameObject> GameZoneCells = new List<GameObject>();            // Special collider labels that indicate that this is an external cell.
        Vector3 StartPosition = new Vector3(330, 0.2f, -20);                // The starting position of the first panel
        Vector3 CurrentPosition = StartPosition;                            // The current position is calculated by adjusting the cycle number without offset.
        Vector3 EyePosition = new Vector3(CurrentPosition.x - 5, CurrentPosition.y + 2, CurrentPosition.z - 5); // The position of an imaginary eye that checks whether a cell is empty or that there is a cell of mist in it
        GameObject Judge = new GameObject();                                // This “FigureJudge” object will throw rays in different directions and decide whether to be inside or outside the outlined edges.
        Judge.transform.SetParent(GameObject.FindGameObjectWithTag("AnotherObjects").transform);    // We adopt "FigureJudge" to the object "Other categories"
        Judge.name = "Judge";                                               // Give the object its name "Judge"
        FogCell PreservedСell;                                              // Variable for retained cell
        CellGroup PreservedGroup = new CellGroup();                         // This object will store the outer group of fog cells.
        RaycastHit JudgeRayHit;                                             // Сдесь данные об объекте в который был пущен луч
        int DotDetectorMask = 1 << 9;                                       // Specify 9 layer as a layer with which the beam will contact
        DotDetectorMask = ~DotDetectorMask;                                 // Invert the layer so that only this layer is ignored.
        int FogCell = 1 << 11;                                              // Specify 11 layer as a layer with which the beam will contact
        int GameZoneCellLayer = 1 << 13;                                    // We specify the 13th layer as the layer with which the beam will contact
        List<Vector3> Chosens = new List<Vector3>();                        // List of panels that are scheduled to put in the internal line
        Ray RayPanel;                                                       // The beam that is needed to determine the position of the next panel

        GameObject InnerCellGroup = new GameObject();                                                // Create a group for inner fog cells.
        InnerCellGroup.name = "InsideGroup_" + (NumberLines - 1);                                    // Give her name
        InnerCellGroup.transform.SetParent(GameObject.FindWithTag("InsideFogCellGroup").transform);  // We adopt it in a group of interior panels.

        for (ushort a = 0; a < 600; a++)                                    // This is where the first panel creation cycle occurs 600 times.
        {
            bool InsideTouching = false;                                    // This variable indicates whether the mist cell has touched the beam.
            bool GameZone = false;                                          // Variable indicating whether spaced fog cells are in the playing area.

            // To begin with, we define a supposedly empty cell inside the inner line to put the inner fog cell there.
            if (!Physics.Raycast(EyePosition, Vector3.down, Mathf.Infinity, FogCell | GameZoneCellLayer))               // If the testing ray of the eye has passed through the testing cell, then there is no fog cell here and we continue the process.
            {
                InsideTouching = true;        // Set the variable "InsideTouching" state "true", if the opposite is not proved, it will remain in the state "True"

                Judge.transform.position = new Vector3(CurrentPosition.x - 5, 0.3f, CurrentPosition.z - 5);             // Assign the judge a place in the center of the cell at the line collider level

                for (byte b = 0; b < 4; b++)                                // We continue the iteration 4 times by throwing rays in 4 directions.
                {
                    if (Physics.Raycast(Judge.transform.position, RayDirections[b], out JudgeRayHit, Mathf.Infinity, DotDetectorMask))  // If the beam (infinitely long) crashed into the collider
                    {
                        if (JudgeRayHit.collider.tag == "OutsideLine")                          // If the beam hit the board of the playing field or the external line collider
                        {
                            InsideTouching = false;                                                                                     // We indicate that this cell is located outside the inside line.
                            break;                                                                                                      // Break cycle
                        }
                    }
                }
            }
            if (InsideTouching)                                                 // If the fog cell is supposed to be inside a new inside line
            {
                GO = Instantiate(FogCells[0]);                                  // Copy the whole cell onto the stage
                GO.transform.SetParent(InnerCellGroup.transform);               // We adopt a cage of fog in the group of internal panels
                GO.name = "FogCell_" + (NumberLines - 1);                       // Give her the name
                GO.tag = "FogCell";                                             // Specify the tag for the fog cell
                GO.transform.localScale = new Vector3(10, 1, 10);               // Specify the size of the "Fog Panel"
                GO.transform.position = CurrentPosition;                        // Assign current position
                GO.layer = 11;                                                  // Give it a FogCell layer
                GO.GetComponent<Renderer>().material = GM.SheetMaterial;        // Assign it a material from a variable "SheetMaterial"
                GOCol = GO.AddComponent<BoxCollider>();                         // Create a collider for the game object and place it in the FCCol variable.
                GOCol.size = new Vector3(1, 1, 1);                              // We give him dimensions suitable for the form of the panel.     
                PossibleGameZoneCells.Add(GO);                                  // We enter the next fog cell into the list of supposedly external cells.
                Chosens.Add(CurrentPosition);                                   // We put the current checked position in the list of favorites

                for (int b = 0; b < 500; b++)                                   // 420 Repeat the cycle until we fill the inner space of the inner line with the cells of the fog
                {
                    if (Chosens.Count > 0)                                      // If there are still unprocessed positions in the favorites list
                    {
                        Judge.transform.position = new Vector3(Chosens[0].x - 5, 0.3f, Chosens[0].z - 5);           // Assign the judge a place in the center of the cell at the line collider level

                        for (ushort c = 0; c < 4; c++)                                          // We let the beam into 4 sides arranging fog cells in the neighboring cells if they are empty and entering their coordinates in the "Chosens" list
                        {
                            if (!Physics.Raycast(Judge.transform.position, RayDirections[c], out JudgeRayHit, 5.4f, DotDetectorMask))    // If the beam (long a little more than the panel) If the beam has nothing to hit
                            {
                                RayPanel = new Ray(Chosens[0], RayDirections[c]);               // We write the starting coordinates in the beam first on the list of chosen and the direction depends on the cycle number
                                GO = Instantiate(FogCells[0]);                                  // Copy the whole cell onto the stage
                                GO.transform.SetParent(InnerCellGroup.transform);               // We adopt a cage of fog in the group of internal panels
                                GO.name = "FogCell_" + (NumberLines - 1);                       // Give her the name
                                GO.tag = "FogCell";                                             // Specify the tag for the fog cell
                                GO.transform.localScale = new Vector3(10, 1, 10);               // Specify the size of the "Fog Panel"
                                GO.transform.position = RayPanel.GetPoint(10);                  // Assign the current position of the new panel
                                GO.layer = 11;                                                  // Give it a FogCell layer
                                GO.GetComponent<Renderer>().material = GM.SheetMaterial;        // Assign it a material from a variable "SheetMaterial"
                                GOCol = GO.AddComponent<BoxCollider>();                         // Create a collider for the game object and place it in the FCCol variable.
                                GOCol.size = new Vector3(1, 1, 1);                              // We give him dimensions suitable for the form of the panel.     
                                PossibleGameZoneCells.Add(GO);                                  // We enter the next fog cell into the list of supposedly external cells.
                                Chosens.Add(RayPanel.GetPoint(10));                             // We let the beam 10 units long, find out the place where you need to put a new panel and write its position at the end of the list

                            }
                            else                                                    // Otherwise, if the beam of something touched
                            {
                                if (JudgeRayHit.transform.tag == "OutsideLine")     // If the beam touched the outer line
                                    GameZone = true;                                // The fog cell found that it is inside the game zone.
                            }
                        }
                        Chosens.RemoveAt(0);                                        // Remove the waste coordinates from the top of the list.
                    }
                    else
                    {
                        break;
                    }
                }

                if (GameZone)                                                    // If we have just filled the playing area with the cells of the fog
                {
                    GameObject GameZoneGroup = GameObject.FindGameObjectWithTag("GameZoneGroup");       // Find an object with the tag "GameZoneGroup"
                    for (; PossibleGameZoneCells.Count > 0;)                                            // Continue the cycle until we replace all mistakenly placed fog cells with warning colliders.
                    {
                        GameObject GameZoneCell = new GameObject();                                     // Create an object "GameZoneCell"
                        GameZoneCell.transform.SetParent(GameZoneGroup.transform);                      // We bring this object to the group.
                        GameZoneCell.name = "GameZoneCell";                                             // Give it a name
                        GameZoneCell.tag = "GameZoneCell";                                              // Give it a tag
                        GameZoneCell.layer = 13;                                                        // Specify the layer for the game cell
                        GameZoneCell.transform.position = PossibleGameZoneCells[0].transform.position;  // We put this object in place of one of the fog cells
                        BoxCollider GZCCollider = GameZoneCell.AddComponent<BoxCollider>();             // Add the component "BoxCollider" to the object "GameZoneCell", and save this component to the variable "GZCCollider"
                        GZCCollider.center = new Vector3(-5, 0, -5);                                    // Specify the center of the collider for this object.
                        GZCCollider.size = new Vector3(9.5f, 1, 9.5f);                                  // Specify the size of the collider for this object.
                        Destroy(PossibleGameZoneCells[0]);                                              // We destroy that cell of fog in whose place we put the object "GameZoneCell"
                        PossibleGameZoneCells.RemoveAt(0);                                              // Remove the next fog cell from the "PossibleGameZoneCells" list
                        GameZoneCells.Add(GameZoneCell);                                                // We place a new cell collider in the appropriate list
                    }
                }
                else                                                                                    // Otherwise, if the cells of the fog are in the right place - inside the inner line
                {
                    for (int b = 0; b < PossibleGameZoneCells.Count; b++)
                    {
                        PreservedСell = new FogCell();                                      // Create a new object for the saved cell.
                        PreservedСell.Point = PossibleGameZoneCells[b].transform.position;  // Save cell position
                        PreservedСell.FogType = GameManager.FogCellTypes.FullCell;          // Save cell type
                        PreservedGroup.Cells.Add(PreservedСell);                            // Save the fog cell to the fog cell list
                    }

                    PreservedGroup.GroupType = GameManager.CellGroupTypes.InsideCellGroup;  // Save the fog cell group type
                    CellGroupList.Add(PreservedGroup);                                      // Add a saved group of cells to the list of groups of cells

                    break;  // If we filled the whole possible area with the mist cells without touching the outer line, this means the mist cells were inside the inner line and the cycle can be interrupted
                }
            }
            else                                                                                                    // Otherwise, if no contact of the beam was detected
            {
                if (CurrentPosition.x > 10)                                                                         // If the coordinates for the current panel have not dropped below the standard position on the X axis
                {
                    if (CurrentPosition.z > -171)                                                                   // If the panel we are going to put is less than -171 on the Z axis
                        CurrentPosition = new Vector3(CurrentPosition.x, StartPosition.y, CurrentPosition.z - 10);  // Then decrease the position for the next panel along the "Z" axis by 10 units.
                    else if (CurrentPosition.z <= -170 && CurrentPosition.z >= -180)                                // If the panel we are going to put is between the specified coordinates
                        CurrentPosition = new Vector3(CurrentPosition.x - 10, StartPosition.y, StartPosition.z);    // And reduce the position for the next panel by 10 "X" axes by 10 units and put them on the "Z" axis at the start.
                }
                EyePosition = new Vector3(CurrentPosition.x - 5, CurrentPosition.y + 2, CurrentPosition.z - 5);     // Update EyePosition
            }
        }
        if (GameZoneCells.Count != 0)                               // If the list with collider cells marking the game zone is not empty
        {
            Debug.Log("В списке PossibleGameZoneCells " + PossibleGameZoneCells.Count + " объектов");
            Debug.Log("Список возможно клетки игровой зоны не пуст, но так как мы расставили клетки внутри внутренней линии надо очистить этот список");
            for (; GameZoneCells.Count > 0;)                        // We continue the cycle while the list with game cells-colliders does not become empty
            {
                Destroy(GameZoneCells[0]);                          // Destroy the cell-collider on scene
                GameZoneCells.RemoveAt(0);                          // Remove it from the list
            }
        }
    }


    bool ComparisonOfNumbers(float NomberA, float NomberB, float Difference)    // This method compares two numbers and returns the answer: Do the indicated numbers differ by the indicated value
    {
        bool Answer = false;                                        // This variable indicates whether the indicated numbers differ by the specified value.
        float RealDifference = 0;

        if (NomberA > NomberB)                                      // If the variable "A" is greater than the variable "B"
            RealDifference = NomberA - NomberB;                     // Subtract from the value of the variable "A" the value of the variable "B" by calculating the difference between them

        if (NomberB > NomberA)                                      // If the variable "B" is greater than the variable "A"
            RealDifference = NomberB - NomberA;                     // Subtract from the value of the variable "B" the value of the variable "A" by calculating the difference between them

        if (RealDifference >= Difference)                           // If the difference between "A" and "B" is greater than the specified value
            Answer = true;                                          // We indicate that the numbers differ by more than the specified value

        return Answer;                                              // We return the answer from the method
    }


    void MethodOneLinWasFinished()                                  // Метод вызываемый событием "Одна линия была законченна"
    {
        SelPo.Clear();                                              // Сlear the list "SelPo"
        LineRenderersGOs.Clear();                                   // Clear the list "LineRenderersGOs"
        NumberLine ++;                                              // Увеличиваем на один номер линии с которой мы сейчас работаем
        Destroy(GameObject.FindGameObjectWithTag("GhostLine"));     // Remove the ghost line
        ActiveLinePoint = 0;                                        // We specify the value of the variable "sdsfd" as zero
    }


    void MethodDrawTheFollowingLine()                               // Method called by the event "Draw the next line"
    {
        FillTheFieldWithExternalOrInternalCells();                  // Call the field fill method with internal or external fog cells.
    }


    void FillTheFieldWithExternalOrInternalCells()                  // This method is called when you need to fill the field with internal or external cells of the fog. He chooses which cells fill the field and fills it.
    {
        if (LineHasBeenDelineated)                                  // If the last line was completely drawn
        {
            if (NumberLines == 1) FillTheEmptySpace();              // If we drew the first line. Call the method that fills non-game space with panels
            else FillTheInnerSpace();                               // Otherwise, if we drew not the first line call the method that fills the space inside the internal lines.
        }
    }


    void MethodLoadGameLevel()                                      // This method is called by the load game level event.
    {
        LoadMap();                                                  // Call the map download method
    }


    void OnDisable()
    {
        GM.SetStateMenu -= MethodSetStateMenu;                      // We write off the method "MethodSetStateMenu" from the event "SetStateMenu"
        GM.OneLineWasFinished -= MethodOneLinWasFinished;           // We write off the method "MethodOneLineWasFinished" from the event "OneLineWasFinished"
        GM.DrawTheFollowingLine -= MethodDrawTheFollowingLine;      // Unsubscribe the "MethodDrawTheFollowingLine" method from the "DrawTheFollowingLine" event
        GM.PlayGame -= MethodPlayGame;                              // We write off the method "MethodPlayGame" from the event "PlayGame"
        GM.LoadGameLevel -= MethodLoadGameLevel;                    // We write off the method "MethodLoadGameLevel" from the event "LoadGameLevel"
    }
}

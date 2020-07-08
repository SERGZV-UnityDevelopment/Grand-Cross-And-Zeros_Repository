using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public Camera SecondCamera;                                                 // Auxiliary camera
    public UIScript UIS;                                                        // Variable for object of class "UIScript"
    public CellsMaker CM;                                                       // Variable for object of class "CellsMaker"
    public NNSettings NNS;                                                      // Class instance responsible for setting neural networks
    public SaveLoad SL;                                                         // SaveLoad class object
    public enum GameStates { Menu, Scheme, Game }                               // Перечисление состояний игры (Состояние меню, Состояние рисования схемы игрового поля, Состояние игры)
    public GameStates Gamestate;                                                // Переменая для текущего состояния игры
    public enum FogCellTypes { FullCell, CircumcisedFogCellZ, CircumcisedFogCellX, CircumcisedFogCellXZ, FogSpecial } // 5 Types of fog cells.
    public enum CellGroupTypes { OutsideCellGroup, InsideCellGroup }            // 2 types of fog cell group, outer fog cell and inner fog cell
    enum MaterialStates { Standart, Shine, Loose }                              // Enumerations for player material states
    public enum GameTypes { NotSet, Walkthrough, CustomMapGame }                // This listing contains the types of games available in this game, the usual walkthrough or playing on your map
    public GameTypes GameType;                                                  // This variable indicates what type of game we are playing, passing or playing on a custom map
    public delegate void Standart();                                            // Стандартный делегат для событий игры
    public event Standart SetStateMenu;                                         // Event "Set the state of the game (Menu)"
    public event Standart SetStateShceme;                                       // Событие "Установить состояние игры (Схема)"
    public event Standart SetStateGame;                                         // Событие "Установить состояние игры (Игра)"
    public event Standart TheFirstPointIsSet;                                   // This event indicates that the first point of the field line has been set.
    public event Standart TheNextPointIsSet;                                    // This event indicates that the next point of the field line has been set.
    public event Standart OneLineWasFinished;                                   // Событие "Одна линия была законченна"
    public event Standart DrawTheFollowingLine;                                 // Событие "Рисуем следующую линию"
    public event Standart PlayGame;                                             // Событие "Играть текущий чертёж"
    public event Standart Surrender;                                            // Event indicating that one player has given up
    public event Standart EveryoneSurrendered;                                  // Event indicating that all but one player gave up
    public event Standart ShowWalking;                                          // This event is triggered when you need to show who is walking now.
    public event Standart SaveGameEvent;                                        // This event saves the game.
    public event Standart LoadGameEvent;                                        // This event Load the game.
    public event Standart LoadGameLevel;                                        // This event loads the selected level.
    public event Standart PlayAgain;                                            // This event re-launches the current level.
    public event Standart PlayNextLevel;                                        // This event while passing the game loads the next level if it is available.
    public event Standart StartingCellNotFound;                                 // This event is fired if the starting cell was not found by the computer.
    public Shader BacklightShader;                                              // Backlight shader
    public Material SheetMaterial;                                              // Материал листа бокнота
    public Material FigureBack;                                                 // Shader Background player figure
    public Shader LooserShader;                                                 // Shader Background player figure for losing players
    public byte NumberOfPlayers = 2;                                            // Total number of players
    public GameObject[] SideLimiter = new GameObject[4];                        // 4 collider cutters
    public Material[] Materials;                                                // Массив материалов доступных для выбора
    public GameObject[] Figures;                                                // Массив фигурок игроков
    public GameObject ColorGridPanel;                                           // Object containing 40 objects with color buttons  
    public List<int> GamePoints = new List<int>();                              // Список количества фигур (Крестиков, ноликов и тогдалее) поставленных каждым игроком
    public List<string> NamesTurn = new List<string>();                         // Список имён игроков
    public List<Material> MaterialsTurn = new List<Material>();                 // List of selected player materials
    public List<Material> BackMaterialsTurn = new List<Material>();             // Materials of the cells on which the figures stand, separately for each player
    public List<Color32> ColorsTurn = new List<Color32>();                      // List of player colors for text display
    public List<byte> FiguresTurn = new List<byte>();                           // Список какая фигурка у какого игрока
    public List<bool> IsHeAHuman = new List<bool>();                            // Список состояний для каждого игрока робот ли он или нет (Значение True означает что этот игрок человек)
    public List<bool> Surrendered = new List<bool>();                           // Список состояний для каждого игрока сдалься ли он или нет (Значение True означает что сдалься)
    public List<byte> ComputerDifficulty = new List<byte>();                    // Computer opponent difficulty
    public List <byte> ComputerEntityNumbers = new List<byte>();                // Player Number > Computer Number in Category
    public byte Progress;                                                       // The number of the last passed level is stored here.                                                      
    public byte WinNomber { get; set; }                                         // Сдесь будет храниться номер выигравшего игрока
    public byte Turn = 0;                                                       // Номер игрока
    int LayerCellDetecor = 1 << 10;                                             // Маска для луча которая определяет что он будет сталькиваться с коллайдерами со слоя "CellsDetectors"
    int LayerFogCell = 1 << 11;                                                 // Маска для луча кторая определяет что он будет сталкиваться ещё и с коллайдерами слоя "FogCell"
    int LayerFigure = 1 << 12;                                                  // Маска для луча определяющая что он будет сталкиваться ещё и с коллайдерами "PlayerFigure"
    GameObject FigureJudge;                                                     // Переменная для пустышки судьи
    Ray ray;                                                                    // Луч пущенный из начала камеры к пальцу
    RaycastHit hit;                                                             // Эта переменная нужна чтобы определять детекторы на которые указала стрелка или палец
    Vector3 HTP;                                                                // Сдесь будет храниться лишь позиция того объекта в который ударилься луч, чтобы можно было короче обращаться к этой позиции
    Vector3[] RayDirections = new Vector3[8];                                   // This array contains 8 ray directions that are used in different game processes.
    List<Vector3> WinPlayerFiguresPosition = new List<Vector3>();               // Список позиций фигурок выигравшего игрока
    public float DelayForMenu = 4;                                              // Time delay before the menu is displayed at the end of the game
    public bool FirstCallShowWalking = true;                                    // This variable says that the event "ShowWalking" was called the first time.
    public bool FirstRoundOfMovesEnded = false;                                 // This variable indicates whether the first round of moves was completed.
    public bool RoundEnded { get; set; }                                        // This Autoproperty indicates whether the match was played and whether it is already possible to put figures
    public bool BlockingMoves = false;                                          // A variable that says whether to block player figures moves
    float DelayShowWalking = 2.4f;                                              // This variable indicates the delay time to display the player’s figure on the first turn.
    public bool ComputerPlayerWasCalled = false;                                // The variable indicates whether the current computer player has been called.
    public bool FirstMoveNotFound = false;                                      // This variable will become “true” if any of the computer players does not find a cell for their first move.
    SigmoidalNetwork NNetwork1;                                                 // 1 Competing neural network
    SigmoidalNetwork NNetwork2;                                                 // 2 Competing neural network


    void OnEnable()
    {
        SetStateMenu += MethodSetStateMenu;                 // We sign the method "MethodSetStateMenu" to the event "SetStateMenu"
        SetStateShceme += MethodSetStateScheme;             // We sign the "SetStateShceme" method for the event "MethodSetStateScheme"
        SetStateGame += MethodSetStateGame;                 // We sign the "SetStateGame" method for the event "MethodSetStateGame"
        Surrender += MethodSurrender;                       // Подписываем метод "MethodSurrender" на событие "Surrender"     
        EveryoneSurrendered += MethodEveryoneSurrendered;   // Подписываем метод "MethodEveryoneSurrendered" на событие "EveryoneSurrendered"
        ShowWalking += MethodShowWalking;                   // We sign the "ShowWalking" method for the event "MethodShowWalking"
        PlayGame += MethodPlayGame;                         // We subscribe the "MethodPlayGame" method to the "PlayGame" event
        PlayAgain += MethodPlayAgain;                       // We subscribe the "MethodPlayAgain" method to the "PlayAgain" event
        PlayNextLevel += MethodPlayNextLevel;               // We subscribe the "MethodPlayNextLevel" method to the "PlayNextLevel" event
    }

   
    void Start()
    {
        FillingTheInitialData();                    // Fill in the initial data of the players

        RayDirections[0] = new Vector3(1, 0, 0);    // Луч пускаемый в верхнюю границу доски
        RayDirections[1] = new Vector3(0, 0, -1);   // Луч пускаемый в правую границу доски
        RayDirections[2] = new Vector3(-1, 0, 0);   // Луч пускаемый в нижнюю границу доски
        RayDirections[3] = new Vector3(0, 0, 1);    // Луч пускаемый в левую границу доски
        RayDirections[4] = new Vector3(1, 0, -1);   // Луч пускаемый в верх и право наискоски
        RayDirections[5] = new Vector3(-1, 0, -1);  // Луч пускаемый в низ и в право наискоски
        RayDirections[6] = new Vector3(-1, 0, 1);   // Луч пускаемый в низ и в лево наискоски   
        RayDirections[7] = new Vector3(1, 0, 1);    // Луч пускаемый в верх и в лево наискоски

        FigureJudge = new GameObject();             // Создаём пустышку судью
        FigureJudge.transform.SetParent(GameObject.FindGameObjectWithTag("AnotherObjects").transform);    // Удочеряем "Судью" объекту "Другие категории"
        FigureJudge.name = "FigureJudge";           // Даём объекту его имя
        FigureJudge.tag = "FigureJudge";            // Даём ему тег "Судья"

        RoundEnded = true;                          // Set the value to the "RoundEnded" auto property

        LoadGameEvent();                            // Call the game download method

        StartCoroutine(ShowAdversting());           // We call the coroutine showing advertising as soon as it is ready
    }

    IEnumerator ShowAdversting()
    {
        bool Ready = false;                                     // This variable contains the value whether the advertisement is ready.

        for (int i = 0; Ready == false; i++)                    // We continue the cycle until the advertisement is ready
        {
            if (ShowAd.AdShow())                                // If the advertisement is ready, then we show the advertisement
                Ready = true;                                   // We indicate that the advertisement is ready for display
            else
            {
                Ready = false;                                  // We indicate that the advertisement is not ready for display
                yield return new WaitForSeconds(1f);            // Make a delay
            }
        }
    }


    void Update()
    {
        if (Gamestate == GameStates.Game)                   // Если состояние игры - игра   
        {
            Game();                                         // Вызываем метод игра
        }
    }


    void Game()                                     // This method handles game logic.
    {
        bool FriendlyFigure = false;                // Эта переменная говорит есть ли рядом дружественная фигура

        RaycastHit LocalHit;                        // Эта переменная нужна чтобы определять есть ли рядом коллайдеры фигурок выбранного игрока

        if (IsHeAHuman[Turn] == true)               // Если это человек
        {
            if (!BlockingMoves && !UIS.ShowFigures)      // If variable FirstPress equals true & ShowFigures equals false and The screen zoom panel is off
            {
                if (Input.GetMouseButtonUp(0))              // Если мы отпустили кнопку мыши
                {
                    if (GamePoints[Turn] == 0)              // Если у этого игрока в его списке крестиков еще нет поставленных крестиков то ставим этот крестик где хотим
                    {
                        ray = Camera.main.ScreenPointToRay(Input.mousePosition);                                                    // Пускаем луч из камеры к стрелке и возвращаем этот луч в переменную "ray"
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerCellDetecor | LayerFogCell | LayerFigure))           // Пускаем луч от камеры к пальцу игрока 
                        {
                            if (hit.collider.gameObject.layer == 10)                                                                // And if this ray hits an object located on the "CellDetector" layer
                            {
                                PlaceTheFigureOnTheField(hit.transform.position);   // Put the player’s figure on the chosen place
                                FirstRoundOfMoves();                                // Show the figure of the next player or indicate that the first round of moves is completed
                                GamePoints[Turn]++;                                 // We add one to the number of figures for this player.
                                NextPlayer();                                       // Указываем что дальше мы будем работать со следующим игроком
                            }
                        }
                    }
                    else                                                            // Otherwise, if this is not the first move of this player
                    {
                        ray = Camera.main.ScreenPointToRay(Input.mousePosition);                                                    // Пускаем луч из камеры к стрелке и возвращаем этот луч в переменную "ray"
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerCellDetecor | LayerFogCell | LayerFigure))           // Пускаем луч от камеры к пальцу игрока 
                        {
                            if (hit.collider.gameObject.layer == 10)                                                                // И если этот луч ударилься об объект находящийся на слое CellDetector
                            {
                                FigureJudge.transform.position = hit.transform.position;                                            // Помещаем судью на предполагаемое место
                                for (byte a = 0; a < RayDirections.Length; a++)                                                     // Стреляем лучом из судьи в 6 сторон пока не найдём дружественную фигуру
                                {
                                    if (Physics.Raycast(FigureJudge.transform.position, RayDirections[a], out LocalHit, 7.5f, LayerFigure))     // If the ray hit the figure (We check it by layer)
                                    {
                                        if (LocalHit.collider.GetComponent<PlayerFigure>().NomberPlayer == Turn)                    // If the ray hits the figure with the same player number that is now walking
                                        {
                                            FriendlyFigure = true;          // Ставим что рядом найденна фигура игрока который сейчас ходит
                                            break;                          // Прерываем цикл
                                        }
                                    }
                                }
                                if (FriendlyFigure)                                                                         // Если рядом есть дружественная фигура
                                {
                                    PlaceTheFigureOnTheField(hit.transform.position);   // Put the player’s figure on the chosen place
                                    GamePoints[Turn]++;                                 // We add one to the number of figures for this player. 
                                    NextPlayer();                                       // We indicate that further we will work with the next player
                                }
                            }
                        }
                    }
                }
            }
        }
        else                                                    // Otherwise, if the computer is moving
        {
            if (!RoundEnded)                                    // If the round is not over yet, you can make a move
            {
                if (!ComputerPlayerWasCalled)                   // If we have not yet called the move method of this computer player
                {
                    if (ComputerDifficulty[Turn] < 2)           // If handwritten intelligence difficulty is selected
                        StartCoroutine(SAMove());               // Calling Handwriting Intelligence Method
                    else { }                                    // If complexity involves the use of neural networks
                    //   NNMove();                              // Call the "NNMove" method
                }
            }
        }

//        if (FirstPress == false) WaitingToReleaseButton();      // If the variable FirstPress is false, we call the method WaitingToReleaseButton.
    }

    IEnumerator SAMove()        // Make a move with a simple algorithm
    {
        ComputerPlayerWasCalled = true;     // We indicate that we called the method of the move of this computer player

        HandwrittenIntelligence HIS = FindAComputerPlayerScript(Turn);  // Find the script of the desired computer player

        if (!FirstRoundOfMovesEnded)        // If the first round of moves has not yet ended
        {
            yield return new WaitForSeconds(DelayForMenu);               // Make a delay

            HIS.MakeAMove();                // Making the AI make a move

            if (!FirstMoveNotFound)         // If there are no computer players who did not find the first cell
                FirstRoundOfMoves();        // Show the figure of the next player or indicate that the first round of moves is completed
        }
        else                                // Otherwise, if the first round of moves has been completed
            HIS.MakeAMove();                // Making the AI make a move

        if (!FirstMoveNotFound)             // If there are no computer players who did not find the first cell
        {
            if (!HIS.ImpossibleToGo)                // If the computer make a move
            {
                NextPlayer();                       // We indicate that further we will work with the next player
                ComputerPlayerWasCalled = false;    // We indicate that in the new move the computer can again go
            }
            else                                    // Otherwise, if the computer did not find a free cell for the move
            {
                CallSurrender();                    // Call the surrender player event
                ComputerPlayerWasCalled = false;    // We indicate that in the new move the computer can again go
            }
        }
        else
            StartingCellNotFound();     // We call the "StartingCellNotFound" event
    }

    void NNMove()       // Neural network controlled motion
    {
        Debug.Log("Игрок номер " + Turn + " ходит компьютер");
        // Для начала сделаем так чтобы компьютер выбирал случайную клетку и ставил свою фигурку в неё, для этого в первом ходе нужно создать так называемый сканер который определит какая клетка доступна для хода а какая заблокированна

//        byte[] State = new byte[25];                            // Массив состояний каждой клетки, для начала он будет равен 5х5=25 клеток и будет иvеть 4 состояния (0-пустая, 1-заблокированная, своифи гурки, вражесские фигурки)
//        GameObject Scanner = new GameObject();                  // Create a scanner object

        NNetwork1 = new SigmoidalNetwork(NNS.InputCount_1, NNS.HasBias_1, NNS.Layers_1, typeof(SigmoidalNeuron)); // We call the constructor of the first neural network

        if (NNS.HumanLearning)                                  // If the training is planned by a person
        {
         //   Debug.Log("Обучение запланированно производится человеком");
         //   Debug.Log("Количество входов первой неросети " + NNetwork1.InputCount + " Имеет ли сеть BIAS? " + NNetwork1.HasBias + ". Количество слоёв нейросети " + NNetwork1.Layers.Count);

            for (int i = 0; i < NNetwork1.Layers.Count; i++)    // Цикл для отображения в debug.log сообщений о количестве нейронов в каждом слое
            {
            //    Debug.Log("Длинна " + i + " слоя = " + NNetwork1.Layers[i].Length);
            //    Debug.Log("Количество входов для нейронов " + i + " слоя = " + NNetwork1.Layers[i][0].Weights.);
            }
        }
        else
        {
         //   Debug.Log("Запланированно конкурентное самообучение");
        }

        GamePoints[Turn]++;         // We add one to the number of figures for this player. 
        NextPlayer();               // Указываем что дальше мы будем работать со следующим игроком
    }


    void FirstRoundOfMoves()        // This method is called as auxiliary in the first round of moves, causing the figures of players to be shown, and tracking when the first round of moves ends
    {
        if (NumberOfPlayers != (Turn + 1))                      // If this is not the last figure in the first cycle
            ShowWalking();                                      // Call the event showing the figures and player's nickname
        else                                                    // Otherwise, if we have already completed the first round of moves
        {
            UIS.Panels[3].GetComponent<Image>().color = new Color32(255, 255, 255, 255);// Making the hidden panel not transparent
            UIS.Panels[3].GetComponentInChildren<Button>().interactable = true;         // Making a hidden panel responsive
            FirstRoundOfMovesEnded = true;                                              // Specify the value of the variable "FirstRoundOfMovesEnded" as true
        }
    }

    void FillTheRemainingCells()                                // Этот метод при сдаче всех игроков кроме последнего заполняет все доступные ячейки для выигравшего игрока
    {
        Ray CheckingRay = new Ray();                            // Проверяющий луч
        RaycastHit CheckHit;                                    // Сюда вернётся RaycastHit проверяющего луча
        Vector3 StartRayOrigin = new Vector3(325, 5, -25);      // Начальная точка луча
        Vector3 FinderPos = Vector3.zero;                       // The position from which the ray will check the neighboring cells for accessibility
        Vector3 OverPosition;                                   // There will be a position above the next figure in the "WinPlayerFiguresPosition" list

        int DotDetLayerMask = 1 << 9;                           // Указываем 9 слой (DotDetector) как слой с которым будет контактировать луч
        DotDetLayerMask = ~DotDetLayerMask;                     // Инвертируем слой чтобы только этот слой и игнорировалься лучём

        CheckingRay.origin = StartRayOrigin;                    // Стартовая точка проверяющего луча
        CheckingRay.direction = new Vector3(0, -1, 0);          // Направление луча

        for (int a = 0; a < 600; a++)                           // Продолжаем цикл до тех пор пока не пройдём все клетки игрового поля
        {
            if (Physics.Raycast(CheckingRay, out CheckHit, Mathf.Infinity, LayerCellDetecor | LayerFogCell | LayerFigure))      // Стреляем лучом в очередную клетку и узнаём свободна она или чем то закрыта
            {
                // If the ray in the cycle came across another figure with the same name as the winning player & this ray hit a figure with the same player number that won
                if (CheckHit.collider.name == NamesTurn[Turn] && CheckHit.collider.GetComponent<PlayerFigure>().NomberPlayer == Turn)
                {
                    WinPlayerFiguresPosition.Add(CheckHit.transform.position);                  // Добавляем позицию очередной найденной фигурки в список 
                }
            }
            else                                                // Если луч не врезалься не в один из вышеперечисленных коллайдеров 
                break;                                          // То прерываем цикл

            if (CheckingRay.origin.z < -165)                                                                                    // Если позиция луча подошла к правому борту
                CheckingRay.origin = new Vector3(CheckingRay.origin.x - 10, CheckingRay.origin.y, StartRayOrigin.z);            // Двигаем стартовую точку луча на 10 юнитов вниз и ставим в начало по оси "Z"
            else                                                                                                                // Инае если мы ещё не у правого борта
                CheckingRay.origin = new Vector3(CheckingRay.origin.x, CheckingRay.origin.y, CheckingRay.origin.z - 10);        // Двигаем стартовую точку луча на 10 юнитов в право
        }

        for (int b = 0; 0 < WinPlayerFiguresPosition.Count; b++)                    // Продолжаем цикл до тех пор пока не переберём весь список фигурок выигравшего игрока             
        {
            OverPosition = new Vector3(WinPlayerFiguresPosition[0].x, 5, WinPlayerFiguresPosition[0].z);    // Save the position over the next winner figure in the list "WinPlayerFiguresPosition"

            for (byte c = 0; c < 8; c++)                                            // Проходим цикл 8 раз опрашивая все соседние клетки фигурки
            {
                Ray ray = new Ray(OverPosition, RayDirections[c]);                  // Create a ray in one of 8 sides from the "OverPosition" position
                FinderPos = ray.GetPoint(10);                                       // Calculating a point in the "ray" direction in 10 units

                if (Physics.Raycast(FinderPos, Vector3.down, out CheckHit, 8f, DotDetLayerMask)) // If the beam launched down hit a collider other than "DotDetector"
                {
                    if (CheckHit.collider.gameObject.layer == 10)                   // И этот коллайдер это CellDetector
                    {
                        PlaceTheFigureOnTheField(CheckHit.transform.position);      // Put the player’s figure on the chosen place
                        WinPlayerFiguresPosition.Add(CheckHit.transform.position);  // Добавляем в конце списка фигурок новую пустую клетку
                        GamePoints[Turn]++;                                         // We add one to the number of figures for this player. 
                    }
                }
            }
            WinPlayerFiguresPosition.RemoveAt(0);               // Удаляем позицию фигурки с которой мы только что отработали
        }
    }

    public void PlaceTheFigureOnTheField(Vector3 FigurinePlace)     // This method puts the player’s figure on the field.
    {
        GameObject PlayerFigure;                                    // Player Figure Object
        BoxCollider PlayerCollider;                                 // Collider for player figure
        Transform FigureParent;                                     // Parent, for player figure

        if (!FirstRoundOfMovesEnded)                                // If this is the first round of moves
        {
            GameObject NewPlayerFiguresCategory = new GameObject();                                     // Create a category for the figures of this player
            NewPlayerFiguresCategory.name = NamesTurn[Turn] + "_FiguresCategory";                       // Give him the name of the player for whom this category is created.
            NewPlayerFiguresCategory.transform.SetParent(GameObject.FindWithTag("TicTacToe").transform);// We place this category in the category for figures of all players
        }

        PlayerFigure = Instantiate(Figures[FiguresTurn[Turn]]);                                 // Put the walking player figure on stage
        PlayerFigure.GetComponent<PlayerFigure>().NomberPlayer = Turn;                          // We indicate in the script the figures to which player it belongs
        FigureParent = GameObject.FindWithTag("TicTacToe").transform.GetChild(Turn);            // We find the appropriate category for the figures of this player
        PlayerFigure.transform.SetParent(FigureParent);                                         // Put the figure in the appropriate category on the stage
        PlayerFigure.transform.position = FigurinePlace;                                        // Assign to it the position of the cell that the player pressed
        PlayerFigure.transform.localScale = new Vector3(10, 10, 10);                            // Set her size
        PlayerFigure.GetComponent<MeshRenderer>().material = MaterialsTurn[Turn];               // We assign the figurine its material
        PlayerFigure.transform.GetChild(0).GetComponent<MeshRenderer>().material = BackMaterialsTurn[Turn]; // Assign the background material of the figure from the list of these materials "BackMaterialsTurn"
        PlayerFigure.layer = 12;                                                                // Place it on the "Figure" layer.
        PlayerFigure.name = NamesTurn[Turn];                                                    // Assign the player’s nickname to the player’s figure
        PlayerFigure.tag = "PlayerFigure";                                                      // Assign the "PlayerFigure" tag to the figure.
        PlayerCollider = PlayerFigure.AddComponent<BoxCollider>();                              // We hang a square collider on it
        PlayerCollider.center = new Vector3(0, 0, 0);                                           // Set the location of the center of the collider to the center point of the origin of the model of the figure
        PlayerCollider.size = new Vector3(0.95f, 0.1f, 0.95f);                                  // Задаём размер коллайдеру
    }

    public void NextPlayer()                                               // Этот метод переключает игроков покругу
    {
        // First, we deal with the material of the current player
        if (Surrendered[Turn] == true)                              // If the current player now surrendered
            FlickerOfMaterial(MaterialStates.Loose);                // Then we give him the color of the surrendered.
        else                                                        // Otherwise, if he does not give up
            FlickerOfMaterial(MaterialStates.Standart);             // We call the backlighting method forcing it to turn off the highlight of the current player

        // Then we deal with the next player and his material.
        if (Turn < NumberOfPlayers - 1)     // Если номер игрока который ходит меньше общего количества игроков
            Turn++;                         // Увеличиваем номер игрока который ходит на 1
        else                                // Иначе если одинаково
            Turn = 0;                       // Возвращаем очередь на 0

        if (Surrendered[Turn] == true)      // If the player to who we switched now surrendered
            NextPlayer();                   // Call this switch method again

        if (FirstRoundOfMovesEnded)                                 // If the first cycle of moves was made
            FlickerOfMaterial(MaterialStates.Shine);                // Make the next player's shader blink
    }


    void FlickerOfMaterial(MaterialStates State)                    // This method causes the current player to flicker
    {
        if (State == MaterialStates.Standart)                                                               // If the condition of the material is equal to the "Standard"
        {
            MaterialsTurn[Turn].shader = Shader.Find("Universal Render Pipeline/Simple Lit");             // Assigning player material to a standard shader.
        }
        else if (State == MaterialStates.Shine)                                                             // If the condition of the material is equal "Shine"
            MaterialsTurn[Turn].shader = BacklightShader;                                                   // We assign shader shine to the player material
        else                                                                                                // If the condition of the material is equal to "Loose"
        {
            BackMaterialsTurn[Turn].shader = LooserShader;                                                  // Making a background player figures, black
            MaterialsTurn[Turn].shader = Shader.Find("Universal Render Pipeline/Simple Lit");             // Assigning player material to a standard shader.
        }
    }


    void MethodSurrender()                                  // Этот метод подписан на событие "Сдаться"
    {
        byte NumberSurrendered = 0;                         // Количество сдавшихся
        Surrendered[Turn] = true;                           // Записываем сдавшегося игрока в сдавшиеся

        for (byte a = 0; a < Surrendered.Count; a++)        // Continue the cycle until we have passed the entire list of surrendered / not surrendered
        {
            if (Surrendered[a] == true)                     // Если игрок с этим номером сдалься
                NumberSurrendered++;                        // Прибавляем на 1 количество сдавшихся
            else                                            // Иначе если игрок с этим номером не сдалься
                WinNomber = a;                              // Указываем что этот игрок не сдалься
        }

        if (NumberSurrendered == Surrendered.Count - 1)     // If all but one player is surrendered, then he is the winner
            EveryoneSurrendered();                          // We call an event "EveryoneSurrendered"
        else                                                // Otherwise, if there are still players
            NextPlayer();                                   // We indicate that further we will work with the next player             
    }


    void MethodEveryoneSurrendered()                                // The method triggered by the event "All surrendered"
    {
        RoundEnded = true;                                          // We indicate that the round is over and the figures can no longer be put
        FlickerOfMaterial(MaterialStates.Loose);                    // Turn off for the last player who gave up a blinking shader.       
        Turn = WinNomber;                                           // Указываем что дальше мы будем работать с выигравшим игроком
        FillTheRemainingCells();                                    // We call the method filling all available cells for the winning player
        FlickerOfMaterial(MaterialStates.Shine);                    // Making the winning player's shader blinking

        // If game mode, walkthrough & the winning player is a person and not a computer & if the player has passed the last available level
        if (GameType == GameTypes.Walkthrough && IsHeAHuman[Turn] == true && UIS.SelectedPassageMap > SL.SLO.Progress )  
        {
            SL.SLO.Progress++;                                      // Increase player progress in passing
            SL.SavingType = SaveLoad.WhatWeSaving.Progress;         // We indicate that we are going to keep the progress of the player
            SaveGameEvent();                                        // Call save game
        }

        StartCoroutine(MethodEveryoneSurrenderedСontinuation());    // We call coroutine
    }
    IEnumerator MethodEveryoneSurrenderedСontinuation()
    {
        yield return new WaitForSeconds(DelayForMenu);              // Make a delay

        FlickerOfMaterial(MaterialStates.Standart);                 // Make the winning player's shader non-blinking.
    }

    HandwrittenIntelligence FindAComputerPlayerScript(byte NomberPlayer)                    // This method finds the script of the desired computer player
    {
        GameObject ComputerCategory = GameObject.FindWithTag("CategoryOfComputerOpponents");// Find a category for computer entities
        byte nomberComputerincategory = ComputerEntityNumbers[NomberPlayer];                // We find out the number of the computer entity in the parent object "CategoryOfComputerOpponents"
        return ComputerCategory.transform.GetChild(nomberComputerincategory).GetComponent<HandwrittenIntelligence>();
    }


//    void WaitingToReleaseButton()                                   // This method waits for the first mouse release.
//    {
//        if (Input.GetMouseButtonUp(0))                              // If we let go of the mouse button
//            FirstPress = true;                                      // We set the value variable FirstPress as true
//    }


    void FillingTheInitialData()                                        // This method fills in the players' initial data before starting a new game.
    {
        NumberOfPlayers = 2;                                                                            // Set the starting number of players 2
        NamesTurn.Add("Zero");                                                                          // Fill the list "NamesTurn" primary data
        NamesTurn.Add("Cross");                                                                         // Fill the list "NamesTurn" primary data
        MaterialsTurn.Add(new Material(Materials[25]));                                                 // Fill the list "MaterialsTurn" primary data
        MaterialsTurn.Add(new Material(Materials[0]));                                                  // Fill the list "MaterialsTurn" primary data
        BackMaterialsTurn.Add(new Material(FigureBack));                                                // Fill the list "BackMaterialsTurn" primary data
        BackMaterialsTurn.Add(new Material(FigureBack));                                                // Fill the list "BackMaterialsTurn" primary data
        ColorsTurn.Add(ColorGridPanel.transform.GetChild(25).GetChild(0).GetComponent<Image>().color);  // Fill the list "ColorsTurn" primary data
        ColorsTurn.Add(ColorGridPanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().color);   // Fill the list "ColorsTurn" primary data
        GamePoints.Add(0);                                              // Fill the list "GamePoints" primary data
        GamePoints.Add(0);                                              // Fill the list "GamePoints" primary data
        FiguresTurn.Add(0);                                             // Fill the list "FiguresTurn" primary data
        FiguresTurn.Add(1);                                             // Fill the list "FiguresTurn" primary data
        IsHeAHuman.Add(true);                                           // Fill the list "IsHeAHuman" primary data
        IsHeAHuman.Add(true);                                           // Fill the list "IsHeAHuman" primary data
        Surrendered.Add(false);                                         // Fill the list "Surrendered" primary data
        Surrendered.Add(false);                                         // Fill the list "Surrendered" primary data
        ComputerDifficulty.Add(0);                                      // Specify the difficulty of 1 player as 0
        ComputerDifficulty.Add(0);                                      // Specify the difficulty of 2 player as 0
        ComputerEntityNumbers.Add(0);                                   // Fill the list "ComputerEntityNumbers" primary data
        ComputerEntityNumbers.Add(0);                                   // Fill the list "ComputerEntityNumbers" primary data
    }


    void MethodSetStateMenu()                                           // The method called by the event "SetStateMenu"
    {
        Gamestate = GameStates.Menu;                                    // We indicate that now the state of play "Menu"
        GameObject Figures = GameObject.FindWithTag("TicTacToe");       // We find the object with tag "TicTacToe" and to increase the performance we put it in the skin variable, then we turn to it
        GameObject FogCells = GameObject.FindWithTag("FogCellGroup");   // We find the object with tag "FogCellGroup" and to increase the performance we put it in the skin variable, then we turn to it
        GameObject Lines = GameObject.FindWithTag("LineRendererGroup"); // Find the object with the tag "LineRendererGroup" and put it in the variable "Lines"

        NamesTurn.Clear();                                              // Clear the "NamesTurn" list
        MaterialsTurn.Clear();                                          // Clear the "MaterialsTurn" list
        BackMaterialsTurn.Clear();                                      // Clear the "BackMaterialsTurn" list
        ColorsTurn.Clear();                                             // Clear the "ColorsTurn" list
        GamePoints.Clear();                                             // Clear the "Gamepoints" list
        FiguresTurn.Clear();                                            // Clear the "FiguresTurn" list
        IsHeAHuman.Clear();                                             // Clear the "IsHeAHuman" list
        Surrendered.Clear();                                            // Clear the "Surrendered" list
        ComputerDifficulty.Clear();                                     // Clear the "ComputerDifficulty" list
        ComputerEntityNumbers.Clear();                                  // Clear the "ComputerEntityNumbers" list

        FillingTheInitialData();                                        // Fill in the initial data of the players
        FirstCallShowWalking = true;                                    // We indicate that the variable "FirstCallShowWalking" now is true
        FirstRoundOfMovesEnded = false;                                 // We indicate that the first round of moves has not yet been completed.

        for (byte a = 0; a < CM.LineRenderersGOs.Count; a++)            // Continue the cycle until we remove all lines from the scene
        {
            Destroy(CM.LineRenderersGOs[a]);                            // Delete the next line from the scene
        }

        CM.BorderLine = new GameObject();                               // Create a GameObject "BorderLine" for the LineRenderer component
        CM.LineRenderersGOs.Clear();                                    // Clear the list LineRenderersGOs

        for (int a = 0; a < Figures.transform.childCount; a++)          // Continue the cycle until the object "Figures" has children
        {
            Destroy(Figures.transform.GetChild(a).gameObject);          // Destroy groups of figures for all players
        }

        for (int a = 0; a < FogCells.transform.GetChild(1).childCount; a++) // We continue the cycle until we remove all the outer cells of the fog
        {
            Destroy(FogCells.transform.GetChild(1).GetChild(a).gameObject); // Destroy another outer fog cot
        }

        for (int a = 0; a < FogCells.transform.GetChild(2).childCount; a++)         // We continue the cycle until we destroy all the internal groups of "FogCells"
        {
            Destroy(FogCells.transform.GetChild(2).GetChild(a).gameObject);         // Destroy all the internal groups of fog cells
        }

        for (int a = 0; a < Lines.transform.childCount; a++)            // Continue the cycle until the object "FogCells" has children
        {
            Destroy(Lines.transform.GetChild(a).gameObject);            // Destroy all the lines
        }

        if (GameObject.FindGameObjectWithTag("CategoryOfComputerOpponents").transform.childCount > 0) // If there are objects with computer intelligence scripts on the scene
        {   // Since deleting an object is delayed until the end of the frame and "COCO.transform.childCount" simply does not count that there are fewer children,
            // I use a variable to put the deleted object into it and first untie it from the parent so that the "COCO.transform.childCount" command is correct determined that there were fewer children
            GameObject COCO = GameObject.FindGameObjectWithTag("CategoryOfComputerOpponents").gameObject;  // We put a computer entity in its corresponding category
            GameObject ObjectToBeDeleted;                                   // Another object ready for deletion

            for (byte a = 0; a < 20; a++)                                   // We continue the cycle until we delete all objects with scripts of "computer intelligence"
            {
                if (COCO.transform.childCount > 0)                          // If there are still objects on the scene containing scripts of "computer intelligence"
                {
                    ObjectToBeDeleted = COCO.transform.GetChild(0).gameObject;  // Put the next object in the variable "ObjectToBeDeleted"
                    ObjectToBeDeleted.transform.parent = null;                  // Derive an object from under the influence of a parent
                    Destroy(ObjectToBeDeleted);                                 // We remove the next object with computer intelligence from the scene
                }
                else                                                        // If there are no objects found on the scene with computer intelligence scripts
                {
                    break;                                                  // Interrupt the cycle
                }
            }
        }

        Turn = 0;                                           // We reset turn of players
    }


    void MethodSetStateScheme()                             // Method called event "Set game state scheme"
    {
        Gamestate = GameStates.Scheme;                      // Indicate that now the state of the game "Scheme"
    }


    void MethodSetStateGame()               // Метод вызванный событием "Установить состояние игры (Игра)"
    {
        Gamestate = GameStates.Game;        // Указываем что состояние игры теперь "Игра"
    }


    void MethodShowWalking()                                // The method triggered by the event "ShowWalking"
    {
        UIS.ShowFigures = true;                             // We indicate that we are now showing the figures
        StartCoroutine(MethodSetStateMenuContinuation());   // We start the method of coroutine MethodSetStateMenuContinuationOld
    }
    IEnumerator MethodSetStateMenuContinuation()
    {
        yield return new WaitForSeconds(DelayShowWalking);  // We make a delay
        UIS.ShowFigures = false;                            // We indicate that we no longer show the figures
        FirstCallShowWalking = false;                       // We indicate that the variable "FirstCallShowWalking" is false
    }


    void MethodPlayGame()                                       // This method is called the "Play Current Drawing" event.
    {
        RoundEnded = false;                                     // We indicate that the level is not over yet
        AddingAllComputerEntities();                            // We call the method of creating computer entities
        ShowWalking();                                          // We call an event "ShowWalking"
    }


    void MethodPlayAgain()                                          // This method is called the "PlayAgain" event.
    {
        GameObject Figures = GameObject.FindWithTag("TicTacToe");   // We find the object with the tag "TicTacToe" and put it in the variable "Figures"
        HandwrittenIntelligence HIS;                                // This will store the “AI” called in the current iteration

        for (int a = 0; a < NumberOfPlayers; a++)                   // We continue the cycle until we pass all the existing players
        {
            Destroy(Figures.transform.GetChild(a).gameObject);      // Delete the group of figures of the next player
            GamePoints[a] = 0;                                      // Reset points for each player
            Surrendered[a] = false;                                 // We indicate the status of the next player as "not giving up"
            BackMaterialsTurn[a] = new Material(FigureBack);        // Set the standard material for the background of the next player’s figure
        }

        for (byte i = 0; i < NumberOfPlayers; i++)                  // We continue the cycle until we get through all the computer players
        {
            if (IsHeAHuman[i] == false)                             // If this player is a computer
            {
                HIS = FindAComputerPlayerScript(i);                 // Find the script of the desired computer player
                HIS.ImpossibleToGo = false;                         // Zeroing the value of the next "AI" saying that he has where to go
                HIS.MyСells.Clear();                                // We clear the list of "MyCells" from the next computer player
            }
        }

        FirstCallShowWalking = true;                                // We indicate that the variable "FirstCallShowWalking" now is true
        FirstRoundOfMovesEnded = false;                             // We indicate that the first round of moves has not yet been completed.

        Turn = 0;                                               // We reset turn of players
        RoundEnded = false;                                     // We indicate that the round was not completed
        ShowWalking();                                          // We call an event "ShowWalking"
    }


    void MethodPlayNextLevel()                                          // This method is called the "PlayNextLevel" event.
    {
        GameObject Figures = GameObject.FindWithTag("TicTacToe");       // We find the object with the tag "TicTacToe" and put it in the variable "Figures"
        GameObject FogCells = GameObject.FindWithTag("FogCellGroup");   // We find the object with tag "FogCellGroup" and to increase the performance we put it in the skin variable, then we turn to it
        HandwrittenIntelligence HIS;                                    // This will store the “AI” called in the current iteration

        for (int a = 0; a < NumberOfPlayers; a++)                       // We continue the cycle until we pass all the existing players
        {
            Destroy(Figures.transform.GetChild(a).gameObject);          // Delete the group of figures of the next player
            GamePoints[a] = 0;                                          // Reset points for each player
            Surrendered[a] = false;                                     // We indicate the status of the next player as "not giving up"
            BackMaterialsTurn[a] = new Material(FigureBack);            // Set the standard material for the background of the next player’s figure
        }

        for (byte a = 0; a < CM.LineRenderersGOs.Count; a++)            // Continue the cycle until we remove all lines from the scene
        {
            Destroy(CM.LineRenderersGOs[a]);                            // Delete the next line from the scene
        }

        for (int a = 0; a < FogCells.transform.GetChild(1).childCount; a++) // We continue the cycle until we remove all the outer cells of the fog
        {
            Destroy(FogCells.transform.GetChild(1).GetChild(a).gameObject); // Destroy another outer fog cot
        }

        for (int a = 0; a < FogCells.transform.GetChild(2).childCount; a++) // We continue the cycle until we destroy all the internal groups of "FogCells"
        {
            Destroy(FogCells.transform.GetChild(2).GetChild(a).gameObject); // Destroy all the internal groups of fog cells
        }

        for (byte i = 0; i < NumberOfPlayers; i++)                  // We continue the cycle until we get through all the computer players
        {
            if (IsHeAHuman[i] == false)                             // If this player is a computer
            {
                HIS = FindAComputerPlayerScript(i);                 // Find the script of the desired computer player
                HIS.ImpossibleToGo = false;                         // Zeroing the value of the next "AI" saying that he has where to go
                HIS.MyСells.Clear();                                // We clear the list of "MyCells" from the next computer player
            }
        }

        FirstCallShowWalking = true;                            // We indicate that the variable "FirstCallShowWalking" now is true
        FirstRoundOfMovesEnded = false;                         // We indicate that the first round of moves has not yet been completed.

        if (GameType == GameTypes.Walkthrough)                  // If the player is playing the passage mode
            UIS.SelectedPassageMap++;                           // Increase the selected level of passage for loading by one to load the next level
        else if (GameType == GameTypes.CustomMapGame)           // Otherwise if the player is playing custom map mode
            UIS.SelectedCustomMap++;                            // Increase the selected level of custom map for loading by one to load the next level

        LoadGameLevel();                                        // Raise a level load event

        Turn = 0;                                               // We reset turn of players
        RoundEnded = false;                                     // We indicate that the round was not completed
        ShowWalking();                                          // We call an event "ShowWalking"
    }


    void AddingAllComputerEntities()                            // This method creates control objects for all computer players, and puts them in the appropriate categories
    {
        HandwrittenIntelligence HIScript;                       // Variable for the control script of the processed AI entity
        byte CEN = 0;                                           // Computer entity number listed in entity category
        for (int i = 0; i < NumberOfPlayers; i++)               // We continue the cycle until we create all the computer entities specified in the game settings
        {
            if (!IsHeAHuman[i])                                 // If this player is computer
            {
                GameObject GO = new GameObject();               // Create a empty
                GO.transform.SetParent(GameObject.FindGameObjectWithTag("CategoryOfComputerOpponents").transform);  // We put a computer entity in its corresponding category
                HIScript = GO.AddComponent<HandwrittenIntelligence>();          // Add a control script to this object, and put a link to this script in the "HIScript" variable
                HIScript.ComputerDifficulty = ComputerDifficulty[i];            // We indicate in the script variable which difficulty was chosen for it.
                HIScript.PlayerNomber = (byte)i;                                // Save the player number in the script
                GO.name = "Computer_N" + i + "_D" + ComputerDifficulty[i];      // Set the name of the computer entity object
                ComputerEntityNumbers[i] = CEN;                                 // We indicate that the “i” of the computer player has a number in the computer category equal to “CEN”
                CEN++;                                                          // Increase "CEN" by one
            }
        }
    }


    void OnDisable()
    {
        SetStateMenu -= MethodSetStateMenu;                 // We write off the method "MethodSetStateMenu" from the event "SetStateMenu"
        SetStateShceme -= MethodSetStateScheme;             // Unsubscribe the method "MethodSetStateScheme" from the event "Set game state scheme"
        SetStateGame -= MethodSetStateGame;                 // We sign the "SetStateGame" method for the event "MethodSetStateGame"   
        Surrender -= MethodSurrender;                       // Отписываем метод "MethodSurrender" от события "Surrender"       
        EveryoneSurrendered -= MethodEveryoneSurrendered;   // Отписываем метод "MethodEveryoneSurrendered" от события "EveryoneSurrendered"
        ShowWalking -= MethodShowWalking;                   // Unsubscribe the "ShowWalking" method from the "MethodShowWalking" event
        PlayGame -= MethodPlayGame;                         // We write off the method "MethodPlayGame" from the event "PlayGame"
        PlayAgain -= MethodPlayAgain;                       // We write off the method "MethodPlayAgain" from the event "PlayAgain"
        PlayNextLevel -= MethodPlayNextLevel;               // We write off the method "MethodPlayNextLevel" from the event "PlayNextLevel"
    }


//----------------------------------------------------------------------------------------------- Event calls -----------------------------------------------------------------------------------------------------
    public void CallDrawTheFollowingLine()      // Этот метод вызывает событие "Рисуем следующую линию"
    {
        DrawTheFollowingLine();                 // Вызываем событие "Рисуем следующую линию"
    }


    public void CallOneLinWasFinished()         // Этот метод вызывает событие "Одна линия была законченна" 
    {
        OneLineWasFinished();                   // Вызываем событие "Одна линия был законченна"
    }


    public void CallSetStateMenu()              // This method calls the "Set the state of the game Menu" event (from UIScript)
    {
        SetStateMenu();                         // Call the event "SetStateMenu"
    }


    public void CallSaveGameEvent()             // This method calls the "SaveGameEvent" event
    {
        SaveGameEvent();                        // Call event "SaveGameEvent"
    }


    public void CallSetStateScheme()            // Этот метод вызывает событие "Установить состояние игры схема"
    {
        SetStateShceme();                       // Вызываем событие "Установить состояние игры схема"
    }


    public void CallSetStateGame()              // Этот метод вызывает событие "Установить состояние игры игра"
    {
        SetStateGame();                         // Вызываем событие игра началась
    }


    public void CallPlayGame()                  // Этот метод вызывает событие "Играть текущий чертёж"
    {
        PlayGame();                             // Вызываем событие "Играть текущий чертёж"
    }


    public void CallSurrender()                 // This method raises the "give up" event.
    {
        Surrender();                            // Вызывыаем событие "Сдаться"
    }


    public void CallShowWalking()               // This method raises the "ShowWalking" event.
    {
        ShowWalking();                          // We call an event "ShowWalking"
    }


    public void CallTheFirstPointIsSet()        // This method raises the "CallTheFirstPointIsSet" event.
    {
        TheFirstPointIsSet();                   // We call an event "CallTheFirstPointIsSet"
    }


    public void CallTheNextPointIsSet()         // This method raises the "TheNextPointIsSet" event.
    {
        TheNextPointIsSet();                    // We call an event "TheNextPointIsSet"
    }


    public void CallLoadGameLevel()             // This method raises the level load event.
    {
        LoadGameLevel();                        // Raise a level load event
    }

    public void CallPlayAgain()                 // This method triggers an event to "PlayAgain".
    {
        PlayAgain();                            // Call the "Play Again" event.
    }

    private void CallStartingCellNotFound()     // This method triggers an event to "CallStartingCellNotFound".
    {
        StartingCellNotFound();                 // Call the "StartingCellNotFound" event.
    }

    public void CallPlayNextLevel()             // This method triggers an event to "PlayNextLevel".
    {
        PlayNextLevel();                        // Call the "Play Again" event.
    }
}

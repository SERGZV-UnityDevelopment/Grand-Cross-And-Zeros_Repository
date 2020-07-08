using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraScript : MonoBehaviour
{
    public GameManager GM;                                          // Variable storing object of class GameManager
    public GameObject CamCat;                                       // Game Object on which all the cameras of the scene hang
    public Animator AnimatedСameraParent;                           // Переменная для контроля анимация главной камеры
//  public Vector3 CamPos;                                          // The last recorded position of the camera to move in the game


    void OnEnable()
    {
        GM.SetStateShceme += MethodSetStateScheme;                  // We sign the method "MethodSetStateScheme" to the event "SetStateShceme"
        GM.SetStateMenu += MethodSetStateMenu;                      // We sign the method "MethodSetStateMenu" to the event "SetStateMenu"
        GM.LoadGameLevel += MethodLoadGameLevel;                    // We sign the method "MethodLoadGameLevel" to the event "LoadGameLevel"
    }


    void MethodSetStateScheme()                                     // Метод вызываемый событием "Установить состояние игры схема"
    {
        PointTheCameraAtTheMap();                                   // Call the method "PointTheCameraAtTheMap"
    }


    void MethodLoadGameLevel()                                      // This method is called by the load game level event.
    {
        PointTheCameraAtTheMap();                                   // Call the method "PointTheCameraAtTheMap"
    }


    void MethodSetStateMenu()                                       // The method called by the event "SetStateMenu"
    {
        
        AnimatedСameraParent.SetBool("GoToTheMenu", true);          // Set the parameter "go to the main menu" true value to start the animation animation of the camera to the main menu
        StartCoroutine(MethodSetStateMenuContinuation());           // Launch the coroutine - the continuation of the method "Set the game state of the menu"
    }
    IEnumerator MethodSetStateMenuContinuation()                    // Korutina - continuation of the method above
    {
        yield return new WaitForSeconds(1);                         // We make a delay
        AnimatedСameraParent.SetBool("GoToTheMenu", false);         // Set the parameter "go to the main menu" the value of the false setting it in the starting position
    }

    
    void PointTheCameraAtTheMap()
    {
        AnimatedСameraParent.SetBool("GoToTheTable", true);         // Устанавливаем параметру передти к столу значение правда для запуска анимации камеры перехода к столу
        StartCoroutine(MethodPointTheCameraAtTheMapСontinuation()); // Запускаем корутину - продолжение метода "Установить состояние игры схема"
    }
    IEnumerator MethodPointTheCameraAtTheMapСontinuation()          // Korutina - continuation of the method above
    {
        yield return new WaitForSeconds(1);                         // Делаем задержку
        AnimatedСameraParent.SetBool("GoToTheTable", false);        // Устанавливаем параметру "перейти к столу" значение ложь ставя его в начальное положение
    }

  
    void OnDisable()
    {
        GM.SetStateShceme -= MethodSetStateScheme;                  // Отписываем метод "MethodSetStateScheme" от события "Установить состояние игры схема"
        GM.SetStateMenu -= MethodSetStateMenu;                      // We write off the method "MetnodsetsStatmenu" from the event "SetStatmenu"
        GM.LoadGameLevel -= MethodLoadGameLevel;                    // We write off the method "MethodLoadGameLevel" from the event "LoadGameLevel"
    }
}


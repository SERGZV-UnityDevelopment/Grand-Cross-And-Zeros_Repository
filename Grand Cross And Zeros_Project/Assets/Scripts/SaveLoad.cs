// This script saves and loads data.
// Save folder on windows Application.Persistentdatapah - C:\Users\SERG__ZV\AppData\LocalLow\SERGZV\GrandCrossAndZeros\
// Save folder on Android Application.Persistentdatapah - Android/data/com.SERG__ZV.GrandCrossAndZeros/files (папка скрыта и после компилирования структура конечной папки иная)
// UIS.Panels[14].transform.GetChild(2).GetComponent<Text>().text = "Папка сохранения не найденна создаём её";           // Быстрый пример обращения к игровой консоли
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Resources;


public class SaveLoad : MonoBehaviour
{
    public SavedData SLO;                                   // (SaveLoadObject) Object to load and save
    public Level CurrentPassageLevel;                       // The current loaded passage level that we are going to pass
    public GameManager GM;                                  // Variable storing object of class GameManager
    public CellsMaker CM;                                   // The "CellsMaker" script is stored here.
    public UIScript UIS;                                    // Variable for object of class "UIScript"
    public ScreenshotScript SS;                             // Here will be a script taking screenshots
    public bool TheMapHasBeenSaved = false;                 // The variable indicates whether the method of saving the "SaveGame" card in the "SaveLoad" class was completed.
    public bool LoadPassageMap;                             // We load the map of passage? true - we load map passage, false - we load custom map
    public int DownloadableLevelNumber;                     // Downloadable level number
    public enum WhatWeSaving : byte                         // Listing What We Save 0-Nothing, 1-CustomMap, 2-Progress, 3-PassageMap, DeleteUserMap
    {
        Nothing,
        CustomMap,
        Progress,
        SavingPassageMap,
        DeleteUserMap
    }   
    public WhatWeSaving SavingType;                                                     // A specific variable indicating what we save this time

    void OnEnable()
    {
        GM.SaveGameEvent += SaveGame;                               // We sign the method "SaveGame" to the event "SaveGameEvent"
        GM.LoadGameEvent += LoadGame;                               // We sign the method "LoadGame" to the event "LoadGameEvent"
    }

    public List<int> GetNumMapScreen                                // Property, Number Maps Screenshots
    {
        get
        {
            if (SLO != null)                                        // If the save object exists
                return SLO.NumberMapsScreenshots;                   // Return the "NumberMapsScreenshots" list
            else                                                    // If the save object does not exist
                return null;                                        // Return null
        }
    }
    
    public void LoadGame()                                          // This method completely loads the game at the start of the game, the progress in the passage as well as the maps created
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Saves"))           // If the game does not have a save folder
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");   // Create a folder for saving
        }

        if (File.Exists(Application.persistentDataPath + "/Saves/GameData.bytes"))     // If the save file exists
        {
            //deserialize
            // Call implicitly the garbage collector after loading a file using the "using" operator. Create a stream with the file name "fs" and read in the variable "fs" data from the stream.
            using (Stream fs = File.OpenRead(Application.persistentDataPath + "/Saves/GameData.bytes"))
            {
                SLO = (SavedData)new BinaryFormatter().Deserialize(fs);             // Load all the information from it into the object(TestLoad) of the class (Saved data).
            }
            StartCoroutine(LoadGameСontinuation());                                 // Call coroutine
        }
    }
    IEnumerator LoadGameСontinuation()                              // Coroutine, a continuation of the "LoadGame" method
    {
        UnityWebRequest www;                                        // Variable for UnityWebRequest

        if (SLO.NumberMapsScreenshots.Count > 0)                    // If at least one custom map screenshot exists
        {
            for (byte a = 0; a < SLO.NumberMapsScreenshots.Count; a++)                                      // We continue the cycle until we upload all the screenshots to the "ListOfCustomMapScreenshots" list
            {
                www = UnityWebRequestTexture.GetTexture("file://" + Application.persistentDataPath + "/Saves/" + SLO.NumberMapsScreenshots[a] + ".png");   // Load the screenshot into a variable "www"
                yield return www.SendWebRequest();  // We are waiting for the screenshot to load.
                UIS.ListOfCustomMapScreenshots.Add(((DownloadHandlerTexture)www.downloadHandler).texture);  // We load the next screenshot of the custom map into the list "ListOfCustomMapScreenshots"
            }
        }
    }

    public void LoadPassageLevel()  // This method loads one specific passage level on demand.
    {
        TextAsset BinFileMap = Resources.Load<TextAsset>("PassageMaps/Maps/" + (UIS.SelectedPassageMap - 1));   // We load the level as a binary file into the variable "BinFileMap"

        //deserialize
        // Call implicitly the garbage collector after loading a file using the "using" operator. Create a stream with the file name "fs" and read in the variable "fs" data from the stream.
        using (Stream fs = new MemoryStream(BinFileMap.bytes))
        {
            CurrentPassageLevel = (Level)new BinaryFormatter().Deserialize(fs);         // Load all the information from it into the object(TestLoad) of the class (Saved data).
        }
    }

    public void SaveGame()                                                              // This method saves game map.
    {
        if (SavingType == SaveLoad.WhatWeSaving.CustomMap)                              // If we save a custom map
        {
            Level lvl = new Level();                                                    // Create a new object (lvl) of my class (LevelList),
            lvl.Lines.AddRange(CM.LineList);                                            // Add to the object "lvl" a list of saved lines
            lvl.CellGroup.AddRange(CM.CellGroupList);                                   // Add saved fog cells to object "lvl"

            SLO.CustomLevels.LevelList.Add(lvl);                                        // Add a new created level to the object (SLO) of the serializing class (saved data)

            if (SLO.NumberMapsScreenshots.Count == 0)                                   // If the list of maps names-numbers is empty
            {
                Debug.Log("Список номеров карт пуст");
                SLO.NumberMapsScreenshots.Add(0);                                       // So we create in it the first object with the number 0
                SLO.NumLastScreen = 0;                                                  // Specify the number under which the first screenshot was saved

                Debug.Log("Номер последнего сохранённого скриншота " + SLO.NumLastScreen);
            }
            else                                                                        // Otherwise, if the number of at least one map is saved
            {
                Debug.Log("Список номеров карт больше нуля и был равен: " + SLO.NumberMapsScreenshots.Count);
                for (int i = 0; i < SLO.NumberMapsScreenshots.Count; i++)
                {
                    Debug.Log("Номер очередного номера скриншота: " + i + " - " + SLO.NumberMapsScreenshots[i]);
                }

                Debug.Log("Под каким номером была сохранена предыдущая карта " + SLO.NumLastScreen);

                SLO.NumLastScreen++;

                Debug.Log("Под каким номером мы сохраним новую карту " + SLO.NumLastScreen);

                SLO.NumberMapsScreenshots.Add(SLO.NumLastScreen);         // Save the name of the map as the number to save it

                

                Debug.Log("Теперь номер последней сохранённой карты равен: " + SLO.NumLastScreen);
            }

            //serialize
            // Call implicitly the garbage collector after saving a file using the "using" operator. Create a stream with the name "fs", Immediately create a file by the path specified in the code.
            using (Stream fs = File.Create(Application.persistentDataPath + "/Saves/GameData.bytes"))
            {
                new BinaryFormatter().Serialize(fs, SLO);                               // We save all data to a file in the path specified in the code.
            }

            StartCoroutine(SaveGameСontinuation());
        }
        else if (SavingType == SaveLoad.WhatWeSaving.Progress)                          // If we keep progress
        {
            //serialize
            // Call implicitly the garbage collector after saving a file using the "using" operator. Create a stream with the name "fs", Immediately create a file by the path specified in the code.
            using (Stream fs = File.Create(Application.persistentDataPath + "/Saves/GameData.bytes"))
            {
                new BinaryFormatter().Serialize(fs, SLO);  // We save all data to a file in the path specified in the code.
            }
        }
        else if (SavingType == SaveLoad.WhatWeSaving.SavingPassageMap)                  // If we save the passage map
        {
            Level PassageLevel = new Level();                                           // Create a new object (PassageLevel) of my class (Level)
            PassageLevel.Lines.AddRange(CM.LineList);                                   // Add to the object "lvl" a list of saved lines
            PassageLevel.CellGroup.AddRange(CM.CellGroupList);                          // Add saved fog cells to object "lvl"

            if (!Directory.Exists(Application.persistentDataPath + "/Saves/PassageMap"))           // If the game does not have a save folder
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves/PassageMap");   // Create a folder for saving
            }

            //serialize
            // Call implicitly the garbage collector after saving a file using the "using" operator. Create a stream with the name "fs", Immediately create a file by the path specified in the code.
            using (Stream fs = File.Create(Application.persistentDataPath + "/Saves/PassageMap/0.bytes"))
            {
                new BinaryFormatter().Serialize(fs, PassageLevel);      // We save all data to a file in the path specified in the code.
            }
        }
        else if (SavingType == SaveLoad.WhatWeSaving.DeleteUserMap)  // If we delete the user map
        {
            //serialize
            // Call implicitly the garbage collector after saving a file using the "using" operator. Create a stream with the name "fs", Immediately create a file by the path specified in the code.
            using (Stream fs = File.Create(Application.persistentDataPath + "/Saves/GameData.bytes"))
            {
                new BinaryFormatter().Serialize(fs, SLO);  // We save all data to a file in the path specified in the code.
            }
        }

        TheMapHasBeenSaved = true;                                                  // We indicate that the preservation in this method was completed to the end
    }
    IEnumerator SaveGameСontinuation()                              // Coroutine, a continuation of the "LoadGame" method
    {
        UnityWebRequest www;    // Variable for UnityWebRequest

        if (SLO.NumberMapsScreenshots.Count > 0)            // If at least one custom map screenshot exists
        {
            // Load the screenshot into a variable "www"
            www = UnityWebRequestTexture.GetTexture("file://" + Application.persistentDataPath + "/Saves/" + SLO.NumberMapsScreenshots[SLO.NumberMapsScreenshots.Count - 1] + ".png");   

            for (int i = 0; i < 15; i++)                    // We continue the cycle 15 times, this should be enough for verification
            {
                yield return new WaitForSeconds(0.1f);      // We are waiting for 0.1 second
                if (SS.SavedСurrentScreenshot == true)
                {
                    break;
                }
            }

            yield return www.SendWebRequest();              // We are waiting for the screenshot to load.
            UIS.ListOfCustomMapScreenshots.Add(((DownloadHandlerTexture)www.downloadHandler).texture);  // We load the next screenshot of the custom map into the list "ListOfCustomMapScreenshots"     
            StartCoroutine(UIS.MapSavedNotice());           // We call the coroutine notifying that the map was saved
        }
    }


    public void DeleteCustomMap()                           // This method removes a screenshot of the user map as well as data about it in the save file.
    {
        SavingType = SaveLoad.WhatWeSaving.DeleteUserMap;               // We indicate that we want to delete the user map

        int RemovableScreenshot = UIS.SelectedCustomMap - 1;            // Get the screenshot number to be deleted.

        File.Delete(Application.persistentDataPath + "/Saves/" + SLO.NumberMapsScreenshots[RemovableScreenshot] + ".png");      // Delete the screenshot of the selected map
        UIS.ListOfCustomMapScreenshots.RemoveAt(RemovableScreenshot);   // Delete a screenshot from the list of screenshots to display
        SLO.NumberMapsScreenshots.RemoveAt(RemovableScreenshot);        // Delete the number of this map from the list of map numbers
        SLO.CustomLevels.LevelList.RemoveAt(RemovableScreenshot);       // Delete selected level from save
        GM.CallSaveGameEvent(); // Call the game save event
        
        if (UIS.SelectedCustomMap > UIS.ListOfCustomMapScreenshots.Count)   // If the current map number is now greater than the total number of maps
            UIS.SelectedCustomMap = UIS.ListOfCustomMapScreenshots.Count;   // We indicate the number of the current card as the last

        UIS.LoadScreenshotsOfMaps();                                    // We often start the map loading method to update the map selection window
    }

    private void OnDisable()
    {
        GM.SaveGameEvent -= SaveGame;           // We write off the method "MethodPlayGame" from the event "PlayGameButton"
        GM.LoadGameEvent -= LoadGame;           // We write off the method "LoadGame" from the event "LoadGameEvent"
    }
}

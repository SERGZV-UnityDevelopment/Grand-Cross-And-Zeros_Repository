using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ScreenshotScript : MonoBehaviour
{
    public GameManager GM;                                          // Variable storing object of class GameManager
    public SaveLoad SL;                                             // SaveLoad class object
    public bool SavedСurrentScreenshot = false;                     // This variable indicates whether the last screenshot was saved.
    Camera CamComponent;			                                // Link to the camera component


    void OnEnable()
    {
        GM.SaveGameEvent += SaveGame;                               // We sign the "SaveGame" method for the event "SaveGameEvent"
    }


    private void Start()
    {
        CamComponent = GetComponent<Camera>();						// Get the camera component from the current camera object "CameraForScreenshotOfTheMap" and place it in the CamClone variable.
    }


    public void SaveGame()                                                  // This method saves game data.
    {
        if (SL.SavingType == SaveLoad.WhatWeSaving.CustomMap)               // If we save a custom map
        {
            transform.position = new Vector3(170, 250, -100);               // Indicate the position of the photographing camera
            transform.rotation = Quaternion.Euler(90, 90, 0);               // Specify the rotation of the photographing camera
            StartCoroutine(SaveMapСontinuation());                          // We call coroutine
        }   
        else if (SL.SavingType == SaveLoad.WhatWeSaving.SavingPassageMap)   // If we save the passage map
        {
            transform.position = new Vector3(170, 250, -100);               // Indicate the position of the photographing camera
            transform.rotation = Quaternion.Euler(90, 90, 0);               // Specify the rotation of the photographing camera
            StartCoroutine(SaveMapСontinuation());                          // We call coroutine
        }
    }
    IEnumerator SaveMapСontinuation()                               // This method is a continuation of the SaveGame method.
    {
        for (;!SL.TheMapHasBeenSaved;)                              // Repeat the cycle until saving is performed in the class "SaveLoad"
            yield return new WaitForSeconds(0.1f);                  // We make a delay before the next check.
        
        yield return new WaitForEndOfFrame();                       // Waiting for the end of frame rendering

        int width = Screen.width;                                                                   // Determine the screen size by height we put it in the variable width
        int height = Screen.height;                                                                 // Determine the screen size to the width we put it in the variable height

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);                   // Create a new variable of the texture type and define its parameters.
        RenderTexture RendTex = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);   // Render texture in which the image will first be rendered

        CamComponent.targetTexture = RendTex;										                // Set the "CameraForScreenshotOfTheMap" camera to RenderTexture as the target texture
        CamComponent.Render();																        // Render the image from the camera "CameraForScreenshotOfTheMap" into the render texture
        RenderTexture.active = RendTex;											                    // Specify the active render texture from where we consider the method ReadPixels

        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);                                        // Read pixels from screen to texture.
        tex.Apply();                                                                                // Save texture

        var bytes = tex.EncodeToPNG();                                                              // Save texture as bytes to variable "bytes"
        Destroy(tex);                                                                               // Destroy the texture that is no longer needed

        int nomber_map = 0;                                                                         // By default, set the map number as 0

        if (SL.SavingType == SaveLoad.WhatWeSaving.CustomMap)                                       // If we save a custom map
            nomber_map = SL.GetNumMapScreen[SL.GetNumMapScreen.Count - 1];                          // We find out the map number that we now save

        if (SL.SavingType == SaveLoad.WhatWeSaving.CustomMap)
            File.WriteAllBytes(Application.persistentDataPath + "/Saves/" + nomber_map + ".png", bytes);// Save the stream of bytes in the image file PNG with variable name "nomber_map"
        else if (SL.SavingType == SaveLoad.WhatWeSaving.SavingPassageMap)
            File.WriteAllBytes(Application.persistentDataPath + "/Saves/PassageMap/0.png", bytes);// Save the stream of bytes in the image file PNG with variable name "nomber_map"

        SavedСurrentScreenshot = true;                              // We indicate that saving the current screenshot was completed

        yield return new WaitForSeconds(1);                         // Waiting one second

        SavedСurrentScreenshot = true;                              // We set the default value for the variable to avoid errors
    }
  

    void OnDisable()
    {
        GM.SaveGameEvent -= SaveGame;                               // Unsubscribe the "SaveGame" method from the "SaveGameEvent" event
    }
}

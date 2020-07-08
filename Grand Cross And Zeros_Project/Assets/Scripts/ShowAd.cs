using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class ShowAd : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IOS
    public static int countPress = 0;

    void Start()
    {
        if (Advertisement.isSupported)      // Если объявления поддерживаются
        { 
            //Advertisement.Initialize("5ed6a35ce9cbc0b67a4cbc07"); //ID here
            Advertisement.Initialize("3637099"); //ID here
        }
        else Debug.LogError("Не поддерживаются объявления, нужно узнать как включить их поддержку");
    }


    public static void AdShow(int showEvery)
    {
        countPress += 1;

        if (countPress % showEvery == 0 && Advertisement.IsReady())
        {
            Advertisement.Show();
        }
    }


    public static bool AdShow()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
            return true;
        }
        else
        {
            return false;
        }
    }
#endif
}

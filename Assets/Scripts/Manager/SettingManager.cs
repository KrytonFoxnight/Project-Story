using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    List<Resolution> resolutions = new List<Resolution>();

    void Awake()
    {
        resolutions.AddRange(Screen.resolutions);
        foreach(Resolution item in resolutions)
        {
            Debug.Log(item.width + " * " + item.height + " " + item.refreshRate);
        }
    }

    public void FullScreen()
    {
        Screen.fullScreen = true;
    }

    public void WindowedScreen()
    {
        Screen.fullScreen = false;
    }
}

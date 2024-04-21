using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControl : MonoBehaviour
{

    public EventHandler unpause;

    public void Resume()
    {
        gameObject.SetActive(false);
        unpause.Invoke(this, EventArgs.Empty);
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}

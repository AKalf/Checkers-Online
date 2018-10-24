using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyConsole : MonoBehaviour
{

    private static MyConsole instance = null;

    public Text debuggerText = null;

    // Use this for initialization
    void Start()
    {
        instance = this;
    }
    
  
    public static MyConsole GetInstance()
    {
        return instance;
    }
    public void ShowMessage(string msg)
    {
        if (debuggerText != null)
        {
            debuggerText.text += msg + "\n";
            debuggerText.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f;
        }

    }
}


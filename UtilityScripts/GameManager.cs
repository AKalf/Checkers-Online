using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Client.GetInstance().LoadGameScene();
        if (GameObject.Find("Server") != null)
        {
            GameObject.Find("Server").GetComponent<Server>().LoadGameScene();
            //Debug.Log("Server loading game");
        }


    }
}
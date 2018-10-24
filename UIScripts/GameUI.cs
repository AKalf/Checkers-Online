using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public InputField chatField;
    public Text turnText;
    private static GameUI inst;

    private void Awake() {
        inst = this;
        //chatField = GameObject.Find("ChatInputField").GetComponent<InputField>();
    }
    private void Update()
    {
        if (chatField.text != "" && Input.GetKey(KeyCode.Return))
        {
            Client.GetInstance().SendMessageToServer("MSG~" + chatField.text);
            chatField.text = "";
        }
    }
    public static GameUI GetInst() {
        return inst;
    }

    public void SetPlayerTurnText(string playerTurn) {
        if (UserInput.GetInstance().GetPlayerColor() == "White")
        {
           
            if (playerTurn == "White")
            {               
                turnText.GetComponentInParent<Image>().color = Color.green;
            }
            else {                
                turnText.GetComponentInParent<Image>().color = Color.red;
            }
        }
        else if (UserInput.GetInstance().GetPlayerColor() == "Black")
        {
            
            if (playerTurn == "Black")
            {              
                turnText.GetComponentInParent<Image>().color = Color.green;
            }
            else
            {              
                turnText.GetComponentInParent<Image>().color = Color.red;
            }
        }
        if (playerTurn == "White")
        {
            turnText.text = "White player's turn";
        }
        else {
            turnText.text = "Black player's turn";
        }
    }

    public void QuitGame()
    {
        try
        {
            Client.GetInstance().SendMessageToServer("QG~");
        }
        catch (Exception ex ){
            Debug.Log("Could not reach server. Error: " + ex.Message);
        }
        SceneManager.LoadScene(0);
        
        
    }
}

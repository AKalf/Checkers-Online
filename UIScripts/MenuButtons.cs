using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MenuButtons : MonoBehaviour {

    private static string playerNameProfile= "";

    public GameObject mainMenu;
    public GameObject hostMenu;
    public GameObject connectMenu;
    public GameObject waitingPanel;
    public InputField hostAddress;
    public InputField playerName;
    public InputField connectAddress;

    private GameObject serverGO;
    private GameObject clientGO;
    private void Start()
    {
        if (Server.GetInstance() != null) {
            Destroy(Server.GetInstance().gameObject);
        }
        if (Client.GetInstance() != null)
        {
            Destroy(Client.GetInstance().gameObject);
        }
        playerName.text = playerNameProfile;
    }
    // Update is called once per frame
    void Update ()
    {
        // if a server is created
        if (serverGO != null) {
            // when he has been initialized and is ready
            if (serverGO.GetComponent<Server>().HasServerStarted() && clientGO == null) {
                // create a client for the host
                clientGO = new GameObject("Client");
                //Debug.Log("client go created");
                clientGO.AddComponent<Client>().InitializeClient((playerName.text == "") ? "Host" : playerName.text, hostAddress.text);
            }
        }
	}
    // Changes UI elements
    public void GoToClientMenuButton() {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }
    // Changes UI elements
    public void GoToHostMenuButton() {
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);
    }
    // creates a gameobject "Server", adds "Server" component to it, tries to initialize server and then goes to "waiting for player" screen 
    public void StartHosting()
    {
        playerNameProfile = playerName.text;
        if (GameObject.Find("Client") != null)
        {
           
            Destroy(GameObject.Find("Client"));
        }
        else if (GameObject.Find("Server") != null) {
            Destroy(GameObject.Find("Server"));
        }
        if ((hostAddress.text != "" || hostAddress.text == "Write your ip") && clientGO == null)
        {
            try
            {
                serverGO = new GameObject("Server");               
                serverGO.AddComponent<Server>().InitializeServer(hostAddress.text);
                hostMenu.SetActive(false);
                waitingPanel.SetActive(true);
                if (serverGO != null) {
                    //Debug.Log("ServerGO created");
                    MyConsole.GetInstance().ShowMessage("Server created successfully");
                }
            }
            catch (Exception ex) {
                Destroy(serverGO);
                Debug.Log(ex.Message);
                MyConsole.GetInstance().ShowMessage("Given IP is not correct");
            
            }
        }              
        else {
            hostAddress.text = "Write your ip";
        }
        
    }
    // Creates a client and initializes it
    public void ConnectToServerButton() {
        playerNameProfile = playerName.text;
        if (GameObject.Find("Client") != null) 
        {
            Destroy(GameObject.Find("Client"));
        }
        if (connectAddress.text != "" && clientGO == null)
        {
            clientGO = new GameObject("Client");
            try
            {
                clientGO.AddComponent<Client>().InitializeClient((playerName.text == "") ? "Client" : playerName.text, connectAddress.text);
            }
            catch (Exception ex) {
                Destroy(clientGO.gameObject);
                MyConsole.GetInstance().ShowMessage("Host ip is not valid");
                Debug.Log("Could not connect to server: " + ex.Message);
            }
        }       
    }
    // Back to the main menu
    public void BackButton() {
        Destroy(serverGO);
        Destroy(clientGO);
        serverGO = null;
        clientGO = null;
        mainMenu.SetActive(true);
        hostMenu.SetActive(false);
        connectMenu.SetActive(false);
        waitingPanel.SetActive(false);

    }
    public void QuitAppButton() {
        Application.Quit();
    }
}

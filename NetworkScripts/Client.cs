
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Xml;

class Client : MonoBehaviour
{
    static Client inst = null;

    int serverPort = 60000;

    string playerName;
    string playerColor = "";
    string readData = "";

    bool readingBoard = false;
    bool isSocketReady = false;
  

    MyConsole console;
    UserInput userInput;
    BoardManager board;

    TcpClient client = null; // The Socket class allows you to perform both synchronous and asynchronous data transfer using any of the communication protocols listed in the ProtocolType enumeration.
    NetworkStream clientStream;
    StreamWriter writer;
    StreamReader reader;


    void Update()
    {
        if (isSocketReady == true && client.Connected)
        {
            if (readingBoard == true)
            {
                Debug.Log("reading board");
                string tempData = "";
                tempData = reader.ReadLine();
                if (tempData != "~")
                {
                    readData = readData + tempData;
                }
                else
                {
                    Debug.Log("reading board ended");
                    List<PieceInfo> pieces = DeSerialize(readData);
                    board.SetPiecesPosition(pieces);
                    readingBoard = false;
                }
            }
            if (clientStream.DataAvailable && readingBoard == false)
            {              
                string data = reader.ReadLine();                
                if (data != null)
                {
                    OnIncomingData(data);

                }
            }
        }     
    }


    public static Client GetInstance()
    {
        return inst;
    }
    // Use this for initializing a new component of type: Client
    public void InitializeClient(string tempPlayerName, string tempHostAddress)
    {
        if (inst == null)
        {
            inst = this;
            console = GameObject.Find("MyConsole").GetComponent<MyConsole>();
            DontDestroyOnLoad(gameObject);
            playerName = tempPlayerName;
            ConnectToServer(tempHostAddress, serverPort);
        }
    }
    // try to connect to server
    private bool ConnectToServer(string hostAddress, int port)
    {
        if (!isSocketReady)
        {
            try
            {
                client = new TcpClient(hostAddress, port);
                clientStream = client.GetStream();
                reader = new StreamReader(clientStream, true);
                writer = new StreamWriter(clientStream);
                SendMessageToServer("CNAME~" + playerName);
                isSocketReady = true;
            }
            catch (Exception ex)
            {
                console.ShowMessage("Could not connect to address ");
                Destroy(gameObject);
                Debug.Log(ex.Message);
            }
        }
        return isSocketReady;
    }
   
    // send messages
    public void SendMessageToServer(string data)
    {
        Debug.Log("client sending messaga " + data);
        writer.WriteLine(data);
        writer.Flush();


    }

    // read messages
    private void OnIncomingData(string data)
    {
        string[] lines;
        lines = data.Split('~');
        for (int i = 0; i != lines.Length; i++)
        {
            switch (lines[i])
            {
                case "STRTG": // start game, set this player color                    
                    SceneManager.LoadScene(1);
                    playerColor = (lines[i + 1]); // set this player color                   
                    break;
                case "MSG": // a message to write to console
                    console.ShowMessage(lines[i + 1]);
                    break;
                case "MOV": // a move for GenerateBoard to do
                    DecodeMovData(lines[i + 1], lines[i + 2], lines[i + 3], lines[i + 4]);
                    break;
                case "PCLRN": // set player color to none
                    playerColor = "";
                    userInput.SetPlayerColor(playerColor);
                    break;
                case "QG": // message that somebody has left the game                    
                    SceneManager.LoadScene(0);
                    Destroy(gameObject);
                    break;
                case "GRABB": // indicates that board should be updated and reading-board-proccess should start 
                    readingBoard = true;
                    break;
                case "PT": // sets current player turn
                    Debug.Log("Received PT," + lines[i + 1]);
                    if (lines[i + 1] == "White")
                    {
                        userInput.SetTurnForPlayer("White");
                        Debug.Log("entered PT true");
                    }
                    else if(lines[i + 1] == "Black") {
                        userInput.SetTurnForPlayer("Black");
                        Debug.Log("entered PT false");
                    }
                    
                    break;
            }
            Debug.Log("Incoming client message: " + data);
        }
    }
    // Decode "MOV" messages and send data to GenerateBoard in order to move the right piece to the right place
    private void DecodeMovData(string pieceXpos, string pieceZpos, string boardCubeXpos, string boardCubeZpos)
    {
        //Debug.Log("Decoding MOV mesg...");
        int pXpos = -1;
        int pZpos = -1;
        int bXpos = -1;
        int bZpos = -1;
        //Debug.Log("p" + pieceXpos + " " + pieceZpos + "b" + boardCubeXpos + " " + boardCubeZpos);
        for (int i = 1; i != 9; i++)
        {

            //Debug.Log("i = " + i.ToString());
            if (i.ToString() == pieceXpos)
            {

                pXpos = i;
            }
            if (i.ToString() == pieceZpos)
            {
                pZpos = i;
            }
            if (i.ToString() == boardCubeXpos)
            {
                bXpos = i;
            }
            if (i.ToString() == boardCubeZpos)
            {
                bZpos = i;
            }
        }
        //Debug.Log("p" + pXpos + " " + pZpos + "b" + bXpos + " " + bZpos);
        if (pXpos > -1 && pZpos > -1 && bXpos > -1 && bZpos > -1)
        {
            //Debug.Log("Moving piece");
            try
            {
                BoardManager.GetInstance().MovePiece(board.FindPieceByPosition(pXpos, pZpos), board.FindBoardCubekByPosition(bXpos, bZpos));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    // Called by gameManager, when entering "GAME" scene in order to update this class refferences with the ones from "GAME" scene
    public void LoadGameScene() {
        try
        {
            console = GameObject.Find("MyConsole").GetComponent<MyConsole>();
            board = BoardManager.GetInstance();
            userInput = UserInput.GetInstance();
            userInput.SetPlayerColor(playerColor);
            SendMessageToServer("RDYG~");
            console.ShowMessage("*Your color is: *" + playerColor + "*");           
        }
        catch (Exception ex){
            Debug.Log("Could not load game scene " + ex);
        }
    }
    public List<PieceInfo> DeSerialize(string xmlString)
    {     
        XmlSerializer xmlSerializer;
        MemoryStream memStream = null;
        try
        {          
            xmlSerializer = new XmlSerializer(typeof(List<PieceInfo>));            
            byte[] bytes = new byte[xmlString.Length];
            Encoding.ASCII.GetBytes(xmlString, 0, xmlString.Length, bytes, 0);
            memStream = new MemoryStream(bytes);
            memStream.Seek(0, SeekOrigin.Begin);
            object objectFromXml = xmlSerializer.Deserialize(memStream);
            List<PieceInfo> pieces = (List<PieceInfo>)objectFromXml;           
            return pieces;
        }        
        catch (Exception ex)
        {
            Debug.Log ("Error when de-serializing: " + ex.Message);
            MyConsole.GetInstance().ShowMessage("Could not synchronize game please quit and try again");
            return null;
        }
       
    }
    

    private void OnDestroy()
    {
        if (inst == this)
        {
            inst = null;
        }
        Close();
        
    }
    public void Close()
    {
        try
        {
            if (client != null && BoardManager.GetInstance() != null && client.Connected)
            {
                try
                {
                    SendMessageToServer("QG~"); // send message that this client is leaving the connection
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
            if (writer != null) // if writer exist, close it
            {
                writer.Close();
            }
            if (clientStream != null) // if writer exist, close it
            {
                clientStream.Close();
            }
            if (reader != null) // if writer exist, close it
            {
                reader.Close();
            }
            client.Close();
        }
        catch (Exception ex)
        {
            Debug.Log("Error when closing client: " + ex.Message);
        }
    }
    
    private void OnApplicationQuit()
    {
        if (client != null) {
            Close();
        }
           
        
    }
}

        
        
        
        
    


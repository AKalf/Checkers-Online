using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
/*
* Server can handle more than 2 clients, however the host and the fisrt client, 
* that connects to the server will play. The rest will be spectators
*/
public class Server : MonoBehaviour
{

    static Server inst;

    int port = 60000;

    bool isGameRunning = false;
    bool gameEnded = false;
    bool serverStarted;

    Piece[,] map;
    List<ClientInfo> clients = new List<ClientInfo>();
    List<ClientInfo> clientsToBeDisconnected = new List<ClientInfo>();
 
    BoardManager board;
    MyConsole console;    
    
    TcpListener tcpListener;   


    public static Server GetInstance() {
        return inst;
    }  
    
    private void Update()
    {
        if (serverStarted)
        {
            RemoveDisconnectedClients();
            foreach (ClientInfo client in clients)
            {
                CheckForDisconnectedClients(client);
                ReadFromConnectedClients(client);
            }
            
        }


    }

    // use this for initialization of server components
    public void InitializeServer(string address)
    {
        if (inst == null)
        {
            inst = this;
            DontDestroyOnLoad(gameObject);
            
            // translate string address to number
            string[] serverAddressparts = (address).Split(new char[] { '.' });
            byte[] serverAddressBytes = new byte[4];
            // Take each element from array: addressparts, convert them to byte and save each to a different shell 
            for (int i = 0; i != serverAddressparts.Length; i++)
            {
                serverAddressBytes[i] = Byte.Parse(serverAddressparts[i]);
            }
            

            IPAddress serverAddress = new IPAddress(serverAddressBytes);
            IPEndPoint localEndpoint = new IPEndPoint(serverAddress, port);
            // create socket
            tcpListener = new TcpListener(localEndpoint);
            tcpListener.Start();
            console = MyConsole.GetInstance();
           
            try
            {
                StartListening();
            }
            catch (Exception ex)
            {
                Debug.Log("Error on initializing server:  " + ex);

            }
        }

    }
    // server starts listening for incoming connections
    private void StartListening()
    {
        try
        {
            serverStarted = true;
            //Debug.Log("Listening");
            tcpListener.BeginAcceptSocket(AcceptTcpClient, tcpListener);
        }
        catch (Exception ex)
        {
            Debug.Log("Could not start listening " + ex.Message);
        }

    }
    // Accepts tcpClients that request for connection
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState; // get the socket trying to connect
        ClientInfo newClient = new ClientInfo(listener.EndAcceptTcpClient(ar)); // create a new client and put to the socket var, the socket that was just accepted
        clients.Add(newClient); // add him to the list of clients    
        
        StartListening(); // keep listening
       
    }
    
    // Send message from server to all clients
    private void BroadCast(string data)
    {
        foreach (ClientInfo client in clients)
        {

            try
            {
                StreamWriter writer = new StreamWriter(client.tcpClient.GetStream());
                writer.WriteLine(data);
                Debug.Log("Broadcasting: " + data);
                writer.Flush();
               
            }
            catch (Exception ex)
            {
                Debug.Log("error when trying to broadcast " + ex.Message);
            }
           
        }
    }
    private void BroadCast(string data, ClientInfo clientNotToSend)
    {
        foreach (ClientInfo client in clients)
        {
            if (client != clientNotToSend)
            {

                try
                {
                    StreamWriter writer = new StreamWriter(client.tcpClient.GetStream());
                    writer.WriteLine(data);
                    writer.Flush();
                   

                }
                catch (Exception ex)
                {
                    console.ShowMessage("error when trying to broadcast " + ex.Message);
                }
            }
        }
    }
    // Send message to specific clients
    private void SendMessageToClients(ClientInfo[] tempClients, string tempData) {
        foreach (ClientInfo c in tempClients) {
            StreamWriter writer = new StreamWriter(c.tcpClient.GetStream());
            writer.WriteLine(tempData);
            writer.Flush();
            Debug.Log("sending data: " + tempData);
            
        }
    }
    // Send message to a client
    private void SendMessageToClient(ClientInfo tempClient, string tempData)
    {         
        StreamWriter writer = new StreamWriter(tempClient.tcpClient.GetStream());
        writer.WriteLine(tempData);
        Debug.Log("Sending message to client: " + tempClient.name + tempData);
        writer.Flush();
        
       
    }
    private void SendObjectToClient(ClientInfo client, object objToSend) {
        StreamWriter writer = new StreamWriter(client.tcpClient.GetStream());
        writer.WriteLine(objToSend);
        writer.Flush();
        
        

    }

    // Checks if client is still connected
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected) // check if socket is connected
            {
                if (c.Client.Poll(0, SelectMode.SelectRead)) // if client's socket is readable
                {
                    return (!(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0)); // if there are data to read, return true;

                }
                return true;

            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Disconnection error" + ex);
            return false;
        }
    }
    // checks if a client should be disconnected. Reads messages from connected clients
    private void ReadFromConnectedClients(ClientInfo client)
    {
        // Read messages
        if (client.tcpClient.Connected)
        {
            NetworkStream s = client.tcpClient.GetStream();
            if (s.DataAvailable)
            {
                StreamReader reader = new StreamReader(s, true);
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(client, data);
                }
            }
        }
    }
    // check if a client is dc
    private void CheckForDisconnectedClients(ClientInfo client) {       
        if (!IsConnected(client.tcpClient))
        {
            if (client == clients[0])
            {
                try
                {
                    BroadCast("QG~", client);
                }
                catch
                {
                    Debug.Log("Server closed, could not send message to clients to quit");
                }
            }
            clientsToBeDisconnected.Add(client);
            client.tcpClient.Close();          
        }
    }
    // Removes disconnected clients from clients list
    private void RemoveDisconnectedClients() {
        /*
         * This functionality should be seperated from CheckForDisconnectedClients(), so foreach loop does not break
        */
        foreach (ClientInfo client in clientsToBeDisconnected)
        {
            clients.Remove(client);
        }
        clientsToBeDisconnected.Clear();
    }
    // Configures messages from clients
    private void OnIncomingData (ClientInfo client, string data)
    {
        string[] lines;       
        lines = data.Split('~');
        for (int i = 0; i != lines.Length; i ++) {
            switch (lines[i]) {
                case "CNAME": // the name of the client that send the message                                             
                    client.name = lines[i + 1];
                    CheckForGameStart();
                    BroadCast("MSG~" + client.name + " has joined");
                    break;
                case "MSG": // a message to broadcast
                    BroadCast("MSG~"+client.name+": " + lines[i + 1]);
                    break;
                case "MOV": // a message containing information about a player's move   
                    if (!gameEnded)
                    {
                        BroadCast("MOV~" + lines[i + 1] + "~" + lines[i + 2] + "~" + lines[i + 3] + "~" + lines[i + 4]);
                    }
                    break;
                case "QG": // message saying that somebody left the game

                    if (client == clients[0])
                    {
                        try
                        {
                            BroadCast("QG~", client); // change the player color of everyone to nothing                              
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("Could not reach client " + ex.Message);
                        }
                    }
                    else if (client == clients[1] && !gameEnded) // if the client was the black player
                    {
                        isGameRunning = false;
                        gameEnded = true;
                        BroadCast("PCLRN~ ~MSG~Player2 has left the game, player1 has won", client); // change the player color of everyone to nothing     
                        
                    }
                    else
                    {
                        BroadCast("MSG~" + client.name + " has left the game", client);
                    }

                    break;
                case "GE": // game ended, someone won
                    BroadCast("MSG~" + lines[i + 1] + " has won");
                    BroadCast("PCLRN~"); // set the color of everyone to nothing  
                    gameEnded = true;
                    isGameRunning = false;
                    break;
                case "RDYG": // a client is ready for game   
                    client.rdyForGame = true;
                    if (clients[0].rdyForGame && clients[1].rdyForGame && !isGameRunning && !gameEnded)
                    {
                        
                        SendMessageToClient(clients[0], "PT~White~MSG~Game started");
                        SendMessageToClient(clients[1], "PT~White~MSG~Game started");
                        SendMessageToClients(new ClientInfo[] { clients[0], clients[1] }, "MSG~Player 1: " + clients[0].name + "~MSG~Player 2: " + clients[1].name);
                        isGameRunning = true;
                    }
                    
                    else if ((isGameRunning||gameEnded) ) // if the client is a "spectator" 
                    {
                        SendMessageToClient(client, "GRABB~");   // send him the current state of the board 
                        SendObjectToClient(client, Serialize(ConfigPiecesForSerilization()));
                        SendMessageToClient(client, "~");
                        SendMessageToClient(client, "PT~" + UserInput.GetInstance().GetPlayerTurn()); //send current player's turn
                        if (gameEnded) {
                            SendMessageToClient(client, "MSG~Game has ended");
                        }
                    }                    
                    break;
            }
        }
        Debug.Log("Incoming message to server " + data);
    }     
   
    // Get if server is ready
    public bool HasServerStarted() {
        return serverStarted;
    }
    // Check if two clients have been connected and start a game
    private void CheckForGameStart()
    {
        Debug.Log("clients " + clients.Count);
        if (!isGameRunning && !gameEnded && clients.Count == 2)
        {
            SendMessageToClient(clients[0], "STRTG~White"); // send message to host that game started and he is the white player                             
            SendMessageToClient(clients[1], "STRTG~Black"); // send message to first client that joined, to start the game and that he is the black player                     
        }
        else if (isGameRunning||gameEnded)
        {
            SendMessageToClient(clients[clients.Count-1], "STRTG~");
        }                      
    }
    // Updates references to match "Game" scene's GameObjects
    public void LoadGameScene()
    {
        try
        {
            console = GameObject.Find("MyConsole").GetComponent<MyConsole>();
            board = BoardManager.GetInstance();       
        }
        catch (Exception ex)
        {
            Debug.Log("Could not load game scene " + ex);
        }
    }


    private List<PieceInfo> ConfigPiecesForSerilization() {
        List<PieceInfo> pieces = new List<PieceInfo>();
        Piece[,] map = board.GetPiecesPositions();
        for (int i = 1; i != map.GetLength(0); i++) {
            for (int j = 1; j != map.GetLength(1); j++) {
                if (map[i, j] != null) {
                    PieceInfo pInfo = new PieceInfo();
                    pInfo.SetPieceInfo(i, j, (map[i, j].tag == "WhitePiece") ? true : false, (map[i, j].IsAKing()) ? true : false);
                    //Debug.Log("Piece ready for serilization: x: " + pInfo.xPos + " y: " + pInfo.zPos + " " + pInfo.isWhite + " " + pInfo.isKing);
                    pieces.Add(pInfo);

                }
            }
        }
        return pieces;

    }
    private string Serialize(List<PieceInfo> pieces)
    {
        /*
        try
        {
            Debug.Log("STARTING SERILIZATION");
            List<PieceInfo> pieces = ConfigPiecesForSerilization();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(clientStream, pieces);
            Debug.Log("ENDED SERILIZATION");
        }
        
        */
        
        StreamWriter stWriter = null;
        XmlSerializer xmlSerializer;
        string buffer;
        try
        {
            xmlSerializer = new XmlSerializer(typeof(List<PieceInfo>));
            MemoryStream memStream = new MemoryStream();
            stWriter = new StreamWriter(memStream);
            XmlSerializerNamespaces xs = new XmlSerializerNamespaces();
            xs.Add("", "");
            xmlSerializer.Serialize(stWriter, pieces, xs);
            buffer = Encoding.ASCII.GetString(memStream.GetBuffer());
            Debug.Log("buffer " + buffer);
            return buffer;
        }
        
        
        catch (Exception Ex)
        {
            throw Ex;
        }       
       // Debug.Log("Returning serilized object");
        //console.ShowMessage("Returning serilized object");
       
    }
   
    private void OnDestroy()
    {
        if (inst == this)
        {
            inst = null;
        }
        if (tcpListener != null) {
            foreach (ClientInfo c in clients)
            {
                c.tcpClient.Close();
            }
            tcpListener.Stop();
            
        }
       
    }
    private void OnApplicationQuit()
    {
        BroadCast("QG~");
        if (tcpListener != null)
        {
            foreach (ClientInfo c in clients) {
                c.tcpClient.Close();
            }
            tcpListener.Stop();
        }
    }
}

/*
 * Holds info about connected clients 
*/ 
public class ClientInfo
{
    public string name;
    public string color;
    public bool rdyForGame = false;
    public TcpClient tcpClient;
    public NetworkStream stream;
    public StreamReader reader;
    public StreamWriter writer;

    public ClientInfo(TcpClient tempClient)
    {
        tcpClient = tempClient;
        stream = tcpClient.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);

    }
    ~ClientInfo() {
        reader.Close();
        writer.Flush();
        writer.Close();
        tcpClient.Close();
    }
}

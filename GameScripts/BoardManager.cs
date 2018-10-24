using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoardManager : MonoBehaviour {

    private static BoardManager inst;

    public GameObject piece;
    public GameObject boardCube;

    private GameObject[,] boardCubes = new GameObject[9, 9];
    
    private Piece[,] piecesPositions = new Piece[9, 9];

    private bool gameEnded = false;
    
    private void Awake()
    {
        inst = this;
        GenerateBoardTable();
    }

    private void Update()
    {
        if (!gameEnded)
        {
            CheckWinCondition();

        }
    }
    // move pieces on the board. This is called when Client receives "MOV" message from server
    public void MovePiece(Piece piece, GameObject boardCube)
    {
        piece.gameObject.transform.position = boardCube.transform.position + new Vector3(0, .6f, 0);        
        if ((int)piece.GetPositionOnBoard().y - (int)FindBoardCubePosition(boardCube).y == 2 || (int)piece.GetPositionOnBoard().y - (int)FindBoardCubePosition(boardCube).y == -2) { // for some reason Mathf.Abs did not return true here
            //Debug.Log("yes it is a killing move");
            if ((int)FindBoardCubePosition(boardCube).x > piece.GetPositionOnBoard().x)
            {
                //Debug.Log("the dead is right from the killer");
                if ((int)FindBoardCubePosition(boardCube).y > piece.GetPositionOnBoard().y)
                {
                    Piece attackedPiece = piecesPositions[(int)piece.GetPositionOnBoard().x + 1, (int)piece.GetPositionOnBoard().y + 1];
                    piecesPositions[(int)piece.GetPositionOnBoard().x - 1, (int)piece.GetPositionOnBoard().y - 1] = null;
                    //Debug.Log("the dead is lower from the killer");
                    Destroy(attackedPiece.gameObject);
                }
                else if ((int)FindBoardCubePosition(boardCube).y < piece.GetPositionOnBoard().y)
                {
                    Piece attackedPiece = piecesPositions[(int)piece.GetPositionOnBoard().x + 1, (int)piece.GetPositionOnBoard().y - 1];
                    piecesPositions[(int)piece.GetPositionOnBoard().x + 1, (int)piece.GetPositionOnBoard().y - 1] = null;
                    //Debug.Log("the dead is lower from the killer");
                    Destroy(attackedPiece.gameObject);
                }
                else
                {
                   // Debug.Log(" i dont know if the dead is higher or lower");
                }
            }
            else if ((int)FindBoardCubePosition(boardCube).x < piece.GetPositionOnBoard().x)
            {
                //Debug.Log("the dead is left from the killer");
                if ((int)FindBoardCubePosition(boardCube).y > piece.GetPositionOnBoard().y)
                {
                    Piece attackedPiece = piecesPositions[(int)piece.GetPositionOnBoard().x - 1, (int)piece.GetPositionOnBoard().y + 1];
                    piecesPositions[(int)piece.GetPositionOnBoard().x - 1, (int)piece.GetPositionOnBoard().y + 1] = null;
                    //Debug.Log("the dead is lower from the killer");
                    Destroy(attackedPiece.gameObject);
                }
                else if ((int)FindBoardCubePosition(boardCube).y < piece.GetPositionOnBoard().y)
                {
                    Piece attackedPiece = piecesPositions[(int)piece.GetPositionOnBoard().x - 1, (int)piece.GetPositionOnBoard().y - 1];
                    piecesPositions[(int)piece.GetPositionOnBoard().x - 1, (int)piece.GetPositionOnBoard().y - 1] = null;
                    //Debug.Log("the dead is lower from the killer");
                    Destroy(attackedPiece.gameObject);
                }
                else {
                    //Debug.Log(" i dont know if the dead is higher or lower");
                }
            }
            else {
                //Debug.Log(" i dont know if the dead is left or right");
            }
        }               
        piecesPositions[(int)piece.GetPositionOnBoard().x, (int)piece.GetPositionOnBoard().y] = null; // set piece's previous position to null
        piecesPositions[(int)FindBoardCubePosition(boardCube).x, (int)FindBoardCubePosition(boardCube).y] = piece; // set the new piece's position
        // check if piece shoud become a king
        if (piece.GetPositionOnBoard().y == 8 && piece.gameObject.tag == "WhitePiece")
        {
            piece.PromoteToKing();           
        }
        else if (piece.GetPositionOnBoard().y == 1 && piece.gameObject.tag == "BlackPiece") {
            piece.PromoteToKing();           
        }
        if (UserInput.GetInstance().GetPlayerTurn() == "White")
        {
            UserInput.GetInstance().SetTurnForPlayer("Black");
        }
        else if (UserInput.GetInstance().GetPlayerTurn() == "Black")
        {
            UserInput.GetInstance().SetTurnForPlayer("White");
        }
    }
  
    // sets pieces position in sync with server
    public void SetPiecesPosition(List<PieceInfo> tempPieces)
    {
        //Debug.Log("tempPieces count " + tempPieces.Count);
        int i = 0;
        Piece[,] newPieces = new Piece[9, 9]; 
        /*
         * Destroy all pieces 
        */ 
        for(int x = 0; x < piecesPositions.GetLength(0); x++) {
            for (int z = 0; z < piecesPositions.GetLength(1); z++)
            {
                if (piecesPositions[x,z] != null)
                {
                    
                    Destroy(piecesPositions[x, z].gameObject);
                }
                piecesPositions[x, z] = null;
            }
        }
        // spawn pieces accordingly to the map, that server has sent
        foreach (PieceInfo p in tempPieces) {                                 
            if ( p != null && i < tempPieces.Count )
            {                                             
                GameObject newPiece = Instantiate(piece, boardCubes[tempPieces[i].xPos,tempPieces[i].zPos].transform.position + new Vector3 (0,0.6f,0), Quaternion.Euler(new Vector3(0, 0, 0)));
                
                newPieces[tempPieces[i].xPos, tempPieces[i].zPos] = newPiece.AddComponent<Piece>();
                if (tempPieces[i].isWhite)
                {
                    newPiece.GetComponent<MeshRenderer>().material.color = Color.white;
                    newPiece.tag = "WhitePiece";
                }
                else {
                    newPiece.GetComponent<MeshRenderer>().material.color = Color.black;
                    newPiece.tag = "BlackPiece";
                }
                if (tempPieces[i].isKing) {
                    newPiece.GetComponent<Piece>().PromoteToKing();
                }             
                //Debug.Log("Setting piece: x " + tempPieces[i].xPos + " y " + tempPieces[i].zPos);                              
                i++;                               
            }                  
        }
        piecesPositions = newPieces;

    }
    public static BoardManager GetInstance() {
        return inst;
    }
    // Get the array that holds all pieces positions
    public Piece[,] GetPiecesPositions() {
        return piecesPositions;
    }
    // Get the array of all boardCubes positions
    public GameObject[,] GetBoardCubesPositions() {
        return boardCubes;
    }
    // Get a vector2 containg the position of the given boardCube;
    public Vector2 FindBoardCubePosition(GameObject boardCube) {
        Vector2 position = new Vector2(-1, -1);
        for (int z = 1; z < boardCubes.GetLength(1) ; z++) {
            for (int x = 1; x < boardCubes.GetLength(0) ; x++) {
                if (boardCubes[x, z] == boardCube) {
                    position = new Vector2(x, z);
                }

            }
        }
        return position;
    }
    // Get a boardCube by giving its position
    public GameObject FindBoardCubekByPosition(int tempX, int tempZ) {
        return boardCubes[tempX, tempZ];                              
    }
    // Get a piece by giving it position 
    public Piece FindPieceByPosition(int x, int z) {
        return piecesPositions[x, z];
    }
    // checks if a player has won
    private void CheckWinCondition()
    {
        if (Server.GetInstance() != null)
        {
           
            if (GameObject.FindGameObjectWithTag("WhitePiece") == null)
            {
                Debug.Log("Sendimg message: black player won");
                Client.GetInstance().SendMessageToServer("GE~Black player");
                gameEnded = true;
            }
            if (GameObject.FindGameObjectWithTag("BlackPiece") == null)
            {
                Debug.Log("Sendimg message: white player won");
                Client.GetInstance().SendMessageToServer("GE~White player");
                gameEnded = true;
            }
        }
    }
    // generate the board
    private void GenerateBoardTable()
    {

        GameObject boardCubesPapa = new GameObject("BoardCubesPapa");
        boardCubesPapa.transform.position = Vector3.zero;
        for (int zAxis = 1; zAxis != 9; zAxis++)
        {
            for (int xAxis = 1; xAxis != 9; xAxis++)
            {
                GameObject thisBlock = Instantiate(boardCube, new Vector3(xAxis, 0, zAxis), new Quaternion(0, 0, 0, 0));
                thisBlock.layer = 8;
                boardCubes[xAxis, zAxis] = thisBlock;
                thisBlock.transform.SetParent(boardCubesPapa.transform);
                //Debug.Log("Cube created at: x " + xAxis + " z " + zAxis);
                if (zAxis % 2 != 0)
                {
                    if (xAxis % 2 == 0)
                    {
                        thisBlock.GetComponent<MeshRenderer>().material.color = Color.black;
                    }
                    else
                    {
                        thisBlock.GetComponent<MeshRenderer>().material.color = Color.white;
                    }
                }
                else if (zAxis % 2 == 0)
                {
                    if (xAxis % 2 != 0)
                    {
                        thisBlock.GetComponent<MeshRenderer>().material.color = Color.black;
                    }
                    else
                    {
                        thisBlock.GetComponent<MeshRenderer>().material.color = Color.white;
                    }
                }

            }
        }
        GeneratePieces();

    }
    // generate pieces
    private void GeneratePieces()
    {
        GameObject whitePiecesPapa = new GameObject("WhitePiecesPapa");
        whitePiecesPapa.transform.position = Vector3.zero;
        GameObject blackPiecesPapa = new GameObject("BlackPiecesPapa");
        blackPiecesPapa.transform.position = Vector3.zero;
        float yOffset = 0.6f;
        int nameIndex = 1;

        /*
         * Spawn white pieces 
        */ 
        for (int z = 1; z < 3; z++)
        {
            
            for (int x = 1 ; x != 9; x ++)
            {
                if (z == 1)
                {
                    GameObject newPiece = Instantiate(piece, new Vector3(x, yOffset, z), Quaternion.Euler(new Vector3(0, 0, 0)));
                    newPiece.GetComponent<MeshRenderer>().material.color = Color.white;
                    newPiece.tag = "WhitePiece";
                    newPiece.name = ("WhitePiece " + nameIndex.ToString());
                    nameIndex++;
                    newPiece.layer = 9;
                    piecesPositions[x, z] = newPiece.AddComponent<Piece>();
                    newPiece.transform.SetParent(whitePiecesPapa.transform);
                    //Debug.Log("White piece created at: x " + x + " z " + z);
                    x = x + 1;
                }
                else if (z == 2) {
                    GameObject newPiece = Instantiate(piece, new Vector3(x+1, yOffset, z), Quaternion.Euler(new Vector3(0, 0, 0)));
                    newPiece.GetComponent<MeshRenderer>().material.color = Color.white;
                    newPiece.tag = "WhitePiece";
                    newPiece.name = ("WhitePiece " + nameIndex.ToString());
                    nameIndex++;
                    newPiece.layer = 9;
                    piecesPositions[x+1, z] = newPiece.AddComponent<Piece>();
                    newPiece.transform.SetParent(whitePiecesPapa.transform);
                    //Debug.Log("White piece created at: x " + (x+1)+ " z " + z);
                    x = x + 1;
                }
            }    
        }
        /*
         * Spawn black pieces 
        */
        nameIndex = 1;
        for (int z = 1; z < 3; z++)
        {
                                  
            for (int x = 1; x != 9; x ++)
            {
                if (z == 1)
                {
                    GameObject newPiece = Instantiate(piece, new Vector3(x, yOffset, z + 6), Quaternion.Euler(new Vector3(0, 0, 0)));
                    newPiece.GetComponent<MeshRenderer>().material.color = Color.black;
                    newPiece.tag = "BlackPiece";
                    newPiece.name = ("BlackPiece " + nameIndex.ToString());
                    nameIndex++;
                    newPiece.layer = 10;                   
                    piecesPositions[x, z+6] = newPiece.AddComponent<Piece>();
                    newPiece.transform.SetParent(blackPiecesPapa.transform);
                    //Debug.Log("Black piece created at: x " + x + " z " + (z + 6));
                    x = x + 1;
                }
                else if (z == 2) {
                    GameObject newPiece = Instantiate(piece, new Vector3(x+1, yOffset, z + 6), Quaternion.Euler(new Vector3(0, 0, 0)));
                    newPiece.GetComponent<MeshRenderer>().material.color = Color.black;
                    newPiece.name = ("BlackPiece " + nameIndex.ToString());
                    nameIndex++;
                    newPiece.tag = "BlackPiece";
                    newPiece.layer = 10;
                    piecesPositions[x+1, z+6] = newPiece.AddComponent<Piece>();
                    newPiece.transform.SetParent(blackPiecesPapa.transform);
                    //Debug.Log("Black piece created at: x " + (x+1) + " z " + (z + 6));
                    x = x + 1;

                }
            }
           
            
        }
    }
   
}

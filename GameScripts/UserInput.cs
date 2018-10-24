using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour {

    private string playerTurn = "";
    private string playerColor;

    private Piece selectedPiece;
    private GameObject selectedBlock; 
    private BoardManager generateBoard;
    private static UserInput inst;

    public static UserInput GetInstance() { 
       
        return inst;
    }

    void Awake() {
        inst = this;
    }
    // Use this for initialization
    void Start()
    {      
        generateBoard = BoardManager.GetInstance();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {           
            if (playerTurn == "White" && playerColor == "White")
            {
                SelectWhitePieceToMove();
            }
            else if (playerTurn == "Black" && playerColor == "Black")
            {
                SelectBlackPieceToMove();
            }           
        }
        
    }


    // White player's input
    private void SelectWhitePieceToMove() {
        Ray ray1 = Camera.main.ScreenPointToRay(Input.mousePosition); // a ray from camera to mouse's click position
        RaycastHit hitInfo;       
        // raycast to check if user clicked on white piece
        if (Physics.Raycast(ray1, out hitInfo, 200f, LayerMask.GetMask("WhitePiece"), QueryTriggerInteraction.Collide))
        {
            selectedPiece = hitInfo.collider.gameObject.GetComponent<Piece>(); // make that piece the selected one
            //Debug.Log("selected piece: " + selectedPiece.name + " x " + selectedPiece.GetPositionOnBoard().x + " y " + selectedPiece.GetPositionOnBoard().y);           
        }
        else if (Physics.Raycast(ray1, out hitInfo, 200f, LayerMask.GetMask("BoardCube"), QueryTriggerInteraction.Ignore) && selectedPiece != null)
        {
            selectedBlock = hitInfo.transform.gameObject;
            //Debug.Log("boardCube selected: x " + generateBoard.FindBoardCubePosition(selectedBlock).x + " y " + generateBoard.FindBoardCubePosition(selectedBlock).y);
            // if there is no piece that is moving and user has selected a piece               
            // if the block he clicked is an available block to move for selectedPiece
            if (selectedPiece.GetIfMoveAvailable(generateBoard.FindBoardCubePosition(selectedBlock)))
            {
                //Debug.Log(selectedPiece + " can move to " + generateBoard.FindBoardCubePosition(selectedBlock));               
                Client.GetInstance().SendMessageToServer("MOV~" + (int)selectedPiece.GetPositionOnBoard().x + "~" + (int)selectedPiece.GetPositionOnBoard().y + "~" + (int)(generateBoard.FindBoardCubePosition(selectedBlock).x) + "~" + (int)(generateBoard.FindBoardCubePosition(selectedBlock).y));                
                selectedPiece = null;
                selectedBlock = null;               
            }
            else
            {
                selectedBlock = null;
            }
        }
        else
        {
            hitInfo = new RaycastHit();
            selectedPiece = null;
            selectedBlock = null;
           
        }           
    }
    // Black player's input
    private void SelectBlackPieceToMove() {
        Ray ray1 = Camera.main.ScreenPointToRay(Input.mousePosition); // a ray from camera to mouse's click position
        RaycastHit hitInfo;
        // raycast to check if user clicked on white piece
        if (Physics.Raycast(ray1, out hitInfo, 200f, LayerMask.GetMask("BlackPiece"), QueryTriggerInteraction.Collide) )
        {
            selectedPiece = hitInfo.collider.gameObject.GetComponent<Piece>(); // make that piece the selected one
            //Debug.Log("selected piece: " + selectedPiece.name + " x " + selectedPiece.GetPositionOnBoard().x + " y " + selectedPiece.GetPositionOnBoard().y);
        }
        else if (Physics.Raycast(ray1, out hitInfo, 200f, LayerMask.GetMask("BoardCube"), QueryTriggerInteraction.Ignore) && selectedPiece != null)
        {
            selectedBlock = hitInfo.transform.gameObject;
            //Debug.Log("boardCube selected: x " + generateBoard.FindBoardCubePosition(selectedBlock).x + " y " + generateBoard.FindBoardCubePosition(selectedBlock).y);
            // if there is no piece that is moving and user has selected a piece               
            // if the block he clicked is an available block to move for selectedPiece
            if (selectedPiece.GetIfMoveAvailable(generateBoard.FindBoardCubePosition(selectedBlock)))
            {
                //Debug.Log(selectedPiece + " can move to " + selectedBlock);                          
                Client.GetInstance().SendMessageToServer("MOV~" + (int)selectedPiece.GetPositionOnBoard().x + "~" + (int)selectedPiece.GetPositionOnBoard().y + "~" + (int)(generateBoard.FindBoardCubePosition(selectedBlock).x) + "~" + (int)(generateBoard.FindBoardCubePosition(selectedBlock).y));                
                selectedPiece = null;
                selectedBlock = null;
            }
            else
            {
                selectedBlock = null;
            }
        }
        else
        {
            hitInfo = new RaycastHit();
            selectedPiece = null;
            selectedBlock = null;
     
        }        
    }
    public string GetPlayerTurn() {
        return playerTurn;
    }
    public void SetPlayerColor(string color) {
        playerColor = color;      
       
        //Debug.Log("Color set to " + color);
    }
    public string GetPlayerColor() {
        return playerColor;
    }
    public void SetTurnForPlayer(string b) {
        playerTurn = b;
        GameUI.GetInst().SetPlayerTurnText(b);
        //Debug.Log("turn for white player " + b);
    } 
}

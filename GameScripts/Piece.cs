using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private bool isKing = false;
    private int xPosition;
    private int zPosition;
    Piece[,] piecesPositions = new Piece[9,9];
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    // returns the total of available moves for this piece
    public bool[,] GetAvailableMoves()
    {
        bool[,] availablePositions = new bool[9, 9];
        FindPositionOnBoard();
        int zOffset = 1;
        if (this.gameObject.tag == "BlackPiece") {
            zOffset = zOffset * -1;
        }       
        if (CheckForEnemies()[0] == false && xPosition + 1 >= 1 && xPosition + 1 <= 8 && zPosition + zOffset >= 1 && zPosition + zOffset <= 8)
        {
            if (piecesPositions[xPosition + 1, zPosition + zOffset] == null)
            {
                availablePositions[xPosition + 1, zPosition + zOffset] = true;
            }
        }
        else if (xPosition + 2 >= 1 && xPosition + 2 <= 8 && zPosition + zOffset * 2 >= 1 && zPosition + zOffset * 2 <= 8)
        {
            availablePositions[xPosition + 2, zPosition + zOffset*2] = true;
        }
        if (CheckForEnemies()[1] == false && xPosition - 1 >= 1 && xPosition - 1 <= 8 && zPosition + zOffset >= 1 && zPosition + zOffset <= 8)
        {
            if (piecesPositions[xPosition - 1, zPosition + zOffset] == null)
            {
                availablePositions[xPosition - 1, zPosition + zOffset] = true;
            }
           
        }
        else if ((xPosition - 2 >= 1 && xPosition - 2 <= 8 && zPosition + zOffset * 2 >= 1 && zPosition + zOffset * 2 <= 8))
        {
            availablePositions[xPosition - 2, zPosition + zOffset * 2] = true;
        }
        if (isKing) {
            zOffset = zOffset * -1;
            if (CheckForEnemies()[2] == false && xPosition + 1 >= 1 && xPosition + 1 <= 8 && zPosition + zOffset >= 1 && zPosition + zOffset <= 8)
            {
                if (piecesPositions[xPosition + 1, zPosition + zOffset] == null)
                {
                    availablePositions[xPosition + 1, zPosition + zOffset] = true;
                }
            }
            else if (xPosition + 2 >= 1 && xPosition + 2 <= 8 && zPosition + zOffset * 2 >= 1 && zPosition + zOffset * 2 <= 8)
            {
                availablePositions[xPosition + 2, zPosition + zOffset * 2] = true;
            }
            if (CheckForEnemies()[3] == false && xPosition - 1 >= 1 && xPosition - 1 <= 8 && zPosition + zOffset >= 1 && zPosition + zOffset <= 8)
            {
                if (piecesPositions[xPosition - 1, zPosition + zOffset] == null)
                {
                    availablePositions[xPosition - 1, zPosition + zOffset] = true;
                }

            }
            else if ((xPosition - 2 >= 1 && xPosition - 2 <= 8 && zPosition + zOffset * 2 >= 1 && zPosition + zOffset * 2 <= 8))
            {
                availablePositions[xPosition - 2, zPosition + zOffset * 2] = true;
            }
        }

        return availablePositions;
    }
    // promotes this piece to king
    public void PromoteToKing() {
        isKing = true;
        if (this.tag == "WhitePiece")
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
        }
    }
    // checks if a specific move is available
    public bool GetIfMoveAvailable(Vector2 position)
    {  
        if (GetAvailableMoves()[(int)position.x, (int)position.y] == true)
        {
            return true;
        }
        else {
            return false;
        }             
    }
    // returns the position of this piece 
    public Vector2 GetPositionOnBoard() {
        Vector2 position;
        FindPositionOnBoard();
        position = new Vector2(xPosition, zPosition);
        return position;

    }
    // returns if this piece is a king
    public bool IsAKing() {
        return isKing;
    } 
    // find this piece's position on board
    private void FindPositionOnBoard()
    {
        piecesPositions = BoardManager.GetInstance().GetPiecesPositions();
        for (int z = 1; z < piecesPositions.GetLength(1); z++)
        {
            for (int x = 1; x < piecesPositions.GetLength(0); x++)
            {
                if (piecesPositions[x, z] == this) {
                    xPosition = x;
                    zPosition = z;
                }
            }

        }
    }
    // checks if enemies exist at the positions this piece can attack
    private bool[] CheckForEnemies() {
        bool[] positionsOfEnemies = new bool[4];
        int offset = 1;
        if (this.gameObject.tag == "BlackPiece") {
            offset = offset * -1;
        }
        if (xPosition +1 >= 1 && xPosition +1 <= 8 && zPosition + offset >= 1 && zPosition + offset <= 8)
        {
            if (piecesPositions[xPosition + 1, zPosition + offset] != null)
            {
                if (piecesPositions[xPosition + 1, zPosition + offset].gameObject.tag != this.gameObject.tag)
                {
                    positionsOfEnemies[0] = true;
                }
                else
                {
                    //Debug.Log("There is an ally on this block");
                    positionsOfEnemies[0] = false;
                }
            }
            else
            {
                //Debug.Log("there is no piece on block: x " + (xPosition + 1) + " z " + (zPosition + offset));
                positionsOfEnemies[0] = false;
            }
        }
        else
        {
            positionsOfEnemies[0] = false;
        }
        if (xPosition -1 >= 1 && xPosition - 1 <= 8 && zPosition + offset >= 1 && zPosition + offset <= 8)
        {
            if (piecesPositions[xPosition - 1, zPosition + offset] != null)
            {
                if (piecesPositions[xPosition - 1, zPosition + offset].gameObject.tag != this.gameObject.tag)
                {
                    positionsOfEnemies[1] = true;
                }
                else
                {
                    //Debug.Log("There is an ally on this block");
                    positionsOfEnemies[1] = false;
                }
            }
            else
            {
                //Debug.Log("there is no piece on block: x " + (xPosition - 1) + " z " + (zPosition + offset));
                positionsOfEnemies[1] = false;
            }
        }
        else
        {
            positionsOfEnemies[1] = false;
        }
        if (isKing) {
            offset = offset * -1;
            if (xPosition + 1 >= 1 && xPosition + 1 <= 8 && zPosition + offset >= 1 && zPosition + offset <= 8)
            {
                if (piecesPositions[xPosition + 1, zPosition + offset] != null)
                {
                    if (piecesPositions[xPosition + 1, zPosition + offset].gameObject.tag != this.gameObject.tag)
                    {
                        positionsOfEnemies[2] = true;
                    }
                    else
                    {
                        //Debug.Log("There is an ally on this block");
                        positionsOfEnemies[2] = false;
                    }
                }
                else
                {
                    //Debug.Log("there is no piece on block: x " +(xPosition+1) +" z " +(zPosition+offset) );
                    positionsOfEnemies[2] = false;
                }
            }
            else
            {
                positionsOfEnemies[2] = false;
            }
            if (xPosition - 1 >= 1 && xPosition - 1 <= 8 && zPosition + offset >= 1 && zPosition + offset <= 8)
            {
                if (piecesPositions[xPosition - 1, zPosition + offset] != null)
                {
                    if (piecesPositions[xPosition - 1, zPosition + offset].gameObject.tag != this.gameObject.tag)
                    {
                        positionsOfEnemies[3] = true;
                    }
                    else
                    {
                        //Debug.Log("There is an ally on this block");
                        positionsOfEnemies[3] = false;
                    }
                }
                else
                {
                    //Debug.Log("there is no piece on block: x " + (xPosition - 1) + " z " + (zPosition + offset));
                    positionsOfEnemies[3] = false;
                }
            }
            else
            {
                positionsOfEnemies[3] = false;
            }
        }
        return positionsOfEnemies;
        
    }
}

[Serializable]
public class PieceInfo
{
    public int xPos;
    public int zPos;
    public bool isWhite;
    public bool isKing;
    public void SetPieceInfo(int x, int z, bool c, bool k)
    {
        xPos = x;
        zPos = z;
        isWhite = c;
        isKing = k;
    }
}

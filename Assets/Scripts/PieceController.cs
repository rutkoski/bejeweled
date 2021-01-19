using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    public GameController game;
    
    [SerializeField] private int m_pieceType;
    public int PieceType
    {
        get => m_pieceType;
        set { m_pieceType = value; }
    }

    [SerializeField] private int m_row;
    public int Row
    {
        get { return m_row; }
        set { m_row = value; }
    }

    [SerializeField] private int m_col;
    public int Col
    {
        get { return m_col; }
        set { m_col = value; }
    }

    public bool removed;
}

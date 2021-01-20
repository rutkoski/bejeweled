using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PieceFactory))]
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public enum GameState
    {
        Init,
        Idle,
        Animation,
    }

    [SerializeField] private BoardData m_boardData;

    [SerializeField] private int m_rows;
    public int Rows => m_rows;

    [SerializeField] private int m_cols;
    public int Cols => m_cols;

    private int[] m_board;

    [SerializeField] private float m_pieceDistance;
    public float PieceDistance => m_pieceDistance;

    [SerializeField] private Transform m_container;

    private PieceFactory m_pieceFactory;

    private PieceController[] m_pieces;

    private GameState m_state;
    public GameState State => m_state;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;

        m_pieceFactory = GetComponent<PieceFactory>();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        ResetGame();
        StartGame();
    }

    private void ResetGame()
    {
        m_state = GameState.Init;

        if (m_pieces != null)
        {
            foreach (PieceController piece in m_pieces)
            {
                if (piece)
                {
                    Destroy(piece.gameObject);
                }
            }
        }

        if (m_boardData)
        {
            m_rows = m_boardData.Rows;
            m_cols = m_boardData.Cols;
            m_board = m_boardData.Board.ToArray();
        }
        else
        {
            m_board = new int[m_rows * m_cols];
            for (int i = 0; i < m_board.Length; i++)
            {
                m_board[i] = -1;
            }
        }

        m_pieces = new PieceController[m_rows * m_cols];
    }

    public void StartGame()
    {
        for (int i = 0; i < m_rows; i++)
        {
            for (int j = 0; j < m_cols; j++)
            {
                PieceController piece = SpawnPiece(i, j, m_board[GetIndex(i, j)]);
            }
        }

        m_state = GameState.Animation;
    }

    public int GetIndex(int row, int col)
    {
        return row * m_cols + col;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ResetGame();
            StartGame();
        }

        if (m_state == GameState.Init) return;

        if (m_state == GameState.Animation)
        {
            bool animate = false;

            foreach (PieceController piece in m_pieces)
            {
                if (!piece || piece.Removed) continue;

                Vector3 sourcePos = piece.transform.position;
                Vector3 targetPos = GetPiecePosition(piece.Row, piece.Col);

                float dist = (sourcePos - targetPos).magnitude;

                if (dist > 0.01f)
                {
                    animate = true;

                    //targetPos = Vector3.Lerp(sourcePos, targetPos, 2f * Time.deltaTime);
                    targetPos = new Vector3(sourcePos.x, Mathf.Max(targetPos.y, sourcePos.y - 3f * Time.deltaTime));
                }

                piece.transform.position = targetPos;
            }

            if (!animate) m_state = GameState.Idle;
        }
        else if (Merge())
        {
            for (int i = m_rows - 1; i >= 0; i--)
            {
                for (int j = 0; j < m_cols; j++)
                {
                    PieceController piece = GetPieceAt(i, j);

                    if (piece)
                    {
                        int row = FindEmptyRow(i, j);

                        if (row >= 0)
                        {
                            SetPieceAt(i, j, null);
                            SetPieceAt(row, j, piece);

                            m_state = GameState.Animation;
                        }
                    }

                    if (SpawnNewPieces(j))
                    {
                        m_state = GameState.Animation;
                    }
                }
            }
        }
    }

    private bool SpawnNewPieces(int j)
    {
        int row = FindEmptyRow(j);

        if (row < 0) return false;

        int i = -1;

        while (row >= 0)
        {
            PieceController piece = SpawnPiece(row, j, -1);
            piece.transform.position = GetPiecePosition(i, j);

            row--;
            i--;
        }

        return true;
    }

    public bool WillMerge(PieceController piece)
    {
        List<PieceController> pieces;

        return WillMerge(piece, out pieces);
    }

    public bool WillMerge(PieceController piece, out List<PieceController> pieces)
    {
        pieces = new List<PieceController>();

        if (!piece || piece.Removed) return false;

        bool merged = false;

        int minRow, maxRow, minCol, maxCol;

        int row = piece.Row;
        int col = piece.Col;

        minRow = maxRow = piece.Row;
        minCol = maxCol = piece.Col;

        while (minRow > 0 && GetPieceAt(minRow - 1, col) is PieceController other && other.PieceType == piece.PieceType)
        {
            minRow--;
        }

        while (maxRow < m_rows - 1 && GetPieceAt(maxRow + 1, col) is PieceController other && other.PieceType == piece.PieceType)
        {
            maxRow++;
        }

        while (minCol > 0 && GetPieceAt(row, minCol - 1) is PieceController other && other.PieceType == piece.PieceType)
        {
            minCol--;
        }

        while (maxCol < m_cols - 1 && GetPieceAt(row, maxCol + 1) is PieceController other && other.PieceType == piece.PieceType)
        {
            maxCol++;
        }

        if (maxRow - minRow + 1 >= 3)
        {
            for (int i = minRow; i <= maxRow; i++)
            {
                PieceController other = GetPieceAt(i, col);

                if (other) pieces.Add(other);
            }

            merged = true;
        }

        if (maxCol - minCol + 1 >= 3)
        {
            for (int j = minCol; j <= maxCol; j++)
            {
                PieceController other = GetPieceAt(row, j);

                if (other) pieces.Add(other);
            }

            merged = true;
        }

        return merged;
    }

    private bool Merge()
    {
        bool merged = false;

        foreach (PieceController piece in m_pieces)
        {
            List<PieceController> pieces;

            if (WillMerge(piece, out pieces))
            {
                foreach (PieceController other in pieces)
                {
                    int row = other.Row;
                    int col = other.Col;

                    other.Removed = true;
                    other.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                    Destroy(other.gameObject);

                    SetPieceAt(row, col, null);
                }

                merged = true;
            }
        }

        return merged;
    }

    public void SwapPieces(PieceController piece, PieceController other)
    {
        int row = piece.Row;
        int col = piece.Col;

        SetPieceAt(other.Row, other.Col, piece);
        SetPieceAt(row, col, other);
    }

    private PieceController SpawnPiece(int row, int col, int pieceType)
    {
        PieceController piece = m_pieceFactory.Create(pieceType);

        piece.transform.SetParent(m_container);

        piece.transform.position = GetPiecePosition(row, col);

        SetPieceAt(row, col, piece);

        return piece;
    }

    private PieceController SpawnPiece(int col, int pieceType)
    {
        int row = FindEmptyRow(col);

        if (row < 0)
        {
            throw new Exception("Invalid position");
        }

        return SpawnPiece(row, col, pieceType);
    }

    public void SetPieceAt(int row, int col, PieceController piece)
    {
        m_pieces[GetIndex(row, col)] = piece;

        if (piece)
        {
            piece.Row = row;
            piece.Col = col;
        }
    }

    private int FindEmptyRow(int col)
    {
        if (col < 0 || col >= m_cols)
            throw new Exception("Invalid position");

        int row = -1;

        while (row < m_rows - 1 && !GetPieceAt(row + 1, col))
        {
            row++;
        }

        return row;
    }

    private int FindEmptyRow(int row, int col)
    {
        if (row < 0 || row >= m_rows || col < 0 || col >= m_cols)
            throw new Exception("Invalid position");

        //row--;

        while (row < m_rows - 1 && !GetPieceAt(row + 1, col))
        {
            row++;
        }

        return row;
    }

    public PieceController GetPieceAt(int row, int col)
    {
        if (row < 0 || row >= m_rows || col < 0 || col >= m_cols)
            throw new Exception("Invalid position");

        return m_pieces[GetIndex(row, col)];
    }

    public Vector3 GetPiecePosition(int row, int col)
    {
        Vector3 position = m_container.transform.position;
        return new Vector3(position.x + col * m_pieceDistance, position.y - row * m_pieceDistance);
    }

    private void OnDrawGizmos()
    {
        Vector3 size = new Vector3(m_pieceDistance, m_pieceDistance, m_pieceDistance);

        for (int i = 0; i < m_rows; i++)
        {
            for (int j = 0; j < m_cols; j++)
            {
                Gizmos.DrawWireCube(GetPiecePosition(i, j), size);
            }
        }
    }
}

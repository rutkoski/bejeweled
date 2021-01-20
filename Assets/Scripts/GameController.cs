using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PieceFactory))]
[RequireComponent(typeof(AnimationController))]
public class GameController : MonoBehaviour
{
    public delegate void ScoreUpdatedEvent(object sender, EventArgs args);

    public static event ScoreUpdatedEvent OnScoreUpdated;

    public static GameController Instance { get; private set; }

    public enum GameState
    {
        Init,
        Idle,
        Dropping,
        Merging,
        Spawning,
        Swapping,
    }

    [SerializeField] private int m_mergeScore;

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
    public PieceController[] Pieces => m_pieces;

    private GameState m_state;
    public GameState State => m_state;

    private int m_score = 0;
    public int Score => m_score;

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
        RestartGame();
    }

    public void RestartGame()
    {
        ResetGame();
        StartGame();
    }

    private void ResetGame()
    {
        StopAllCoroutines();

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

        m_score = 0;

        OnScoreUpdated?.Invoke(this, new EventArgs());
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

        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(1f);

        m_state = GameState.Idle;
    }

    public int GetIndex(int row, int col)
    {
        return row * m_cols + col;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            RestartGame();
        }

        if (m_state == GameState.Init) return;

        if (m_state == GameState.Dropping)
        {
            bool animate = false;

            foreach (PieceController piece in m_pieces)
            {
                if (!piece || piece.Removed) continue;

                animate |= AnimationController.Instance.AnimatePieceDrop(piece);
            }

            if (!animate) m_state = GameState.Idle;
        }
        else if (m_state == GameState.Swapping)
        {
            if (!AnimationController.Instance.HasSwapJobs())
            {
                m_state = GameState.Idle;
            }
        }
        else if (m_state == GameState.Merging)
        {
            if (!AnimationController.Instance.AnimateMerge())
            {
                m_state = GameState.Spawning;
            }
        }
        else if (m_state == GameState.Spawning)
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

                            m_state = GameState.Dropping;
                        }
                    }

                    if (SpawnNewPieces(j))
                    {
                        m_state = GameState.Dropping;
                    }
                }
            }
        }
        else if (Merge())
        {
            m_state = GameState.Merging;
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

        int score = 0;

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
                    other.Merging = true;

                    score += m_mergeScore;
                }

                merged = true;
            }
        }

        if (score > 0)
        {
            m_score += score;

            OnScoreUpdated?.Invoke(this, new EventArgs());
        }

        return merged;
    }

    public void SwapPieces(PieceController piece, PieceController other)
    {
        int row = piece.Row;
        int col = piece.Col;

        SetPieceAt(other.Row, other.Col, piece);
        SetPieceAt(row, col, other);

        //piece.transform.position = GetPiecePosition(piece.Row, piece.Col);
        //other.transform.position = GetPiecePosition(other.Row, other.Col);

        m_state = GameState.Swapping;
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

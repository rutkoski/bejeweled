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
        End,
    }

    [Tooltip("Amount of points for each piece in a merge")]
    [SerializeField] private int m_mergeScore;

    [Tooltip("Represents the initial state of the match.")]
    [SerializeField] private BoardData m_boardData;

    [Tooltip("Board size (rows)")]
    [SerializeField] private int m_rows;
    public int Rows => m_rows;

    [Tooltip("Board size (columns)")]
    [SerializeField] private int m_cols;
    public int Cols => m_cols;

    private int[] m_board;

    [Tooltip("Distance between the pieces on the board")]
    [SerializeField] private float m_pieceDistance;
    public float PieceDistance => m_pieceDistance;

    [Tooltip("The container for the pieces")]
    [SerializeField] private Transform m_container;
    public Transform Container => m_container;

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
        //RestartGame();
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

        HasAvailablePlays();
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

        GetComponent<UIController>().ShowGame();
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(1f);

        m_state = GameState.Dropping;
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

        if (m_state == GameState.Init || m_state == GameState.End) return;

        switch (m_state)
        {
            /**
             * Drop animation
             */

            case GameState.Dropping:
                bool animate = false;

                foreach (PieceController piece in m_pieces)
                {
                    if (!piece || piece.Removed) continue;

                    animate |= AnimationController.Instance.AnimatePieceDrop(piece);
                }

                if (!animate)
                {
                    m_state = GameState.Idle;

                    CheckEndGame();
                }

                break;

            /**
             * Swap animation
             */

            case GameState.Swapping:
                if (!AnimationController.Instance.HasSwapJobs())
                {
                    m_state = GameState.Idle;
                }

                break;

            /**
             * Merge animation
             */

            case GameState.Merging:
                if (!AnimationController.Instance.AnimateMerge())
                {
                    m_state = GameState.Spawning;
                }

                break;

            /**
             * Spawn new pieces
             */

            case GameState.Spawning:
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

                break;

            /**
             * Check if pieces can be merged
             */

            default:
                if (Merge())
                {
                    SFX.Instance.PlayOneShot(SFXData.Type.Positive);

                    m_state = GameState.Merging;
                }

                break;
        }
    }

    /**
     * Game ends if there are no available plays
     */
    private void CheckEndGame()
    {
        if (!HasAvailablePlays())
        {
            m_state = GameState.End;

            GetComponent<UIController>().ShowEnd();
        }
    }

    /**
     * Spawn new pieces in specified column, if there is space available
     */
    private bool SpawnNewPieces(int col)
    {
        int row = FindEmptyRow(col);

        if (row < 0) return false;

        int i = -1;

        while (row >= 0)
        {
            PieceController piece = SpawnPiece(row, col, -1);
            piece.transform.position = GetPiecePosition(i, col);

            row--;
            i--;
        }

        return true;
    }

    /**
     * Check if there is at least one available play (pieces can be merged)
     */
    private bool HasAvailablePlays()
    {
        bool found = false;

        foreach (PieceController piece in m_pieces)
        {
            if (!piece || piece.Removed) continue;

            int row = piece.Row;
            int col = piece.Col;

            // 10    10
            // 01 or 10
            // 10    01

            if (row + 2 < m_rows
                && (MatchType(piece, GetPieceAt(row + 1, col)) || (col + 1 < m_cols && MatchType(piece, GetPieceAt(row + 1, col + 1))))
                && (MatchType(piece, GetPieceAt(row + 2, col)) || (col + 1 < m_cols && MatchType(piece, GetPieceAt(row + 2, col + 1))))
            )
            {
                found = true;
                break;
            }

            // 01    01
            // 10 or 01
            // 01    10

            if (row > 1
                && (MatchType(piece, GetPieceAt(row - 1, col)) || (col + 1 < m_cols && MatchType(piece, GetPieceAt(row - 1, col + 1))))
                && (MatchType(piece, GetPieceAt(row - 2, col)) || (col + 1 < m_cols && MatchType(piece, GetPieceAt(row - 2, col + 1))))
            )
            {
                found = true;
                break;
            }

            // 1    1
            // 1 or 0
            // 0    1
            // 1    1

            if (row + 3 < m_rows)
            {
                int c = 0;
                c += MatchType(piece, GetPieceAt(row + 1, col)) ? 1 : 0;
                c += MatchType(piece, GetPieceAt(row + 2, col)) ? 1 : 0;
                c += MatchType(piece, GetPieceAt(row + 3, col)) ? 1 : 0;
                if (c >= 2)
                {
                    found = true;
                    break;
                }
            }

            // 101 or 110
            // 010    001

            if (col + 2 < m_cols
                && (MatchType(piece, GetPieceAt(row, col + 1)) || (row + 1 < m_rows && MatchType(piece, GetPieceAt(row + 1, col + 1))))
                && (MatchType(piece, GetPieceAt(row, col + 2)) || (row + 1 < m_rows && MatchType(piece, GetPieceAt(row + 1, col + 2))))
            )
            {
                found = true;
                break;
            }

            // 010 or 001
            // 101    110

            if (col > 1
                && (MatchType(piece, GetPieceAt(row, col - 1)) || (row + 1 < m_rows && MatchType(piece, GetPieceAt(row + 1, col - 1))))
                && (MatchType(piece, GetPieceAt(row, col - 2)) || (row + 1 < m_rows && MatchType(piece, GetPieceAt(row + 1, col - 2))))
            )
            {
                found = true;
                break;
            }

            // 1011 or 1101

            if (col + 3 < m_cols)
            {
                int c = 0;
                c += MatchType(piece, GetPieceAt(row, col + 1)) ? 1 : 0;
                c += MatchType(piece, GetPieceAt(row, col + 2)) ? 1 : 0;
                c += MatchType(piece, GetPieceAt(row, col + 3)) ? 1 : 0;
                if (c >= 2)
                {
                    found = true;
                    break;
                }
            }
        }

        return found;
    }

    /**
     * Check if two pieces are of the same type
     */
    public bool MatchType(PieceController piece, PieceController other)
    {
        if (!other || other.Removed || other.PieceType != piece.PieceType) return false;

        return true;
    }

    /**
     * Check if piece will be merged
     */
    public bool WillMerge(PieceController piece)
    {
        List<PieceController> pieces;

        return WillMerge(piece, out pieces);
    }

    /**
     * Check if piece will be merged and fill list with merged pieces
     */
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

    /**
     * Apply all available merges
     */
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

    /**
     * Swap two pieces
     */
    public void SwapPieces(PieceController piece, PieceController other)
    {
        int row = piece.Row;
        int col = piece.Col;

        SetPieceAt(other.Row, other.Col, piece);
        SetPieceAt(row, col, other);

        m_state = GameState.Swapping;
    }

    /**
     * Spawn piece of specified type at row and column
     */
    private PieceController SpawnPiece(int row, int col, int pieceType)
    {
        PieceController piece = m_pieceFactory.Create(pieceType);

        piece.transform.SetParent(m_container);

        piece.transform.position = GetPiecePosition(row, col);

        SetPieceAt(row, col, piece);

        return piece;
    }

    /**
     * Spawn piece of specified type at column
     */
    private PieceController SpawnPiece(int col, int pieceType)
    {
        int row = FindEmptyRow(col);

        if (row < 0)
        {
            throw new Exception("Invalid position");
        }

        return SpawnPiece(row, col, pieceType);
    }

    /**
     * Set piece position
     */
    public void SetPieceAt(int row, int col, PieceController piece)
    {
        m_pieces[GetIndex(row, col)] = piece;

        if (piece)
        {
            piece.Row = row;
            piece.Col = col;
        }
    }

    /**
     * Find empty row at specified column
     */
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

    /**
     * Find empty row at specified column, begining at row
     */
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

    /**
     * Get piece at position
     */
    public PieceController GetPieceAt(int row, int col)
    {
        if (row < 0 || row >= m_rows || col < 0 || col >= m_cols)
            throw new Exception("Invalid position");

        return m_pieces[GetIndex(row, col)];
    }

    /**
     * Get piece absolute (world) position
     */
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

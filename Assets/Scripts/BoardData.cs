using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu]
public class BoardData : ScriptableObject
{
    [Multiline]
    public string board;

    [SerializeField] private List<int> m_board;
    public List<int> Board => m_board;

    [SerializeField] private int m_rows;
    public int Rows => m_rows;

    [SerializeField] private int m_cols;
    public int Cols => m_cols;

    private void ParseBoard()
    {
        m_board = new List<int>();
        m_rows = 0;
        m_cols = 0;

        int row = 0;
        int cols = -1;

        string[] lines = board.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (string line in lines)
        {
            int col = 0;

            foreach (string pieceType in line.Split(','))
            {
                m_board.Add(int.Parse(pieceType));

                col++;
            }

            if (cols < 0)
            {
                cols = col;
            }
            else if (cols != col)
            {
                throw new Exception("Board has an uneven number of columns");
            }

            row++;
        }

        m_rows = row;
        m_cols = cols;
    }

    private void OnValidate()
    {
        ParseBoard();
    }
}

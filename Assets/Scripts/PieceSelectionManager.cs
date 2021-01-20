using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PieceSelectionManager
{
    private static PieceController m_piece;
    public static PieceController Piece
    {
        get => m_piece;
        set
        {
            if (m_piece == value) return;

            if (m_piece)
            {
                m_piece.Selected = false;
            }

            m_piece = value;

            if (m_piece)
            {
                m_piece.Selected = true;
            }
        }
    }

    private static PieceController m_other;
    public static PieceController Other
    {
        get => m_other;
        set
        {
            if (m_other == value) return;

            if (m_other)
            {
                m_other.Selected = false;
            }

            m_other = value;

            if (m_other)
            {
                m_other.Selected = true;
            }
        }
    }

    public static void Clear()
    {
        Piece = null;
        Other = null;
    }
}

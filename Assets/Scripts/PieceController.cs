using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceController : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    private bool m_dragging;
    private Vector2 m_initPos;
    private bool m_horizontal;
    private int m_min;
    private int m_max;
    private PieceController other;

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        m_initPos = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_horizontal = Mathf.Abs(m_initPos.x - eventData.position.x) > Mathf.Abs(m_initPos.y - eventData.position.y);

        if (m_horizontal)
        {
            m_min = Math.Max(m_col - 1, 0);
            m_max = Math.Min(m_col + 1, game.Cols - 1);
        }
        else
        {
            m_min = Math.Max(m_row - 1, 0);
            m_max = Math.Min(m_row + 1, game.Rows - 1);
        }

        m_dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_dragging) return;

        if (other)
        {
            other.transform.position = game.GetPiecePosition(other.Row, other.Col);
        }

        int row = Row;
        int col = Col;

        if (m_horizontal)
        {
            col = -1;

            if (eventData.position.x > m_initPos.x && m_max != Col)
            {
                col = m_max;
            }
            else if (eventData.position.x < m_initPos.x && m_min != Col)
            {
                col = m_min;
            }
        }
        else
        {
            row = -1;

            if (eventData.position.y < m_initPos.y && m_max != Row)
            {
                row = m_max;
            }
            else if (eventData.position.y > m_initPos.y && m_min != Row)
            {
                row = m_min;
            }
        }

        if (row != -1 && col != -1)
        {
            other = game.GetPieceAt(row, col);
            other.transform.position = game.GetPiecePosition(Row, Col);

            transform.position = game.GetPiecePosition(row, col);

            game.SwapPieces(this, other);

            if (!game.WillMerge(this) && !game.WillMerge(other))
            {
                game.SwapPieces(this, other);

                transform.position = game.GetPiecePosition(this.Row, this.Col);
                other.transform.position = game.GetPiecePosition(other.Row, other.Col);
            }
        }
        else
        {
            transform.position = game.GetPiecePosition(Row, Col);
        }

        StopDrag();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //StopDrag();
    }

    private void StopDrag()
    {
        m_dragging = false;

        if (other)
        {
            //other.transform.position = game.GetPiecePosition(other.Row, other.Col);
            //transform.position = game.GetPiecePosition(Row, Col);

            other = null;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceDragController : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PieceController piece;

    private GameController Game => GameController.Instance;

    private void Awake()
    {
        piece = GetComponent<PieceController>();
    }

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
            m_min = Math.Max(piece.Col - 1, 0);
            m_max = Math.Min(piece.Col + 1, Game.Cols - 1);
        }
        else
        {
            m_min = Math.Max(piece.Row - 1, 0);
            m_max = Math.Min(piece.Row + 1, Game.Rows - 1);
        }

        m_dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_dragging) return;

        if (other)
        {
            other.transform.position = Game.GetPiecePosition(other.Row, other.Col);
        }

        int row = piece.Row;
        int col = piece.Col;

        if (m_horizontal)
        {
            col = -1;

            if (eventData.position.x > m_initPos.x && m_max != piece.Col)
            {
                col = m_max;
            }
            else if (eventData.position.x < m_initPos.x && m_min != piece.Col)
            {
                col = m_min;
            }
        }
        else
        {
            row = -1;

            if (eventData.position.y < m_initPos.y && m_max != piece.Row)
            {
                row = m_max;
            }
            else if (eventData.position.y > m_initPos.y && m_min != piece.Row)
            {
                row = m_min;
            }
        }

        if (row != -1 && col != -1)
        {
            other = Game.GetPieceAt(row, col);
            other.transform.position = Game.GetPiecePosition(piece.Row, piece.Col);

            transform.position = Game.GetPiecePosition(row, col);

            Game.SwapPieces(piece, other);

            if (!Game.WillMerge(piece) && !Game.WillMerge(other))
            {
                Game.SwapPieces(piece, other);

                transform.position = Game.GetPiecePosition(piece.Row, piece.Col);
                other.transform.position = Game.GetPiecePosition(other.Row, other.Col);
            }
        }
        else
        {
            transform.position = Game.GetPiecePosition(piece.Row, piece.Col);
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

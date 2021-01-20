using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceDragController : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler
{
    private PieceController Piece;

    private GameController Game => GameController.Instance;

    private bool m_dragging;
    private Vector2 m_initPos;
    private bool m_horizontal;
    private int m_min;
    private int m_max;

    private void Awake()
    {
        Piece = GetComponent<PieceController>();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (Game.State != GameController.GameState.Idle) return;
        if (Piece.Removed) return;

        m_initPos = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Game.State != GameController.GameState.Idle) return;
        if (Piece.Removed) return;

        m_horizontal = Mathf.Abs(m_initPos.x - eventData.position.x) > Mathf.Abs(m_initPos.y - eventData.position.y);

        if (m_horizontal)
        {
            m_min = Math.Max(Piece.Col - 1, 0);
            m_max = Math.Min(Piece.Col + 1, Game.Cols - 1);
        }
        else
        {
            m_min = Math.Max(Piece.Row - 1, 0);
            m_max = Math.Min(Piece.Row + 1, Game.Rows - 1);
        }

        m_dragging = true;

        PieceSelectionManager.Clear();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Game.State != GameController.GameState.Idle) return;
        if (Piece.Removed) return;

        if (!m_dragging) return;

        int row = Piece.Row;
        int col = Piece.Col;

        if (m_horizontal)
        {
            col = -1;

            if (eventData.position.x > m_initPos.x && m_max != Piece.Col)
            {
                col = m_max;
            }
            else if (eventData.position.x < m_initPos.x && m_min != Piece.Col)
            {
                col = m_min;
            }
        }
        else
        {
            row = -1;

            if (eventData.position.y < m_initPos.y && m_max != Piece.Row)
            {
                row = m_max;
            }
            else if (eventData.position.y > m_initPos.y && m_min != Piece.Row)
            {
                row = m_min;
            }
        }

        if (row != -1 && col != -1)
        {
            PieceController other = Game.GetPieceAt(row, col);

            Game.SwapPieces(Piece, other);

            if (!Game.WillMerge(Piece) && !Game.WillMerge(other))
            {
                Game.SwapPieces(Piece, other);

                AnimationController.Instance.AnimateSwapRevert(Piece, other);
            }
            else
            {
                AnimationController.Instance.AnimateSwap(Piece, other);
            }
        }

        StopDrag();
    }

    private void StopDrag()
    {
        m_dragging = false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceClickController : MonoBehaviour, IPointerClickHandler
{
    private PieceController Piece;

    private GameController Game => GameController.Instance;

    private void Awake()
    {
        Piece = GetComponent<PieceController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Game.State != GameController.GameState.Idle) return;
        if (Piece.Removed) return;

        if (PieceSelectionManager.Piece == Piece)
        {
            PieceSelectionManager.Clear();
        }
        else if (PieceSelectionManager.Piece)
        {
            PieceSelectionManager.Other = Piece;
        }
        else
        {
            PieceSelectionManager.Piece = Piece;
        }

        if (PieceSelectionManager.Piece && PieceSelectionManager.Other)
        {
            if (IsAdjacent(PieceSelectionManager.Piece, PieceSelectionManager.Other))
            {
                Game.SwapPieces(PieceSelectionManager.Piece, PieceSelectionManager.Other);

                if (!Game.WillMerge(PieceSelectionManager.Piece) && !Game.WillMerge(PieceSelectionManager.Other))
                {
                    Game.SwapPieces(PieceSelectionManager.Piece, PieceSelectionManager.Other);

                    AnimationController.Instance.AnimateSwapRevert(PieceSelectionManager.Piece, PieceSelectionManager.Other);
                }
                else
                {
                    AnimationController.Instance.AnimateSwap(PieceSelectionManager.Piece, PieceSelectionManager.Other);
                }
            }

            PieceSelectionManager.Clear();
        }
    }

    private bool IsAdjacent(PieceController piece, PieceController other)
    {
        if (piece.Row == other.Row && (piece.Col == other.Col - 1 || piece.Col == other.Col + 1))
            return true;

        if (piece.Col == other.Col && (piece.Row == other.Row - 1 || piece.Row == other.Row + 1))
            return true;

        return false;
    }
}

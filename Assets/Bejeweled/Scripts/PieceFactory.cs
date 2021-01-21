using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameController))]
public class PieceFactory : MonoBehaviour
{
    [SerializeField] private PieceController m_piecePrefab;

    public PieceController Create()
    {
        return Create(-1);
    }

    public PieceController Create(int pieceType)
    {
        PieceController piece = Instantiate(m_piecePrefab);
        piece.PieceType = pieceType;

        return piece;
    }
}

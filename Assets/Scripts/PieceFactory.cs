using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameController))]
public class PieceFactory : MonoBehaviour
{
    public GameController game;

    [SerializeField] private PieceController m_piecePrefab;

    [SerializeField] private Color32[] m_colors;

    public PieceController Create()
    {
        return Create(-1);
    }

    public PieceController Create(int pieceType)
    {
        if (pieceType == -1)
            pieceType = UnityEngine.Random.Range(0, m_colors.Length);

        PieceController piece = Instantiate(m_piecePrefab);
        piece.game = game;
        piece.PieceType = pieceType;
        piece.GetComponentInChildren<Renderer>().material.color = m_colors[piece.PieceType];

        return piece;
    }
}

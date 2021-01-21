using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    [SerializeField] private Sprite[] m_sprites;

    [SerializeField] private int m_pieceType;
    public int PieceType
    {
        get => m_pieceType;
        set
        {
            if (m_pieceType == value) return;

            m_pieceType = value;

            if (m_pieceType == -1)
            {
                m_pieceType = UnityEngine.Random.Range(0, m_sprites.Length);
            }

            SetPieceType();
        }
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

    public bool Removed;
    public bool Merging;

    private bool m_selected;
    public bool Selected
    {
        get => m_selected;
        set
        {
            if (m_selected == value) return;

            m_selected = value;

            float s = m_selected ? 0.8f : 1f;

            transform.localScale = new Vector3(s, s, s);
        }
    }

    private void SetPieceType()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = m_sprites[m_pieceType];
    }

    private void OnValidate()
    {
        m_pieceType = Math.Max(0, Math.Min(m_sprites.Length - 1, m_pieceType));

        SetPieceType();
    }
}

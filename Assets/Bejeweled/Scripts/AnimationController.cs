using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static AnimationController Instance { get; private set; }

    private GameController Game => GameController.Instance;

    public float pieceDropSpeed = 3f;
    public float pieceSwapSpeed = 3f;
    public float pieceMergeSpeed = 3f;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public bool AnimatePieceDrop(PieceController piece)
    {
        bool animate = false;

        Vector3 sourcePos = piece.transform.position;
        Vector3 targetPos = Game.GetPiecePosition(piece.Row, piece.Col);

        float dist = (sourcePos - targetPos).magnitude;

        if (dist > 0.01f)
        {
            animate = true;

            targetPos = new Vector3(sourcePos.x, Mathf.Max(targetPos.y, sourcePos.y - pieceDropSpeed * Time.deltaTime));
        }

        piece.transform.position = targetPos;

        return animate;
    }

    public bool AnimateMerge()
    {
        bool animate = false;

        foreach (PieceController piece in Game.Pieces)
        {
            if (!piece) continue;

            if (piece.Merging)
            {
                float s = piece.transform.localScale.x;

                if (s > 0.01f)
                {
                    s = Mathf.Max(0, s - pieceMergeSpeed * Time.deltaTime);

                    piece.transform.localScale = new Vector3(s, s, s);
                }
                else
                {
                    piece.Merging = false;

                    Destroy(piece.gameObject);

                    Game.SetPieceAt(piece.Row, piece.Col, null);
                }

                animate = true;
            }
        }

        return animate;
    }

    private class SwapJob
    {
        private PieceController m_piece;
        private PieceController m_other;

        private float m_speed;

        private bool m_revert;
        private bool m_reverting;
        
        private bool m_animated;
        public bool Animated => m_animated;

        private LinearTween m_tween0;
        private LinearTween m_tween1;

        public SwapJob(PieceController piece, PieceController other, float speed, bool revert = false)
        {
            m_piece = piece;
            m_other = other;

            m_speed = speed;

            m_revert = revert;

            if (m_revert)
            {
                m_tween0 = new LinearTween(piece.transform, piece.transform.position, GameController.Instance.GetPiecePosition(other.Row, other.Col), speed);
                m_tween1 = new LinearTween(other.transform, other.transform.position, GameController.Instance.GetPiecePosition(piece.Row, piece.Col), speed);
            }
            else
            {
                m_tween0 = new LinearTween(piece.transform, piece.transform.position, GameController.Instance.GetPiecePosition(piece.Row, piece.Col), speed);
                m_tween1 = new LinearTween(other.transform, other.transform.position, GameController.Instance.GetPiecePosition(other.Row, other.Col), speed);
            }
        }

        public void Animate()
        {
            bool a = m_tween0.Animate();
            bool b = m_tween1.Animate();

            m_animated = a || b;

            if (m_revert && !m_animated && !m_reverting)
            {
                m_tween0 = new LinearTween(m_piece.transform, m_piece.transform.position, GameController.Instance.GetPiecePosition(m_piece.Row, m_piece.Col), m_speed);
                m_tween1 = new LinearTween(m_other.transform, m_other.transform.position, GameController.Instance.GetPiecePosition(m_other.Row, m_other.Col), m_speed);

                m_reverting = true;
                m_animated = true;
            }
        }
    }

    private List<SwapJob> m_swaps = new List<SwapJob>();

    public void AnimateSwap(PieceController piece, PieceController other)
    {
        m_swaps.Add(new SwapJob(piece, other, pieceSwapSpeed));
    }

    public void AnimateSwapRevert(PieceController piece, PieceController other)
    {
        m_swaps.Add(new SwapJob(piece, other, pieceSwapSpeed, true));
    }

    public bool HasSwapJobs()
    {
        return m_swaps.Count > 0;
    }

    private void Update()
    {
        if (HasSwapJobs())
        {
            m_swaps.ForEach(job =>
            {
                job.Animate();
            });

            m_swaps.RemoveAll(job => !job.Animated);
        }
    }
}

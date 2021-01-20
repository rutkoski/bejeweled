using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearTween
{
    private Transform m_target;

    private Vector3 m_start;

    private Vector3 m_end;

    private float m_speed;
    
    private float m_startTime;
    
    private float m_length;

    public LinearTween(Transform target, Vector3 start, Vector3 end, float speed)
    {
        m_target = target;
        m_start = start;
        m_end = end;
        m_speed = speed;

        m_startTime = Time.time;

        m_length = Vector3.Distance(start, end);
    }

    public bool Animate()
    {
        bool animated = false;

        float dist = (Time.time - m_startTime) * m_speed;

        float t = dist / m_length;

        Vector3 position = m_end;

        if (t < 0.99)
        {
            animated = true;

            position = Vector3.Lerp(m_start, m_end, t);
        }

        m_target.position = position;

        return animated;
    }
}

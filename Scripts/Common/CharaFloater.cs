using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaFloater : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float m_verticalWidth = default;
    [SerializeField, Range(0f, 0.05f)]
    private float m_moveSpeed = default;

    private float m_defaultPosY = default;
    private float m_addRad = 0f;

    // Start is called before the first frame update
    void Start()
    {
        m_defaultPosY = this.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        var pos = this.transform.position;
        pos.y = m_defaultPosY + Mathf.Sin(m_addRad) * m_verticalWidth;
        this.transform.position = pos;

        m_addRad += m_moveSpeed;
    }
}

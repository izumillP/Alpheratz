using System;
using UnityEngine;

public class TimeExecutor : MonoBehaviour
{
    private Action m_action = null;

    private bool m_executeFlag = false;
    private float m_elapsedTime = 0f;
    private float m_intervalTime = 0f;

    // Update is called once per frame
    void Update()
    {
        if (m_action != null && m_executeFlag)
        {
            m_elapsedTime += Time.deltaTime;
            if (m_elapsedTime >= m_intervalTime)
            {
                m_action();
                m_elapsedTime = 0f;
            }
        }
    }

    /// <summary>
    /// ���s�^�C�}�[������
    /// </summary>
    /// <param name="action">���s�������֐�</param>
    /// <param name="interval">���s����Ԋu(�b)</param>
    public void Initialize(Action action, float interval)
    {
        m_intervalTime = interval;
        m_action = action;
    }

    /// <summary>
    /// ���s�^�C�}�[����J�n
    /// </summary>
    /// <param name="isImmediate">�������s</param>
    public void Play(bool isImmediate = true)
    {
        if (isImmediate)
        {
            m_elapsedTime = m_intervalTime;
        }
        m_executeFlag = true;
    }

    /// <summary>
    /// ���s�^�C�}�[�ꎞ��~
    /// </summary>
    public void Pause()
    {
        m_executeFlag = false;
    }

    /// <summary>
    /// ���s�^�C�}�[��~
    /// </summary>
    public void Stop()
    {
        m_elapsedTime = 0f;
        m_executeFlag = false;
    }

    /// <summary>
    /// �^�C�}�[�̊Ԋu��ύX����
    /// </summary>
    /// <param name="interval"></param>
    public void ChangeInterval(float interval)
    {
        m_intervalTime = interval;
    }

    /// <summary>
    /// ���s�^�C�}�[�̎��s����֐���ύX����
    /// </summary>
    /// <param name="action"></param>
    public void ChangeAction(Action action)
    {
        m_action = action;
    }

    /// <summary>
    /// �o�ߎ��Ԃ����Z�b�g
    /// </summary>
    public void ResetElapsedTime()
    {
        m_elapsedTime = 0f;
    }

    /// <summary>
    /// ���s�^�C�}�[���Đ�����Ă��邩�H
    /// </summary>
    /// <returns>true:�Đ�/false:��~or�ꎞ��~</returns>
    public bool IsPlay()
    {
        return m_executeFlag;
    }

    public float GetElapsedTime()
    {
        return m_elapsedTime;
    }
}
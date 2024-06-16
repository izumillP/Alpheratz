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
    /// 実行タイマー初期化
    /// </summary>
    /// <param name="action">実行したい関数</param>
    /// <param name="interval">実行する間隔(秒)</param>
    public void Initialize(Action action, float interval)
    {
        m_intervalTime = interval;
        m_action = action;
    }

    /// <summary>
    /// 実行タイマー動作開始
    /// </summary>
    /// <param name="isImmediate">即時実行</param>
    public void Play(bool isImmediate = true)
    {
        if (isImmediate)
        {
            m_elapsedTime = m_intervalTime;
        }
        m_executeFlag = true;
    }

    /// <summary>
    /// 実行タイマー一時停止
    /// </summary>
    public void Pause()
    {
        m_executeFlag = false;
    }

    /// <summary>
    /// 実行タイマー停止
    /// </summary>
    public void Stop()
    {
        m_elapsedTime = 0f;
        m_executeFlag = false;
    }

    /// <summary>
    /// タイマーの間隔を変更する
    /// </summary>
    /// <param name="interval"></param>
    public void ChangeInterval(float interval)
    {
        m_intervalTime = interval;
    }

    /// <summary>
    /// 実行タイマーの実行する関数を変更する
    /// </summary>
    /// <param name="action"></param>
    public void ChangeAction(Action action)
    {
        m_action = action;
    }

    /// <summary>
    /// 経過時間をリセット
    /// </summary>
    public void ResetElapsedTime()
    {
        m_elapsedTime = 0f;
    }

    /// <summary>
    /// 実行タイマーが再生されているか？
    /// </summary>
    /// <returns>true:再生/false:停止or一時停止</returns>
    public bool IsPlay()
    {
        return m_executeFlag;
    }

    public float GetElapsedTime()
    {
        return m_elapsedTime;
    }
}
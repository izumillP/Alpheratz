using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private HardDropEffect m_hardDropEffectPrefab = null;
    [SerializeField] private ClearLineEffect m_clearLineEffectPrefab = null;
    [SerializeField] private int m_hardDropEffectPoolNum = 30;
    [SerializeField] private int m_clearLineEffectPoolNum = 4;

    [SerializeField] private float m_hardDropEffectDuration = 0.5f;
    [SerializeField] private float m_clearLineEffectDuration = 0.5f;

    private List<HardDropEffect> m_hardDropEffectPool = new();
    private List<ClearLineEffect> m_clearLineEffectPool = new();

    private void Start()
    {
        for (int i = 0; i < m_hardDropEffectPoolNum; i++)
        {
            var instance = Instantiate(m_hardDropEffectPrefab);
            m_hardDropEffectPool.Add(instance);
        }

        for (int i = 0; i < m_clearLineEffectPoolNum; i++)
        {
            var instance = Instantiate(m_clearLineEffectPrefab);
            m_clearLineEffectPool.Add(instance);
        }
    }

    public void PlayHardDropEffect(Vector3 position)
    {
        foreach(var effect in m_hardDropEffectPool)
        {
            if(!effect.gameObject.activeSelf)
            {
                effect.Play(m_hardDropEffectDuration, position);
                break;
            }
        }
    }

    public void PlayClearLineEffect(Vector3 position)
    {
        foreach (var effect in m_clearLineEffectPool)
        {
            if (!effect.gameObject.activeSelf)
            {
                effect.Play(m_clearLineEffectDuration, position);
                break;
            }
        }
    }
}

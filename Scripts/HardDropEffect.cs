using System;
using UnityEngine;

public class HardDropEffect : MonoBehaviour
{
    private float m_elapsedTime = 0f;
    private float m_duration = 0f;
    private bool m_isPlay = false;
    private SpriteRenderer m_spriteRenderer = null;
    private float m_alpha = default;
    private ParticleSystem m_particleSystem = null;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isPlay)
        {
            return;
        }

        m_elapsedTime += Time.deltaTime;
        m_alpha = Mathf.Clamp(m_duration - m_elapsedTime, 0f, m_duration);
        var color = m_spriteRenderer.color;
        color.a = m_alpha;
        m_spriteRenderer.color = color;

    }

    public void Play(float duration, Vector3 position)
    {
        this.gameObject.transform.position = position;
        this.gameObject.SetActive(true);

        m_isPlay = true;
        m_duration = duration;
        var main = m_particleSystem.main;
        main.simulationSpeed = 4f;
        m_particleSystem.Play();
    }

    public void OnParticleSystemStopped()
    {
        this.gameObject.SetActive(false);
        m_elapsedTime = 0f;
        m_isPlay = false;
    }
}

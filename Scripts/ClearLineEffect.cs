using UnityEngine;

public class ClearLineEffect : MonoBehaviour
{
    private float m_elapsedTime = 0f;
    private float m_duration = 0f;
    private bool m_isPlay = false;
    private SpriteRenderer m_spriteRenderer = null;
    private float m_alpha = default;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        m_spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (m_elapsedTime > m_duration)
        {
            this.gameObject.SetActive(false);
            m_elapsedTime = 0f;
            m_isPlay = false;
        }

    }

    public void Play(float duration, Vector3 position)
    {
        var tempPosition = position;
        tempPosition.z = -0.001f;
        this.gameObject.transform.position = tempPosition;
        this.gameObject.transform.localScale = new Vector3(10f, 1f, 1f);
        this.gameObject.SetActive(true);

        m_isPlay = true;
        m_duration = duration;
    }
}
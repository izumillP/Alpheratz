using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Empty,
    Ghost,
    Sentinel,
    Wall,
    Dummy,
    Obstacle,
    Mino,
    BlockTypeMax
}

public enum MinoType
{
    Empty,
    Dummy,
    Obstacle,
    MinoT,
    MinoS,
    MinoZ,
    MinoL,
    MinoJ,
    MinoO,
    MinoI,
    MinoTypeMax
}

public class Block : MonoBehaviour
{
    public BlockType BlockType { get; set; } = BlockType.Empty;
    public MinoType MinoType { get; set; } = MinoType.Empty;

    [SerializeField] private Color32 m_minoColorWall    = Color.white;
    [SerializeField] private Color32 m_minoColorDummy   = Color.white;
    [SerializeField] private Color32 m_minoObstacle     = Color.white;
    [SerializeField] private Color32 m_minoColorT       = Color.white;
    [SerializeField] private Color32 m_minoColorS       = Color.white;
    [SerializeField] private Color32 m_minoColorZ       = Color.white;
    [SerializeField] private Color32 m_minoColorL       = Color.white;
    [SerializeField] private Color32 m_minoColorJ       = Color.white;
    [SerializeField] private Color32 m_minoColorO       = Color.white;
    [SerializeField] private Color32 m_minoColorI       = Color.white;

    [SerializeField] private Sprite m_blockSprite = null;
    [SerializeField] private Sprite m_ghostSprite = null;

    private SpriteRenderer m_spriteRenderer = null;

    // Start is called before the first frame update
    void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DyeBlock()
    {
        if (BlockType == BlockType.Empty || BlockType == BlockType.Sentinel)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
            if (BlockType == BlockType.Wall)
            {
                m_spriteRenderer.material.color = m_minoColorWall;
                return;
            }
        }
        switch (MinoType)
        {
            case MinoType.Dummy:
                m_spriteRenderer.material.color = m_minoColorDummy;
                break;
            case MinoType.Obstacle:
                m_spriteRenderer.material.color = m_minoColorDummy;
                break;
            case MinoType.MinoT:
                m_spriteRenderer.material.color = m_minoColorT;
                break;
            case MinoType.MinoS:
                m_spriteRenderer.material.color = m_minoColorS;
                break;
            case MinoType.MinoZ:
                m_spriteRenderer.material.color = m_minoColorZ;
                break;
            case MinoType.MinoL:
                m_spriteRenderer.material.color = m_minoColorL;
                break;
            case MinoType.MinoJ:
                m_spriteRenderer.material.color = m_minoColorJ;
                break;
            case MinoType.MinoO:
                m_spriteRenderer.material.color = m_minoColorO;
                break;
            case MinoType.MinoI:
                m_spriteRenderer.material.color = m_minoColorI;
                break;
            default:
                Debug.Log("ミノのタイプが予想される範囲内にないなんてびっくりしたよね。");
                break;
        }
        if (BlockType == BlockType.Ghost)
        {
            /*
            var color = m_spriteRenderer.material.color;
            color.a = 0.25f;
            m_spriteRenderer.material.color = color;
            */
            m_spriteRenderer.sprite = m_ghostSprite;
        }
        else
        {
            m_spriteRenderer.sprite = m_blockSprite;
        }
    }

    public void DyeBlockGray()
    {
        m_spriteRenderer.material.color = m_minoColorDummy;
    }

}
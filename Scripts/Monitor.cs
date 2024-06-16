using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monitor : MonoBehaviour 
{
    [SerializeField] private Block m_blockObject = null;
    [SerializeField] private float m_monitorScale = 1f;
    private const int maxMinoSize = 4;
    public BlockType BlockType { get; set; } = BlockType.Empty;
    public Block[,] MinoMonitor { get; set; } = new Block[maxMinoSize, maxMinoSize];
    private Mino m_mino = new Mino();

    private void OnValidate()
    {
        var scale = new Vector3(4f,3f,1f);
        this.transform.localScale = scale*m_monitorScale;
    }
    private void Awake()
    {
        GetComponent<SpriteRenderer>().material.color = new Color(0f, 0f, 0f, 0f);
    }

    public void GenerateMonitor()
    {
        for (int x = 0; x < maxMinoSize; x++)
        {
            float monitorPosX = transform.position.x + x * m_monitorScale;
            for (int y = 0; y < maxMinoSize; y++)
            {
                float monitorPosY = transform.position.y - y * m_monitorScale;
                MinoMonitor[x, y] = Instantiate<Block>(m_blockObject,new Vector3(monitorPosX, monitorPosY, 0f), Quaternion.identity);
                MinoMonitor[x, y].gameObject.transform.localScale *= m_monitorScale;
            }
        }
    }

    public void EraseMonitor()
    {
        for (int x = 0; x < maxMinoSize; x++)
        {
            for (int y = 0; y < maxMinoSize; y++)
            {
                MinoMonitor[x, y].BlockType = BlockType.Empty;
                MinoMonitor[x, y].MinoType = MinoType.Empty;
                MinoMonitor[x, y].DyeBlock();
            }
        }
    }
    public void PutMinoIntoMonitor(MinoType minoType)
    {
        float tempMonitorPosX = 0f;
        float tempMonitorPosY = 0f;

        m_mino.GenerateMino(minoType);
        EraseMonitor();
        for (int x = 0; x < m_mino.Size; x++)
        {
            for (int y = 0; y < m_mino.Size; y++)
            {
                if (m_mino.Shape[x,y])
                {
                    MinoMonitor[x, y].BlockType = BlockType.Mino;
                    MinoMonitor[x, y].MinoType = minoType;
                    tempMonitorPosX = transform.position.x + (x + (maxMinoSize - m_mino.Size) * 0.5f) * m_monitorScale;
                    tempMonitorPosY = transform.position.y - y * m_monitorScale;
                    MinoMonitor[x, y].gameObject.transform.position = new Vector3(tempMonitorPosX, tempMonitorPosY, 0f);
                    MinoMonitor[x, y].DyeBlock();
                }
            }
        }
    }

    public void DyeMinoGray(bool cancel = false)
    {
        for (int x = 0; x < m_mino.Size; x++)
        {
            for (int y = 0; y < m_mino.Size; y++)
            {
                if (m_mino.Shape[x, y])
                {
                    if (cancel)
                    {
                        MinoMonitor[x, y].DyeBlock();
                    }
                    else
                    {
                        MinoMonitor[x, y].DyeBlockGray();
                    }
                    
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public const int STAGE_MAX_X    = SENTINEL_NUM + WALL_NUM + BOX_X_NUM + WALL_NUM + SENTINEL_NUM;
    public const int STAGE_MAX_Y    = SENTINEL_NUM + WALL_NUM + BOX_Y_NUM + ROOF_SPACE_NUM + SKY_SPACE_NUM + SENTINEL_NUM;
    public const int BOX_X_NUM      = 10;
    public const int BOX_Y_NUM      = 20;
    public const int WALL_NUM       = 1;
    public const int SENTINEL_NUM   = 2;
    public const int ROOF_SPACE_NUM = 2;
    public const int SKY_SPACE_NUM  = 20;

    private Block[,] m_stage = new Block[STAGE_MAX_X, STAGE_MAX_Y];
    [SerializeField] private Block m_blockObject = null; // prefabを読み込むUnityの処理をする。

    public void GenerateStage()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                float tempPosX = this.transform.position.x + x - SENTINEL_NUM;
                float tempPosY = this.transform.position.y + STAGE_MAX_Y - y - SENTINEL_NUM;
                m_stage[x, y] = Instantiate<Block>(m_blockObject, new Vector3(tempPosX, tempPosY, 0f), Quaternion.identity);
            }
        }
    }

    public void CreateBox()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                if (x == SENTINEL_NUM || x == STAGE_MAX_X - SENTINEL_NUM - 1 || y == STAGE_MAX_Y - SENTINEL_NUM - 1)
                {
                    m_stage[x, y].BlockType = BlockType.Wall;
                    m_stage[x, y].MinoType = MinoType.Empty;
                }
                if (y < ROOF_SPACE_NUM + SKY_SPACE_NUM + SENTINEL_NUM)
                {
                    m_stage[x, y].BlockType = BlockType.Empty;
                    m_stage[x, y].MinoType = MinoType.Empty;
                }
                if (x < SENTINEL_NUM || x > STAGE_MAX_X - SENTINEL_NUM - 1 || y > STAGE_MAX_Y - SENTINEL_NUM - 1)
                {
                    m_stage[x, y].BlockType = BlockType.Sentinel;
                    m_stage[x, y].MinoType = MinoType.Empty;
                }
                if ((x == SENTINEL_NUM || x == STAGE_MAX_X - SENTINEL_NUM - 1) && y < SENTINEL_NUM + SKY_SPACE_NUM + ROOF_SPACE_NUM)
                {
                    m_stage[x, y].BlockType = BlockType.Sentinel;
                    m_stage[x, y].MinoType = MinoType.Empty;
                }
            }
        }
    }

    public void PrintBlockStatus()
    {
        for (int x = 0; x < STAGE_MAX_X; x++)
        {
            for (int y = 0; y < STAGE_MAX_Y; y++)
            {
                m_stage[x, y].DyeBlock();
                // ステージのブロック状態に合わせて描写する。
            }
        }
    }

    public void FillCustomBlocks()
    {
        var instance = DebugStageGeneratorParameter.instance;
        if (!instance.IsDebugMode) 
        {
            return;
        }

        for (int x = 0; x < BOX_X_NUM; x++)
        {
            for (int y = 0; y < BOX_Y_NUM; y++)
            {
                int tempPosition = x + BOX_X_NUM * y;
                if (instance.DebugStageMap[tempPosition])
                {
                    int tempX = x + SENTINEL_NUM + WALL_NUM;
                    int tempY = y + SENTINEL_NUM + SKY_SPACE_NUM + ROOF_SPACE_NUM;
                    m_stage[tempX, tempY].BlockType = BlockType.Dummy;
                    m_stage[tempX, tempY].MinoType = MinoType.Dummy;
                }
            }
        }
    }

    public void PutMino(Mino mino,bool isErase = false, bool isGhost = false) // アクティヴミノの移動の後画像をクリアする処理を入れる。
    {
        int px = isGhost ? mino.GhostPosX : mino.PosX;
        int py = isGhost ? mino.GhostPosY : mino.PosY;
        var shape = isGhost ? mino.GhostShape : mino.Shape;

        for (int x = 0; x < mino.Size; x++)
        {
            for (int y = 0; y < mino.Size; y++)
            {
                if (shape[x, y])
                {
                    m_stage[px + x, py + y].BlockType = isErase ? BlockType.Empty : (isGhost ? BlockType.Ghost : BlockType.Mino);
                    m_stage[px + x, py + y].MinoType = isErase ? MinoType.Empty : mino.Type;
                } // アクティヴミノの現在位置を始動に該当範囲内のブロックのステータスを変更する。
            }
        }
    }
    public bool IsCanPut(Mino mino, bool isGhost = false)
    {
        var shape = isGhost ? mino.GhostShape : mino.Shape;
        for (int x = 0; x < mino.Size; x++)
        {
            for (int y = 0; y < mino.Size; y++)
            {
                if (shape[x, y])
                {
                    int px = isGhost ? mino.GhostPosX : mino.PosX;
                    int py = isGhost ? mino.GhostPosY : mino.PosY;
                    if (m_stage[px + x, py + y].BlockType > BlockType.Ghost)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public List<int> ClearLinesCheck()
    {
        var clearLines = new List<int>();
        for (int y = 0 + SENTINEL_NUM; y < STAGE_MAX_Y - 1 - SENTINEL_NUM; y++)
        {
            bool isFilledLine = true;
            for (int x = 1 + SENTINEL_NUM; x < STAGE_MAX_X - 1 - SENTINEL_NUM; x++)
            {
                if (m_stage[x, y].BlockType <= BlockType.Ghost)
                {
                    isFilledLine = false;
                }
            }
            if (isFilledLine)
            {
                clearLines.Add(y);
                // Debug.Log("消したyの値は" + y);
            }
        }
        return clearLines;
    }


    public void ClearLines(int line)
    {
        for (int x = 1 + SENTINEL_NUM; x < STAGE_MAX_X - 1 - SENTINEL_NUM; x++)
        {
            m_stage[x, line].BlockType = BlockType.Empty;
            m_stage[x, line].MinoType = MinoType.Empty;
        }
    }

    public void DecendLines(int line)
    {
        for (int y2 = line; y2 > SENTINEL_NUM; y2--)
        {
            for (int x2 = 1 + SENTINEL_NUM; x2 < STAGE_MAX_X - 1 - SENTINEL_NUM; x2++)
            {
                m_stage[x2, y2].BlockType = m_stage[x2, y2 - 1].BlockType;
                m_stage[x2, y2].MinoType = m_stage[x2, y2 - 1].MinoType;
            }
        }
    }

    public bool CheckPerfectClear()
    {
        bool isBlockEmpty = true;
        bool isMinoEmpty = true;

        for (int x = 0; x < BOX_X_NUM; x++)
        {
            for (int y = 0; y < BOX_Y_NUM; y++)
            {
                int tempX = x + SENTINEL_NUM + WALL_NUM;
                int tempY = y + SENTINEL_NUM + SKY_SPACE_NUM + ROOF_SPACE_NUM;

                isBlockEmpty &= m_stage[tempX, tempY].BlockType == BlockType.Empty;
                isMinoEmpty &= m_stage[tempX, tempY].MinoType == MinoType.Empty;
            }
        }
        return isBlockEmpty && isMinoEmpty;
    }

    public TSpinType CheckTSpin(Mino mino)
    {
        if (mino.Type != MinoType.MinoT)
        {
            return TSpinType.NoTSpin;
        }
        
        int surroundingBlocks = 0;

        bool[,] miniTSpinChecker = new bool[2,2];
        int NormalTSpinCounter = 0;

        switch (mino.Direction)
        {
            case MinoDirection.North:
                miniTSpinChecker[0, 0] = true;
                miniTSpinChecker[1, 0] = true;
                break;
            case MinoDirection.East:
                miniTSpinChecker[1, 0] = true;
                miniTSpinChecker[1, 1] = true;
                break;
            case MinoDirection.South:
                miniTSpinChecker[1, 1] = true;
                miniTSpinChecker[0, 1] = true;
                break;
            case MinoDirection.West:
                miniTSpinChecker[0, 1] = true;
                miniTSpinChecker[0, 0] = true;
                break;
            default:
                Debug.Log("方向がおかしい！　バグっている！");
                return TSpinType.NoTSpin;
        }

        for(int x = 0; x < 2; x++)
        {
            for(int y = 0; y < 2; y++)
            {
                if (m_stage[mino.PosX + x * 2, mino.PosY + y * 2].BlockType > BlockType.Ghost)
                {
                    if(miniTSpinChecker[x, y])
                    {
                        NormalTSpinCounter++;
                    }
                    surroundingBlocks++;
                }
                // Debug.Log("Tスピンチェック" + (mino.PosX + x * 2) + "," + (mino.PosY + y * 2));
                // Debug.Log(m_stage[mino.PosX + x * 2, mino.PosY + y * 2].BlockType);
            }
        }
        // Debug.Log("Tスピン判定" + surroundingBlocks);
        // Debug.Log("普通のTSpinですか:" + (NormalTSpinCounter == 2));
        // Debug.Log("SRSは何段階ですか:" + mino.CurrentSRS);

        if (surroundingBlocks >= 3)
        {
            if (NormalTSpinCounter >= 2 || mino.CurrentSRS == 5)
            {
                return TSpinType.NormalTSpin;
            }
            else
            {
                return TSpinType.MiniTSpin;
            }
        }
        else
        {
            return TSpinType.NoTSpin;
        }
    }

    public Vector3 GetBlockVector(int posX, int posY)
    {
        return m_stage[posX, posY].gameObject.transform.position;
    }
}

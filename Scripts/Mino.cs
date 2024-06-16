using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MinoDirection
{
    MinMinoDirection,
    North,
    East,
    South,
    West,
    MaxMinoDirection
}
public class Mino // アクティヴミノの挙動に必要な変数や関数を集めたクラス。
{
    private Stage m_stage = null;
    public MinoDirection Direction { get; private set; } = MinoDirection.North;
    public int PosX { get; set; } = default;
    public int PosY { get; set; } = default;
    public int GhostPosX { get; set; } = default;
    public int GhostPosY { get; set; } = default;
    public int Size { get; private set; } = default;
    public MinoType Type { get; set; } = MinoType.Empty;
    public bool[,] Shape { get; private set; } = new bool[4, 4];
    public bool[,] GhostShape { get; private set; } = new bool[4, 4];
    public int MaxPosY { get; set; } = 0;
    public bool IsMaxPosRenew { get; set; } = false;
    public int CurrentSRS { get; set; } = 0;

    private readonly bool[,] minoT =
        {
        { false, true, false},
        { true, true, true},
        { false, false, false}
        };

    private readonly bool[,] minoS =
        {
        { false, true, true, },
        { true, true, false, },
        { false, false, false}
        };

    private readonly bool[,] minoZ =
    {
        { true, true, false},
        { false, true, true},
        { false, false, false}
        };

    private readonly bool[,] minoL =
    {
        { true, false, false},
        { true, true, true},
        { false, false, false}
     };

    private readonly bool[,] minoJ =
    {
        { false, false, true},
        { true, true, true},
        { false, false, false}
    };

    private readonly bool[,] minoO =
    {
        {true, true},
        {true, true}
    };

    private readonly bool[,] minoI =
    {
        { false, false, false, false },
        { true, true, true, true },
        { false, false, false, false },
        { false, false, false, false },
    };

    public void SetStageInstance(Stage stage)
    {
        m_stage = stage;
    }
    public void GenerateMino(MinoType minoType)
    {
        const int WALL_NUM = 1;
        const int SENTINEL_NUM = 2;
        const int ROOF_SPACE_NUM = 2;
        const int SKY_SPACE_NUM = 20;

        switch (minoType)
        {
            case MinoType.MinoT:
                Shape = minoT;
                PosX = 3;
                PosY = 0;
                Size = 3;
                break;
            case MinoType.MinoS:
                Shape = minoS;
                PosX = 3;
                PosY = 0;
                Size = 3;
                break;
            case MinoType.MinoZ:
                Shape = minoZ;
                PosX = 3;
                PosY = 0;
                Size = 3;
                break;
            case MinoType.MinoL:
                Shape = minoL;
                PosX = 3;
                PosY = 0;
                Size = 3;
                break;
            case MinoType.MinoJ:
                Shape = minoJ;
                PosX = 3;
                PosY = 0;
                Size = 3;
                break;
            case MinoType.MinoO:
                Shape = minoO;
                PosX = 4;
                PosY = 0;
                Size = 2;
                break;
            case MinoType.MinoI:
                Shape = minoI;
                PosX = 3;
                PosY = 0;
                Size = 4;
                break;
            default:
                Shape = null;
                PosX = default;
                PosY = default;
                return;
        }
        
        PosX += SENTINEL_NUM + WALL_NUM;
        PosY += SENTINEL_NUM + SKY_SPACE_NUM + ROOF_SPACE_NUM;
        GhostPosX = PosX;
        GhostPosY = PosY;
        MaxPosY = PosY;
        IsMaxPosRenew = false;
        Direction = MinoDirection.North;
        Shape = Util.RightRot(Shape); // 配列を読む際に逆転する書き方をしているので調整する。
        CopyOriginToGhost();
        Type = minoType;
    }

    public void CopyOriginToGhost()
    {
        GhostPosX = PosX;
        GhostPosY = PosY;

        GhostShape = new bool[Size, Size];
        Array.Copy(Shape, GhostShape, Shape.Length);
    }

    private bool SuperRotationSystem(MinoDirection prevDirection)
    {
        int prevPosX = PosX;
        int prevPosY = PosY;
        // SRS1段階
        if (m_stage.IsCanPut(this))
        {
            CurrentSRS = 1;
            return true;
        }

        // SRS2段階
        if (Type != MinoType.MinoI)
        {
            switch (prevDirection)
            {
                case MinoDirection.North:
                case MinoDirection.South:
                    switch (Direction)
                    {
                        case MinoDirection.East:
                            PosX--;
                            break;
                        case MinoDirection.West:
                            PosX++;
                            break;
                    }
                    break;

                case MinoDirection.East:
                    PosX++;
                    break;
                case MinoDirection.West:
                    PosX--;
                    break;
            }
        }
        else
        {
            switch (prevDirection)
            {
                case MinoDirection.North:
                    switch (Direction)
                    {
                        case MinoDirection.East:
                            PosX -= 2;
                            break;
                        case MinoDirection.West:
                            PosX--;
                            break;
                    }
                    break;
                case MinoDirection.East:
                    switch (Direction)
                    {
                        case MinoDirection.South:
                            PosX--;
                            break;
                        case MinoDirection.North:
                            PosX += 2;
                            break;
                    }
                    break;
                case MinoDirection.South:
                    switch (Direction)
                    {
                        case MinoDirection.West:
                            PosX += 2;
                            break;
                        case MinoDirection.East:
                            PosX++;
                            break;
                    }
                    break;
                case MinoDirection.West:
                    switch (Direction)
                    {
                        case MinoDirection.North:
                            PosX++;
                            break;
                        case MinoDirection.South:
                            PosX -= 2;
                            break;
                    }
                    break;
            }
        }
        if (m_stage.IsCanPut(this))
        {
            CurrentSRS = 2;
            return true;
        }

        // SRS3段階
        if (Type != MinoType.MinoI)
        {
            switch (prevDirection)
            {
                case MinoDirection.North:
                case MinoDirection.South:
                    PosY--;
                    break;
                case MinoDirection.East:
                case MinoDirection.West:
                    PosY++;
                    break;
            }
        }
        else
        {
            switch (prevDirection)
            {
                case MinoDirection.North:
                    PosX += 3;
                    break;
                case MinoDirection.East:
                case MinoDirection.West:
                    switch (Direction)
                    {
                        case MinoDirection.South:
                            PosX += 3;
                            break;
                        case MinoDirection.North:
                            PosX -= 3;
                            break;
                    }
                    break;
                case MinoDirection.South:
                    PosX -= 3;
                    break;
            }
        }
        if (m_stage.IsCanPut(this))
        {
            CurrentSRS = 3;
            return true;
        }


        // SRS4段階
        PosX = prevPosX;
        PosY = prevPosY;
        if (Type != MinoType.MinoI)
        {
            switch (prevDirection)
            {
                case MinoDirection.North:
                case MinoDirection.South:
                    PosY += 2;
                    break;
                case MinoDirection.East:
                case MinoDirection.West:
                    PosY -= 2;
                    break;
            }
        }
        else
        {
            switch (prevDirection)
            {
                case MinoDirection.North:
                    switch (Direction)
                    {
                        case MinoDirection.East:
                            PosX -= 2;
                            PosY++;
                            break;
                        case MinoDirection.West:
                            PosX--;
                            PosY -= 2;
                            break;
                    }
                    break;
                case MinoDirection.East:
                    switch (Direction)
                    {
                        case MinoDirection.South:
                            PosX--;
                            PosY -= 2;
                            break;
                        case MinoDirection.North:
                            PosX += 2;
                            PosY--;
                            break;
                    }
                    break;
                case MinoDirection.South:
                    switch (Direction)
                    {
                        case MinoDirection.West:
                            PosX += 2;
                            PosY--;
                            break;
                        case MinoDirection.East:
                            PosX++;
                            PosY += 2;
                            break;
                    }
                    break;
                case MinoDirection.West:
                    switch (Direction)
                    {
                        case MinoDirection.North:
                            PosX++;
                            PosY += 2;
                            break;
                        case MinoDirection.South:
                            PosX -= 2;
                            PosY++;
                            break;
                    }
                    break;
            }
        }

        if (m_stage.IsCanPut(this))
        {
            CurrentSRS = 4;
            return true;
        }

        // SRS5段階
        if (Type != MinoType.MinoI)
        {
            switch (prevDirection)
            {
                case MinoDirection.North:
                case MinoDirection.South:
                    switch (Direction)
                    {
                        case MinoDirection.East:
                            PosX--;
                            break;
                        case MinoDirection.West:
                            PosX++;
                            break;
                    }
                    break;
                case MinoDirection.East:
                    PosX++;
                    break;
                case MinoDirection.West:
                    PosX--;
                    break;
            }
        }
        else
        {
            PosX = prevPosX;
            PosY = prevPosY;
            switch (prevDirection)
            {
                case MinoDirection.North:
                    switch (Direction)
                    {
                        case MinoDirection.East:
                            PosX += 2;
                            PosY--;
                            break;
                        case MinoDirection.West:
                            PosX += 2;
                            PosY++;
                            break;
                    }
                    break;
                case MinoDirection.East:
                    switch (Direction)
                    {
                        case MinoDirection.South:
                            PosX += 2;
                            PosY++;
                            break;
                        case MinoDirection.North:
                            PosX--;
                            PosY += 2;
                            break;
                    }
                    break;
                case MinoDirection.South:
                    switch (Direction)
                    {
                        case MinoDirection.West:
                            PosX--;
                            PosY += 2;
                            break;
                        case MinoDirection.East:
                            PosX -= 2;
                            PosY--;
                            break;
                    }
                    break;
                case MinoDirection.West:
                    switch (Direction)
                    {
                        case MinoDirection.North:
                            PosX -= 2;
                            PosY--;
                            break;
                        case MinoDirection.South:
                            PosX++;
                            PosY -= 2;
                            break;
                    }
                    break;
            }
        }

        if (m_stage.IsCanPut(this))
        {
            CurrentSRS = 5;
            return true;
        }

        PosX = prevPosX;
        PosY = prevPosY;
        return false;
    }
    public void SuperLeftRot()
    {
        var prevDirection = Direction;
        Shape = Util.LeftRot(Shape);
        Direction--;
        if (Direction == MinoDirection.MinMinoDirection)
        {
            Direction = MinoDirection.West;
        }
        SuperRotationSystem(prevDirection);
    }
    public void SuperRightRot()
    {
        var prevDirection = Direction;
        Shape = Util.RightRot(Shape);
        Direction++;
        if (Direction == MinoDirection.MaxMinoDirection)
        {
            Direction = MinoDirection.North;
        }
        SuperRotationSystem(prevDirection);
    }

    public void GoToLeft()
    {
        PosX--;
    }

    public void GoToRight()
    {
        PosX++;
    }

    public void Drop()
    {
        PosY++;
        if (PosY > MaxPosY)
        {
            MaxPosY = PosY;
            IsMaxPosRenew = true;
           // Debug.Log("最大値更新:"+MaxPosY);
        }
    }

    public void UnDrop()
    {
        PosY--;
    }
    public void GhostDrop()
    {
        GhostPosY++;
    }

    public void GhostUnDrop()
    {
        GhostPosY--;
    }
}

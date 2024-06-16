using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum LeftRight
    {
        noLeftRight,
        Left,
        Right,
        MaxLeftRight
    }


    // ゲームステート関連
    private bool m_isFirstUpdate = true;
    private bool m_isGameOver = false;
    private bool m_isPauseMenu = false;

    [SerializeField] private bool m_isDebug = false;

    // システムタイマー関連(固定)
    [SerializeField] private float m_baseDropSpeed = 1f; // 自動落下の基本速度
    [SerializeField] private float m_buttonPressedJudgeSpan = 0.3f; // 長押し判定の時間
    [SerializeField] private float m_minoMoveSpan = 0.05f; // ブロック移動の間隔
    [SerializeField] private float m_minoLockdownTime = 0.5f; // ロックダウンの猶予時間
    [SerializeField] private float m_dropSpeedMag = 20f; // ソフトドロップの落下速度係数

    // スコア(レベル)関連
    [SerializeField] private StatusManager m_statusManager = null;
    private float m_dropSpeed = 1f; // 自動落下の速度管理変数
    private bool m_isLastMoveLandSpin = false;

    // ステージ生成インスタンス
    [SerializeField] private Stage m_stage = null;
    [SerializeField] private Monitor[] m_nextMonitors = null;
    [SerializeField] private Monitor m_holdMonitor = null;

    // ミノ生成関連
    private Mino m_mino = new(); // 落下中のミノのインスタンス
    private MinoQueue m_queue = new();

    // 直近消したライン
    private List<int> m_currentClearedLines = new();

    // ホールド関連
    private bool m_isHoldLock = false;
    private MinoType m_holdMino = MinoType.Empty;

    // タイマー関連
    private TimeExecutor m_dropExecutor = null;
    private TimeExecutor m_moveExecutor = null;
    private TimeExecutor m_pushExecutor = null;
    private bool m_isSoftDrop = false; // ソフトドロップ時はタイマーを別枠にするためのフラグ
    private bool m_isMoving = false;
    private LeftRight m_leftRight = LeftRight.noLeftRight;

    // ロックダウン関連
    private TimeExecutor m_lockDownExecutor = null;
    [SerializeField] private int m_lockDownMax = 14;
    private bool m_isHardDrop = false; // ハードドロップ時はタイマーを別枠にするためのフラグ
    private int m_lockDownCount = 0; // ロックダウンのリセット回数

    // インプット関連
    private PlayerInput m_playerGameInput = null;
    private PlayerInput m_playerMenuInput = null;

    // エフェクト関連
    [SerializeField] private EffectManager m_effectManager = null;
    private TimeExecutor m_clearEffectExecutor = null;
    private bool m_isClearEffect = false;

    // 数字区切り
    [SerializeField] private int m_numberSection = 4;

    // サウンド関連
    [SerializeField] private SoundManager m_soundManager = null;


    void Start()
    {
        Util.SetNumberCulture(m_numberSection);

        m_stage.GenerateStage();
        m_stage.CreateBox();
        m_stage.FillCustomBlocks();


        m_statusManager.ResetStatus();

        m_mino.SetStageInstance(m_stage);

        GenerateMonitors();

        m_dropExecutor = this.gameObject.AddComponent<TimeExecutor>();
        m_moveExecutor = this.gameObject.AddComponent<TimeExecutor>();
        m_lockDownExecutor = this.gameObject.AddComponent<TimeExecutor>();
        m_pushExecutor = this.gameObject.AddComponent<TimeExecutor>();
        m_clearEffectExecutor = this.gameObject.AddComponent<TimeExecutor>();

        m_statusManager.TimerStart();

        m_playerGameInput = this.gameObject.GetComponent<PlayerInput>();
        m_playerMenuInput = this.gameObject.GetComponent<PlayerInput>();
    }
    void Update()
    {
        if (m_isGameOver)
        {
            m_statusManager.TimerPause();
            // Debug.Log("おしまい！　" + m_statusManager.GetTotalClearedLines() + "ライン消しました。スコアは" + m_statusManager.GetScore() + "です。また遊んでね！");
            return;
        }

        if (m_isPauseMenu)
        {
            ControlMenu();
        }

        PreControlMino();

        if (m_clearEffectExecutor.IsPlay())
        {
            return;
        }

        if (!m_isSoftDrop)
        {
            CalculateDropSpeed();
            m_dropExecutor.ChangeInterval(m_dropSpeed);
        }

        if (m_isFirstUpdate) // 現在の処理では1秒目が空白になるため別途1秒目専用の処理を走らせる。
        {
            var instance = DebugStageGeneratorParameter.instance;

            // 初期ミノ生成
            CreateMino(m_queue.GetNextMino());
            // モニター内表示削除
            m_holdMonitor.EraseMonitor();
            SetHoldMinoIntoMonitor(instance.minoType);
            m_holdMino = instance.minoType;

            // ミノを降らせる。
            m_dropExecutor.Initialize(() =>
            {
                if (MoveMino(m_mino.Drop, m_mino.UnDrop) && m_isSoftDrop)
                {
                    m_statusManager.AddDropScore();
                }
            }, m_baseDropSpeed);

            m_dropExecutor.Play(true);

            m_clearEffectExecutor.Initialize(() =>
            {
                foreach (var line in m_currentClearedLines)
                {
                    m_stage.DecendLines(line);
                }
                m_clearEffectExecutor.Stop();
                m_dropExecutor.Play(true);
                m_isClearEffect = false;
                CreateMino(m_queue.GetNextMino());

            }, 0.5f);

            m_isFirstUpdate = false;
        }

        ControlMino();

        if (m_isClearEffect)
        {
            m_clearEffectExecutor.Play(false);
            m_dropExecutor.Stop();
        }
        else
        {
            m_stage.PrintBlockStatus();
        }

        m_statusManager.PrintTime();
    }

    private void OnGUI()
    {
        if (!m_isDebug)
        {
            return;
        }
        var style = new GUIStyle
        {
            fontSize = 40
        };
        GUI.Label(new Rect(10, 10, 200, 30), "ロックダウンカウント:" + m_lockDownCount, style);
        GUI.Label(new Rect(10, 50, 200, 30), "ロックダウン経過時間:" + m_lockDownExecutor.GetElapsedTime(), style);
        GUI.Label(new Rect(10, 90, 200, 30), "ドロップ経過時間:" + m_dropExecutor.GetElapsedTime(), style);
    }

    /// <summary>
    /// 落下速度の計算をする。
    /// </summary>
    /// <param name="totalClearedLines">消したラインの合計。</param>
    private void CalculateDropSpeed()
    {
        int tempLevel = m_statusManager.GetLevel() - 1;
        m_dropSpeed = Mathf.Pow(0.8f - tempLevel * 0.007f, tempLevel);
    }

    /// <summary>
    /// 待ちミノとホールドミノを表示するモニターを生成する。
    /// </summary>
    private void GenerateMonitors()
    {
        foreach (var monitor in m_nextMonitors)
        {
            monitor.GenerateMonitor();
        }

        m_holdMonitor.GenerateMonitor();
    }

    /// <summary>
    /// 次以降のミノをモニターに入れる。
    /// モニターは6個なので、7個以上入れないでね(配列外参照)。
    /// </summary>
    /// <param name="minoType">ミノのタイプの長さ6配列。</param>
    private void SetNextMinoIntoMonitor(MinoType[] minoType)
    {
        for (int i = 0; i < m_nextMonitors.Length; i++)
        {
            m_nextMonitors[i].PutMinoIntoMonitor(minoType[i]);
        }
    }

    /// <summary>
    /// ホールドしたミノをモニターに入れる。
    /// </summary>
    /// <param name="minoType">ミノタイプ。Empty以外を入れてください。</param>
    private void SetHoldMinoIntoMonitor(MinoType minoType)
    {
        m_holdMonitor.PutMinoIntoMonitor(minoType);
    }

    /// <summary>
    /// 新規落下ミノを生成する。
    /// </summary>
    /// <param name="minoType">ミノタイプ。Empty以外を入れてください。</param>
    private void CreateMino(MinoType minoType)
    {
        m_mino.GenerateMino(minoType); // 初期位置にミノを生成する。
        if (m_stage.IsCanPut(m_mino))
        {
            SetNewMino();
        }
        else
        {
            m_mino.UnDrop();
            if (m_stage.IsCanPut(m_mino))
            {
                SetNewMino();
            }
            else
            {
                m_mino.UnDrop();
                if (m_stage.IsCanPut(m_mino))
                {
                    SetNewMino();
                }
                else
                {
                    m_isGameOver = true;
                    m_dropExecutor.Stop();
                    m_lockDownExecutor.Stop();
                    Debug.Log("ミノが出せない");
                    return;
                }
            }
        }

        SetNextMinoIntoMonitor(m_queue.MinoQueueList.ToArray());
    }

    /// <summary>
    /// 次のミノを位置を計算してセットする。
    /// </summary>
    private void SetNewMino()
    {
        m_stage.PutMino(m_mino);
        m_mino.CopyOriginToGhost();
        PutGhost();
    }

    /// <summary>
    /// ミノの操作をする。
    /// </summary>
    private void ControlMino()
    {
        var actionMap = m_playerGameInput.currentActionMap;
        bool isMoved = false;

        if (actionMap["MoveLeft"].WasPressedThisFrame())
        {
            isMoved = ControlMoveLeftMino();
            m_isLastMoveLandSpin = false;
            m_leftRight = LeftRight.Left;
            if (isMoved)
            {
                m_soundManager.PlaySE_mao(0);
            }
        }
        else if (actionMap["MoveRight"].WasPressedThisFrame())
        {

            isMoved = ControlMoveRightMino();
            m_isLastMoveLandSpin = false;
            m_leftRight = LeftRight.Right;
            if (isMoved)
            {
                m_soundManager.PlaySE_mao(0);
            }
        }

        if (m_isMoving)
        {
            if (m_leftRight == LeftRight.Left)
            {
                m_moveExecutor.Initialize(() => { ControlMoveLeftMino(); }, m_minoMoveSpan);
                m_moveExecutor.Play(false);
            }
            else if (m_leftRight == LeftRight.Right)
            {
                m_moveExecutor.Initialize(() => { ControlMoveRightMino(); }, m_minoMoveSpan);
                m_moveExecutor.Play(false);
            }
        }

        if (actionMap["MoveLeft"].WasReleasedThisFrame() || actionMap["MoveRight"].WasReleasedThisFrame())
        {
            m_moveExecutor.Stop();
            m_pushExecutor.Stop();
            m_isMoving = false;
            m_leftRight = LeftRight.noLeftRight;
        }

        if (actionMap["SoftDrop"].WasPressedThisFrame())
        {
            m_dropExecutor.ChangeInterval(m_dropSpeed / m_dropSpeedMag);
            m_dropExecutor.Play(true);
            m_isSoftDrop = true;
        }

        if (actionMap["SoftDrop"].WasReleasedThisFrame())
        {
            m_dropExecutor.ChangeInterval(m_dropSpeed);
            m_dropExecutor.Play(true);
            m_isSoftDrop = false;
        }

        if (actionMap["LeftRot"].WasPressedThisFrame())
        {
            if (MoveMino(m_mino.SuperLeftRot, m_mino.SuperRightRot))
            {
                PutGhost();
                LockDownReset();
                m_soundManager.PlaySE_lab(1);
            }
            m_isLastMoveLandSpin = m_lockDownExecutor.IsPlay();
        }

        if (actionMap["RightRot"].WasPressedThisFrame())
        {
            if (MoveMino(m_mino.SuperRightRot, m_mino.SuperLeftRot))
            {
                PutGhost();
                LockDownReset();
                m_soundManager.PlaySE_lab(1);
            }
            m_isLastMoveLandSpin = m_lockDownExecutor.IsPlay();
        }

        if (actionMap["HardDrop"].WasPressedThisFrame())
        {
            m_isHardDrop = true;
            while (MoveMino(m_mino.Drop, m_mino.UnDrop))
            {
                m_statusManager.AddDropScore(true);
            }

            var isDrownX = new bool[m_mino.Size];

            for (int x = 0; x < m_mino.Size; x++)
            {
                for (int y = 0; y < m_mino.Size; y++)
                {
                    if (m_mino.Shape[x, y] && !isDrownX[x])
                    {
                        m_effectManager.PlayHardDropEffect(m_stage.GetBlockVector(m_mino.PosX + x, m_mino.PosY + y));
                        isDrownX[x] = true;
                    }
                }
            }
        }

        if (actionMap["Hold"].WasPressedThisFrame())
        {
            HoldMino();
        }

        if (actionMap["Pause"].WasPressedThisFrame())
        {
            m_isPauseMenu = true;
            Debug.Log("ぽーず！");
            m_playerMenuInput.currentActionMap = m_playerMenuInput.actions.actionMaps[1];
        }
    }

    private void PreControlMino()
    {
        var actionMap = m_playerGameInput.currentActionMap;
        if (actionMap["MoveLeft"].WasPressedThisFrame())
        {
            m_pushExecutor.Initialize(() => { m_isMoving = true; m_pushExecutor.Stop(); }, m_buttonPressedJudgeSpan);
            m_pushExecutor.Play(false);
            m_leftRight = LeftRight.Left;
        }
        else if(actionMap["MoveRight"].WasPressedThisFrame())
        {
            m_pushExecutor.Initialize(() => { m_isMoving = true; m_pushExecutor.Stop(); }, m_buttonPressedJudgeSpan);
            m_pushExecutor.Play(false);
            m_leftRight = LeftRight.Right;
        }
        if (actionMap["MoveLeft"].WasReleasedThisFrame() || actionMap["MoveRight"].WasReleasedThisFrame())
        {
            m_moveExecutor.Stop();
            m_pushExecutor.Stop();
            m_isMoving = false;
            m_leftRight = LeftRight.noLeftRight;
        }
    }

    private bool ControlMoveLeftMino()
    {
        bool retIsMoved = MoveMino(m_mino.GoToLeft, m_mino.GoToRight);
        if (retIsMoved)
        {
            PutGhost();
            LockDownReset();
            m_soundManager.PlaySE_mao(0);
        }
        return retIsMoved;
    }
    private bool ControlMoveRightMino()
    {
        bool retIsMoved = MoveMino(m_mino.GoToRight, m_mino.GoToLeft);
        if (retIsMoved)
        {
            PutGhost();
            LockDownReset();
            m_soundManager.PlaySE_mao(0);
        }
        return retIsMoved;
    }

    private void ControlMenu()
    {
        m_isPauseMenu = false;
        Debug.Log("ポーズ解除");
        m_playerMenuInput.currentActionMap = m_playerMenuInput.actions.actionMaps[0];
    }

    /// <summary>
    /// ミノの移動を行う。Doingを行い、それが不可能ならUndoingを行い元の位置に設置する。
    /// </summary>
    /// <param name="Doing">移動処理。左右、下、回転。</param>
    /// <param name="Undoing">逆移動処理。右左、上、逆回転</param>
    /// <param name="isLanding">移動処理の後に設置判定を行うかのフラグ。trueの場合ライン消しのミノ生成を行う。</param>
    /// <param name="isGhost">移動するミノがゴーストかどうかのフラグ。</param>
    /// <returns></returns>
    private bool MoveMino(Action Doing, Action Undoing, bool isGhost = false)
    {
        bool result = false;

        m_stage.PutMino(m_mino, true, isGhost);
        Doing();
        if (m_stage.IsCanPut(m_mino, isGhost))
        {
            result = true;
        }
        else
        {
            Undoing();
        }

        m_mino.Drop();

        if (!m_stage.IsCanPut(m_mino))
        {
            m_dropExecutor.Stop();
            m_lockDownExecutor.Initialize(Landing, m_minoLockdownTime);
            m_lockDownExecutor.Play(m_isHardDrop);
            m_isHardDrop = false;
        }
        else
        {
            m_dropExecutor.Play(false);
            m_lockDownExecutor.Stop();
        }

        m_mino.UnDrop();

        m_stage.PutMino(m_mino, false, isGhost);

        return result;
    }

    /// <summary>
    /// ロックダウンのカウンター処理。
    /// </summary>
    private void LockDownReset()
    {
        if (!m_lockDownExecutor.IsPlay()) // 空中での左右移動や回転でカウントが増えない処理
        {
            return;
        }
        Debug.Log("IsMaxPosRenew" + m_mino.IsMaxPosRenew);

        if (m_mino.IsMaxPosRenew)
        {
            m_lockDownCount = 0;
            Debug.Log("Max Y座標更新");
            m_lockDownExecutor.ResetElapsedTime(); // ここ関連にロックダウンのバグの可能性アリ
            m_mino.IsMaxPosRenew = false;
            return;
        }

        if(m_lockDownCount < m_lockDownMax)
        {
            m_lockDownExecutor.ResetElapsedTime();
            m_lockDownCount++;
            Debug.Log("ロックダウンのカウントの数" + m_lockDownCount);
            Debug.Log("ロックダウン経過時間" + m_lockDownExecutor.GetElapsedTime());
        }
        else
        {
            Landing();
            m_lockDownExecutor.Stop();
            m_lockDownCount = 0;
            Debug.Log("緊急事態宣言");
        }
    }

    /// <summary>
    /// ロックダウンしたときの処理。
    /// </summary>
    private void Landing()
    {
        while (MoveMino(m_mino.Drop, m_mino.UnDrop));

        TSpinType tempTSpinType = TSpinType.NoTSpin;
        if (m_isLastMoveLandSpin)
        {
            tempTSpinType = m_stage.CheckTSpin(m_mino);
        }

        m_currentClearedLines = m_stage.ClearLinesCheck();
        int tempClearedLineNum = m_currentClearedLines.Count;
        foreach (int y in m_currentClearedLines)
        {
            m_effectManager.PlayClearLineEffect(m_stage.GetBlockVector(3, y));
            m_stage.ClearLines(y);
        }

        m_stage.PrintBlockStatus();

        m_statusManager.AddClearScore(tempClearedLineNum, tempTSpinType);
        m_statusManager.PrintScoreBonus(tempClearedLineNum,tempTSpinType);
        m_statusManager.PrintBackToBack();

        m_isHoldLock = false;

        m_lockDownCount = 0;
        m_lockDownExecutor.Stop();

        m_isClearEffect = tempClearedLineNum > 0;
        Debug.Log("m_isClearEffect:" + m_isClearEffect);

        m_statusManager.PrintPerfectClear(m_stage.CheckPerfectClear());
        if (tempTSpinType != TSpinType.NoTSpin)
        {
            m_soundManager.PlaySE_lab(3);
        }
        else
        {
            m_soundManager.PlaySE_lab(2);
        }

        m_statusManager.PrintRen();

        if(m_currentClearedLines.Count != 0)
        {
            m_soundManager.PlaySE_lab(4);
        }

        m_holdMonitor.DyeMinoGray(true);

        if (!m_isClearEffect)
        {
            CreateMino(m_queue.GetNextMino());
        }

        if (!m_isGameOver)
        {
            m_dropExecutor.Play(false);
        }

    }

    /// <summary>
    /// ゴーストの設置。
    /// </summary>
    private void PutGhost()
    {
        m_stage.PutMino(m_mino, true,true);
        m_mino.CopyOriginToGhost();
        while (MoveMino(m_mino.GhostDrop, m_mino.GhostUnDrop, true));
        m_stage.PutMino(m_mino);
    }

    /// <summary>
    /// ミノのホールドをする。
    /// </summary>
    private void HoldMino()
    {
        if (m_isHoldLock)
        {
            return;
        }

        var blockType = m_mino.Type;
        m_stage.PutMino(m_mino, true);
        m_stage.PutMino(m_mino, true, true);
        SetHoldMinoIntoMonitor(blockType);
        if (m_holdMino == MinoType.Empty)
        {
            CreateMino(m_queue.GetNextMino());
            PutGhost();
        }
        else
        {
            CreateMino(m_holdMino);
            PutGhost();
        }

        m_holdMonitor.DyeMinoGray();

        m_holdMino = blockType;
        m_isHoldLock = true;
        m_lockDownExecutor.ResetElapsedTime();
        m_soundManager.PlaySE_lab(5);
    }
}

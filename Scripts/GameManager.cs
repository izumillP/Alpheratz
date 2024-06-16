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


    // �Q�[���X�e�[�g�֘A
    private bool m_isFirstUpdate = true;
    private bool m_isGameOver = false;
    private bool m_isPauseMenu = false;

    [SerializeField] private bool m_isDebug = false;

    // �V�X�e���^�C�}�[�֘A(�Œ�)
    [SerializeField] private float m_baseDropSpeed = 1f; // ���������̊�{���x
    [SerializeField] private float m_buttonPressedJudgeSpan = 0.3f; // ����������̎���
    [SerializeField] private float m_minoMoveSpan = 0.05f; // �u���b�N�ړ��̊Ԋu
    [SerializeField] private float m_minoLockdownTime = 0.5f; // ���b�N�_�E���̗P�\����
    [SerializeField] private float m_dropSpeedMag = 20f; // �\�t�g�h���b�v�̗������x�W��

    // �X�R�A(���x��)�֘A
    [SerializeField] private StatusManager m_statusManager = null;
    private float m_dropSpeed = 1f; // ���������̑��x�Ǘ��ϐ�
    private bool m_isLastMoveLandSpin = false;

    // �X�e�[�W�����C���X�^���X
    [SerializeField] private Stage m_stage = null;
    [SerializeField] private Monitor[] m_nextMonitors = null;
    [SerializeField] private Monitor m_holdMonitor = null;

    // �~�m�����֘A
    private Mino m_mino = new(); // �������̃~�m�̃C���X�^���X
    private MinoQueue m_queue = new();

    // ���ߏ��������C��
    private List<int> m_currentClearedLines = new();

    // �z�[���h�֘A
    private bool m_isHoldLock = false;
    private MinoType m_holdMino = MinoType.Empty;

    // �^�C�}�[�֘A
    private TimeExecutor m_dropExecutor = null;
    private TimeExecutor m_moveExecutor = null;
    private TimeExecutor m_pushExecutor = null;
    private bool m_isSoftDrop = false; // �\�t�g�h���b�v���̓^�C�}�[��ʘg�ɂ��邽�߂̃t���O
    private bool m_isMoving = false;
    private LeftRight m_leftRight = LeftRight.noLeftRight;

    // ���b�N�_�E���֘A
    private TimeExecutor m_lockDownExecutor = null;
    [SerializeField] private int m_lockDownMax = 14;
    private bool m_isHardDrop = false; // �n�[�h�h���b�v���̓^�C�}�[��ʘg�ɂ��邽�߂̃t���O
    private int m_lockDownCount = 0; // ���b�N�_�E���̃��Z�b�g��

    // �C���v�b�g�֘A
    private PlayerInput m_playerGameInput = null;
    private PlayerInput m_playerMenuInput = null;

    // �G�t�F�N�g�֘A
    [SerializeField] private EffectManager m_effectManager = null;
    private TimeExecutor m_clearEffectExecutor = null;
    private bool m_isClearEffect = false;

    // ������؂�
    [SerializeField] private int m_numberSection = 4;

    // �T�E���h�֘A
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
            // Debug.Log("�����܂��I�@" + m_statusManager.GetTotalClearedLines() + "���C�������܂����B�X�R�A��" + m_statusManager.GetScore() + "�ł��B�܂��V��łˁI");
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

        if (m_isFirstUpdate) // ���݂̏����ł�1�b�ڂ��󔒂ɂȂ邽�ߕʓr1�b�ڐ�p�̏����𑖂点��B
        {
            var instance = DebugStageGeneratorParameter.instance;

            // �����~�m����
            CreateMino(m_queue.GetNextMino());
            // ���j�^�[���\���폜
            m_holdMonitor.EraseMonitor();
            SetHoldMinoIntoMonitor(instance.minoType);
            m_holdMino = instance.minoType;

            // �~�m���~�点��B
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
        GUI.Label(new Rect(10, 10, 200, 30), "���b�N�_�E���J�E���g:" + m_lockDownCount, style);
        GUI.Label(new Rect(10, 50, 200, 30), "���b�N�_�E���o�ߎ���:" + m_lockDownExecutor.GetElapsedTime(), style);
        GUI.Label(new Rect(10, 90, 200, 30), "�h���b�v�o�ߎ���:" + m_dropExecutor.GetElapsedTime(), style);
    }

    /// <summary>
    /// �������x�̌v�Z������B
    /// </summary>
    /// <param name="totalClearedLines">���������C���̍��v�B</param>
    private void CalculateDropSpeed()
    {
        int tempLevel = m_statusManager.GetLevel() - 1;
        m_dropSpeed = Mathf.Pow(0.8f - tempLevel * 0.007f, tempLevel);
    }

    /// <summary>
    /// �҂��~�m�ƃz�[���h�~�m��\�����郂�j�^�[�𐶐�����B
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
    /// ���ȍ~�̃~�m�����j�^�[�ɓ����B
    /// ���j�^�[��6�Ȃ̂ŁA7�ȏ����Ȃ��ł�(�z��O�Q��)�B
    /// </summary>
    /// <param name="minoType">�~�m�̃^�C�v�̒���6�z��B</param>
    private void SetNextMinoIntoMonitor(MinoType[] minoType)
    {
        for (int i = 0; i < m_nextMonitors.Length; i++)
        {
            m_nextMonitors[i].PutMinoIntoMonitor(minoType[i]);
        }
    }

    /// <summary>
    /// �z�[���h�����~�m�����j�^�[�ɓ����B
    /// </summary>
    /// <param name="minoType">�~�m�^�C�v�BEmpty�ȊO�����Ă��������B</param>
    private void SetHoldMinoIntoMonitor(MinoType minoType)
    {
        m_holdMonitor.PutMinoIntoMonitor(minoType);
    }

    /// <summary>
    /// �V�K�����~�m�𐶐�����B
    /// </summary>
    /// <param name="minoType">�~�m�^�C�v�BEmpty�ȊO�����Ă��������B</param>
    private void CreateMino(MinoType minoType)
    {
        m_mino.GenerateMino(minoType); // �����ʒu�Ƀ~�m�𐶐�����B
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
                    Debug.Log("�~�m���o���Ȃ�");
                    return;
                }
            }
        }

        SetNextMinoIntoMonitor(m_queue.MinoQueueList.ToArray());
    }

    /// <summary>
    /// ���̃~�m���ʒu���v�Z���ăZ�b�g����B
    /// </summary>
    private void SetNewMino()
    {
        m_stage.PutMino(m_mino);
        m_mino.CopyOriginToGhost();
        PutGhost();
    }

    /// <summary>
    /// �~�m�̑��������B
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
            Debug.Log("�ہ[���I");
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
        Debug.Log("�|�[�Y����");
        m_playerMenuInput.currentActionMap = m_playerMenuInput.actions.actionMaps[0];
    }

    /// <summary>
    /// �~�m�̈ړ����s���BDoing���s���A���ꂪ�s�\�Ȃ�Undoing���s�����̈ʒu�ɐݒu����B
    /// </summary>
    /// <param name="Doing">�ړ������B���E�A���A��]�B</param>
    /// <param name="Undoing">�t�ړ������B�E���A��A�t��]</param>
    /// <param name="isLanding">�ړ������̌�ɐݒu������s�����̃t���O�Btrue�̏ꍇ���C�������̃~�m�������s���B</param>
    /// <param name="isGhost">�ړ�����~�m���S�[�X�g���ǂ����̃t���O�B</param>
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
    /// ���b�N�_�E���̃J�E���^�[�����B
    /// </summary>
    private void LockDownReset()
    {
        if (!m_lockDownExecutor.IsPlay()) // �󒆂ł̍��E�ړ����]�ŃJ�E���g�������Ȃ�����
        {
            return;
        }
        Debug.Log("IsMaxPosRenew" + m_mino.IsMaxPosRenew);

        if (m_mino.IsMaxPosRenew)
        {
            m_lockDownCount = 0;
            Debug.Log("Max Y���W�X�V");
            m_lockDownExecutor.ResetElapsedTime(); // �����֘A�Ƀ��b�N�_�E���̃o�O�̉\���A��
            m_mino.IsMaxPosRenew = false;
            return;
        }

        if(m_lockDownCount < m_lockDownMax)
        {
            m_lockDownExecutor.ResetElapsedTime();
            m_lockDownCount++;
            Debug.Log("���b�N�_�E���̃J�E���g�̐�" + m_lockDownCount);
            Debug.Log("���b�N�_�E���o�ߎ���" + m_lockDownExecutor.GetElapsedTime());
        }
        else
        {
            Landing();
            m_lockDownExecutor.Stop();
            m_lockDownCount = 0;
            Debug.Log("�ً}���Ԑ錾");
        }
    }

    /// <summary>
    /// ���b�N�_�E�������Ƃ��̏����B
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
    /// �S�[�X�g�̐ݒu�B
    /// </summary>
    private void PutGhost()
    {
        m_stage.PutMino(m_mino, true,true);
        m_mino.CopyOriginToGhost();
        while (MoveMino(m_mino.GhostDrop, m_mino.GhostUnDrop, true));
        m_stage.PutMino(m_mino);
    }

    /// <summary>
    /// �~�m�̃z�[���h������B
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

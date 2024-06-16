using System;
using UnityEngine;
using TMPro;
public enum TSpinType
{
    NoTSpin,
    MiniTSpin,
    NormalTSpin,
    TSpinTypeMax
}
public class StatusManager : MonoBehaviour
{
    [SerializeField] private int m_initialLevel = 1;
    [SerializeField] private int m_levelMax = 15;
    [SerializeField] private TextMeshProUGUI m_clearedLineText = null;
    [SerializeField] private TextMeshProUGUI m_levelText = null;
    [SerializeField] private TextMeshProUGUI m_scoreText = null;
    [SerializeField] private TextMeshProUGUI m_timeText = null;
    [SerializeField] private TextMeshProUGUI m_scoreBonus = null;
    [SerializeField] private TextMeshProUGUI m_backToBack = null;
    [SerializeField] private TextMeshProUGUI m_perfect = null;
    [SerializeField] private TextMeshProUGUI m_ren = null;

    private static int m_levelUpLineNum = 10;
    private int m_level = 0;
    private int m_score = 0;
    private int m_totalClearedLines = 0;
    private float m_time = 0f;

    private bool m_isTimerStopped = true;

    private bool m_isBackToBack = false;
    private bool m_isBackToBackPrint = false;
    private int Ren { get; set; } = 0;

    private void Update()
    {
        if (!m_isTimerStopped)
        {
            m_time += Time.deltaTime;
        }
    }

    public void TimerStart()
    {
        m_isTimerStopped = false;
    }

    public void TimerStop()
    {
        m_isTimerStopped = true;
        m_time = 0f;
    }

    public void TimerPause()
    {
        m_isTimerStopped = true;
    }

    private void ResetClearedLines()
    {
        m_totalClearedLines = 0;
    }

    private void ResetLevel()
    {
        m_level = m_initialLevel;
    }
    private void ResetScore()
    {
        m_score = 0;
    }
    private void ResetTime()
    {
        m_time = 0f;
    }
    private void ResetScoreBonus()
    {
        m_scoreBonus.text = "";
    }
    private void ResetBackToBack()
    {
        m_backToBack.text = "";
    }
    private void ResetPerfect()
    {
        m_perfect.text = "";
    }
    private void ResetRen()
    {
        m_ren.text = "";
    }
    public void ResetStatus()
    {
        ResetClearedLines();
        ResetLevel();
        ResetScore();
        ResetTime();
        ResetScoreBonus();
        ResetBackToBack();
        ResetPerfect();
        ResetRen();

        PrintStatus();
    }

    private void CalcLevel(bool noLevelLimit = false)
    {
        int tempLevel = m_initialLevel + m_totalClearedLines / m_levelUpLineNum;
        m_level = noLevelLimit ? tempLevel : Math.Min(m_levelMax, tempLevel);
    }
    public void PrintStatus()
    {
        m_clearedLineText.text = string.Format("{0:#,0}", m_totalClearedLines);
        m_levelText.text = string.Format("{0:#,0}", m_level);
        m_scoreText.text = string.Format("{0:#,0}", m_score);
    }

    public void PrintTime()
    {
        m_timeText.text = TimeSpan.FromSeconds(m_time).ToString(@"mm\:ss\.ff");
    }
    public void AddDropScore(bool isHardDrop = false)
    {
        const int hardDropScore = 2;
        const int softDropScore = 1;
        m_score += isHardDrop ? hardDropScore : softDropScore;
        PrintStatus();
    }

    public void PrintScoreBonus(int clearedLine = 0, TSpinType tSpinType = TSpinType.NoTSpin)
    {
        var bonusText = "";
        bool isBonus;

        if (clearedLine == 4)
        {
            bonusText = "TETRIS";
            isBonus = true;
        }
        else
        {
            switch (tSpinType)
            {
                case TSpinType.NoTSpin:
                    bonusText = "";
                    break;
                case TSpinType.MiniTSpin:
                    bonusText = "miniTSPIN";
                    break;
                case TSpinType.NormalTSpin:
                    bonusText = "TSPIN";
                    break;
                default:
                    bonusText = "What's!?";
                    break;
            }
            isBonus = true;
        }
        if (isBonus)
        {
            // ウィンドウを消す処理
            m_scoreBonus.text = bonusText;
        }
    }

    public void PrintBackToBack()
    {
        if (m_isBackToBack && m_isBackToBackPrint)
        {
            m_backToBack.text = "Back\nTo Back";
            m_isBackToBackPrint = false;
        }
        else
        {
            m_backToBack.text = "";
        }
    }

    public void PrintRen()
    {
        if(Ren <= 1)
        {
            ResetRen();
            return;
        }
        m_ren.text = string.Format("{0:#,0}", (Ren - 1) + " REN");
    }

    public void PrintPerfectClear(bool isPerefect)
    {
        if (isPerefect)
        {
            m_perfect.text = "PERFECT!";
        }
        else
        {
            m_perfect.text = "";
        }
    }

    public void AddClearScore(int currentClearedLines, TSpinType tSpinType = TSpinType.NoTSpin)
    {
        if (currentClearedLines > 4)
        {
            Debug.Log("消したラインが4超過なんてびっくりだよね。");
            return;
        }

        int clearScore;

        switch (tSpinType)
        {
            case TSpinType.NoTSpin:
                clearScore = (200 * currentClearedLines - (currentClearedLines == 0 || currentClearedLines == 4 ? 0 : 100)) * m_level;
                break;
            case TSpinType.MiniTSpin:
                clearScore = (currentClearedLines + 1) * 100 * m_level;
                break;
            case TSpinType.NormalTSpin:
                clearScore = (currentClearedLines + 1) * 400 * m_level;
                break;
            default:
                clearScore = 0;
                break;
        }

        if (currentClearedLines >=1 && currentClearedLines <= 3 && tSpinType == TSpinType.NoTSpin)
        {
            m_isBackToBack = false;
            m_isBackToBackPrint = false;
        }

        if (m_isBackToBack && currentClearedLines != 0)
        {
            // Debug.Log("Back To Back Bonus" + (clearScore / 2));
            clearScore += clearScore / 2;
            m_isBackToBackPrint = true;
        }

        if(currentClearedLines == 4 || (tSpinType >= TSpinType.MiniTSpin && currentClearedLines != 0))
        {
            m_isBackToBack = true;
        }

        if (currentClearedLines != 0)
        {
            Ren++;
        }
        else
        {
            Ren = 0;
        }

        m_score += clearScore;
        Debug.Log("スコア加算:" + clearScore);

        m_totalClearedLines += currentClearedLines;
        CalcLevel();

        PrintStatus();
    }

    public int GetTotalClearedLines()
    {
        return m_totalClearedLines;
    }
    public int GetLevel()
    {
        return m_level;
    }
    public float GetScore()
    {
        return m_score;
    }

}

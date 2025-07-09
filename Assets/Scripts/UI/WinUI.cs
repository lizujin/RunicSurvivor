// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 胜利UI界面控制器，当玩家完成所有波次后显示
/// </summary>
public class WinUI : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject winPanel;                         // 胜利面板
    public Text victoryText;                            // 胜利标题
    public Text statsText;                              // 统计信息文本
    public Text waveText;                               // 波次文本
    public Text timeText;                               // 时间文本
    public Button restartButton;                        // 再来一次按钮
    public Button quitButton;                           // 退出按钮
    
    private void Awake()
    {
        // 设置按钮监听
        SetupButtons();
    }
    
    private void OnEnable()
    {
        // 确保每次显示面板时都重新设置按钮
        SetupButtons();
    }
    
    // 设置按钮监听器
    private void SetupButtons()
    {
        if (restartButton != null)
        {
            // 移除所有现有监听器以避免重复
            restartButton.onClick.RemoveAllListeners();
            // 添加监听器
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (quitButton != null)
        {
            // 移除所有现有监听器以避免重复
            quitButton.onClick.RemoveAllListeners();
            // 添加监听器
            quitButton.onClick.AddListener(QuitGame);
        }
        
        Debug.Log("WinUI: 按钮监听器已设置");
    }
    
    private void OnDestroy()
    {
        // 移除按钮监听
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }
    }
    
    /// <summary>
    /// 显示胜利面板
    /// </summary>
    public void ShowWinPanel()
    {
        // 激活胜利面板
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        
        // 更新统计信息
        UpdateStats();
        
        // 播放胜利音效
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.enabled)
        {
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// 更新统计信息
    /// </summary>
    private void UpdateStats()
    {
        // 获取GameManager引用
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;
        
        // 更新击杀数
        if (statsText != null)
        {
            int totalKills = gameManager.GetTotalEnemiesKilled();
            statsText.text = $"总击杀数: {totalKills}";
        }
        
        // 更新波次
        if (waveText != null)
        {
            int currentWave = gameManager.CurrentWave;
            int totalWaves = gameManager.waveCount;
            waveText.text = $"通过波次: {currentWave}/{totalWaves}";
        }
        
        // 更新游戏时间
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60);
            int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad % 60);
            timeText.text = $"游戏时间: {minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    private void RestartGame()
    {
        // 重新加载当前场景
        Time.timeScale = 1f; // 确保时间正常流动
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    private void QuitGame()
    {
        #if UNITY_EDITOR
        // 在编辑器中停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 在实际发布版本中退出游戏
        Application.Quit();
        #endif
    }
}

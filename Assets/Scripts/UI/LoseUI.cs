// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏失败UI管理器，处理游戏失败时的UI显示和重新开始游戏的逻辑
/// </summary>
public class LoseUI : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject losePanel;            // 失败面板
    public Text gameOverText;               // 失败文本
    public Text scoreText;                  // 分数文本
    public Text waveText;                   // 波次文本
    public Text survivalTimeText;           // 生存时间文本
    
    [Header("按钮")]
    public Button restartButton;            // 重新开始按钮
    public Button quitButton;               // 退出按钮
    
    [Header("动画设置")]
    public float fadeInTime = 1.0f;         // 淡入时间
    public bool useAnimation = true;        // 是否使用动画
    
    // 内部变量
    private CanvasGroup canvasGroup;
    private GameManager gameManager;
    private float gameStartTime;
    
    private void Awake()
    {
        // 获取组件
        canvasGroup = losePanel?.GetComponent<CanvasGroup>();
        if (canvasGroup == null && losePanel != null)
        {
            canvasGroup = losePanel.AddComponent<CanvasGroup>();
        }
        
        // 记录游戏开始时间
        gameStartTime = Time.time;
        
        // 初始化时隐藏失败面板
        HideLosePanel();
    }
    
    private void Start()
    {
        // 获取GameManager引用
        gameManager = GameManager.Instance;
        
        // 注册按钮事件
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        // 注册游戏结束事件
        if (gameManager != null)
        {
            gameManager.OnGameOver += HandleGameOver;
        }
        else
        {
            Debug.LogError("LoseUI无法找到GameManager实例");
        }
    }
    
    private void OnDestroy()
    {
        // 取消事件注册
        if (gameManager != null)
        {
            gameManager.OnGameOver -= HandleGameOver;
        }
        
        // 取消按钮事件
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
    /// 处理游戏结束事件
    /// </summary>
    private void HandleGameOver(bool isVictory)
    {
        // 只处理失败情况
        if (!isVictory)
        {
            ShowLosePanel();
            
            // 更新UI显示的游戏数据
            UpdateGameStats();
        }
    }
    
    /// <summary>
    /// 更新游戏统计信息
    /// </summary>
    private void UpdateGameStats()
    {
        if (gameManager == null) return;
        
        // 获取游戏数据
        int totalKills = 0;
        if (gameManager != null)
        {
            // 确保安全调用
            totalKills = gameManager.GetTotalEnemiesKilled();
        }
        int wave = gameManager.CurrentWave;
        float survivalTime = Time.time - gameStartTime; // 使用时间差计算生存时间
        
        // 更新UI
        if (scoreText != null)
        {
            scoreText.text = $"击杀数: {totalKills}";
        }
        
        if (waveText != null)
        {
            waveText.text = $"波次: {wave}/{gameManager.waveCount}";
        }
        
        if (survivalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60);
            int seconds = Mathf.FloorToInt(survivalTime % 60);
            survivalTimeText.text = $"生存时间: {minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// 显示失败面板
    /// </summary>
    public void ShowLosePanel()
    {
        if (losePanel == null) return;
        
        // 显示面板
        losePanel.SetActive(true);
        
        // 使用动画淡入
        if (useAnimation && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            // 立即显示
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
            }
        }
        
        // 暂停游戏
        Time.timeScale = 0;
    }
    
    /// <summary>
    /// 隐藏失败面板
    /// </summary>
    public void HideLosePanel()
    {
        if (losePanel == null) return;
        
        // 隐藏面板
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
        
        losePanel.SetActive(false);
        
        // 恢复游戏时间
        Time.timeScale = 1;
    }
    
    /// <summary>
    /// 淡入动画协程
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0;
        canvasGroup.alpha = 0;
        
        while (elapsed < fadeInTime)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeInTime);
            yield return null;
        }
        
        canvasGroup.alpha = 1;
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 恢复游戏时间
        Time.timeScale = 1;
        
        // 使用GameManager的重新开始方法
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            // 回退方案: 直接重载当前场景
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        // 在编辑器中
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        // 在独立应用中
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 手动触发游戏失败（用于测试）
    /// </summary>
    public void TriggerGameLoss()
    {
        if (gameManager != null)
        {
            gameManager.EndGame(false);
        }
        else
        {
            ShowLosePanel();
        }
    }
}

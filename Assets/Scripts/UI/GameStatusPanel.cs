// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏状态面板控制器，负责更新游戏状态UI元素
/// </summary>
public class GameStatusPanel : MonoBehaviour
{
    [Header("状态UI组件")]
    public Text waveText;                // 波次文本
    public Text timeText;                // 时间文本
    public Text goldText;                // 金币文本
    public Text levelText;               // 等级文本
    public Slider expBar;                // 经验条
    public Text expValue;               // 经验值文本
    public Slider healthBar;             // 生命条
    public Text healthValue;            // 生命值文本
    
    [Header("更新设置")]
    public float updateInterval = 0.1f;   // 更新间隔（秒）
    public bool debugMode = false;        // 调试模式
    
    // 引用
    private GameManager gameManager;
    private PlayerController playerController;
    
    // 内部变量
    private float timeSinceUpdate = 0f;
    private float gameTime = 0f;
    
    private void Awake()
    {
        // 自动查找组件（如果未手动指定）
        InitializeComponents();
    }
    
    private void Start()
    {
        // 获取管理器引用
        gameManager = GameManager.Instance;
        playerController = FindObjectOfType<PlayerController>();
        
        // 设置初始值
        UpdateAllUI();
        
        // 添加事件监听
        if (playerController != null)
        {
            playerController.OnPlayerDamage += HandlePlayerDamage;
        }
        
        if (gameManager != null)
        {
            gameManager.OnGoldChanged += HandleGoldChanged;
            gameManager.OnPlayerLevelUp += HandlePlayerLevelUp;
            gameManager.OnWaveStart += HandleWaveStart;
        }
    }
    
    private void OnDestroy()
    {
        // 移除事件监听
        if (playerController != null)
        {
            playerController.OnPlayerDamage -= HandlePlayerDamage;
        }
        
        if (gameManager != null)
        {
            gameManager.OnGoldChanged -= HandleGoldChanged;
            gameManager.OnPlayerLevelUp -= HandlePlayerLevelUp;
            gameManager.OnWaveStart -= HandleWaveStart;
        }
    }
    
    /// <summary>
    /// 初始化组件（如果未手动指定）
    /// </summary>
    private void InitializeComponents()
    {
        if (waveText == null)
            waveText = transform.Find("WaveText")?.GetComponent<Text>();
            
        if (timeText == null)
            timeText = transform.Find("TimeText")?.GetComponent<Text>();
            
        if (goldText == null)
            goldText = transform.Find("GoldText")?.GetComponent<Text>();
            
        if (levelText == null)
            levelText = transform.Find("LevelText")?.GetComponent<Text>();
            
        if (expBar == null)
            expBar = transform.Find("ExpBar")?.GetComponent<Slider>();
            
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.GetComponent<Slider>();
        
        // 日志输出已找到的组件，方便调试
        if (debugMode)
        {
            Debug.Log($"状态面板组件初始化: 波次文本 {(waveText != null ? "已找到" : "未找到")}");
            Debug.Log($"状态面板组件初始化: 时间文本 {(timeText != null ? "已找到" : "未找到")}");
            Debug.Log($"状态面板组件初始化: 金币文本 {(goldText != null ? "已找到" : "未找到")}");
            Debug.Log($"状态面板组件初始化: 等级文本 {(levelText != null ? "已找到" : "未找到")}");
            Debug.Log($"状态面板组件初始化: 经验条 {(expBar != null ? "已找到" : "未找到")}");
            Debug.Log($"状态面板组件初始化: 生命条 {(healthBar != null ? "已找到" : "未找到")}");
        }
    }
    
    private void Update()
    {
        // 累计时间
        timeSinceUpdate += Time.deltaTime;
        gameTime += Time.deltaTime;
        
        // 按指定间隔更新UI（避免每帧更新）
        if (timeSinceUpdate >= updateInterval)
        {
            UpdateAllUI();
            timeSinceUpdate = 0f;
        }
        
        // 更新时间UI（这个可以每帧更新）
        UpdateTimeUI();
    }
    
    /// <summary>
    /// 更新所有UI元素
    /// </summary>
    public void UpdateAllUI()
    {
        UpdateWaveUI();
        UpdateGoldUI();
        UpdateLevelUI();
        UpdateExpUI();
        UpdateHealthUI();
        // 时间UI会在Update方法中每帧更新
    }
    
    /// <summary>
    /// 更新波次UI
    /// </summary>
    public void UpdateWaveUI()
    {
        if (waveText != null && gameManager != null)
        {
            waveText.text = $"第 {gameManager.CurrentWave}/{gameManager.waveCount} 波";
            if (debugMode) Debug.Log($"更新波次UI: {waveText.text}");
        }
    }
    
    /// <summary>
    /// 更新时间UI
    /// </summary>
    public void UpdateTimeUI()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    /// <summary>
    /// 更新金币UI
    /// </summary>
    public void UpdateGoldUI()
    {
        if (goldText != null && gameManager != null)
        {
            goldText.text = $"金币: {gameManager.GetGold()}";
            if (debugMode) Debug.Log($"更新金币UI: {goldText.text}");
        }
    }
    
    /// <summary>
    /// 更新等级UI
    /// </summary>
    public void UpdateLevelUI()
    {
        if (levelText != null && gameManager != null)
        {
            levelText.text = $"等级: {gameManager.GetLevel()}";
            if (debugMode) Debug.Log($"更新等级UI: {levelText.text}");
        }
    }
    
    /// <summary>
    /// 更新经验条UI
    /// </summary>
    public void UpdateExpUI()
    {
        if (expBar != null && gameManager != null)
        {
            float progress = gameManager.GetExpProgress();
            expBar.value = progress;
            
            // 更新经验值文本 (当前经验/升级所需经验)
            if (expValue != null)
            {
                int currentExp = gameManager.GetExp();
                int expForNextLevel = gameManager.GetExpForNextLevel();
                expValue.text = $"{currentExp}/{expForNextLevel}";
            }
            
            if (debugMode)
            {
                Debug.Log($"更新经验条: {progress:F2}, 当前经验: {gameManager.GetExp()}, 升级所需: {gameManager.GetExpForNextLevel()}");
            }
        }
    }
    
    /// <summary>
    /// 更新生命条UI
    /// </summary>
    public void UpdateHealthUI()
    {
        if (healthBar != null && playerController != null)
        {
            float healthPercentage = (float)playerController.currentHealth / playerController.maxHealth;
            healthBar.value = healthPercentage;
            
            // 更新生命值文本 (当前生命/最大生命)
            if (healthValue != null)
            {
                int currentHealth = Mathf.RoundToInt(playerController.currentHealth);
                int maxHealth = Mathf.RoundToInt(playerController.maxHealth);
                healthValue.text = $"{currentHealth}/{maxHealth}";
            }
            
            // 确保健康值在正确范围内
            if (healthBar.value < 0) healthBar.value = 0;
            if (healthBar.value > 1) healthBar.value = 1;
            
            if (debugMode)
            {
                Debug.Log($"更新生命条: 当前生命 {playerController.currentHealth}, 最大生命 {playerController.maxHealth}, 百分比: {healthPercentage:F2}");
            }
        }
    }
    
    #region 事件处理
    
    /// <summary>
    /// 处理玩家受伤事件
    /// </summary>
    private void HandlePlayerDamage(float amount, float currentHealth)
    {
        UpdateHealthUI();
    }
    
    /// <summary>
    /// 处理金币变化事件
    /// </summary>
    private void HandleGoldChanged(int currentGold)
    {
        UpdateGoldUI();
    }
    
    /// <summary>
    /// 处理玩家升级事件
    /// </summary>
    private void HandlePlayerLevelUp(int newLevel, int goldReward)
    {
        UpdateLevelUI();
        UpdateExpUI();
        // 金币会通过OnGoldChanged事件更新
    }
    
    /// <summary>
    /// 处理波次开始事件
    /// </summary>
    private void HandleWaveStart(int waveNumber)
    {
        UpdateWaveUI();
    }
    
    #endregion
    
    /// <summary>
    /// 重置游戏时间
    /// </summary>
    public void ResetGameTime()
    {
        gameTime = 0f;
        UpdateTimeUI();
    }
}

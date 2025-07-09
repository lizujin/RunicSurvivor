// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts.Managers;
using SimpleSystem;

/// <summary>
/// 游戏管理器，控制游戏流程和核心游戏状态
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("游戏配置")]
    public GameDifficulty difficulty = GameDifficulty.Normal;
    public int startingGold = 0;
    public int waveCount = 30;
    public float gameSpeed = 1f;
    public float restTimeBetweenWaves = 30f;
    
    [Header("经验系统")]
    public int[] expRequiredForLevel = new int[30]; // 每级所需经验，与每级生成的敌人数量匹配
    public int[] goldRewardPerLevel = new int[30]; // 每级金币奖励，在Awake中初始化
    public int maxLevel = 29;
    public int baseExpRequired = 10; // 基础所需经验值
    public int expIncreasePerLevel = 5; // 每级增加的经验值
    
    [Header("队伍设置")]
    public List<GameObject> teamObjects = new List<GameObject>();
    public int maxTeamSize = 6;
    
    // 游戏状态
    private int currentWave = 0;
    private int currentExp = 0;
    private int currentLevel = 1;
    private int gold = 0;
    private float waveTimer = 0f;
    private bool isPaused = false;
    private bool isGameOver = false;
    private bool isWaveInProgress = false;
    
    // 敌人统计
    private Dictionary<string, int> enemyKills = new Dictionary<string, int>();
    private int totalEnemiesKilled = 0;
    
    // 状态属性
    public int CurrentWave => currentWave;
    public int CurrentLevel => currentLevel;
    public bool IsWaveInProgress => isWaveInProgress;
    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;
    
    // 事件系统
    public delegate void PlayerLevelUpHandler(int newLevel, int goldReward);
    public event PlayerLevelUpHandler OnPlayerLevelUp;
    
    public delegate void GameStateChangedHandler(bool isPaused);
    public event GameStateChangedHandler OnGameStateChanged;
    
    public delegate void WaveStartHandler(int waveNumber);
    public event WaveStartHandler OnWaveStart;
    
    public delegate void WaveEndHandler(int waveNumber, bool success);
    public event WaveEndHandler OnWaveEnd;
    
    public delegate void GameOverHandler(bool victory);
    public event GameOverHandler OnGameOver;
    
    public delegate void GoldChangedHandler(int currentGold);
    public event GoldChangedHandler OnGoldChanged;
    
    // 单例实现
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    private EnemyManager _enemyManager;
    private bool _inited = false;

    private Transform _playerRoot;

    public Transform PlayerRoot => _playerRoot;

    private Transform _enemyRoot;
    public Transform EnemyRoot => _enemyRoot;

    private Transform _effectRoot;
    public Transform EffectRoot => _effectRoot;

    private void Awake()
    {
        _playerRoot = GameObject.Find("PlayerRoot").transform;
        _enemyRoot = GameObject.Find("EnemyRoot").transform;
        _effectRoot = GameObject.Find("EffectRoot").transform;
        // 单例设置
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 初始化升级经验要求 - 波数*5
        for (int i = 0; i < expRequiredForLevel.Length; i++)
        {
            expRequiredForLevel[i] = (i) * 5;
        }
        
        // 初始化金币奖励 - 线性增长从2到6
        for (int i = 0; i < goldRewardPerLevel.Length; i++) 
        {
            // 2 + level * (4/29) = 2 + level * 0.1379...
            goldRewardPerLevel[i] = Mathf.RoundToInt(2f + i * (4f / 29f));
        }
        
        // 初始化游戏状态
        InitGame();
    }
    
    private void Start()
    {
        // 初始化世界
        World.GetInstance().Start();
    }
    
    private void Update()
    {
        World.GetInstance().Update(Time.deltaTime);
        // 游戏暂停或结束时不更新
        if (isPaused || isGameOver) return;
        
        // 调整游戏速度
        Time.timeScale = gameSpeed;
        
        // 更新波次计时器
        if (!isWaveInProgress && currentWave < waveCount)
        {
            waveTimer -= Time.deltaTime;
            
            if (waveTimer <= 0f)
            {
                StartNextWave();
            }
        }
    }
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void InitGame()
    {
        if (_inited)
        {
            return;
        }
        _inited = true;

        // 重置游戏状态
        currentWave = 0;
        currentExp = 0;
        currentLevel = 1;
        gold = startingGold;
        waveTimer = restTimeBetweenWaves;
        isPaused = false;
        isGameOver = false;
        isWaveInProgress = false;
        
        // 初始化世界
        World.GetInstance().Init();
        // 根据难度调整参数
        AdjustDifficulty();
        
        // 通知金币变化
        OnGoldChanged?.Invoke(gold);
    }
    
    /// <summary>
    /// 根据难度调整游戏参数
    /// </summary>
    private void AdjustDifficulty()
    {
        switch (difficulty)
        {
            case GameDifficulty.Easy:
                startingGold = 8;
                restTimeBetweenWaves = 40f;
                break;
            case GameDifficulty.Normal:
                // 保持默认值
                break;
            case GameDifficulty.Hard:
                startingGold = 3;
                restTimeBetweenWaves = 25f;
                break;
            case GameDifficulty.Expert:
                startingGold = 2;
                restTimeBetweenWaves = 20f;
                break;
        }
        
        gold = startingGold;
    }


    public EnemyManager GetEnemyManager()
    {
        if (_enemyManager)
            return _enemyManager;
        // 通知敌人管理器生成敌人
        _enemyManager = FindObjectOfType<EnemyManager>();
        return _enemyManager;
    }

    /// <summary>
    /// 开始下一波
    /// </summary>
    public void StartNextWave()
    {
        if (isWaveInProgress || isGameOver) return;
        
        currentWave++;
        isWaveInProgress = true;
        
        // 触发波次开始事件
        OnWaveStart?.Invoke(currentWave);
        
        // 通知敌人管理器生成敌人
        EnemyManager enemyManager = GetEnemyManager();
        if (enemyManager != null)
        {
            enemyManager.SpawnWave(currentWave);
        }
    }
    
    /// <summary>
    /// 结束当前波次
    /// </summary>
    public void EndWave(bool success)
    {
        if (!isWaveInProgress) return;
        
        isWaveInProgress = false;
        
        // 触发波次结束事件
        OnWaveEnd?.Invoke(currentWave, success);
        
        // 检查游戏是否结束
        if (currentWave >= waveCount)
        {
            // 确保最后一波敌人也能触发升级
            // 给予经验奖励，确保升级
            if (success && currentLevel < maxLevel)
            {
                // 计算需要多少经验才能升级
                int expNeeded = expRequiredForLevel[currentLevel] - currentExp;
                if (expNeeded > 0)
                {
                    Debug.Log($"最后一波结束: 添加{expNeeded}经验以确保升级");
                    AddExperience(expNeeded);
                }
            }
            
            EndGame(true); // 胜利
            return;
        }
        
        // 设置下一波次的计时器
        waveTimer = restTimeBetweenWaves;
    }
    
    /// <summary>
    /// 结束游戏
    /// </summary>
    public void EndGame(bool victory)
    {
        if (isGameOver) return;
        
        isGameOver = true;
        isWaveInProgress = false;
        
        // 触发游戏结束事件
        OnGameOver?.Invoke(victory);
        
        // 根据游戏结果显示对应的UI
        if (victory)
        {
            Debug.Log("[GameManager] 游戏胜利！显示胜利界面");
            EnsureWinUIExists();
        }
        else
        {
            Debug.Log("[GameManager] 游戏失败！显示失败界面");
            EnsureLoseUIExists();
        }
        
        // 停止游戏时间流动
        Time.timeScale = 0;
    }
    
    /// <summary>
    /// 确保WinUI存在（如果不存在则创建）
    /// </summary>
    public void EnsureWinUIExists()
    {
        // 检查场景中是否已存在WinUI组件
        WinUI existingWinUI = FindObjectOfType<WinUI>();
        
        if (existingWinUI == null)
        {
            // 创建一个新的Canvas
            GameObject winUICanvas = new GameObject("WinUICanvas");
            Canvas canvas = winUICanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 确保显示在其他UI的上层
            
            // 添加CanvasScaler组件使UI适应不同屏幕尺寸
            CanvasScaler scaler = winUICanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // 添加GraphicRaycaster组件以使按钮可交互
            winUICanvas.AddComponent<GraphicRaycaster>();
            
            // 创建金色背景面板
            GameObject winPanel = new GameObject("WinPanel");
            winPanel.transform.SetParent(winUICanvas.transform, false);
            
            // 添加Panel组件和尺寸
            RectTransform panelRect = winPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // 添加Image组件作为背景
            Image panelImage = winPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // 黑色背景带一点金色
            
            // 创建标题文本
            GameObject titleObj = CreateTextObject("VictoryText", "胜利！", 72, new Color(1f, 0.9f, 0.2f)); // 金色文本
            titleObj.transform.SetParent(winPanel.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.95f);
            titleRect.sizeDelta = new Vector2(500, 100);
            
            // 创建分数文本
            GameObject scoreObj = CreateTextObject("StatsText", $"总击杀数: {totalEnemiesKilled}", 36, Color.white);
            scoreObj.transform.SetParent(winPanel.transform, false);
            RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 0.7f);
            scoreRect.anchorMax = new Vector2(0.5f, 0.8f);
            scoreRect.sizeDelta = new Vector2(400, 80);
            
            // 创建波次文本
            GameObject waveObj = CreateTextObject("WaveText", $"通过波次: {currentWave}/{waveCount}", 36, Color.white);
            waveObj.transform.SetParent(winPanel.transform, false);
            RectTransform waveRect = waveObj.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.5f, 0.6f);
            waveRect.anchorMax = new Vector2(0.5f, 0.7f);
            waveRect.sizeDelta = new Vector2(400, 80);
            
            // 创建游戏时间文本
            int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60);
            int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad % 60);
            GameObject timeObj = CreateTextObject("TimeText", $"游戏时间: {minutes:00}:{seconds:00}", 36, Color.white);
            timeObj.transform.SetParent(winPanel.transform, false);
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.5f, 0.5f);
            timeRect.anchorMax = new Vector2(0.5f, 0.6f);
            timeRect.sizeDelta = new Vector2(400, 80);
            
            // 创建再来一次按钮
            GameObject restartButtonObj = CreateButtonObject("RestartButton", "再来一次", new Color(0.2f, 0.6f, 1f, 1f));
            restartButtonObj.transform.SetParent(winPanel.transform, false);
            RectTransform restartRect = restartButtonObj.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0.5f, 0.3f);
            restartRect.anchorMax = new Vector2(0.5f, 0.4f);
            restartRect.sizeDelta = new Vector2(300, 60);
            
            // 创建退出按钮
            GameObject quitButtonObj = CreateButtonObject("QuitButton", "退出游戏", new Color(1f, 0.3f, 0.3f, 1f));
            quitButtonObj.transform.SetParent(winPanel.transform, false);
            RectTransform quitRect = quitButtonObj.GetComponent<RectTransform>();
            quitRect.anchorMin = new Vector2(0.5f, 0.2f);
            quitRect.anchorMax = new Vector2(0.5f, 0.3f);
            quitRect.sizeDelta = new Vector2(300, 60);
            
            // 为Canvas添加WinUI组件
            WinUI winUI = winUICanvas.AddComponent<WinUI>();
            
            // 设置WinUI组件属性
            winUI.winPanel = winPanel;
            winUI.victoryText = titleObj.GetComponent<Text>();
            winUI.statsText = scoreObj.GetComponent<Text>();
            winUI.waveText = waveObj.GetComponent<Text>();
            winUI.timeText = timeObj.GetComponent<Text>();
            winUI.restartButton = restartButtonObj.GetComponent<Button>();
            winUI.quitButton = quitButtonObj.GetComponent<Button>();
            
            // 手动调用显示方法来确保胜利UI立即显示
            winUI.ShowWinPanel();
        }
    }
    
    /// <summary>
    /// 确保LoseUI存在（如果不存在则创建）
    /// </summary>
    private void EnsureLoseUIExists()
    {
        // 检查场景中是否已存在LoseUI组件
        LoseUI existingLoseUI = FindObjectOfType<LoseUI>();
        
        if (existingLoseUI == null)
        {
            // 创建一个新的Canvas
            GameObject loseUICanvas = new GameObject("LoseUICanvas");
            Canvas canvas = loseUICanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 确保显示在其他UI的上层
            
            // 添加CanvasScaler组件使UI适应不同屏幕尺寸
            CanvasScaler scaler = loseUICanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // 添加GraphicRaycaster组件以使按钮可交互
            loseUICanvas.AddComponent<GraphicRaycaster>();
            
            // 创建黑色半透明背景面板
            GameObject losePanel = new GameObject("LosePanel");
            losePanel.transform.SetParent(loseUICanvas.transform, false);
            
            // 添加Panel组件和尺寸
            RectTransform panelRect = losePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // 添加Image组件作为背景
            Image panelImage = losePanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            // 创建标题文本
            GameObject titleObj = CreateTextObject("GameOverText", "游戏结束", 72, Color.red);
            titleObj.transform.SetParent(losePanel.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.85f);
            titleRect.anchorMax = new Vector2(0.5f, 0.95f);
            titleRect.sizeDelta = new Vector2(500, 100);
            
            // 创建分数文本
            GameObject scoreObj = CreateTextObject("ScoreText", $"击杀数: {totalEnemiesKilled}", 36, Color.white);
            scoreObj.transform.SetParent(losePanel.transform, false);
            RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 0.7f);
            scoreRect.anchorMax = new Vector2(0.5f, 0.8f);
            scoreRect.sizeDelta = new Vector2(400, 80);
            
            // 创建波次文本
            GameObject waveObj = CreateTextObject("WaveText", $"波次: {currentWave}/{waveCount}", 36, Color.white);
            waveObj.transform.SetParent(losePanel.transform, false);
            RectTransform waveRect = waveObj.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.5f, 0.6f);
            waveRect.anchorMax = new Vector2(0.5f, 0.7f);
            waveRect.sizeDelta = new Vector2(400, 80);
            
            // 创建生存时间文本 (显示游戏时间)
            int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60);
            int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad % 60);
            GameObject timeObj = CreateTextObject("SurvivalTimeText", $"生存时间: {minutes:00}:{seconds:00}", 36, Color.white);
            timeObj.transform.SetParent(losePanel.transform, false);
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.5f, 0.5f);
            timeRect.anchorMax = new Vector2(0.5f, 0.6f);
            timeRect.sizeDelta = new Vector2(400, 80);
            
            // 创建重新开始按钮
            GameObject restartButtonObj = CreateButtonObject("RestartButton", "重新开始", new Color(0.2f, 0.6f, 1f, .5f));
            restartButtonObj.transform.SetParent(losePanel.transform, false);
            RectTransform restartRect = restartButtonObj.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0.5f, 0.3f);
            restartRect.anchorMax = new Vector2(0.5f, 0.4f);
            restartRect.sizeDelta = new Vector2(300, 40);
            
            // 创建退出按钮
            GameObject quitButtonObj = CreateButtonObject("QuitButton", "退出游戏", new Color(1f, 0.3f, 0.3f, .5f));
            quitButtonObj.transform.SetParent(losePanel.transform, false);
            RectTransform quitRect = quitButtonObj.GetComponent<RectTransform>();
            quitRect.anchorMin = new Vector2(0.5f, 0.2f);
            quitRect.anchorMax = new Vector2(0.5f, 0.3f);
            quitRect.sizeDelta = new Vector2(300, 40);
            
            // 为Canvas添加LoseUI组件
            LoseUI loseUI = loseUICanvas.AddComponent<LoseUI>();
            
            // 设置LoseUI组件属性
            loseUI.losePanel = losePanel;
            loseUI.gameOverText = titleObj.GetComponent<Text>();
            loseUI.scoreText = scoreObj.GetComponent<Text>();
            loseUI.waveText = waveObj.GetComponent<Text>();
            loseUI.survivalTimeText = timeObj.GetComponent<Text>();
            loseUI.restartButton = restartButtonObj.GetComponent<Button>();
            loseUI.quitButton = quitButtonObj.GetComponent<Button>();
            
            // 手动调用显示方法来确保失败UI立即显示
            loseUI.ShowLosePanel();
        }
    }
    
    /// <summary>
    /// 创建文本UI元素
    /// </summary>
    private GameObject CreateTextObject(string name, string content, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name);
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        return textObj;
    }
    
    /// <summary>
    /// 创建按钮UI元素
    /// </summary>
    private GameObject CreateButtonObject(string name, string text, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        Image image = buttonObj.AddComponent<Image>();
        image.color = color;
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, color.a);
        colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, color.a);
        button.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;
        
        return buttonObj;
    }
    
    /// <summary>
    /// 暂停/恢复游戏
    /// </summary>
    public void TogglePause()
    {
        if (isGameOver) return;
        
        isPaused = !isPaused;
        
        // 暂停时停止时间流动，恢复时恢复时间流动
        Time.timeScale = isPaused ? 0 : gameSpeed;
        
        // 触发游戏状态改变事件
        OnGameStateChanged?.Invoke(isPaused);
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    /// <summary>
    /// 加载主菜单
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // 假设主菜单是场景索引0
    }
    
    /// <summary>
    /// 设置游戏速度
    /// </summary>
    public void SetGameSpeed(float speed)
    {
        gameSpeed = Mathf.Clamp(speed, 0.5f, 3f);
        
        // 如果游戏没有暂停，立即应用速度
        if (!isPaused)
        {
            Time.timeScale = gameSpeed;
        }
    }
    
    #region 经验与等级系统
    
    /// <summary>
    /// 获取当前等级
    /// </summary>
    public int GetLevel()
    {
        return currentLevel;
    }
    
    /// <summary>
    /// 获取当前经验值
    /// </summary>
    public int GetExp()
    {
        return currentExp;
    }
    
    /// <summary>
    /// 获取升级所需经验
    /// </summary>
    public int GetExpForNextLevel()
    {
        if (currentLevel >= maxLevel) return 0;
        return expRequiredForLevel[currentLevel];
    }
    
    /// <summary>
    /// 添加经验值
    /// </summary>
    public void AddExperience(int exp)
    {
        if (currentLevel >= maxLevel) return;
        
        Debug.Log($"添加经验值: {exp}, 原经验值: {currentExp}");
        currentExp += exp;
        Debug.Log($"新经验值: {currentExp}");
        
        // 判断是否为最终波次获胜
        bool isFinalWaveVictory = currentWave >= waveCount;
        
        // 检查是否升级 (传递最终波次标志)
        CheckLevelUp(isFinalWaveVictory);
        
        // 确保UI更新
        OnGoldChanged?.Invoke(gold);  // 使用这个事件来触发UI更新，因为该事件会触发UpdateStatusUI
    }
    
    /// <summary>
    /// 检查是否升级
    /// </summary>
    /// <param name="isFinalWave">是否为最终波次</param>
    private void CheckLevelUp(bool isFinalWave = false)
    {
        while (currentLevel < maxLevel && currentExp >= expRequiredForLevel[currentLevel])
        {
            currentExp -= expRequiredForLevel[currentLevel];
            currentLevel++;
            
            // 计算正确的金币奖励值
            int goldReward = 0;
            if (currentLevel - 1 >= 0 && currentLevel - 1 < goldRewardPerLevel.Length)
            {
                goldReward = goldRewardPerLevel[currentLevel - 1]; // 数组索引从0开始
                Debug.Log($"Level {currentLevel}: 奖励 {goldReward} 金币");
            }
            else
            {
                Debug.LogWarning($"金币奖励索引超出范围: {currentLevel - 1}, 长度: {goldRewardPerLevel.Length}");
                // 使用最后一个值
                goldReward = goldRewardPerLevel[goldRewardPerLevel.Length - 1];
            }
            
            // 消灭场上所有敌人
            DestroyAllEnemies();
            
            // 恢复玩家生命值并增加最大生命值
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                // 增加最大生命值：当前等级 * 2
                playerController.maxHealth += currentLevel * 2;
                
                // 恢复生命值：当前等级 * 2 + 5
                playerController.Heal(currentLevel * 2 + 5);
                
                // 调用OnLevelUp增加移动速度
                playerController.OnLevelUp();
            }
            
            // 添加金币并确保更新完成
            Debug.Log($"添加升级金币奖励: {goldReward}");
            AddGold(goldReward);
            
            // 触发升级事件（确保这一行在AddGold之后，因为事件处理程序需要访问最新的金币值）
            OnPlayerLevelUp?.Invoke(currentLevel, goldReward);
            
            // 仅在非最终波次时打开商店界面
            if (!isFinalWave)
            {
                // 打开商店界面（确保在金币更新后）
                OpenShopOnLevelUp();
            }
            else
            {
                Debug.Log("最终波次胜利，跳过打开商店界面");
            }
        }
    }
    
    /// <summary>
    /// 消灭场上所有敌人
    /// </summary>
    private void DestroyAllEnemies()
    {
        EnemyManager enemyManager = GetEnemyManager();
        if (enemyManager != null)
        {
            List<Enemy> activeEnemies = enemyManager.GetActiveEnemies();
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    // 给予极高伤害，立即消灭敌人
                    float maxDamage = enemy.maxHealth * 1000f; // 确保能击杀任何敌人
                    enemy.TakeDamage(maxDamage);
                }
            }
        }
    }
    
    /// <summary>
    /// 升级时打开商店界面
    /// </summary>
    private void OpenShopOnLevelUp()
    {
        // 如果游戏已经结束则不打开商店
        if (isGameOver)
        {
            Debug.Log("GameManager: 游戏已结束，不打开商店");
            return;
        }
        
        // 判断是否达到最后一波
        if (currentWave >= waveCount)
        {
            Debug.Log("GameManager: 即将到达最后一波，不打开商店");
            return;
        }
        
        // 使用HeroShopManager替代ShopManager
        HeroShopManager heroShopManager = FindObjectOfType<HeroShopManager>();
        if (heroShopManager != null)
        {
            Debug.Log("GameManager: 通过HeroShopManager打开商店 - 升级后");
            heroShopManager.OpenShop();
        }
        else
        {
            Debug.LogError("GameManager: 未找到HeroShopManager，无法在升级后打开商店");
        }
    }
    
    /// <summary>
    /// 获取经验进度(0-1)
    /// </summary>
    public float GetExpProgress()
    {
        if (currentLevel >= maxLevel) return 1f;
        
        int expNeeded = expRequiredForLevel[currentLevel];
        if (expNeeded <= 0) return 1f;
        
        float progress = (float)currentExp / expNeeded;
        return progress;
    }
    
    #endregion
    
    #region 金币系统
    
    /// <summary>
    /// 获取当前金币
    /// </summary>
    public int GetGold()
    {
        return gold;
    }
    
    /// <summary>
    /// 获取总击杀敌人数量
    /// </summary>
    public int GetTotalEnemiesKilled()
    {
        return totalEnemiesKilled;
    }
    
    /// <summary>
    /// 获取击杀特定敌人的数量
    /// </summary>
    public int GetEnemyKillCount(string enemyName)
    {
        if (enemyKills.TryGetValue(enemyName, out int count))
        {
            return count;
        }
        return 0;
    }
    
    /// <summary>
    /// 获取所有敌人击杀统计
    /// </summary>
    public Dictionary<string, int> GetEnemyKillStats()
    {
        return new Dictionary<string, int>(enemyKills);
    }
    
    /// <summary>
    /// 记录敌人击杀
    /// </summary>
    public void RegisterEnemyKill(string enemyName)
    {
        // 增加特定敌人的击杀计数
        if (enemyKills.ContainsKey(enemyName))
        {
            enemyKills[enemyName]++;
        }
        else
        {
            enemyKills.Add(enemyName, 1);
        }
        
        // 增加总击杀数
        totalEnemiesKilled++;
    }
    
    /// <summary>
    /// 添加金币
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        
        Debug.Log($"[金币系统] 添加金币: +{amount} (当前:{gold} -> 新值:{gold + amount})");
        gold += amount;
        
        // 触发金币变化事件
        OnGoldChanged?.Invoke(gold);
        
        // 额外检查和记录，以帮助调试
        if (amount >= 10)
        {
            Debug.LogWarning($"[金币系统] 大额金币添加: +{amount}, 最终金币: {gold}");
        }
    }
    
    /// <summary>
    /// 花费金币
    /// </summary>
    public bool SpendGold(int amount)
    {
        // 检查金币是否足够
        if (gold < amount) 
        {
            Debug.LogWarning($"[金币系统] 金币不足: 尝试花费 {amount}, 但只有 {gold}");
            return false;
        }
        
        Debug.Log($"[金币系统] 花费金币: -{amount} (当前:{gold} -> 新值:{gold - amount})");
        gold -= amount;
        
        // 触发金币变化事件
        OnGoldChanged?.Invoke(gold);
        
        return true;
    }
    
    #endregion
    
    #region 队伍管理方法
    
    /// <summary>
    /// 添加角色到队伍
    /// </summary>
    public void AddCharacterToTeam(GameObject characterObj)
    {
        if (characterObj == null)
        {
            Debug.LogError("Attempted to add null character to team");
            return;
        }

        // 检查是否已经有这个对象
        if (teamObjects.Contains(characterObj))
        {
            Debug.Log("Character already in team: " + characterObj.name);
            return;
        }

        if (teamObjects.Count >= maxTeamSize)
        {
            Debug.Log("Team is already full");
            return;
        }

        Character character = characterObj.GetComponent<Character>();
        if (character == null)
        {
            Debug.LogError("GameObject does not have a Character component: " + characterObj.name);
            return;
        }

        teamObjects.Add(characterObj);
        PositionTeamMembers();
        ReassignTeamNumbers();
        
        // 应用符文效果到新加入队伍的角色
        RunesManager runesManager = FindObjectOfType<RunesManager>();
        if (runesManager != null && character != null)
        {
            int heroLevel = character.tier; // 使用角色的tier作为等级
            runesManager.ApplyRuneEffects(character, heroLevel);
            Debug.Log($"已应用符文效果到角色 {characterObj.name} (等级 {heroLevel})");
        }
        
        // 更新协同效应
        SynergyManager synergyManager = FindObjectOfType<SynergyManager>();
        if (synergyManager != null)
        {
            // 收集队伍中所有角色
            List<Character> teamCharacters = new List<Character>();
            foreach (GameObject obj in teamObjects)
            {
                Character teamChar = obj.GetComponent<Character>();
                if (teamChar != null)
                {
                    teamCharacters.Add(teamChar);
                }
            }
            
            // 更新团队协同效应
            if (teamCharacters.Count > 0)
            {
                // 这里假设SynergyManager有一个方法接受Character列表
                // 您可能需要根据实际实现来调整这里的代码
                synergyManager.UpdateTeamSynergies(teamCharacters);
            }
        }
        
        // 触发队伍更新事件
        OnTeamUpdated?.Invoke();
        Debug.Log($"Added {characterObj.name} to team. Team size: {teamObjects.Count}");
    }
    
    /// <summary>
    /// 重新应用所有队伍成员的符文效果
    /// </summary>
    public void ApplyRunesToAllTeamMembers()
    {
        RunesManager runesManager = FindObjectOfType<RunesManager>();
        if (runesManager == null)
        {
            Debug.LogWarning("找不到RunesManager实例，无法应用符文效果");
            return;
        }
        
        foreach (GameObject teamObj in teamObjects)
        {
            Character character = teamObj.GetComponent<Character>();
            if (character != null)
            {
                int heroLevel = character.tier; // 使用角色的tier作为等级
                runesManager.ApplyRuneEffects(character, heroLevel);
                Debug.Log($"已应用符文效果到角色 {teamObj.name} (等级 {heroLevel})");
            }
        }
        
        Debug.Log("已应用符文效果到所有队伍成员");
    }
    
    /// <summary>
    /// 从队伍中移除角色
    /// </summary>
    public void RemoveCharacterFromTeam(GameObject characterObj)
    {
        if (characterObj == null || !teamObjects.Contains(characterObj))
        {
            return;
        }
        
        teamObjects.Remove(characterObj);
        PositionTeamMembers();
        ReassignTeamNumbers();
        
        // 更新协同效应
        SynergyManager synergyManager = FindObjectOfType<SynergyManager>();
        if (synergyManager != null)
        {
            // 收集队伍中所有角色
            List<Character> teamCharacters = new List<Character>();
            foreach (GameObject obj in teamObjects)
            {
                Character teamChar = obj.GetComponent<Character>();
                if (teamChar != null)
                {
                    teamCharacters.Add(teamChar);
                }
            }
            
            // 更新团队协同效应
            if (teamCharacters.Count > 0)
            {
                synergyManager.UpdateTeamSynergies(teamCharacters);
            }
        }
        
        // 触发队伍更新事件
        OnTeamUpdated?.Invoke();
        Debug.Log($"Removed {characterObj.name} from team. Team size: {teamObjects.Count}");
    }
    
    /// <summary>
    /// 重新定位队伍成员
    /// </summary>
    private void PositionTeamMembers()
    {
        // 这里可以实现队伍成员的自动定位
        // 简单实现：将队伍成员均匀分布
    }
    
    /// <summary>
    /// 重新分配队伍编号
    /// </summary>
    private void ReassignTeamNumbers()
    {
        // 如果需要可以实现队伍编号的重新分配
    }
    
    // 队伍更新事件
    public delegate void TeamUpdatedHandler();
    public event TeamUpdatedHandler OnTeamUpdated;
    
    #endregion
    
    #region 游戏数据保存与加载
    
    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public void SaveGameData()
    {
        // 创建保存数据对象
        GameData data = new GameData
        {
            difficulty = difficulty,
            currentWave = currentWave,
            currentLevel = currentLevel,
            currentExp = currentExp,
            gold = gold,
            totalEnemiesKilled = totalEnemiesKilled
        };
        
        // 将敌人击杀数据转换为可序列化的格式
        List<EnemyKillData> killDataList = new List<EnemyKillData>();
        foreach (var pair in enemyKills)
        {
            killDataList.Add(new EnemyKillData { enemyName = pair.Key, killCount = pair.Value });
        }
        data.enemyKillData = killDataList.ToArray();
        
        // 转换为JSON
        string jsonData = JsonUtility.ToJson(data);
        
        // 保存到PlayerPrefs
        PlayerPrefs.SetString("GameSave", jsonData);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 加载游戏数据
    /// </summary>
    public bool LoadGameData()
    {
        // 检查是否有保存的游戏
        if (!PlayerPrefs.HasKey("GameSave")) return false;
        
        // 获取JSON数据
        string jsonData = PlayerPrefs.GetString("GameSave");
        
        // 转换为对象
        GameData data = JsonUtility.FromJson<GameData>(jsonData);
        
        // 应用数据
        difficulty = data.difficulty;
        currentWave = data.currentWave;
        currentLevel = data.currentLevel;
        currentExp = data.currentExp;
        gold = data.gold;
        totalEnemiesKilled = data.totalEnemiesKilled;
        
        // 加载敌人击杀数据
        enemyKills.Clear();
        if (data.enemyKillData != null)
        {
            foreach (var killData in data.enemyKillData)
            {
                enemyKills[killData.enemyName] = killData.killCount;
            }
        }
        
        // 通知其他系统
        OnGoldChanged?.Invoke(gold);
        
        return true;
    }
    
    /// <summary>
    /// 删除保存的游戏数据
    /// </summary>
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey("GameSave");
        PlayerPrefs.Save();
    }
    
    #endregion
}

/// <summary>
/// 用于保存游戏数据的结构
/// </summary>
[System.Serializable]
public class GameData
{
    public GameDifficulty difficulty;
    public int currentWave;
    public int currentLevel;
    public int currentExp;
    public int gold;
    public int totalEnemiesKilled;
    public EnemyKillData[] enemyKillData;
}

/// <summary>
/// 用于保存敌人击杀数据的结构
/// </summary>
[System.Serializable]
public class EnemyKillData
{
    public string enemyName;
    public int killCount;
}

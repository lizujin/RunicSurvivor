// 本文件完全有AI生成
using UnityEngine;
using Assets.Scripts.Managers;
using SimpleSystem;

/// <summary>
/// 游戏控制器，负责协调各个管理器和初始化游戏
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("管理器预制体")]
    public GameObject enemyManagerPrefab;
    public GameObject heroShopManagerPrefab; // 更改为HeroShopManager
    public GameObject synergyManagerPrefab;
    public GameObject runesManagerPrefab; // 符文系统管理器
    public GameObject uiManagerPrefab;
    
    [Header("参考对象")]
    public Transform playerSpawnPoint;
    public GameObject playerPrefab;
    
    [Header("游戏配置")]
    public GameDifficulty initialDifficulty = GameDifficulty.Normal;
    public bool enableTutorial = true;
    public bool loadSavedGame = true;
    
    // 管理器引用
    private GameManager gameManager;
    private EnemyManager enemyManager;
    private HeroShopManager heroShopManager; // 更改为HeroShopManager
    private SynergyManager synergyManager;
    private RunesManager runesManager; // 符文系统管理器
    private UIManager uiManager;
    
    // 用于避免重复初始化
    private bool isInitialized = false;
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;
        
        InitConfigs();
        Debug.Log("初始化游戏...");
        
        // 创建GameManager单例
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.Log("创建游戏管理器");
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManager = gameManagerObj.AddComponent<GameManager>();
            gameManager.difficulty = initialDifficulty;
        }
        World.GetInstance().Start();
        // 创建EnemyManager
        if (enemyManager == null)
        {
            if (enemyManagerPrefab != null)
            {
                GameObject enemyManagerObj = Instantiate(enemyManagerPrefab);
                enemyManagerObj.name = "EnemyManager";
                enemyManager = enemyManagerObj.GetComponent<EnemyManager>();
            }
            else
            {
                GameObject enemyManagerObj = new GameObject("EnemyManager");
                enemyManager = enemyManagerObj.AddComponent<EnemyManager>();
            }
        }
        
        // 创建HeroShopManager
        if (heroShopManager == null)
        {
            // 首先尝试在场景中查找现有的HeroShopManager
            heroShopManager = FindObjectOfType<HeroShopManager>();
            
            if (heroShopManager == null)
            {
                if (heroShopManagerPrefab != null)
                {
                    GameObject heroShopManagerObj = Instantiate(heroShopManagerPrefab);
                    heroShopManagerObj.name = "HeroShopManager";
                    heroShopManager = heroShopManagerObj.GetComponent<HeroShopManager>();
                    Debug.Log("已创建HeroShopManager从预制体");
                }
                else
                {
                    GameObject heroShopManagerObj = new GameObject("HeroShopManager");
                    heroShopManager = heroShopManagerObj.AddComponent<HeroShopManager>();
                    Debug.Log("已创建HeroShopManager从空对象");
                }
            }
            else
            {
                Debug.Log("找到现有的HeroShopManager");
            }
        }
        
        // 创建SynergyManager
        if (synergyManager == null)
        {
            if (synergyManagerPrefab != null)
            {
                GameObject synergyManagerObj = Instantiate(synergyManagerPrefab);
                synergyManagerObj.name = "SynergyManager";
                synergyManager = synergyManagerObj.GetComponent<SynergyManager>();
            }
            else
            {
                GameObject synergyManagerObj = new GameObject("SynergyManager");
                synergyManager = synergyManagerObj.AddComponent<SynergyManager>();
            }
        }
        
        // 创建RunesManager (符文系统)
        if (runesManager == null)
        {
            // 首先尝试在场景中查找现有的RunesManager
            runesManager = FindObjectOfType<RunesManager>();
            
            if (runesManager == null)
            {
                if (runesManagerPrefab != null)
                {
                    GameObject runesManagerObj = Instantiate(runesManagerPrefab);
                    runesManagerObj.name = "RunesManager";
                    runesManager = runesManagerObj.GetComponent<RunesManager>();
                    Debug.Log("已创建RunesManager从预制体");
                }
                else
                {
                    GameObject runesManagerObj = new GameObject("RunesManager");
                    runesManager = runesManagerObj.AddComponent<RunesManager>();
                    Debug.Log("已创建RunesManager从空对象");
                }
            }
            else
            {
                Debug.Log("找到现有的RunesManager");
            }
        }
        
        // 创建UIManager
        if (uiManager == null)
        {
            if (uiManagerPrefab != null)
            {
                GameObject uiManagerObj = Instantiate(uiManagerPrefab);
                uiManagerObj.name = "UIManager";
                uiManager = uiManagerObj.GetComponent<UIManager>();
            }
            else
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManager = uiManagerObj.AddComponent<UIManager>();
            }
        }
        
        // 尝试加载保存的游戏
        if (loadSavedGame)
        {
            bool loaded = gameManager.LoadGameData();
            loaded = false;
            if (loaded)
            {
                Debug.Log("已加载保存的游戏数据");
            }
            else
            {
                Debug.Log("没有找到保存的游戏数据，开始新游戏");
                gameManager.InitGame();
            }
        }
        else
        {
            gameManager.InitGame();
        }
        
        // 创建玩家
        CreatePlayer();
        
        // 初始化完成
        isInitialized = true;
        
        // 设置事件监听
        SetupEventListeners();
        
        Debug.Log("游戏初始化完成");
    }
    
    private void InitConfigs()
    {
        GameConfigs.GetInstance().InitGameConfigs();
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    public void StartGame()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        Debug.Log("开始游戏");
        
        // 确保UI已设置
        if (uiManager != null)
        {
            uiManager.InitializeUI();
        }
        
        // 显示教程（如果启用）
        if (enableTutorial && uiManager != null)
        {
            uiManager.ShowTutorial();
        }
        else
        {
            // 直接开始第一波
            if (gameManager.CurrentWave == 0)
            {
                gameManager.StartNextWave();
            }
        }
    }
    
    /// <summary>
    /// 设置事件监听
    /// </summary>
    private void SetupEventListeners()
    {
        // GameManager事件
        if (gameManager != null)
        {
            gameManager.OnWaveStart += HandleWaveStart;
            gameManager.OnWaveEnd += HandleWaveEnd;
            gameManager.OnGameOver += HandleGameOver;
            gameManager.OnPlayerLevelUp += HandlePlayerLevelUp;
        }
    }
    
    /// <summary>
    /// 移除事件监听
    /// </summary>
    private void RemoveEventListeners()
    {
        // GameManager事件
        if (gameManager != null)
        {
            gameManager.OnWaveStart -= HandleWaveStart;
            gameManager.OnWaveEnd -= HandleWaveEnd;
            gameManager.OnGameOver -= HandleGameOver;
            gameManager.OnPlayerLevelUp -= HandlePlayerLevelUp;
        }
    }
    
    /// <summary>
    /// 创建玩家
    /// </summary>
    private void CreatePlayer()
    {
        if (playerPrefab != null)
        {
            // 如果未设置玩家生成点，则尝试在场景中查找
            if (playerSpawnPoint == null)
            {
                playerSpawnPoint = GameObject.FindWithTag("PlayerSpawnPoint")?.transform;
                
                // 如果没有找到带标签的，尝试通过名称查找
                if (playerSpawnPoint == null)
                {
                    playerSpawnPoint = GameObject.Find("PlayerSpawnPoint")?.transform;
                }
                
                if (playerSpawnPoint != null)
                {
                    Debug.Log("找到玩家出生点: " + playerSpawnPoint.name);
                }
                else
                {
                    Debug.LogWarning("未找到玩家出生点，将使用默认位置(0,0,0)");
                }
            }
            
            Vector3 spawnPosition = playerSpawnPoint != null ? 
                playerSpawnPoint.position : new Vector3(0, 0, 0);
                
            GameObject playerObj = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerObj.name = "Player";
            
            // 确保有PlayerController组件
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = playerObj.AddComponent<PlayerController>();
            }
            
            // 创建第一个角色
            // 尝试加载初始角色预制体
            GameObject characterPrefab = Resources.Load<GameObject>("Prefabs/Characters/hero/tangseng");
            if (characterPrefab != null)
            {
                // 使用预制体初始化第一个角色
                playerController.InitializeFirstCharacter(characterPrefab);
            }
            else
            {
                // 如果找不到预制体，创建一个默认角色
                Debug.LogWarning("没有找到初始角色预制体，将创建默认角色");
                
                // 创建默认角色对象
                GameObject characterObj = new GameObject("SunWukong");
                characterObj.transform.position = playerObj.transform.position;
                
                // 添加Character组件
                Character character = characterObj.AddComponent<Character>();
                character.characterName = "孙悟空";
                character.description = "西游记的主角，拥有强大的战斗力";
                character.race = RaceType.God;
                character.faction = FactionType.Buddhist;
                
                // 设置基本属性
                character.maxHealth = 150f;
                character.health = 150f;
                character.attack = 15f;
                character.attackSpeed = 1.2f;
                character.attackRange = 3f;
                character.critChance = 0.1f;
                character.critDamage = 2.0f;
                
                // 创建并设置子弹属性
                character.projectileSettings = new Character.ProjectileSettings();
                character.projectileSettings.speed = 20f;
                character.projectileSettings.damage = character.attack;
                character.projectileSettings.lifeTime = 2f;
                character.projectileSettings.pierceCount = 1;
                
                // 添加到玩家队伍
                playerController.AddCharacter(character);
            }
        }
        else
        {
            Debug.LogError("没有设置玩家预制体！");
        }
    }
    
    #region 事件处理
    
    /// <summary>
    /// 处理波次开始事件
    /// </summary>
    private void HandleWaveStart(int waveNumber)
    {
        Debug.Log($"第 {waveNumber} 波开始");
        
        // 通知UI
        if (uiManager != null)
        {
            uiManager.ShowWaveStartMessage(waveNumber);
        }
    }
    
    /// <summary>
    /// 处理波次结束事件
    /// </summary>
    private void HandleWaveEnd(int waveNumber, bool success)
    {
        Debug.Log($"第 {waveNumber} 波结束，状态: {(success ? "成功" : "失败")}");
        
        // 通知UI
        if (uiManager != null)
        {
            uiManager.ShowWaveEndMessage(waveNumber, success);
        }
        
        // 获取游戏管理器来检查当前波次
        if (gameManager != null && success)
        {
            // 判断是否为最后一波
            bool isMaxWave = (waveNumber >= gameManager.waveCount);
            
            // 只有在不是最后一波时才打开商店
            if (!isMaxWave && heroShopManager != null) 
            {
                Debug.Log("尝试通过HeroShopManager打开商店 - 波次结束后");
                heroShopManager.OpenShop();
            }
            else if (isMaxWave)
            {
                Debug.Log("最大波次胜利，不打开商城界面");
            }
            else
            {
                Debug.LogError("未找到HeroShopManager，无法打开商店 - 波次结束后");
                
                // 尝试重新查找HeroShopManager
                heroShopManager = FindObjectOfType<HeroShopManager>();
                if (heroShopManager != null && !isMaxWave)
                {
                    Debug.Log("重新查找到HeroShopManager，尝试打开商店");
                    heroShopManager.OpenShop();
                }
            }
        }
    }
    
    /// <summary>
    /// 处理玩家升级事件
    /// </summary>
    private void HandlePlayerLevelUp(int newLevel, int goldReward)
    {
        Debug.Log($"玩家升级到 {newLevel}，获得 {goldReward} 金币");
        
        // 通知UI
        if (uiManager != null)
        {
            uiManager.ShowLevelUpMessage(newLevel, goldReward);
        }
        
        // 获取游戏管理器检查当前波次
        if (gameManager != null)
        {
            // 判断是否为最后一波
            bool isMaxWave = (gameManager.CurrentWave >= gameManager.waveCount);
            
            // 只有在不是最后一波时才打开商店
            if (!isMaxWave && heroShopManager != null)
            {
                Debug.Log("尝试通过HeroShopManager打开商店 - 玩家升级后");
                heroShopManager.OpenShop();
            }
            else if (isMaxWave)
            {
                Debug.Log("最大波次升级，不打开商城界面");
            }
            else
            {
                Debug.LogError("未找到HeroShopManager，无法打开商店 - 玩家升级后");
                
                // 尝试重新查找HeroShopManager
                heroShopManager = FindObjectOfType<HeroShopManager>();
                if (heroShopManager != null && !isMaxWave)
                {
                    Debug.Log("重新查找到HeroShopManager，尝试打开商店");
                    heroShopManager.OpenShop();
                }
            }
        }
    }
    
    /// <summary>
    /// 处理游戏结束事件
    /// </summary>
    private void HandleGameOver(bool victory)
    {
        Debug.Log($"游戏结束，状态: {(victory ? "胜利" : "失败")}");
        
        // 通知UI
        if (uiManager != null)
        {
            uiManager.ShowGameOverScreen(victory);
        }
        
        // 保存游戏数据（可选）
        if (gameManager != null)
        {
            gameManager.SaveGameData();
        }
    }
    
    #endregion
    
    private void OnEnable()
    {
        if (isInitialized)
        {
            SetupEventListeners();
        }
    }
    
    private void OnDisable()
    {
        RemoveEventListeners();
    }
    
    private void OnDestroy()
    {
        RemoveEventListeners();
    }
    
    private void Start()
    {
        // 在Start中初始化，确保所有Manager都已创建
        if (!isInitialized)
        {
            Initialize();
        }
    }
    
    /// <summary>
    /// 获取游戏管理器
    /// </summary>
    public GameManager GetGameManager()
    {
        return gameManager;
    }
    
    /// <summary>
    /// 获取敌人管理器
    /// </summary>
    public EnemyManager GetEnemyManager()
    {
        return enemyManager;
    }
    
    /// <summary>
    /// 获取英雄商店管理器
    /// </summary>
    public HeroShopManager GetHeroShopManager()
    {
        return heroShopManager;
    }
    
    /// <summary>
    /// 获取协同效应管理器
    /// </summary>
    public SynergyManager GetSynergyManager()
    {
        return synergyManager;
    }
    
    /// <summary>
    /// 获取UI管理器
    /// </summary>
    public UIManager GetUIManager()
    {
        return uiManager;
    }
    
    /// <summary>
    /// 获取符文系统管理器
    /// </summary>
    public RunesManager GetRunesManager()
    {
        return runesManager;
    }
}

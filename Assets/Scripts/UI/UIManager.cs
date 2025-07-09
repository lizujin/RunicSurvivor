// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器，处理游戏界面显示和交互
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("主要UI面板")]
    public GameObject gameplayPanel;     // 游戏界面面板
    public GameObject shopPrefab;        // 商店面板预制体
    public GameObject pausePanel;        // 暂停面板
    public GameObject gameOverPanel;     // 游戏结束面板
    public GameObject winUIPrefab;       // 胜利界面预制体
    public GameObject tutorialPanel;     // 教程面板
    public GameObject synergyPanel;      // 协同效应面板
    public GameObject startPanel;        // 开始界面
    
    [Header("UI容器")]
    public Transform uiRoot;             // UI根节点，用于放置动态生成的UI
    
    [Header("角色UI")]
    public Transform teamInfoContainer;  // 队伍信息容器
    public GameObject characterInfoPrefab; // 角色信息预制体
    
    [Header("消息UI")]
    public GameObject messageBox;        // 消息框
    public Text messageText;             // 消息文本
    public float messageDuration = 3f;   // 消息持续时间
    
    [Header("协同效应UI")]
    public Transform raceContainer;      // 种族容器
    public Transform factionContainer;   // 派别容器
    public GameObject synergyItemPrefab; // 协同效应项预制体
    
    [Header("波次UI")]
    public GameObject waveStartBanner;   // 波次开始横幅
    public Text waveStartText;           // 波次开始文本
    public GameObject waveEndBanner;     // 波次结束横幅
    public Text waveEndText;             // 波次结束文本
    
    [Header("游戏结束UI")]
    public Text gameOverTitleText;       // 游戏结束标题文本
    public Text gameOverStatsText;       // 游戏结束统计文本
    public Button restartButton;         // 重新开始按钮
    public Button mainMenuButton;        // 主菜单按钮
    
    [Header("技能UI")]
    public Image centerSkillIcon;        // 中心角色技能图标
    public Image skillCooldownOverlay;   // 技能冷却覆盖
    
    // 内部变量
    private Dictionary<Character, GameObject> characterInfos = new Dictionary<Character, GameObject>();
    private Dictionary<RaceType, GameObject> raceSynergyItems = new Dictionary<RaceType, GameObject>();
    private Dictionary<FactionType, GameObject> factionSynergyItems = new Dictionary<FactionType, GameObject>();
    private Coroutine messageCoroutine;
    
    // 缓存的管理器引用
    private GameManager gameManager;
    private PlayerController playerController;
    private SynergyManager synergyManager;
    private ShopManager shopManager;
    
    private void Awake()
    {
        // 隐藏所有面板
        Debug.Log("UIManager.Awake(): 初始化UI面板");
        if (gameplayPanel != null) {
            gameplayPanel.SetActive(false); // 初始时隐藏游戏面板，等待开始游戏
            Debug.Log("游戏面板初始隐藏");
        } else {
            Debug.LogWarning("游戏面板引用为空");
        }
        
        // 不再在Awake中处理shopPanel，由HeroShopManager按需生成
        
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (synergyPanel != null) synergyPanel.SetActive(false);
        
        // 隐藏消息框和波次横幅
        if (messageBox != null) messageBox.SetActive(false);
        if (waveStartBanner != null) waveStartBanner.SetActive(false);
        if (waveEndBanner != null) waveEndBanner.SetActive(false);
        uiRoot = GameObject.FindGameObjectWithTag("uiRoot")?.transform;
    }
    
    private void Start()
    {
        // 获取管理器引用
        gameManager = GameManager.Instance;
        playerController = FindObjectOfType<PlayerController>();
        synergyManager = SynergyManager.Instance;
        shopManager = FindObjectOfType<ShopManager>();
        
        // 设置事件监听
        SetupEventListeners();
        
        // 创建开始界面
        ShowStartPanel();
    }
    
    private void OnDestroy()
    {
        // 移除事件监听
        RemoveEventListeners();
    }
    
    /// <summary>
    /// 设置事件监听
    /// </summary>
    private void SetupEventListeners()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayerLevelUp += HandlePlayerLevelUp;
            gameManager.OnGameStateChanged += HandleGameStateChanged;
            gameManager.OnWaveStart += HandleWaveStart;
            gameManager.OnWaveEnd += HandleWaveEnd;
            gameManager.OnGameOver += HandleGameOver;
            gameManager.OnGoldChanged += HandleGoldChanged;
        }
        
        if (playerController != null)
        {
            playerController.OnCharacterAdded += HandleCharacterAdded;
            playerController.OnCharacterRemoved += HandleCharacterRemoved;
            playerController.OnCenterChanged += HandleCenterChanged;
        }
        
        if (synergyManager != null)
        {
            synergyManager.OnSynergyChanged += HandleSynergyChanged;
        }
        
        // 设置按钮事件
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }
    
    /// <summary>
    /// 移除事件监听
    /// </summary>
    private void RemoveEventListeners()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayerLevelUp -= HandlePlayerLevelUp;
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
            gameManager.OnWaveStart -= HandleWaveStart;
            gameManager.OnWaveEnd -= HandleWaveEnd;
            gameManager.OnGameOver -= HandleGameOver;
            gameManager.OnGoldChanged -= HandleGoldChanged;
        }
        
        if (playerController != null)
        {
            playerController.OnCharacterAdded -= HandleCharacterAdded;
            playerController.OnCharacterRemoved -= HandleCharacterRemoved;
            playerController.OnCenterChanged -= HandleCenterChanged;
        }
        
        if (synergyManager != null)
        {
            synergyManager.OnSynergyChanged -= HandleSynergyChanged;
        }
        
        // 移除按钮事件
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        }
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    public void InitializeUI()
    {
        // 激活游戏面板
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(true);
            Debug.Log("[UIManager] 激活游戏面板");
        }
        
        // 更新协同效应UI
        UpdateSynergyUI();
        
        // 清除角色信息面板
        ClearCharacterInfos();
        
        // 如果玩家控制器存在，为队伍中的角色创建信息面板
        if (playerController != null)
        {
            foreach (Character character in playerController.TeamMembers)
            {
                HandleCharacterAdded(character);
            }
        }
    }
    
    /// <summary>
    /// 更新协同效应UI
    /// </summary>
    private void UpdateSynergyUI()
    {
        if (synergyManager == null) return;
        
        // 获取当前协同效应
        Dictionary<RaceType, int> activeRaceSynergies = synergyManager.GetActiveRaceSynergies();
        Dictionary<FactionType, int> activeFactionSynergies = synergyManager.GetActiveFactionSynergies();
        
        // 获取数量统计
        Dictionary<RaceType, int> raceCounts = synergyManager.GetRaceCounts();
        Dictionary<FactionType, int> factionCounts = synergyManager.GetFactionCounts();
        
        // 更新种族协同效应UI
        UpdateRaceSynergyUI(activeRaceSynergies, raceCounts);
        
        // 更新派别协同效应UI
        UpdateFactionSynergyUI(activeFactionSynergies, factionCounts);
    }
    
    /// <summary>
    /// 更新种族协同效应UI
    /// </summary>
    private void UpdateRaceSynergyUI(Dictionary<RaceType, int> activeRaceSynergies, Dictionary<RaceType, int> raceCounts)
    {
        if (raceContainer == null) return;
        
        // 清除废弃的种族项
        List<RaceType> racesToRemove = new List<RaceType>();
        foreach (var pair in raceSynergyItems)
        {
            if (!raceCounts.ContainsKey(pair.Key) || raceCounts[pair.Key] == 0)
            {
                racesToRemove.Add(pair.Key);
            }
        }
        
        foreach (RaceType race in racesToRemove)
        {
            if (raceSynergyItems.TryGetValue(race, out GameObject item))
            {
                Destroy(item);
                raceSynergyItems.Remove(race);
            }
        }
        
        // 更新或创建种族项
        foreach (var pair in raceCounts)
        {
            RaceType race = pair.Key;
            int count = pair.Value;
            
            // 忽略无效种族或数量为0的项
            if (race == RaceType.None || count == 0) continue;
            
            // 获取激活等级
            int activeLevel = activeRaceSynergies.TryGetValue(race, out int level) ? level : 0;
            
            // 获取种族名称和描述
            string raceName = synergyManager.GetRaceTypeString(race);
            string description = synergyManager.GetRaceDescription(race);
            string synergyEffect = activeLevel > 0 ? synergyManager.GetRaceSynergyDescription(race, activeLevel) : "";
            
            // 更新现有项或创建新项
            if (raceSynergyItems.TryGetValue(race, out GameObject item))
            {
                // 更新现有项
                UpdateSynergyItem(item, raceName, count, activeLevel, synergyEffect);
            }
            else
            {
                // 创建新项
                GameObject newItem = CreateSynergyItem(raceName, count, activeLevel, synergyEffect);
                if (newItem != null)
                {
                    newItem.transform.SetParent(raceContainer, false);
                    raceSynergyItems.Add(race, newItem);
                }
            }
        }
    }
    
    /// <summary>
    /// 更新派别协同效应UI
    /// </summary>
    private void UpdateFactionSynergyUI(Dictionary<FactionType, int> activeFactionSynergies, Dictionary<FactionType, int> factionCounts)
    {
        if (factionContainer == null) return;
        
        // 清除废弃的派别项
        List<FactionType> factionsToRemove = new List<FactionType>();
        foreach (var pair in factionSynergyItems)
        {
            if (!factionCounts.ContainsKey(pair.Key) || factionCounts[pair.Key] == 0)
            {
                factionsToRemove.Add(pair.Key);
            }
        }
        
        foreach (FactionType faction in factionsToRemove)
        {
            if (factionSynergyItems.TryGetValue(faction, out GameObject item))
            {
                Destroy(item);
                factionSynergyItems.Remove(faction);
            }
        }
        
        // 更新或创建派别项
        foreach (var pair in factionCounts)
        {
            FactionType faction = pair.Key;
            int count = pair.Value;
            
            // 忽略无效派别或数量为0的项
            if (faction == FactionType.None || count == 0) continue;
            
            // 获取激活等级
            int activeLevel = activeFactionSynergies.TryGetValue(faction, out int level) ? level : 0;
            
            // 获取派别名称和描述
            string factionName = synergyManager.GetFactionTypeString(faction);
            string description = synergyManager.GetFactionDescription(faction);
            string synergyEffect = activeLevel > 0 ? synergyManager.GetFactionSynergyDescription(faction, activeLevel) : "";
            
            // 更新现有项或创建新项
            if (factionSynergyItems.TryGetValue(faction, out GameObject item))
            {
                // 更新现有项
                UpdateSynergyItem(item, factionName, count, activeLevel, synergyEffect);
            }
            else
            {
                // 创建新项
                GameObject newItem = CreateSynergyItem(factionName, count, activeLevel, synergyEffect);
                if (newItem != null)
                {
                    newItem.transform.SetParent(factionContainer, false);
                    factionSynergyItems.Add(faction, newItem);
                }
            }
        }
    }
    
    /// <summary>
    /// 创建协同效应项
    /// </summary>
    private GameObject CreateSynergyItem(string name, int count, int activeLevel, string effect)
    {
        if (synergyItemPrefab == null) return null;
        
        // 实例化协同效应项
        GameObject item = Instantiate(synergyItemPrefab);
        
        // 更新协同效应项
        UpdateSynergyItem(item, name, count, activeLevel, effect);
        
        return item;
    }
    
    /// <summary>
    /// 更新协同效应项
    /// </summary>
    private void UpdateSynergyItem(GameObject item, string name, int count, int activeLevel, string effect)
    {
        // 获取协同效应项的UI元素
        Text nameText = item.transform.Find("NameText")?.GetComponent<Text>();
        Text countText = item.transform.Find("CountText")?.GetComponent<Text>();
        Text levelText = item.transform.Find("LevelText")?.GetComponent<Text>();
        Text effectText = item.transform.Find("EffectText")?.GetComponent<Text>();
        Image background = item.GetComponent<Image>();
        
        // 设置文本
        if (nameText != null) nameText.text = name;
        if (countText != null) countText.text = count.ToString();
        if (levelText != null) levelText.text = activeLevel > 0 ? $"Lv.{activeLevel}" : "";
        if (effectText != null) effectText.text = effect;
        
        // 设置背景颜色
        if (background != null)
        {
            if (activeLevel > 0)
            {
                // 激活状态
                background.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            }
            else
            {
                // 未激活状态
                background.color = new Color(0.7f, 0.7f, 0.7f, 0.3f);
            }
        }
    }
    
    /// <summary>
    /// 创建角色信息面板
    /// </summary>
    private GameObject CreateCharacterInfo(Character character)
    {
        if (characterInfoPrefab == null || teamInfoContainer == null) return null;
        
        // 实例化角色信息面板
        GameObject infoObj = Instantiate(characterInfoPrefab, teamInfoContainer);
        
        // 更新角色信息
        UpdateCharacterInfo(infoObj, character);
        
        return infoObj;
    }
    
    /// <summary>
    /// 更新角色信息面板
    /// </summary>
    private void UpdateCharacterInfo(GameObject infoObj, Character character)
    {
        if (infoObj == null || character == null) return;
        
        // 获取角色信息面板的UI元素
        Text nameText = infoObj.transform.Find("NameText")?.GetComponent<Text>();
        Text levelText = infoObj.transform.Find("LevelText")?.GetComponent<Text>();
        Image healthBar = infoObj.transform.Find("HealthBar")?.GetComponent<Image>();
        Image manaBar = infoObj.transform.Find("ManaBar")?.GetComponent<Image>();
        Image characterImage = infoObj.transform.Find("CharacterIcon")?.GetComponent<Image>();
        Button centerButton = infoObj.transform.Find("CenterButton")?.GetComponent<Button>();
        
        // 设置文本和进度条
        if (nameText != null) nameText.text = character.characterName;
        if (levelText != null) levelText.text = $"Lv.{character.tier}";
        if (healthBar != null) healthBar.fillAmount = character.health / character.maxHealth;
        if (manaBar != null) manaBar.fillAmount = character.mana / character.maxMana;
        
        // 设置角色图标
        if (characterImage != null && character.characterIcon != null)
        {
            characterImage.sprite = character.characterIcon;
            characterImage.enabled = true;
        }
        else if (characterImage != null)
        {
            characterImage.enabled = false;
        }
        
        // 设置中心角色按钮
        if (centerButton != null)
        {
            // 清除旧的事件监听
            centerButton.onClick.RemoveAllListeners();
            
            // 设置新的事件监听
            centerButton.onClick.AddListener(() => {
                if (playerController != null)
                {
                    playerController.SetCenterCharacter(character);
                }
            });
            
            // 如果是中心角色，高亮显示按钮
            if (playerController != null && playerController.CurrentCenterCharacter == character)
            {
                centerButton.GetComponent<Image>().color = new Color(1f, 0.8f, 0.2f);
            }
            else
            {
                centerButton.GetComponent<Image>().color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// 清除所有角色信息面板
    /// </summary>
    private void ClearCharacterInfos()
    {
        foreach (var pair in characterInfos)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }
        
        characterInfos.Clear();
    }
    
    /// <summary>
    /// 显示教程
    /// </summary>
    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 关闭教程
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // 开始第一波
        if (gameManager != null && gameManager.CurrentWave == 0)
        {
            gameManager.StartNextWave();
        }
    }
    
    /// <summary>
    /// 显示波次开始消息
    /// </summary>
    public void ShowWaveStartMessage(int waveNumber)
    {
        if (waveStartBanner != null && waveStartText != null)
        {
            waveStartText.text = $"第 {waveNumber} 波 开始！";
            waveStartBanner.SetActive(true);
            
            // 延迟关闭横幅
            StartCoroutine(HideBannerAfterDelay(waveStartBanner, 2f));
        }
    }
    
    /// <summary>
    /// 显示波次结束消息
    /// </summary>
    public void ShowWaveEndMessage(int waveNumber, bool success)
    {
        if (waveEndBanner != null && waveEndText != null)
        {
            if (success)
            {
                waveEndText.text = $"第 {waveNumber} 波 通过！";
            }
            else
            {
                waveEndText.text = $"第 {waveNumber} 波 失败！";
            }
            
            waveEndBanner.SetActive(true);
            
            // 延迟关闭横幅
            StartCoroutine(HideBannerAfterDelay(waveEndBanner, 2f));
        }
    }
    
    /// <summary>
    /// 显示升级消息
    /// </summary>
    public void ShowLevelUpMessage(int newLevel, int goldReward)
    {
        ShowMessage($"升级到 {newLevel} 级！获得 {goldReward} 金币！");
    }
    
    /// <summary>
    /// 显示游戏结束画面
    /// </summary>
    public void ShowGameOverScreen(bool victory)
    {
        if (victory)
        {
            ShowWinScreen();
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 显示胜利画面
    /// </summary>
    public void ShowWinScreen()
    {
        if (winUIPrefab != null && uiRoot != null)
        {
            // 在UI根节点下实例化胜利界面预制体
            GameObject winUIInstance = Instantiate(winUIPrefab, uiRoot);
            
            // 获取WinUI组件
            WinUI winUI = winUIInstance.GetComponent<WinUI>();
            if (winUI != null)
            {
                // 调用显示方法
                winUI.ShowWinPanel();
                Debug.Log("[UIManager] 成功显示胜利界面");
            }
            else
            {
                Debug.LogError("[UIManager] 胜利界面预制体缺少WinUI组件");
            }
        }
        else
        {
            Debug.LogError("[UIManager] 胜利界面预制体或UI根节点引用为空");
            
            // 如果没有设置胜利界面预制体，让GameManager创建一个默认的胜利界面
            if (gameManager != null)
            {
                gameManager.EnsureWinUIExists();
                Debug.Log("[UIManager] 由GameManager创建默认胜利界面");
            }
        }
    }
    
    /// <summary>
    /// 显示消息
    /// </summary>
    public void ShowMessage(string message)
    {
        if (messageBox != null && messageText != null)
        {
            // 停止之前的消息协程
            if (messageCoroutine != null)
            {
                StopCoroutine(messageCoroutine);
            }
            
            // 设置消息文本
            messageText.text = message;
            messageBox.SetActive(true);
            
            // 启动新的消息协程
            messageCoroutine = StartCoroutine(HideMessageAfterDelay());
        }
    }
    
    /// <summary>
    /// 延迟隐藏消息框
    /// </summary>
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        
        if (messageBox != null)
        {
            messageBox.SetActive(false);
        }
        
        messageCoroutine = null;
    }
    
    /// <summary>
    /// 延迟隐藏横幅
    /// </summary>
    private IEnumerator HideBannerAfterDelay(GameObject banner, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (banner != null)
        {
            banner.SetActive(false);
        }
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    private void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    private void ReturnToMainMenu()
    {
        if (gameManager != null)
        {
            gameManager.LoadMainMenu();
        }
    }
    
    #region 事件处理
    
    /// <summary>
    /// 处理玩家升级事件
    /// </summary>
    private void HandlePlayerLevelUp(int newLevel, int goldReward)
    {
        // 显示升级消息
        ShowLevelUpMessage(newLevel, goldReward);
    }
    
    /// <summary>
    /// 处理游戏状态改变事件
    /// </summary>
    private void HandleGameStateChanged(bool isPaused)
    {
        // 显示或隐藏暂停面板
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
    }
    
    /// <summary>
    /// 处理波次开始事件
    /// </summary>
    private void HandleWaveStart(int waveNumber)
    {
        ShowWaveStartMessage(waveNumber);
    }
    
    /// <summary>
    /// 处理波次结束事件
    /// </summary>
    private void HandleWaveEnd(int waveNumber, bool success)
    {
        ShowWaveEndMessage(waveNumber, success);
    }
    
    /// <summary>
    /// 处理游戏结束事件
    /// </summary>
    private void HandleGameOver(bool victory)
    {
        ShowGameOverScreen(victory);
    }
    
    /// <summary>
    /// 处理金币变化事件
    /// </summary>
    private void HandleGoldChanged(int currentGold)
    {
        // 金币变化由GameStatusPanel处理
    }
    
    /// <summary>
    /// 处理角色添加事件
    /// </summary>
    private void HandleCharacterAdded(Character character)
    {
        // 创建角色信息面板
        GameObject infoObj = CreateCharacterInfo(character);
        if (infoObj != null)
        {
            characterInfos.Add(character, infoObj);
        }
        
        // 更新协同效应UI
        UpdateSynergyUI();
    }
    
    /// <summary>
    /// 处理角色移除事件
    /// </summary>
    private void HandleCharacterRemoved(Character character)
    {
        // 移除角色信息面板
        if (characterInfos.TryGetValue(character, out GameObject infoObj))
        {
            if (infoObj != null)
            {
                Destroy(infoObj);
            }
            
            characterInfos.Remove(character);
        }
        
        // 更新协同效应UI
        UpdateSynergyUI();
    }
    
    /// <summary>
    /// 处理中心角色改变事件
    /// </summary>
    private void HandleCenterChanged(Character oldCenter, Character newCenter)
    {
        // 更新所有角色信息面板
        foreach (var pair in characterInfos)
        {
            UpdateCharacterInfo(pair.Value, pair.Key);
        }
        
        // 更新中心角色技能图标
        UpdateCenterSkillUI();
    }
    
    /// <summary>
    /// 处理协同效应改变事件
    /// </summary>
    private void HandleSynergyChanged(Dictionary<RaceType, int> races, Dictionary<FactionType, int> factions)
    {
        UpdateSynergyUI();
    }
    
    #endregion
    
    /// <summary>
    /// 更新中心角色技能UI
    /// </summary>
    private void UpdateCenterSkillUI()
    {
        if (centerSkillIcon == null || skillCooldownOverlay == null || playerController == null) return;
        
        Character centerChar = playerController.CurrentCenterCharacter;
        if (centerChar != null && centerChar.skills.Count > 0)
        {
            Skill skill = centerChar.skills[0];
            
            // 设置技能图标
            if (skill.icon != null)
            {
                centerSkillIcon.sprite = skill.icon;
                centerSkillIcon.enabled = true;
            }
            else
            {
                centerSkillIcon.enabled = false;
            }
            
            // 设置冷却覆盖
            skillCooldownOverlay.fillAmount = 1f - skill.GetCooldownProgress();
        }
        else
        {
            centerSkillIcon.enabled = false;
            skillCooldownOverlay.fillAmount = 0f;
        }
    }
    
    /// <summary>
    /// 显示开始界面
    /// </summary>
    public void ShowStartPanel()
    {
        // 确保游戏面板初始状态是隐藏的
        if (gameplayPanel != null)
        {
            gameplayPanel.SetActive(false);
        }
        
        // 显示开始界面（已存在于场景中的StartPanel）
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Debug.Log("[UIManager] 显示开始界面");
            
            // 确保StartPanel组件已正确设置
            StartPanel startPanelComponent = startPanel.GetComponent<StartPanel>();
            if (startPanelComponent == null)
            {
                Debug.LogError("[UIManager] 开始界面缺少StartPanel组件");
            }
        }
        else
        {
            Debug.Log("[UIManager] 开始界面引用为空，无法显示开始界面");
            
            // 如果没有找到开始界面，则直接启用游戏面板，默认开始游戏
            if (gameplayPanel != null)
            {
                gameplayPanel.SetActive(true);
            }
            
            if (gameManager != null)
            {
                gameManager.InitGame();
                gameManager.StartNextWave();
                Debug.Log("[UIManager] 由于缺少开始界面，直接开始游戏");
            }
            
            // 初始化UI
            InitializeUI();
        }
    }
    
    private void Update()
    {
        // 更新中心角色技能UI
        UpdateCenterSkillUI();
    }
}

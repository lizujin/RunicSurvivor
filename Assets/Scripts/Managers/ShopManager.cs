// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 商店管理器，处理角色购买和队伍组建
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("商店设置")]
    public int shopSize = 5;                // 商店每次刷新的角色数量
    public int freeRefreshesPerLevel = 1;   // 每升级免费刷新次数
    public int refreshCost = 2;             // 刷新花费
    public int maxCharacterPool = 50;       // 总角色池大小
    
    [Header("稀有度概率")]
    public float[] rarityChances = new float[5] { 0.40f, 0.30f, 0.20f, 0.08f, 0.02f }; // 对应1-5星的概率
    
    [Header("角色预制体")]
    public GameObject[] tier1Characters;    // 1星角色预制体
    public GameObject[] tier2Characters;    // 2星角色预制体
    public GameObject[] tier3Characters;    // 3星角色预制体
    public GameObject[] tier4Characters;    // 4星角色预制体
    public GameObject[] tier5Characters;    // 5星角色预制体
    
    // 当前商店内容和状态
    private List<ShopItemUI> currentShopItems = new List<ShopItemUI>();
    private int freeRefreshesLeft = 0;
    private bool isShopOpen = false;
    
    // 存储当前商店的角色数据和预制体
    private List<Character> shopCharacters = new List<Character>();
    private List<GameObject> shopPrefabs = new List<GameObject>();
    
    // 角色池（按稀有度分类）
    private List<GameObject>[] characterPools = new List<GameObject>[5];
    
    // 单例实现
    private static ShopManager _instance;
    public static ShopManager Instance => _instance;
    
    // 引用UIManager和ShopUI
    private UIManager uiManager;
    private ShopUI shopUI;
    
    // 存储当前创建的商店UI实例
    private GameObject currentShopUIInstance;
    
    private void Awake()
    {
        // 单例设置
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // 初始化角色池
        InitializeCharacterPools();
    }
    
    private void Start()
    {
        // 设置事件监听
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnPlayerLevelUp += HandlePlayerLevelUp;
        }
    }
    
    private void OnDestroy()
    {
        // 取消事件监听
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnPlayerLevelUp -= HandlePlayerLevelUp;
        }
        
        // 确保UI实例被销毁
        DestroyShopUI();
    }
    
    /// <summary>
    /// 初始化角色池
    /// </summary>
    private void InitializeCharacterPools()
    {
        // 为每个稀有度创建一个列表
        for (int i = 0; i < 5; i++)
        {
            characterPools[i] = new List<GameObject>();
        }
        
        // 动态加载英雄预制体
        GameObject[] heroPrefabs = LoadHeroesFromResources();
        
        // 分类并添加到对应稀有度池
        CategorizeHeroesByRarity(heroPrefabs);
        
        // 如果资源加载方式失败，使用Inspector设置的角色
        if (characterPools[0].Count == 0 && characterPools[1].Count == 0 && 
            characterPools[2].Count == 0 && characterPools[3].Count == 0 && 
            characterPools[4].Count == 0)
        {
            Debug.LogWarning("未能从Resources加载英雄预制体，将使用Inspector设置的角色");
            
            // 添加角色到对应池
            AddCharactersToPool(0, tier1Characters);
            AddCharactersToPool(1, tier2Characters);
            AddCharactersToPool(2, tier3Characters);
            AddCharactersToPool(3, tier4Characters);
            AddCharactersToPool(4, tier5Characters);
        }
        else
        {
            Debug.Log($"成功从Resources加载英雄预制体，共 {heroPrefabs.Length} 个角色");
        }
    }
    
    /// <summary>
    /// 从Resources加载所有英雄预制体
    /// </summary>
    private GameObject[] LoadHeroesFromResources()
    {
        // 加载Prefabs/Characters/hero文件夹下的所有预制体
        GameObject[] heroPrefabs = Resources.LoadAll<GameObject>("Prefabs/Characters/hero");
        return heroPrefabs;
    }
    
    /// <summary>
    /// 根据角色属性将英雄分类到不同稀有度池
    /// </summary>
    private void CategorizeHeroesByRarity(GameObject[] heroPrefabs)
    {
        if (heroPrefabs == null || heroPrefabs.Length == 0)
            return;
            
        foreach (GameObject prefab in heroPrefabs)
        {
            Character character = prefab.GetComponent<Character>();
            if (character != null)
            {
                // 根据角色cost或其他属性确定稀有度
                int rarityIndex = DetermineRarity(character);
                
                // 添加到对应稀有度池
                AddCharacterToPools(rarityIndex, prefab);
            }
        }
    }
    
    /// <summary>
    /// 确定角色的稀有度
    /// </summary>
    private int DetermineRarity(Character character)
    {
        // 这里根据角色的属性来确定稀有度
        // 例如，可以基于以下属性:
        // - cost值
        // - 攻击力和生命值的平均值
        // - 是否有特殊技能
        
        // 简单示例: 根据cost值确定稀有度
        if (character.cost >= 5)
            return 4; // 5星
        else if (character.cost >= 4)
            return 3; // 4星
        else if (character.cost >= 3)
            return 2; // 3星
        else if (character.cost >= 2)
            return 1; // 2星
        else
            return 0; // 1星
    }
    
    /// <summary>
    /// 添加单个角色到池中
    /// </summary>
    private void AddCharacterToPools(int poolIndex, GameObject characterPrefab)
    {
        if (poolIndex < 0 || poolIndex >= characterPools.Length || characterPrefab == null)
            return;
            
        // 计算该角色在池中的数量
        int countPerCharacter = maxCharacterPool / 5 / 5; // 假设每个稀有度平均有5种角色
        countPerCharacter = Mathf.Max(1, countPerCharacter); // 至少有1个
        
        // 添加多个相同角色到池中，增加抽取概率
        for (int i = 0; i < countPerCharacter; i++)
        {
            characterPools[poolIndex].Add(characterPrefab);
        }
    }
    
    /// <summary>
    /// 将角色添加到角色池
    /// </summary>
    private void AddCharactersToPool(int poolIndex, GameObject[] characters)
    {
        if (characters == null || characters.Length == 0 || poolIndex < 0 || poolIndex >= characterPools.Length)
            return;
        
        // 按稀有度决定每种角色在池中的数量
        int countPerCharacter = maxCharacterPool / 5 / characters.Length;
        countPerCharacter = Mathf.Max(1, countPerCharacter); // 至少有1个
        
        foreach (GameObject characterPrefab in characters)
        {
            if (characterPrefab != null)
            {
                for (int i = 0; i < countPerCharacter; i++)
                {
                    characterPools[poolIndex].Add(characterPrefab);
                }
            }
        }
    }

    /// <summary>
    /// 打开商店
    /// </summary>
    public void OpenShop()
    {
        Debug.Log("ShopManager.OpenShop() 被调用");
        
        if (!isShopOpen)
        {
            isShopOpen = true;
            Debug.Log("商店状态更新为打开");
            
            // 暂停敌人生成
            EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager != null)
            {
                enemyManager.PauseSpawning();
                Debug.Log("敌人生成已暂停");
            }
            
            // 获取UIManager实例
            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
                Debug.Log("尝试获取UIManager: " + (uiManager != null ? "成功" : "失败"));
            }
            
            // 刷新商店内容
            GenerateShopItems();
            
            // 创建商店UI
            if (uiManager != null && uiManager.shopPrefab != null && uiManager.uiRoot != null)
            {
                Debug.Log("ShopManager: 创建商店UI实例");
                
                // 如果已有实例，先销毁
                if (currentShopUIInstance != null)
                {
                    Destroy(currentShopUIInstance);
                }
                
                // 实例化商店UI预制体
                currentShopUIInstance = Instantiate(uiManager.shopPrefab, uiManager.uiRoot);
                
                Debug.LogError("当前版本不支持ShopManager，请使用HeroShopManager");
                
                // 销毁创建的实例
                if (currentShopUIInstance != null)
                {
                    Destroy(currentShopUIInstance);
                    currentShopUIInstance = null;
                }
                else
                {
                    Debug.LogError("ShopManager: 商店预制体上没有ShopUI组件");
                }
            }
            else
            {
                Debug.LogError("无法创建商店UI：UIManager、shopPrefab或uiRoot为空");
            }
        }
        else
        {
            Debug.Log("商店已经是打开状态，未执行OpenShop操作");
        }
    }
    
    /// <summary>
    /// 关闭商店
    /// </summary>
    public void CloseShop()
    {
        if (isShopOpen)
        {
            isShopOpen = false;
            
            // 恢复敌人生成
            EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager != null)
            {
                enemyManager.ResumeSpawning();
            }
            
            // 隐藏商店UI
            if (shopUI != null)
            {
                shopUI.HideShop();
            }
            
            // 销毁UI实例
            DestroyShopUI();
        }
    }
    
    /// <summary>
    /// 销毁当前商店UI实例
    /// </summary>
    private void DestroyShopUI()
    {
        if (currentShopUIInstance != null)
        {
            Destroy(currentShopUIInstance);
            currentShopUIInstance = null;
            shopUI = null;
        }
    }
    
    /// <summary>
    /// 刷新商店内容
    /// </summary>
    public void RefreshShop()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;
        
        // 检查是否有免费刷新或者足够的金币
        if (freeRefreshesLeft <= 0 && gameManager.GetGold() < refreshCost)
        {
            // 显示"金币不足"提示
            Debug.Log("金币不足，无法刷新商店");
            if (shopUI != null)
            {
                shopUI.ShowNotEnoughGoldMessage();
            }
            return;
        }
        
        // 扣除刷新费用
        if (freeRefreshesLeft > 0)
        {
            freeRefreshesLeft--;
        }
        else
        {
            gameManager.SpendGold(refreshCost);
        }
        
        // 清除当前商店物品
        ClearShopItems();
        
        // 生成新的商店内容
        GenerateShopItems();
        
        // 更新UI
        if (shopUI != null)
        {
            shopUI.UpdateUI();
            shopUI.InitializeShopItems();
        }
    }
    
    /// <summary>
    /// 清除商店内容
    /// </summary>
    private void ClearShopItems()
    {
        foreach (ShopItemUI item in currentShopItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        
        currentShopItems.Clear();
        shopCharacters.Clear();
        shopPrefabs.Clear();
    }
    
    /// <summary>
    /// 生成商店内容
    /// </summary>
    private void GenerateShopItems()
    {
        // 清除之前的数据
        shopCharacters.Clear();
        shopPrefabs.Clear();
        
        // 获取当前玩家等级影响的稀有度概率
        float[] adjustedRarityChances = GetAdjustedRarityChances();
        
        for (int i = 0; i < shopSize; i++)
        {
            // 确定角色稀有度
            int rarityIndex = GetRandomRarityIndex(adjustedRarityChances);
            
            // 从对应池中随机选择角色
            GameObject characterPrefab = GetRandomCharacterFromPool(rarityIndex);
            if (characterPrefab == null) 
            {
                // 添加空项目以保持索引正确
                shopCharacters.Add(null);
                shopPrefabs.Add(null);
                continue;
            }
            
            // 存储角色数据
            Character character = characterPrefab.GetComponent<Character>();
            shopCharacters.Add(character);
            shopPrefabs.Add(characterPrefab);
        }
        
        // 注意：不再在这里创建UI，UI将由ShopUI.cs来处理
    }
    
    /// <summary>
    /// 获取商店项目角色数据
    /// </summary>
    public Character GetShopItemCharacter(int index)
    {
        if (index >= 0 && index < shopCharacters.Count)
        {
            return shopCharacters[index];
        }
        return null;
    }
    
    /// <summary>
    /// 获取商店项目预制体
    /// </summary>
    public GameObject GetShopItemPrefab(int index)
    {
        if (index >= 0 && index < shopPrefabs.Count)
        {
            return shopPrefabs[index];
        }
        return null;
    }
    
    /// <summary>
    /// 购买角色
    /// </summary>
    public bool PurchaseCharacter(int index)
    {
        if (index < 0 || index >= shopCharacters.Count)
            return false;
        
        Character character = shopCharacters[index];
        GameObject prefab = shopPrefabs[index];
        
        if (character == null || prefab == null)
            return false;
            
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return false;
        
        // 检查金币是否足够
        int cost = character.cost;
        if (gameManager.GetGold() < cost)
        {
            // 显示"金币不足"提示
            if (shopUI != null)
            {
                shopUI.ShowNotEnoughGoldMessage();
            }
            return false;
        }
        
        // 扣除金币
        gameManager.SpendGold(cost);
        
        // 添加角色到队伍
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.AddCharacterToTeam(prefab);
        }
        
        // 标记该位置的角色已购买
        shopCharacters[index] = null;
        shopPrefabs[index] = null;
        
        // 更新UI
        if (shopUI != null)
        {
            shopUI.UpdateUI();
            shopUI.InitializeShopItems();
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取剩余免费刷新次数
    /// </summary>
    public int GetFreeRefreshesLeft()
    {
        return freeRefreshesLeft;
    }
    
    /// <summary>
    /// 获取刷新费用
    /// </summary>
    public int GetRefreshCost()
    {
        return refreshCost;
    }
    
    /// <summary>
    /// 获取商店大小
    /// </summary>
    public int GetShopSize()
    {
        return shopSize;
    }
    
    /// <summary>
    /// 设置ShopUI引用
    /// </summary>
    public void SetShopUI(ShopUI ui)
    {
        shopUI = ui;
    }
    
    /// <summary>
    /// 处理商店项锁定状态变化
    /// </summary>
    private void HandleItemLockChanged(int index, bool locked)
    {
        // 可以在这里添加锁定逻辑，例如在刷新时保留锁定项目
        Debug.Log($"商店项 {index} 锁定状态变为: {locked}");
    }
    
    /// <summary>
    /// 获取调整后的稀有度概率
    /// </summary>
    private float[] GetAdjustedRarityChances()
    {
        // 复制原始概率
        float[] adjusted = (float[])rarityChances.Clone();
        
        // 根据玩家等级调整概率
        GameManager gameManager = GameManager.Instance;
        int playerLevel = gameManager != null ? gameManager.GetLevel() : 1;
        
        // 随着等级提高，增加高稀有度的概率
        if (playerLevel >= 3)
        {
            // 提高2星以上的概率
            adjusted[1] += 0.05f;
            adjusted[0] -= 0.05f;
        }
        
        if (playerLevel >= 5)
        {
            // 提高3星以上的概率
            adjusted[2] += 0.05f;
            adjusted[0] -= 0.05f;
        }
        
        if (playerLevel >= 7)
        {
            // 提高4星以上的概率
            adjusted[3] += 0.03f;
            adjusted[0] -= 0.03f;
        }
        
        if (playerLevel >= 9)
        {
            // 提高5星概率
            adjusted[4] += 0.02f;
            adjusted[0] -= 0.02f;
        }
        
        // 确保概率总和为1
        float sum = 0;
        for (int i = 0; i < adjusted.Length; i++)
        {
            adjusted[i] = Mathf.Max(0, adjusted[i]); // 确保不会小于0
            sum += adjusted[i];
        }
        
        // 归一化
        for (int i = 0; i < adjusted.Length; i++)
        {
            adjusted[i] /= sum;
        }
        
        return adjusted;
    }
    
    /// <summary>
    /// 随机获取稀有度索引
    /// </summary>
    private int GetRandomRarityIndex(float[] rarityChances)
    {
        float random = Random.value;
        float cumulativeChance = 0;
        
        for (int i = 0; i < rarityChances.Length; i++)
        {
            cumulativeChance += rarityChances[i];
            if (random < cumulativeChance)
            {
                return i;
            }
        }
        
        return 0; // 默认返回最低稀有度
    }
    
    /// <summary>
    /// 从角色池中随机获取角色
    /// </summary>
    private GameObject GetRandomCharacterFromPool(int poolIndex)
    {
        if (poolIndex < 0 || poolIndex >= characterPools.Length || characterPools[poolIndex].Count == 0)
        {
            // 如果指定稀有度没有角色，降级到较低稀有度
            while (poolIndex >= 0)
            {
                poolIndex--;
                if (poolIndex >= 0 && characterPools[poolIndex].Count > 0)
                {
                    break;
                }
            }
            
            // 如果所有低稀有度都没有角色，返回空
            if (poolIndex < 0)
            {
                return null;
            }
        }
        
        // 随机选择一个角色
        int randomIndex = Random.Range(0, characterPools[poolIndex].Count);
        return characterPools[poolIndex][randomIndex];
    }
    
    /// <summary>
    /// 处理玩家升级
    /// </summary>
    private void HandlePlayerLevelUp(int newLevel, int goldReward)
    {
        // 给予免费刷新
        freeRefreshesLeft += freeRefreshesPerLevel;
        
        // 自动打开商店UI
        OpenShop();
    }
}

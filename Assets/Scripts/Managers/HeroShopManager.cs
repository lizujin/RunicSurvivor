// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 基于Heros.json的商店管理器，处理英雄数据加载和商店逻辑，不负责UI展示
/// </summary>
public class HeroShopManager : MonoBehaviour
{
    [Header("商店设置")]
    public int shopSize = 3;                // 商店每次刷新的英雄数量
    public int freeRefreshesPerLevel = 1;   // 每升级免费刷新次数
    public int refreshCost = 2;             // 刷新花费
    
    [Header("稀有度概率")]
    public float[] rarityChances = new float[5] { 0.40f, 0.30f, 0.20f, 0.08f, 0.02f }; // 对应1-5星的概率
    
    // 当前商店内容和状态
    private List<Hero> shopHeroes = new List<Hero>();
    private List<GameObject> shopPrefabs = new List<GameObject>(); // 缓存英雄预制体
    private List<Character> shopCharacters = new List<Character>(); // 缓存Character组件，用于兼容ShopItemUI
    private int freeRefreshesLeft = 0;
    private bool isShopOpen = false;
    
    // 引用
    private HeroDataManager heroDataManager;
    private UIManager uiManager;
    private ShopUI shopUI;
    
    // 单例实现
    private static HeroShopManager _instance;
    public static HeroShopManager Instance => _instance;
    
    private void Awake()
    {
        // 单例设置
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // 获取或创建HeroDataManager
        heroDataManager = FindObjectOfType<HeroDataManager>();
        if (heroDataManager == null)
        {
            GameObject heroDataManagerObj = new GameObject("HeroDataManager");
            heroDataManager = heroDataManagerObj.AddComponent<HeroDataManager>();
        }
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
    }
    
    // 存储当前创建的商店UI实例
    private GameObject currentShopUIInstance;
    
    /// <summary>
    /// 打开商店
    /// </summary>
    public void OpenShop()
    {
        if (!isShopOpen)
        {
            isShopOpen = true;
            
            // 获取UIManager实例
            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }
            
            // 刷新商店内容
            GenerateShopItems();
            
            // 创建商店UI
            if (uiManager != null && uiManager.shopPrefab != null && uiManager.uiRoot != null)
            {
                Debug.Log("HeroShopManager: 创建商店UI实例");
                
                // 如果已有实例，先销毁
                if (currentShopUIInstance != null)
                {
                    Destroy(currentShopUIInstance);
                }
                
                // 实例化商店UI预制体
                currentShopUIInstance = Instantiate(uiManager.shopPrefab, uiManager.uiRoot);
                
                // 获取ShopUI组件
                shopUI = currentShopUIInstance.GetComponent<ShopUI>();
                if (shopUI != null)
                {
                    // 设置HeroShopManager引用
                    shopUI.SetHeroShopManager(this);
                    
                    // 显示商店UI
                    shopUI.ShowShop();
                }
                else
                {
                    Debug.LogError("HeroShopManager: 商店预制体上没有ShopUI组件");
                }
            }
            else
            {
                Debug.LogError("无法创建商店UI：UIManager、shopPrefab或uiRoot为空");
            }
        }
    }
    
    /// <summary>
    /// 获取当前商店UI实例
    /// </summary>
    public GameObject GetCurrentShopUIInstance()
    {
        return currentShopUIInstance;
    }
    
    /// <summary>
    /// 关闭商店
    /// </summary>
    public void CloseShop()
    {
        if (isShopOpen)
        {
            isShopOpen = false;
            
            // 隐藏商店UI
            if (shopUI != null)
            {
                shopUI.HideShop();
            }
            
            // 可选择延迟销毁UI实例
            if (currentShopUIInstance != null)
            {
                // 如果不想立即销毁，可以使用Invoke延迟销毁
                // Invoke("DestroyShopUI", 1.0f);
                DestroyShopUI();
            }
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
    public bool RefreshShop()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return false;
        
        // 检查是否有免费刷新或者足够的金币
        if (freeRefreshesLeft <= 0 && gameManager.GetGold() < refreshCost)
        {
            Debug.Log("金币不足，无法刷新商店");
            if (shopUI != null)
            {
                shopUI.ShowNotEnoughGoldMessage();
            }
            return false;
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
        
        // 清除当前商店物品数据
        ClearShopItems();
        
        // 生成新的商店内容
        GenerateShopItems();
        
        // 通知UI更新
        if (shopUI != null)
        {
            shopUI.UpdateUI();
            shopUI.InitializeShopItems();
        }
        
        return true;
    }
    
    /// <summary>
    /// 清除商店内容
    /// </summary>
    private void ClearShopItems()
    {
        shopHeroes.Clear();
        shopPrefabs.Clear();
        shopCharacters.Clear();
    }
    
    /// <summary>
    /// 生成商店内容(只处理数据，不负责UI)
    /// </summary>
    private void GenerateShopItems()
    {
        if (heroDataManager == null) 
        {
            Debug.LogError("商店初始化失败：缺少HeroDataManager组件");
            return;
        }
        
        // 获取玩家当前金币
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;
        
        int playerGold = gameManager.GetGold();
        
        // 获取调整后的稀有度概率
        float[] adjustedRarityChances = GetAdjustedRarityChances();
        
        // 清除旧数据
        ClearShopItems();
        
        // 创建一个列表，存储所有可购买的英雄，避免重复
        List<Hero> availableHeroes = new List<Hero>();
        
        // 获取所有可购买的英雄
        for (int cost = 1; cost <= 5; cost++)
        {
            // 只考虑玩家能买得起的英雄
            if (cost <= playerGold)
            {
                List<Hero> heroesOfCost = heroDataManager.GetHeroesByCost(cost);
                if (heroesOfCost != null && heroesOfCost.Count > 0)
                {
                    availableHeroes.AddRange(heroesOfCost);
                }
            }
        }
        
        // 如果没有任何可购买英雄，至少显示最低级的英雄
        if (availableHeroes.Count == 0)
        {
            List<Hero> cheapestHeroes = heroDataManager.GetHeroesByCost(1);
            if (cheapestHeroes != null && cheapestHeroes.Count > 0)
            {
                availableHeroes.AddRange(cheapestHeroes);
            }
        }
        
        // 完全随机打乱英雄列表
        availableHeroes = availableHeroes.OrderBy(x => UnityEngine.Random.value).ToList();
        
        Debug.Log($"找到 {availableHeroes.Count} 个可用英雄");
        
        // 严格限制为商店尺寸（最多3个）
        int heroesToGenerate = Mathf.Min(shopSize, availableHeroes.Count);
        
        Debug.Log($"将生成 {heroesToGenerate} 个商店项目");
        
        // 生成英雄项目，确保不超过shopSize（3个）
        for (int i = 0; i < heroesToGenerate; i++)
        {
            if (i >= availableHeroes.Count)
                break;
                
            Hero hero = availableHeroes[i];
            
            if (hero == null)
                continue;
            
            // 加载英雄预制体
            GameObject prefab = null;
            Character character = null;
            
            if (!string.IsNullOrEmpty(hero.prefab))
            {
                prefab = Resources.Load<GameObject>(hero.prefab);
                if (prefab != null)
                {
                    character = prefab.GetComponent<Character>();
                    if (character == null)
                    {
                        Debug.LogWarning($"英雄预制体 {hero.prefab} 没有Character组件");
                    }
                }
                else
                {
                    Debug.LogError($"无法加载英雄预制体: {hero.prefab}");
                }
            }
            
            // 添加数据到列表
            shopHeroes.Add(hero);
            shopPrefabs.Add(prefab);
            shopCharacters.Add(character);
        }
        
        // 确保商店列表大小与shopSize一致（3个），如果数量不足则添加空值
        while (shopHeroes.Count < shopSize)
        {
            shopHeroes.Add(null);
            shopPrefabs.Add(null);
            shopCharacters.Add(null);
        }
        
        // 严格确保商店列表不会超过shopSize（3个）
        while (shopHeroes.Count > shopSize)
        {
            shopHeroes.RemoveAt(shopHeroes.Count - 1);
            shopPrefabs.RemoveAt(shopPrefabs.Count - 1);
            shopCharacters.RemoveAt(shopCharacters.Count - 1);
        }
        
        Debug.Log($"商店生成完成，共 {shopHeroes.Count} 个项目");
    }
    
    /// <summary>
    /// 获取商店项目英雄数据
    /// </summary>
    public Hero GetShopHero(int index)
    {
        if (index >= 0 && index < shopHeroes.Count)
        {
            return shopHeroes[index];
        }
        return null;
    }
    
    /// <summary>
    /// 购买英雄
    /// </summary>
    public bool PurchaseHero(int index)
    {
        if (index < 0 || index >= shopHeroes.Count)
            return false;
        
        Hero hero = shopHeroes[index];
        if (hero == null)
            return false;
            
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return false;
        
        // 检查金币是否足够
        int cost = hero.cost;
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
        
        // 获取预制体
        GameObject heroPrefab = shopPrefabs[index];
        
        if (heroPrefab == null)
        {
            Debug.LogError($"无法加载英雄预制体: {hero.prefab}");
            return false;
        }
        
        // 添加英雄到队伍
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.AddCharacterToTeam(heroPrefab);
        }
        
        // 标记该英雄已购买
        shopHeroes[index] = null;
        shopPrefabs[index] = null;
        shopCharacters[index] = null;
        
        // 购买成功后关闭商店UI
        CloseShop();
        
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
    /// 处理玩家升级
    /// </summary>
    private void HandlePlayerLevelUp(int newLevel, int goldReward)
    {
        // 给予免费刷新
        freeRefreshesLeft += freeRefreshesPerLevel;
        
        // 自动打开商店UI
        OpenShop();
    }
    
    //============================
    // 以下是为了兼容ShopItemUI而添加的方法
    //============================
    
    /// <summary>
    /// 获取商店项目的Character组件（兼容ShopItemUI）
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
    /// 获取商店项目预制体（兼容ShopItemUI）
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
    /// 购买角色（兼容ShopItemUI - PurchaseCharacter重命名方法）
    /// </summary>
    public void PurchaseCharacter(int index)
    {
        PurchaseHero(index); // 直接调用原有方法
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
}

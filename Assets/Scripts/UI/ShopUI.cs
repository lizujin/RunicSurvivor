// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 商店UI控制器
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI引用")]
    public Text titleText;                    // 商店标题
    public Transform characterListContainer;  // 棋子列表容器
    public Button refreshButton;              // 刷新按钮
    public Text refreshCostText;              // 刷新费用文本
    public Button closeButton;                // 关闭按钮
    public GameObject shopItemPrefab;         // 棋子项目预制体
    public Text goldText;                     // 金币文本
    public Text levelText;                    // 等级文本
    
    [Header("设置")]
    public Color lockedItemColor = new Color(0.9f, 0.9f, 0.4f, 1f); // 锁定项目颜色
    
    // 商店管理器引用
    private HeroShopManager heroShopManager;
    
    // 当前商店项目列表
    private List<ShopItemUI> shopItems = new List<ShopItemUI>();
    
    // 商店项目锁定状态
    private bool[] itemLockedState;
    
    // 标记是否已经初始化
    private bool isInitialized = false;

    // 提供公开的方法来设置HeroShopManager
    public void SetHeroShopManager(HeroShopManager manager)
    {
        heroShopManager = manager;
        Debug.Log("ShopUI: 已设置HeroShopManager引用");
    }

    // 获取当前HeroShopManager引用
    public HeroShopManager GetHeroShopManager()
    {
        return heroShopManager;
    }

    private void Awake()
    {
        // 从该函数移除初始化逻辑，改为在需要时调用Initialize方法
        Debug.Log("ShopUI.Awake() 被调用");
        Initialize();
    }

    /// <summary>
    /// 强制初始化UI元素，可以在任何时候调用以确保UI已设置
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("ShopUI 已初始化，跳过重复初始化");
            return;
        }
        
        Debug.Log("ShopUI.Initialize() 被调用");
        
        // 获取商店管理器
        if (heroShopManager == null)
        {
            heroShopManager = FindObjectOfType<HeroShopManager>();
            if (heroShopManager == null)
            {
                Debug.LogError("初始化失败: HeroShopManager不存在");
                Debug.LogError("找不到HeroShopManager实例，商店功能可能无法正常工作");
            }
            else
            {
                Debug.Log("ShopUI已找到HeroShopManager实例");
            }
        }
        else
        {
            Debug.Log("ShopUI使用已设置的HeroShopManager实例");
        }
        
        // 初始化UI元素
        if (titleText != null)
        {
            titleText.text = "商店";
        }
        else
        {
            Debug.LogError("ShopUI titleText未设置");
        }
        
        // 设置按钮事件
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners(); // 移除所有现有监听以防重复添加
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
            Debug.Log("刷新按钮事件设置成功");
        }
        else
        {
            Debug.LogError("ShopUI refreshButton未设置");
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners(); // 移除所有现有监听以防重复添加
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            Debug.Log("关闭按钮事件设置成功");
        }
        else
        {
            Debug.LogError("ShopUI closeButton未设置");
        }
        
        isInitialized = true;
        Debug.Log("ShopUI初始化完成");
    }
        
    private void OnEnable()
    {
        Debug.Log("ShopUI.OnEnable() 被调用");
        
        // 确保组件已初始化
        if (!isInitialized)
        {
            Debug.Log("ShopUI在OnEnable中尚未初始化，执行初始化");
            Initialize();
        }
        
        if (heroShopManager != null)
        {
            Debug.Log("ShopUI.OnEnable: 初始化商店物品");
            InitializeShopItems();
        }
        else
        {
            Debug.LogError("ShopUI.OnEnable: heroShopManager为空，尝试重新获取");
            heroShopManager = FindObjectOfType<HeroShopManager>();
            if (heroShopManager != null)
            {
                Debug.Log("ShopUI.OnEnable: 重新获取heroShopManager成功");
                InitializeShopItems();
            }
            else
            {
                Debug.LogError("ShopUI.OnEnable: 无法获取HeroShopManager，商店物品将不会被初始化");
            }
        }
    }
        
    private void OnDisable()
    {
        ClearShopItems();
    }
        
    /// <summary>
    /// 初始化商店物品
    /// </summary>
    public void InitializeShopItems()
    {
        // 确保heroShopManager存在
        if (heroShopManager == null)
        {
            Debug.LogError("InitializeShopItems失败: heroShopManager不存在");
            return;
        }
        
        // 检查是否已经有商店项目且数量匹配
        if (shopItems.Count > 0 && shopItems.Count == heroShopManager.shopSize)
        {
            Debug.Log($"商店项目已存在且数量匹配({shopItems.Count})，更新现有项目");
            
            // 更新现有项目而不是重新创建
            for (int i = 0; i < shopItems.Count; i++)
            {
                if (shopItems[i] != null)
                {
                    shopItems[i].RefreshDisplay();
                }
            }
            
            // 更新刷新费用显示
            UpdateRefreshCostText();
            return;
        }
        
        Debug.Log("初始化新的商店项目");
        
        // 清除现有项目
        ClearShopItems();
        
        // 获取商店大小
        int shopSize = heroShopManager.shopSize;
        Debug.Log($"商店大小: {shopSize}");
        
        // 初始化锁定状态数组
        itemLockedState = new bool[shopSize];
        
        // 根据商店内容创建UI项目
        for (int i = 0; i < shopSize; i++)
        {
            CreateShopItemUI(i);
        }
        
        // 更新刷新费用显示
        UpdateRefreshCostText();
    }
    
    /// <summary>
    /// 创建商店项目UI
    /// </summary>
    private void CreateShopItemUI(int index)
    {
        if (characterListContainer == null || shopItemPrefab == null)
        {
            Debug.LogError("CreateShopItemUI: 容器或预制体为空");
            return;
        }
        
        try
        {
            // 首先检查容器是否有效
            if (characterListContainer.gameObject.scene.name == null)
            {
                Debug.LogError("CreateShopItemUI: 容器不在活动场景中，可能是预制体");
                return;
            }
            
            // 先实例化到场景中，然后再设置父对象
            GameObject itemObject = Instantiate(shopItemPrefab);
            if (itemObject == null)
            {
                Debug.LogError("CreateShopItemUI: 商店项目实例化失败");
                return;
            }
            
            // 设置父对象
            itemObject.transform.SetParent(characterListContainer, false);
            
            // 获取ShopItemUI组件
            ShopItemUI itemUI = itemObject.GetComponent<ShopItemUI>();
            if (itemUI == null)
            {
                Debug.LogError("CreateShopItemUI: 商店项目预制体上没有ShopItemUI组件");
                Destroy(itemObject);
                return;
            }
            
            // 设置商店项数据
            itemUI.SetHeroShopManager(heroShopManager);
            itemUI.SetItemIndex(index);
            itemUI.OnLockStateChanged += HandleItemLockStateChanged;
            
            // 添加到列表
            shopItems.Add(itemUI);
            
            // 设置锁定状态
            itemUI.SetLocked(itemLockedState[index]);
            
            Debug.Log($"成功创建商店项UI，索引：{index}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CreateShopItemUI失败: {e.Message}\n{e.StackTrace}");
        }
    }
    
    /// <summary>
    /// 清除所有商店项目
    /// </summary>
    private void ClearShopItems()
    {
        if (shopItems != null && shopItems.Count > 0)
        {
            Debug.Log($"清除商店项目，当前项目数量: {shopItems.Count}");
            
            foreach (ShopItemUI item in shopItems)
            {
                if (item != null)
                {
                    // 移除事件监听
                    item.OnLockStateChanged -= HandleItemLockStateChanged;
                    
                    // 销毁游戏对象
                    if (item.gameObject != null)
                    {
                        Debug.Log($"销毁商店项目: {item.gameObject.name}");
                        Destroy(item.gameObject);
                    }
                }
            }
            
            // 清空列表
            shopItems.Clear();
            Debug.Log("商店项目列表已清空");
        }
    }
    
    /// <summary>
    /// 处理刷新按钮点击
    /// </summary>
    private void OnRefreshButtonClicked()
    {
        if (heroShopManager != null)
        {
            // 保存锁定状态
            SaveLockedStates();
            
            // 刷新商店
            heroShopManager.RefreshShop();
            
            // 重新创建UI项目
            InitializeShopItems();
            
            // 恢复锁定状态
            RestoreLockedStates();
        }
    }
    
    /// <summary>
    /// 处理关闭按钮点击
    /// </summary>
    private void OnCloseButtonClicked()
    {
        Debug.Log("关闭按钮被点击");
        
        if (heroShopManager != null)
        {
            // 调用CloseShop方法
            try
            {
                heroShopManager.CloseShop();
                Debug.Log("成功调用HeroShopManager.CloseShop()");
            }
            catch (System.Exception e)
            {
                Debug.LogError("调用HeroShopManager.CloseShop()失败: " + e.Message);
                
                // 如果直接调用失败，尝试替代方案：隐藏当前面板
                Debug.Log("尝试备用方案：直接隐藏商店界面");
                gameObject.SetActive(false);
                
                // 恢复敌人生成
                EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
                if (enemyManager != null)
                {
                    enemyManager.ResumeSpawning();
                }
            }
        }
        else
        {
            Debug.LogError("HeroShopManager实例为空，无法关闭商店");
            // 隐藏当前面板
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 显示商店界面
    /// </summary>
    public void ShowShop()
    {
        Debug.Log("显示商店界面");
        
        // 只有在商店未激活时才做初始化
        bool wasInactive = !gameObject.activeSelf;
        
        gameObject.SetActive(true);
        
        // 暂停敌人生成
        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.PauseSpawning();
        }
        
        UpdateUI();
        
        // 只有在商店之前未激活时才初始化商店项目
        // 这可以防止重复初始化，因为OnEnable会在激活时调用InitializeShopItems
        if (wasInactive)
        {
            Debug.Log("商店之前未激活，执行初始化商店项目");
            InitializeShopItems();
        }
        else
        {
            Debug.Log("商店已处于激活状态，跳过重复初始化");
            // 如果已经激活，只进行UI更新而不重建物品
            if (shopItems != null && shopItems.Count > 0)
            {
                for (int i = 0; i < shopItems.Count; i++)
                {
                    if (shopItems[i] != null)
                    {
                        shopItems[i].RefreshDisplay();
                    }
                }
                UpdateRefreshCostText();
            }
        }
    }
    
    /// <summary>
    /// 隐藏商店界面
    /// </summary>
    public void HideShop()
    {
        gameObject.SetActive(false);
        
        // 恢复敌人生成
        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.ResumeSpawning();
        }
    }

    /// <summary>
    /// 更新所有UI元素
    /// </summary>
    public void UpdateUI()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;
        
        UpdateGoldText(gameManager.GetGold());
        UpdateLevelText(gameManager.GetLevel());
        UpdateRefreshCostText();
    }
    
    /// <summary>
    /// 更新金币显示
    /// </summary>
    public void UpdateGoldText(int gold)
    {
        if (goldText != null)
        {
            goldText.text = "金币:" + gold.ToString();
        }
    }
    
    /// <summary>
    /// 更新等级显示
    /// </summary>
    public void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = "等级:" + level;
        }
    }
    
    /// <summary>
    /// 更新刷新费用文本
    /// </summary>
    private void UpdateRefreshCostText()
    {
        if (refreshCostText != null && heroShopManager != null)
        {
            int freeRefreshes = heroShopManager.GetFreeRefreshesLeft();
            int cost = heroShopManager.refreshCost;
            
            if (freeRefreshes > 0)
            {
                refreshCostText.text = "免费(" + freeRefreshes + ")";
            }
            else
            {
                refreshCostText.text = cost.ToString();
            }
        }
    }
    
    /// <summary>
    /// 显示金币不足提示
    /// </summary>
    public void ShowNotEnoughGoldMessage()
    {
        Debug.Log("金币不足，无法刷新商店");
        // 这里可以添加UI提示，如飘字或消息框
    }
    
    /// <summary>
    /// 处理项目锁定状态改变
    /// </summary>
    private void HandleItemLockStateChanged(int index, bool locked)
    {
        if (index >= 0 && index < itemLockedState.Length)
        {
            itemLockedState[index] = locked;
        }
    }
    
    /// <summary>
    /// 保存商店项目的锁定状态
    /// </summary>
    private void SaveLockedStates()
    {
        for (int i = 0; i < shopItems.Count && i < itemLockedState.Length; i++)
        {
            ShopItemUI item = shopItems[i];
            if (item != null)
            {
                itemLockedState[i] = item.IsLocked;
            }
        }
    }
    
    /// <summary>
    /// 恢复商店项目的锁定状态
    /// </summary>
    private void RestoreLockedStates()
    {
        for (int i = 0; i < shopItems.Count && i < itemLockedState.Length; i++)
        {
            ShopItemUI item = shopItems[i];
            if (item != null)
            {
                item.SetLocked(itemLockedState[i]);
            }
        }
    }
}

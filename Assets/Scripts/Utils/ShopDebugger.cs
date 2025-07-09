// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于调试商店UI的工具脚本，可以放在任何GameObject上
/// </summary>
public class ShopDebugger : MonoBehaviour
{
    // 单例实现
    private static ShopDebugger _instance;
    public static ShopDebugger Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // 在Start中自动添加Debug键位监听
    private void Start()
    {
        Debug.Log("ShopDebugger: 已启用，按 F9 手动打开商店，按 F10 切换查找方式");
    }
    
    // 允许使用不同方式查找和打开商店
    private bool useAlternativeMethod = false;
    
    // 在每一帧中检测按键输入
    private void Update()
    {
        // F9 键用于手动打开商店
        if (Input.GetKeyDown(KeyCode.F9))
        {
            TryOpenShop();
        }
        
        // F10 键用于切换查找方式
        if (Input.GetKeyDown(KeyCode.F10))
        {
            useAlternativeMethod = !useAlternativeMethod;
            Debug.Log($"ShopDebugger: 切换到{(useAlternativeMethod ? "备用" : "主要")}查找方式");
        }
    }
    
    /// <summary>
    /// 尝试打开商店
    /// </summary>
    public void TryOpenShop()
    {
        Debug.Log("ShopDebugger: 尝试打开商店");
        
        if (useAlternativeMethod)
        {
            OpenShopAlternative();
        }
        else
        {
            OpenShopNormal();
        }
    }
    
    /// <summary>
    /// 使用主要方式尝试打开商店
    /// </summary>
    private void OpenShopNormal()
    {
        HeroShopManager heroShopManager = FindObjectOfType<HeroShopManager>();
        if (heroShopManager != null)
        {
            Debug.Log("ShopDebugger: 找到 HeroShopManager，尝试打开商店");
            heroShopManager.OpenShop();
        }
        else
        {
            Debug.LogError("ShopDebugger: 未找到 HeroShopManager 组件，请确保场景中存在HeroShopManager");
        }
    }
    
    /// <summary>
    /// 使用备用方式尝试打开商店
    /// </summary>
    private void OpenShopAlternative()
    {
        // 尝试查找当前已经存在的ShopUI实例
        ShopUI existingShopUI = FindObjectOfType<ShopUI>();
        if (existingShopUI != null)
        {
            Debug.Log("ShopDebugger: 找到现有的ShopUI实例，尝试显示");
            existingShopUI.gameObject.SetActive(true);
            existingShopUI.Initialize();
            existingShopUI.InitializeShopItems();
            return;
        }

        // 如果没有找到ShopUI实例，尝试使用UIManager创建
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null && uiManager.shopPrefab != null && uiManager.uiRoot != null)
        {
            Debug.Log("ShopDebugger: 找到UIManager，尝试创建ShopUI");
            GameObject shopInstance = Instantiate(uiManager.shopPrefab, uiManager.uiRoot);
            ShopUI shopUI = shopInstance.GetComponent<ShopUI>();
            
            if (shopUI != null)
            {
                Debug.Log("ShopDebugger: 创建ShopUI成功，尝试初始化");
                
                // 查找HeroShopManager并设置到ShopUI中
                HeroShopManager heroShopManager = FindObjectOfType<HeroShopManager>();
                if (heroShopManager != null)
                {
                    shopUI.SetHeroShopManager(heroShopManager);
                }
                else
                {
                    Debug.LogError("ShopDebugger: 未找到HeroShopManager");
                }
                
                shopUI.Initialize();
                shopUI.InitializeShopItems();
                shopUI.ShowShop();
            }
            else
            {
                Debug.LogError("ShopDebugger: 商店预制体上没有ShopUI组件");
                Destroy(shopInstance); // 清理不可用的实例
            }
        }
        else
        {
            // 最后尝试使用HeroShopManager
            Debug.LogError("ShopDebugger: UIManager配置不完整，尝试使用HeroShopManager");
            HeroShopManager heroShopManager = FindObjectOfType<HeroShopManager>();
            if (heroShopManager != null)
            {
                Debug.Log("ShopDebugger: 找到 HeroShopManager，尝试打开商店");
                heroShopManager.OpenShop();
            }
            else
            {
                Debug.LogError("ShopDebugger: 未找到 HeroShopManager");
            }
        }
    }
    
    /// <summary>
    /// 调试商店状态
    /// </summary>
    public void DebugShopStatus()
    {
        Debug.Log("ShopDebugger: ===== 商店状态调试信息 =====");
        
        // 检查UIManager
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            Debug.Log("找到UIManager");
            
            if (uiManager.shopPrefab != null)
            {
                Debug.Log("UIManager.shopPrefab有效");
                
                ShopUI shopUIInPrefab = uiManager.shopPrefab.GetComponent<ShopUI>();
                if (shopUIInPrefab != null)
                {
                    Debug.Log("商店预制体包含ShopUI组件");
                }
                else
                {
                    Debug.LogError("警告：商店预制体没有ShopUI组件");
                }
            }
            else
            {
                Debug.LogError("UIManager.shopPrefab为空");
            }
            
            if (uiManager.uiRoot != null)
            {
                Debug.Log("UIManager.uiRoot有效");
            }
            else
            {
                Debug.LogError("UIManager.uiRoot为空");
            }
        }
        else
        {
            Debug.LogError("未找到UIManager");
        }
        
        // 检查当前场景中的ShopUI实例
        ShopUI[] shopUIs = FindObjectsOfType<ShopUI>();
        Debug.Log($"场景中找到 {shopUIs.Length} 个ShopUI实例");
        
        for (int i = 0; i < shopUIs.Length; i++)
        {
            ShopUI ui = shopUIs[i];
            Debug.Log($"ShopUI #{i}: {ui.gameObject.name} - 激活状态: {ui.gameObject.activeSelf}");
            
            if (ui.GetHeroShopManager() != null)
            {
                Debug.Log($"ShopUI #{i} 已关联HeroShopManager");
            }
            else
            {
                Debug.LogError($"ShopUI #{i} 未关联HeroShopManager");
            }
            
            // 检查基本UI组件
            if (ui.characterListContainer != null)
            {
                Debug.Log($"商店项容器存在，子对象数: {ui.characterListContainer.childCount}");
            }
            else
            {
                Debug.LogError("商店项容器不存在");
            }
        }
        
        Debug.Log("ShopDebugger: ===== 商店管理器状态 =====");
        
        HeroShopManager heroShopManager = FindObjectOfType<HeroShopManager>();
        if (heroShopManager != null)
        {
            Debug.Log("HeroShopManager存在");
            GameObject shopInstance = heroShopManager.GetCurrentShopUIInstance();
            
            if (shopInstance != null)
            {
                Debug.Log($"HeroShopManager已实例化商店UI: {shopInstance.name}");
            }
            else
            {
                Debug.Log("HeroShopManager尚未实例化商店UI");
            }
        }
        else
        {
            Debug.LogError("HeroShopManager不存在 - 应用中需要依赖HeroShopManager来打开商店UI");
        }
    }
}

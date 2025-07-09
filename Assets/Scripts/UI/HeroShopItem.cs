// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 从Heros.json加载的英雄商店项目UI
/// </summary>
public class HeroShopItem : MonoBehaviour
{
    [Header("UI引用")]
    public Image heroIcon;                // 英雄图标
    public Text heroNameText;             // 英雄名称
    public Text costText;                 // 花费文本
    public Button buyButton;              // 购买按钮
    public Button lockButton;             // 锁定按钮
    public Image lockIcon;                // 锁定图标
    public Image background;              // 背景图片
    
    [Header("稀有度颜色")]
    public Color[] rarityColors = new Color[5] {
        new Color(0.7f, 0.7f, 0.7f),   // 1星 - 灰色
        new Color(0.3f, 0.7f, 0.3f),   // 2星 - 绿色
        new Color(0.3f, 0.5f, 0.9f),   // 3星 - 蓝色
        new Color(0.7f, 0.3f, 0.9f),   // 4星 - 紫色
        new Color(0.9f, 0.8f, 0.3f)    // 5星 - 金色
    };
    
    // 商店管理器引用（互斥）
    private ShopManager shopManager;
    private HeroShopManager heroShopManager;
    
    // 英雄数据
    private Hero heroData;
    private int itemIndex;
    
    // 锁定状态
    private bool isLocked = false;
    public bool IsLocked => isLocked;
    
    // 锁定状态改变事件
    public delegate void LockStateChangedHandler(int index, bool locked);
    public event LockStateChangedHandler OnLockStateChanged;
    
    private void Awake()
    {
        // 设置按钮监听
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
        
        if (lockButton != null)
        {
            lockButton.onClick.AddListener(OnLockButtonClicked);
        }
    }
    
    private void OnDestroy()
    {
        // 移除按钮监听
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyButtonClicked);
        }
        
        if (lockButton != null)
        {
            lockButton.onClick.RemoveListener(OnLockButtonClicked);
        }
    }
    
    /// <summary>
    /// 设置ShopManager引用
    /// </summary>
    public void SetShopManager(ShopManager manager)
    {
        shopManager = manager;
        heroShopManager = null; // 确保两个引用互斥
    }
    
    /// <summary>
    /// 设置HeroShopManager引用
    /// </summary>
    public void SetHeroShopManager(HeroShopManager manager)
    {
        heroShopManager = manager;
        shopManager = null; // 确保两个引用互斥
    }
    
    /// <summary>
    /// 设置英雄数据
    /// </summary>
    public void SetHeroData(Hero hero, int index)
    {
        heroData = hero;
        itemIndex = index;
        
        if (hero == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        
        // 设置名称和花费
        if (heroNameText != null)
        {
            heroNameText.text = hero.name;
        }
        
        if (costText != null)
        {
            costText.text = hero.cost.ToString();
        }
        
        // 加载图标
        if (heroIcon != null && !string.IsNullOrEmpty(hero.icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>(hero.icon);
            if (iconSprite != null)
            {
                heroIcon.sprite = iconSprite;
                heroIcon.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"无法加载英雄图标: {hero.icon}");
            }
        }
        
        // 根据cost设置稀有度颜色
        int rarityIndex = Mathf.Clamp(hero.cost - 1, 0, rarityColors.Length - 1);
        if (background != null)
        {
            background.color = rarityColors[rarityIndex];
        }
        
        // 更新锁定状态视觉效果
        UpdateLockVisuals();
    }
    
    /// <summary>
    /// 购买按钮点击处理
    /// </summary>
    private void OnBuyButtonClicked()
    {
        if (heroData == null) return;
        
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.GetGold() < heroData.cost)
        {
            // 显示金币不足提示
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowMessage("金币不足");
            }
            return;
        }
        
        // 根据设置的管理器类型进行相应购买操作
        if (shopManager != null)
        {
            shopManager.PurchaseCharacter(itemIndex);
            gameObject.SetActive(false);
        }
        else if (heroShopManager != null)
        {
            heroShopManager.PurchaseHero(itemIndex);
            gameObject.SetActive(false);
        }
        else
        {
            // 如果两种管理器都没有设置，尝试找到任何可用的管理器
            heroShopManager = HeroShopManager.Instance;
            if (heroShopManager != null)
            {
                heroShopManager.PurchaseHero(itemIndex);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("无法购买角色：未找到任何商店管理器");
            }
        }
    }
    
    /// <summary>
    /// 锁定按钮点击处理
    /// </summary>
    private void OnLockButtonClicked()
    {
        // 切换锁定状态
        SetLocked(!isLocked);
        
        // 触发事件
        OnLockStateChanged?.Invoke(itemIndex, isLocked);
    }
    
    /// <summary>
    /// 设置锁定状态
    /// </summary>
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateLockVisuals();
    }
    
    /// <summary>
    /// 更新锁定状态视觉效果
    /// </summary>
    private void UpdateLockVisuals()
    {
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(isLocked);
        }
    }
    
    /// <summary>
    /// 获取英雄数据
    /// </summary>
    public Hero GetHeroData()
    {
        return heroData;
    }
    
    /// <summary>
    /// 获取英雄预制体路径
    /// </summary>
    public string GetHeroPrefabPath()
    {
        return heroData?.prefab;
    }
}

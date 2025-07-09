// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 商店中单个棋子项目的UI控制器
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI引用")]
    public Image characterImage;           // 棋子图片
    public Text characterNameText;         // 棋子名称
    public TMP_Text costText;              // 棋子花费
    public Text attackText;                // 棋子攻击力
    public Text raceText;                  // 种族特性文本
    public Text classTypeText;             // 流派特性文本
    public Button buyButton;               // 购买按钮
    public Button lockButton;              // 锁定按钮
    public Image lockIcon;                 // 锁定图标
    public Image background;               // 背景图片
    public Image rarityFrame;              // 稀有度框架
    
    [Header("稀有度颜色")]
    public Color[] rarityColors = new Color[5] {
        new Color(0.7f, 0.7f, 0.7f),   // 1星 - 灰色
        new Color(0.3f, 0.7f, 0.3f),   // 2星 - 绿色
        new Color(0.3f, 0.5f, 0.9f),   // 3星 - 蓝色
        new Color(0.7f, 0.3f, 0.9f),   // 4星 - 紫色
        new Color(0.9f, 0.8f, 0.3f)    // 5星 - 金色
    };
    
    // 商店管理器引用 - 互斥引用，仅使用其中一个
    private ShopManager shopManager;
    private HeroShopManager heroShopManager;
    
    // 角色数据
    private Character characterData;
    private GameObject characterPrefab;
    
    // 项目索引
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
        heroShopManager = null; // 清空另一个引用
    }
    
    /// <summary>
    /// 设置HeroShopManager引用
    /// </summary>
    public void SetHeroShopManager(HeroShopManager manager)
    {
        heroShopManager = manager;
        shopManager = null; // 清空另一个引用
    }
    
    /// <summary>
    /// 设置项目索引
    /// </summary>
    public void SetItemIndex(int index)
    {
        itemIndex = index;
        
        // 根据使用的管理器类型获取对应项目的角色数据
        if (shopManager != null)
        {
            characterData = shopManager.GetShopItemCharacter(itemIndex);
            characterPrefab = shopManager.GetShopItemPrefab(itemIndex);
            
            UpdateUI();
        }
        else if (heroShopManager != null)
        {
            characterData = heroShopManager.GetShopItemCharacter(itemIndex);
            characterPrefab = heroShopManager.GetShopItemPrefab(itemIndex);
            
            UpdateUI();
        }
    }
    
    /// <summary>
    /// 更新UI元素
    /// </summary>
    private void UpdateUI()
    {
        if (characterData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        
        // 声明heroData变量到更高的作用域
        Hero heroData = null;
        
        // 根据使用的管理器类型获取Hero数据
        if (heroShopManager != null)
        {
            heroData = heroShopManager.GetShopHero(itemIndex);
        }
        
        // 加载角色图片
        if (characterImage != null)
        {
            
            // 从Hero数据加载图标
            if (heroData != null && !string.IsNullOrEmpty(heroData.icon))
            {
                // 加载Resources中的图标
                Sprite iconSprite = Resources.Load<Sprite>(heroData.icon);
                if (iconSprite != null)
                {
                    characterImage.sprite = iconSprite;
                    characterImage.gameObject.SetActive(true);
                }
                else if (characterData.characterIcon != null)
                {
                    // 如果从Resources加载失败，使用Character的图标作为备选
                    characterImage.sprite = characterData.characterIcon;
                    characterImage.gameObject.SetActive(true);
                    Debug.LogWarning($"无法从{heroData.icon}加载图标，使用Character的图标作为备选");
                }
                else
                {
                    Debug.LogWarning($"无法加载角色图标: {heroData.icon}");
                }
            }
            else if (characterData.characterIcon != null)
            {
                // 使用Character中的图标（兼容原有逻辑）
                characterImage.sprite = characterData.characterIcon;
                characterImage.gameObject.SetActive(true);
            }
        }
        
        // 设置角色名称
        if (characterNameText != null)
        {
            characterNameText.text = characterData.characterName;
        }
        
        // 设置花费
        if (costText != null)
        {
            costText.text = characterData.cost.ToString();
        }
        
        // 设置攻击力
        if (attackText != null)
        {
            attackText.text = $"攻击: {characterData.attack}";
        }
        
        // 设置种族和流派特性
        if (heroShopManager != null && heroData != null)
        {
            
            // 设置种族信息
            if (raceText != null && heroData != null && !string.IsNullOrEmpty(heroData.race))
            {
                raceText.text = $"种族: {heroData.race}";
                raceText.gameObject.SetActive(true);
            }
            else if (raceText != null)
            {
                raceText.gameObject.SetActive(false);
            }
            
            // 设置流派信息
            if (classTypeText != null && heroData != null && !string.IsNullOrEmpty(heroData.classType))
            {
                classTypeText.text = $"符文: {heroData.classType}";
                classTypeText.gameObject.SetActive(true);
            }
            else if (classTypeText != null)
            {
                classTypeText.gameObject.SetActive(false);
            }
        }
        else
        {
            // 如果没有使用HeroShopManager，则隐藏种族和流派信息
            if (raceText != null) raceText.gameObject.SetActive(false);
            if (classTypeText != null) classTypeText.gameObject.SetActive(false);
        }
        
        // 设置稀有度颜色
        int rarityIndex = characterData.tier - 1;
        if (rarityIndex >= 0 && rarityIndex < rarityColors.Length)
        {
            if (rarityFrame != null)
            {
                rarityFrame.color = rarityColors[rarityIndex];
            }
        }
        
        // 更新锁定图标
        UpdateLockVisuals();
    }
    
    /// <summary>
    /// 购买按钮点击处理
    /// </summary>
    private void OnBuyButtonClicked()
    {
        if (characterData == null)
            return;
            
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager不存在，无法购买角色");
            return;
        }

        // 检查金币是否足够
        int currentGold = gameManager.GetGold();
        if (currentGold < characterData.cost)
        {
            // 显示金币不足提示
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowMessage("金币不足");
            }
            return;
        }
        
        // 根据使用的管理器类型来购买角色
        bool purchaseSuccess = false;
        
        if (shopManager != null)
        {
            shopManager.PurchaseCharacter(itemIndex);
            purchaseSuccess = true;
        }
        else if (heroShopManager != null)
        {
            // 直接调用PurchaseHero方法，确保与新版HeroShopManager兼容
            heroShopManager.PurchaseHero(itemIndex);
            purchaseSuccess = true;
        }
        
        // 在购买成功后，禁用自己
        if (purchaseSuccess)
        {
            gameObject.SetActive(false);
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
        
        // 更新背景颜色
        if (background != null)
        {
            Color originalColor = background.color;
            ShopUI shopUI = GetComponentInParent<ShopUI>();
            
            if (isLocked && shopUI != null)
            {
                // 应用锁定颜色
                background.color = shopUI.lockedItemColor;
            }
            else
            {
                // 恢复原始颜色
                background.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a);
            }
        }
    }
    
    /// <summary>
    /// 获取角色数据
    /// </summary>
    public Character GetCharacterData()
    {
        return characterData;
    }
    
    /// <summary>
    /// 获取角色预制体
    /// </summary>
    public GameObject GetCharacterPrefab()
    {
        return characterPrefab;
    }
    
    /// <summary>
    /// 刷新显示内容 - 供外部调用的UI更新方法
    /// </summary>
    public void RefreshDisplay()
    {
        // 根据使用的管理器类型更新角色数据
        if (shopManager != null)
        {
            characterData = shopManager.GetShopItemCharacter(itemIndex);
            characterPrefab = shopManager.GetShopItemPrefab(itemIndex);
        }
        else if (heroShopManager != null)
        {
            characterData = heroShopManager.GetShopItemCharacter(itemIndex);
            characterPrefab = heroShopManager.GetShopItemPrefab(itemIndex);
        }
        
        // 调用现有的UI更新方法
        UpdateUI();
        
        Debug.Log($"商店项目 {itemIndex} 已刷新显示");
    }
}

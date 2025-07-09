// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 用于创建商店棋子项目预制体的辅助类
/// </summary>
[ExecuteInEditMode]
public class ShopItemPrefab : MonoBehaviour
{
    // 商店项目根物体
    public GameObject itemPanel;
    
    // UI元素
    public Image characterImage;
    public Text characterNameText;
    public TMP_Text costText;
    public Button buyButton;
    public Button lockButton;
    public Image lockIcon;
    public Image background;
    public Image rarityFrame;
    
    // 添加ShopItemUI组件
    [ContextMenu("Setup Shop Item UI Components")]
    public void SetupShopItemUIComponents()
    {
        if (gameObject == null) return;
        
        // 添加或获取ShopItemUI组件
        ShopItemUI itemUI = gameObject.GetComponent<ShopItemUI>();
        if (itemUI == null)
        {
            itemUI = gameObject.AddComponent<ShopItemUI>();
        }
        
        // 设置引用
        itemUI.characterImage = characterImage;
        itemUI.characterNameText = characterNameText;
        itemUI.costText = costText;
        itemUI.buyButton = buyButton;
        itemUI.lockButton = lockButton;
        itemUI.lockIcon = lockIcon;
        itemUI.background = background;
        itemUI.rarityFrame = rarityFrame;
        
        Debug.Log("ShopItemUI components setup completed.");
    }
    
    // 创建基本结构
    [ContextMenu("Create Shop Item Structure")]
    public void CreateShopItemStructure()
    {
        // 清理现有结构
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        // 创建主面板
        itemPanel = new GameObject("ItemPanel");
        itemPanel.transform.SetParent(transform, false);
        RectTransform panelRect = itemPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.sizeDelta = new Vector2(180, 240);
        
        // 添加背景
        background = itemPanel.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        // 创建稀有度框架
        GameObject frameObj = new GameObject("RarityFrame");
        frameObj.transform.SetParent(itemPanel.transform, false);
        RectTransform frameRect = frameObj.AddComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;
        
        rarityFrame = frameObj.AddComponent<Image>();
        rarityFrame.color = Color.white;
        rarityFrame.sprite = CreateRectSprite();
        rarityFrame.type = Image.Type.Sliced;
        rarityFrame.pixelsPerUnitMultiplier = 1;
        
        // 创建角色图像区域
        GameObject imageObj = new GameObject("CharacterImage");
        imageObj.transform.SetParent(itemPanel.transform, false);
        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.7f);
        imageRect.anchorMax = new Vector2(0.5f, 0.7f);
        imageRect.sizeDelta = new Vector2(140, 140);
        imageRect.anchoredPosition = Vector2.zero;
        
        characterImage = imageObj.AddComponent<Image>();
        characterImage.preserveAspect = true;
        
        // 创建角色名称文本
        GameObject nameObj = new GameObject("CharacterNameText");
        nameObj.transform.SetParent(itemPanel.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0.4f);
        nameRect.anchorMax = new Vector2(0.5f, 0.4f);
        nameRect.sizeDelta = new Vector2(160, 30);
        nameRect.anchoredPosition = Vector2.zero;
        
        characterNameText.fontSize = 18;
        characterNameText.color = Color.white;
        characterNameText.text = "角色名称";
        
        // 创建购买按钮
        buyButton = CreateButton("BuyButton", itemPanel.transform, new Vector2(0.35f, 0.15f), "购买");
        
        // 创建锁定按钮
        lockButton = CreateButton("LockButton", itemPanel.transform, new Vector2(0.65f, 0.15f), "锁定");
        
        // 创建锁定图标
        GameObject lockIconObj = new GameObject("LockIcon");
        lockIconObj.transform.SetParent(itemPanel.transform, false);
        RectTransform lockIconRect = lockIconObj.AddComponent<RectTransform>();
        lockIconRect.anchorMin = new Vector2(0.9f, 0.9f);
        lockIconRect.anchorMax = new Vector2(0.9f, 0.9f);
        lockIconRect.sizeDelta = new Vector2(30, 30);
        lockIconRect.anchoredPosition = Vector2.zero;
        
        lockIcon = lockIconObj.AddComponent<Image>();
        lockIcon.color = new Color(1, 0.8f, 0.2f);
        // 默认隐藏锁定图标
        lockIcon.gameObject.SetActive(false);
        
        // 创建花费文本
        GameObject costObj = new GameObject("CostText");
        costObj.transform.SetParent(itemPanel.transform, false);
        RectTransform costRect = costObj.AddComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0.1f, 0.9f);
        costRect.anchorMax = new Vector2(0.1f, 0.9f);
        costRect.sizeDelta = new Vector2(50, 30);
        costRect.anchoredPosition = Vector2.zero;
        
        costText = costObj.AddComponent<TextMeshProUGUI>();
        costText.fontSize = 24;
        costText.alignment = TextAlignmentOptions.Center;
        costText.color = new Color(1, 0.8f, 0.2f);
        costText.text = "3";
        
        Debug.Log("Shop item structure created successfully.");
    }
    
    // 创建按钮
    private Button CreateButton(string name, Transform parent, Vector2 anchorPosition, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.sizeDelta = new Vector2(70, 40);
        rect.anchoredPosition = Vector2.zero;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.6f, 0.9f);
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.6f, 0.9f);
        colors.highlightedColor = new Color(0.4f, 0.7f, 1.0f);
        colors.pressedColor = new Color(0.2f, 0.5f, 0.8f);
        button.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return button;
    }
    
    // 创建一个简单的矩形Sprite用于框架
    private Sprite CreateRectSprite()
    {
        Texture2D texture = new Texture2D(10, 10);
        Color[] colors = new Color[100];
        
        // 边框为白色，内部透明
        for (int i = 0; i < 100; i++)
        {
            int x = i % 10;
            int y = i / 10;
            
            if (x == 0 || x == 9 || y == 0 || y == 9)
            {
                colors[i] = Color.white;
            }
            else
            {
                colors[i] = Color.clear;
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, 10, 10),
            new Vector2(0.5f, 0.5f),
            100,
            0,
            SpriteMeshType.FullRect,
            Vector4.one * 3  // 9-slice with 3px border
        );
    }
}

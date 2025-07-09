// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// 商店创建工具 - 提供菜单选项来创建商店UI结构
/// </summary>
public class ShopCreationTool : EditorWindow
{
    [MenuItem("工具/UI创建工具/创建商店界面")]
    static void CreateShopUI()
    {
        // 首先检查是否选中了一个GameObject
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一个游戏对象作为商店UI的父物体", "确定");
            return;
        }
        
        GameObject selectedObject = Selection.activeGameObject;
        
        // 创建商店UI结构
        CreateShopStructure(selectedObject);
        
        // 添加ShopUI组件和设置引用
        SetupShopUIComponents(selectedObject);
        
        Debug.Log("商店UI创建完成");
    }
    
    [MenuItem("工具/UI创建工具/创建商店棋子项目")]
    static void CreateShopItemUI()
    {
        // 首先检查是否选中了一个GameObject
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一个游戏对象作为棋子项目的父物体", "确定");
            return;
        }
        
        GameObject selectedObject = Selection.activeGameObject;
        
        // 创建商店棋子项目结构
        CreateShopItemStructure(selectedObject);
        
        // 添加ShopItemUI组件和设置引用
        SetupShopItemUIComponents(selectedObject);
        
        Debug.Log("商店棋子项目创建完成");
    }
    
    // 创建商店UI结构
    static void CreateShopStructure(GameObject parent)
    {
        // 清理现有结构
        while (parent.transform.childCount > 0)
        {
            DestroyImmediate(parent.transform.GetChild(0).gameObject);
        }
        
        // 创建主面板
        GameObject shopPanel = new GameObject("ShopPanel");
        shopPanel.transform.SetParent(parent.transform, false);
        RectTransform panelRect = shopPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = shopPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // 创建头部区域
        GameObject headerSection = CreateUISection("HeaderSection", shopPanel.transform, 0, 0.85f, 1, 1);
        
        // 创建标题
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(headerSection.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(200, 50);
        titleRect.anchoredPosition = Vector2.zero;
        
        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "商店";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // 创建内容区域
        GameObject contentSection = CreateUISection("ContentSection", shopPanel.transform, 0, 0.15f, 1, 0.85f);
        
        // 创建滚动视图
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(contentSection.transform, false);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(20, 20);
        scrollRect.offsetMax = new Vector2(-20, -20);
        
        ScrollRect scrollComponent = scrollView.AddComponent<ScrollRect>();
        
        // 创建视口
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0.3f);
        
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // 创建内容容器
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = new Vector2(0, 0);
        contentRect.offsetMax = new Vector2(0, 0);
        
        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(180, 240);
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.padding = new RectOffset(10, 10, 10, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;
        
        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // 设置滚动视图引用
        scrollComponent.viewport = viewportRect;
        scrollComponent.content = contentRect;
        scrollComponent.horizontal = false;
        scrollComponent.vertical = true;
        
        // 创建底部区域
        GameObject footerSection = CreateUISection("FooterSection", shopPanel.transform, 0, 0, 1, 0.15f);
        
        // 创建刷新按钮
        Button refreshButton = CreateButton("RefreshButton", footerSection.transform, new Vector2(0.3f, 0.5f), "刷新");
        
        // 创建刷新花费文本
        GameObject costObj = new GameObject("RefreshCostText");
        costObj.transform.SetParent(refreshButton.transform, false);
        RectTransform costRect = costObj.AddComponent<RectTransform>();
        costRect.anchorMin = new Vector2(1, 0.5f);
        costRect.anchorMax = new Vector2(1, 0.5f);
        costRect.sizeDelta = new Vector2(50, 30);
        costRect.anchoredPosition = new Vector2(40, 0);
        
        TMP_Text refreshCostText = costObj.AddComponent<TextMeshProUGUI>();
        refreshCostText.text = "2";
        refreshCostText.fontSize = 24;
        refreshCostText.alignment = TextAlignmentOptions.Center;
        refreshCostText.color = new Color(1, 0.8f, 0.2f);
        
        // 创建关闭按钮
        Button closeButton = CreateButton("CloseButton", footerSection.transform, new Vector2(0.7f, 0.5f), "关闭");
    }
    
    // 创建UI分区
    static GameObject CreateUISection(string name, Transform parent, float minX, float minY, float maxX, float maxY)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);
        
        RectTransform rect = section.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return section;
    }
    
    // 创建按钮
    static Button CreateButton(string name, Transform parent, Vector2 anchorPosition, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.sizeDelta = new Vector2(160, 50);
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
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return button;
    }
    
    // 设置ShopUI组件和引用
    static void SetupShopUIComponents(GameObject gameObject)
    {
        // 添加或获取ShopUI组件
        ShopUI shopUI = gameObject.GetComponent<ShopUI>();
        if (shopUI == null)
        {
            shopUI = gameObject.AddComponent<ShopUI>();
        }
        
        // 设置引用
        Transform shopPanel = gameObject.transform.Find("ShopPanel");
        if (shopPanel != null)
        {
            Transform header = shopPanel.Find("HeaderSection");
            Transform content = shopPanel.Find("ContentSection");
            Transform footer = shopPanel.Find("FooterSection");
            
            if (header != null)
            {
                Transform titleObj = header.Find("TitleText");
                if (titleObj != null)
                {
                    shopUI.titleText = titleObj.GetComponent<Text>();
                }
            }
            
            if (content != null)
            {
                Transform scrollView = content.Find("ScrollView");
                if (scrollView != null)
                {
                    Transform viewport = scrollView.Find("Viewport");
                    if (viewport != null)
                    {
                        Transform contentTransform = viewport.Find("Content");
                        if (contentTransform != null)
                        {
                            shopUI.characterListContainer = contentTransform;
                        }
                    }
                }
            }
            
            if (footer != null)
            {
                Transform refreshButton = footer.Find("RefreshButton");
                if (refreshButton != null)
                {
                    shopUI.refreshButton = refreshButton.GetComponent<Button>();
                    
                    Transform costText = refreshButton.Find("RefreshCostText");
                    if (costText != null)
                    {
                        shopUI.refreshCostText = costText.GetComponent<Text>();
                    }
                }
                
                Transform closeButton = footer.Find("CloseButton");
                if (closeButton != null)
                {
                    shopUI.closeButton = closeButton.GetComponent<Button>();
                }
            }
        }
    }
    
    // 创建商店棋子项目结构
    static void CreateShopItemStructure(GameObject parent)
    {
        // 清理现有结构
        while (parent.transform.childCount > 0)
        {
            DestroyImmediate(parent.transform.GetChild(0).gameObject);
        }
        
        // 创建主面板
        GameObject itemPanel = new GameObject("ItemPanel");
        itemPanel.transform.SetParent(parent.transform, false);
        RectTransform panelRect = itemPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.sizeDelta = new Vector2(180, 240);
        
        // 添加背景
        Image background = itemPanel.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        // 创建稀有度框架
        GameObject frameObj = new GameObject("RarityFrame");
        frameObj.transform.SetParent(itemPanel.transform, false);
        RectTransform frameRect = frameObj.AddComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;
        
        Image rarityFrame = frameObj.AddComponent<Image>();
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
        
        Image characterImage = imageObj.AddComponent<Image>();
        characterImage.preserveAspect = true;
        
        // 创建角色名称文本
        GameObject nameObj = new GameObject("CharacterNameText");
        nameObj.transform.SetParent(itemPanel.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0.4f);
        nameRect.anchorMax = new Vector2(0.5f, 0.4f);
        nameRect.sizeDelta = new Vector2(160, 30);
        nameRect.anchoredPosition = Vector2.zero;
        
        TMP_Text characterNameText = nameObj.AddComponent<TextMeshProUGUI>();
        characterNameText.fontSize = 18;
        characterNameText.alignment = TextAlignmentOptions.Center;
        characterNameText.color = Color.white;
        characterNameText.text = "角色名称";
        
        // 创建购买按钮
        Button buyButton = CreateButton("BuyButton", itemPanel.transform, new Vector2(0.35f, 0.15f), "购买", 70, 40, 16);
        
        // 创建锁定按钮
        Button lockButton = CreateButton("LockButton", itemPanel.transform, new Vector2(0.65f, 0.15f), "锁定", 70, 40, 16);
        
        // 创建锁定图标
        GameObject lockIconObj = new GameObject("LockIcon");
        lockIconObj.transform.SetParent(itemPanel.transform, false);
        RectTransform lockIconRect = lockIconObj.AddComponent<RectTransform>();
        lockIconRect.anchorMin = new Vector2(0.9f, 0.9f);
        lockIconRect.anchorMax = new Vector2(0.9f, 0.9f);
        lockIconRect.sizeDelta = new Vector2(30, 30);
        lockIconRect.anchoredPosition = Vector2.zero;
        
        Image lockIcon = lockIconObj.AddComponent<Image>();
        lockIcon.color = new Color(1, 0.8f, 0.2f);
        // 默认隐藏锁定图标
        lockIconObj.SetActive(false);
        
        // 创建花费文本
        GameObject costObj = new GameObject("CostText");
        costObj.transform.SetParent(itemPanel.transform, false);
        RectTransform costRect = costObj.AddComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0.1f, 0.9f);
        costRect.anchorMax = new Vector2(0.1f, 0.9f);
        costRect.sizeDelta = new Vector2(50, 30);
        costRect.anchoredPosition = Vector2.zero;
        
        TMP_Text costText = costObj.AddComponent<TextMeshProUGUI>();
        costText.fontSize = 24;
        costText.alignment = TextAlignmentOptions.Center;
        costText.color = new Color(1, 0.8f, 0.2f);
        costText.text = "3";
    }
    
    // 创建自定义大小的按钮
    static Button CreateButton(string name, Transform parent, Vector2 anchorPosition, string text, float width, float height, int fontSize)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorPosition;
        rect.anchorMax = anchorPosition;
        rect.sizeDelta = new Vector2(width, height);
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
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return button;
    }
    
    // 设置ShopItemUI组件和引用
    static void SetupShopItemUIComponents(GameObject gameObject)
    {
        // 添加或获取ShopItemUI组件
        ShopItemUI itemUI = gameObject.GetComponent<ShopItemUI>();
        if (itemUI == null)
        {
            itemUI = gameObject.AddComponent<ShopItemUI>();
        }
        
        // 设置引用
        Transform itemPanel = gameObject.transform.Find("ItemPanel");
        if (itemPanel != null)
        {
            itemUI.background = itemPanel.GetComponent<Image>();
            
            Transform frameObj = itemPanel.Find("RarityFrame");
            if (frameObj != null)
            {
                itemUI.rarityFrame = frameObj.GetComponent<Image>();
            }
            
            Transform imageObj = itemPanel.Find("CharacterImage");
            if (imageObj != null)
            {
                itemUI.characterImage = imageObj.GetComponent<Image>();
            }
            
            Transform nameObj = itemPanel.Find("CharacterNameText");
            if (nameObj != null)
            {
                itemUI.characterNameText = nameObj.GetComponent<Text>();
            }
            
            Transform costObj = itemPanel.Find("CostText");
            if (costObj != null)
            {
                itemUI.costText = costObj.GetComponent<TMP_Text>();
            }
            
            Transform buyButton = itemPanel.Find("BuyButton");
            if (buyButton != null)
            {
                itemUI.buyButton = buyButton.GetComponent<Button>();
            }
            
            Transform lockButton = itemPanel.Find("LockButton");
            if (lockButton != null)
            {
                itemUI.lockButton = lockButton.GetComponent<Button>();
            }
            
            Transform lockIcon = itemPanel.Find("LockIcon");
            if (lockIcon != null)
            {
                itemUI.lockIcon = lockIcon.GetComponent<Image>();
            }
        }
    }
    
    // 创建一个简单的矩形Sprite用于框架
    static Sprite CreateRectSprite()
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

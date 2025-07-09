// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 用于创建商店预制体的辅助类
/// </summary>
[ExecuteInEditMode]
public class ShopPrefab : MonoBehaviour
{
    // 商店UI根物体
    public GameObject shopPanel;
    
    // 商店头部区域
    public GameObject headerSection;
    public Text titleText;
    
    // 棋子列表区域
    public GameObject contentSection;
    public Transform characterListContainer;
    public GridLayoutGroup gridLayout;
    
    // 底部区域
    public GameObject footerSection;
    public Button refreshButton;
    public Text refreshCostText;
    public Button closeButton;
    
    // 添加ShopUI组件
    [ContextMenu("Setup Shop UI Components")]
    public void SetupShopUIComponents()
    {
        if (gameObject == null) return;
        
        // 添加或获取ShopUI组件
        ShopUI shopUI = gameObject.GetComponent<ShopUI>();
        if (shopUI == null)
        {
            shopUI = gameObject.AddComponent<ShopUI>();
        }
        
        // 设置引用
        shopUI.titleText = titleText;
        shopUI.characterListContainer = characterListContainer;
        shopUI.refreshButton = refreshButton;
        shopUI.refreshCostText = refreshCostText;
        shopUI.closeButton = closeButton;
        
        Debug.Log("ShopUI components setup completed.");
    }
    
    // 创建基本结构
    [ContextMenu("Create Shop Structure")]
    public void CreateShopStructure()
    {
        // 清理现有结构
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        // 创建主面板
        shopPanel = new GameObject("ShopPanel");
        shopPanel.transform.SetParent(transform, false);
        RectTransform panelRect = shopPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = shopPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // 创建头部区域
        headerSection = CreateUISection("HeaderSection", shopPanel.transform, 0, 0.85f, 1, 1);
        
        // 创建标题
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(headerSection.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(200, 50);
        titleRect.anchoredPosition = Vector2.zero;
        
        titleText = titleObj.AddComponent<Text>();
        titleText.text = "商店";
        titleText.fontSize = 36;
        titleText.color = Color.white;
        
        // 创建内容区域
        contentSection = CreateUISection("ContentSection", shopPanel.transform, 0, 0.15f, 1, 0.85f);
        
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
        
        characterListContainer = content.transform;
        gridLayout = content.AddComponent<GridLayoutGroup>();
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
        footerSection = CreateUISection("FooterSection", shopPanel.transform, 0, 0, 1, 0.15f);
        
        // 创建刷新按钮
        refreshButton = CreateButton("RefreshButton", footerSection.transform, new Vector2(0.3f, 0.5f), "刷新");
        
        // 创建刷新花费文本
        GameObject costObj = new GameObject("RefreshCostText");
        costObj.transform.SetParent(refreshButton.transform, false);
        RectTransform costRect = costObj.AddComponent<RectTransform>();
        costRect.anchorMin = new Vector2(1, 0.5f);
        costRect.anchorMax = new Vector2(1, 0.5f);
        costRect.sizeDelta = new Vector2(50, 30);
        costRect.anchoredPosition = new Vector2(40, 0);
        
        refreshCostText = costObj.AddComponent<Text>();
        refreshCostText.text = "2";
        refreshCostText.fontSize = 24;
        refreshCostText.color = new Color(1, 0.8f, 0.2f);
        
        // 创建关闭按钮
        closeButton = CreateButton("CloseButton", footerSection.transform, new Vector2(0.7f, 0.5f), "关闭");
        
        Debug.Log("Shop structure created successfully.");
    }
    
    // 创建UI分区
    private GameObject CreateUISection(string name, Transform parent, float minX, float minY, float maxX, float maxY)
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
    private Button CreateButton(string name, Transform parent, Vector2 anchorPosition, string text)
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
}

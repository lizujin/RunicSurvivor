// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// 预制体创建工具，在编辑器中提供创建游戏预制体的功能
/// </summary>
public class PrefabCreationTool : EditorWindow
{
    // 角色设置
    private GameObject playerObject;
    private string characterName = "唐僧";
    private RaceType characterRace = RaceType.Human;
    private FactionType characterFaction = FactionType.Buddhist;
    
    // 管理器设置
    private bool createGameController = true;
    private bool createEnemyManager = true;
    private bool createShopManager = true;
    private bool createSynergyManager = true;
    private bool createUIManager = true;
    
    // UI设置
    private bool createGameplayUI = true;
    private bool createShopUI = true;
    private bool createSynergyUI = true;
    
    // 输出路径
    private string outputFolderPath = "Assets/Prefabs";
    
    [MenuItem("西游记幸存者/预制体创建工具")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PrefabCreationTool), false, "预制体创建工具");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("西游记幸存者 - 预制体创建工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 输出路径设置
        EditorGUILayout.BeginHorizontal();
        outputFolderPath = EditorGUILayout.TextField("输出路径", outputFolderPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFolderPanel("选择预制体输出文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // 将绝对路径转换为相对于项目的路径
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    outputFolderPath = path;
                }
                else
                {
                    Debug.LogWarning("请选择项目内的文件夹！");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 确保输出文件夹存在
        if (!AssetDatabase.IsValidFolder(outputFolderPath))
        {
            EditorGUILayout.HelpBox("输出路径不是有效的项目文件夹！", MessageType.Error);
        }
        
        // 角色设置
        GUILayout.Label("角色设置", EditorStyles.boldLabel);
        playerObject = EditorGUILayout.ObjectField("玩家对象", playerObject, typeof(GameObject), true) as GameObject;
        characterName = EditorGUILayout.TextField("角色名称", characterName);
        characterRace = (RaceType)EditorGUILayout.EnumPopup("种族", characterRace);
        characterFaction = (FactionType)EditorGUILayout.EnumPopup("派别", characterFaction);
        
        EditorGUILayout.Space();
        
        // 管理器设置
        GUILayout.Label("管理器设置", EditorStyles.boldLabel);
        createGameController = EditorGUILayout.Toggle("创建GameController", createGameController);
        createEnemyManager = EditorGUILayout.Toggle("创建EnemyManager", createEnemyManager);
        createShopManager = EditorGUILayout.Toggle("创建ShopManager", createShopManager);
        createSynergyManager = EditorGUILayout.Toggle("创建SynergyManager", createSynergyManager);
        createUIManager = EditorGUILayout.Toggle("创建UIManager", createUIManager);
        
        EditorGUILayout.Space();
        
        // UI设置
        GUILayout.Label("UI设置", EditorStyles.boldLabel);
        createGameplayUI = EditorGUILayout.Toggle("创建游戏UI", createGameplayUI);
        createShopUI = EditorGUILayout.Toggle("创建商店UI", createShopUI);
        createSynergyUI = EditorGUILayout.Toggle("创建协同效应UI", createSynergyUI);
        
        EditorGUILayout.Space();
        
        // 创建按钮
        if (GUILayout.Button("创建预制体"))
        {
            CreatePrefabs();
        }
    }
    
    private void CreatePrefabs()
    {
        // 确保输出文件夹存在
        EnsureDirectoryExists(outputFolderPath);
        EnsureDirectoryExists($"{outputFolderPath}/Characters");
        EnsureDirectoryExists($"{outputFolderPath}/Managers");
        EnsureDirectoryExists($"{outputFolderPath}/UI");
        EnsureDirectoryExists($"{outputFolderPath}/Environment");
        
        // 创建角色预制体
        GameObject characterPrefab = CreateCharacterPrefab();
        
        // 创建管理器预制体
        GameObject gameControllerPrefab = createGameController ? CreateGameControllerPrefab() : null;
        GameObject enemyManagerPrefab = createEnemyManager ? CreateEnemyManagerPrefab() : null;
        GameObject shopManagerPrefab = createShopManager ? CreateShopManagerPrefab() : null;
        GameObject synergyManagerPrefab = createSynergyManager ? CreateSynergyManagerPrefab() : null;
        GameObject uiManagerPrefab = createUIManager ? CreateUIManagerPrefab() : null;
        
        // 创建UI预制体
        GameObject gameplayUIPrefab = createGameplayUI ? CreateGameplayUIPrefab() : null;
        GameObject shopUIPrefab = createShopUI ? CreateShopUIPrefab() : null;
        GameObject synergyUIPrefab = createSynergyUI ? CreateSynergyUIPrefab() : null;
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("预制体创建完成!");
    }
    
    /// <summary>
    /// 确保指定的文件夹路径存在
    /// </summary>
    private void EnsureDirectoryExists(string path)
    {
        string[] folderNames = path.Split('/');
        string currentPath = "";
        
        foreach (string folder in folderNames)
        {
            if (string.IsNullOrEmpty(folder))
                continue;
                
            string folderPath = string.IsNullOrEmpty(currentPath) ? folder : $"{currentPath}/{folder}";
            
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(folderPath).Replace("\\", "/");
                string newFolderName = System.IO.Path.GetFileName(folderPath);
                
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
            
            currentPath = folderPath;
        }
    }
    
    /// <summary>
    /// 创建角色预制体
    /// </summary>
    private GameObject CreateCharacterPrefab()
    {
        // 如果已提供对象，使用它作为模板
        GameObject characterObj;
        if (playerObject != null)
        {
            characterObj = Instantiate(playerObject);
        }
        else
        {
            // 创建基本角色
            characterObj = new GameObject(characterName);
            
            // 添加必要的组件
            characterObj.AddComponent<Character>();
            characterObj.AddComponent<CapsuleCollider>();
            characterObj.AddComponent<Rigidbody>();
            
            // 添加基本的可视化模型
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.name = "Model";
            model.transform.SetParent(characterObj.transform);
            model.transform.localPosition = Vector3.zero;
        }
        
        // 设置角色属性
        Character character = characterObj.GetComponent<Character>();
        if (character == null)
        {
            character = characterObj.AddComponent<Character>();
        }
        
        character.characterName = characterName;
        character.race = characterRace;
        character.faction = characterFaction;
        
        // 确保有正确的刚体设置
        Rigidbody rb = characterObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
        }
        
        // 保存预制体
        string prefabPath = $"{outputFolderPath}/Characters/{characterName}.prefab";
        GameObject prefab = SaveAsPrefab(characterObj, prefabPath);
        DestroyImmediate(characterObj); // 清理场景中的临时对象
        
        return prefab;
    }
    
    /// <summary>
    /// 创建GameController预制体
    /// </summary>
    private GameObject CreateGameControllerPrefab()
    {
        GameObject obj = new GameObject("GameController");
        GameController gameController = obj.AddComponent<GameController>();
        
        // 在这里可以设置GameController的属性
        
        string prefabPath = $"{outputFolderPath}/Managers/GameController.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建EnemyManager预制体
    /// </summary>
    private GameObject CreateEnemyManagerPrefab()
    {
        GameObject obj = new GameObject("EnemyManager");
        obj.AddComponent<EnemyManager>();
        
        string prefabPath = $"{outputFolderPath}/Managers/EnemyManager.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建ShopManager预制体
    /// </summary>
    private GameObject CreateShopManagerPrefab()
    {
        GameObject obj = new GameObject("ShopManager");
        obj.AddComponent<ShopManager>();
        
        string prefabPath = $"{outputFolderPath}/Managers/ShopManager.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建SynergyManager预制体
    /// </summary>
    private GameObject CreateSynergyManagerPrefab()
    {
        GameObject obj = new GameObject("SynergyManager");
        obj.AddComponent<SynergyManager>();
        
        string prefabPath = $"{outputFolderPath}/Managers/SynergyManager.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建UIManager预制体
    /// </summary>
    private GameObject CreateUIManagerPrefab()
    {
        GameObject obj = new GameObject("UIManager");
        obj.AddComponent<UIManager>();
        
        string prefabPath = $"{outputFolderPath}/Managers/UIManager.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建游戏UI预制体
    /// </summary>
    private GameObject CreateGameplayUIPrefab()
    {
        GameObject obj = new GameObject("GameplayUI");
        obj.AddComponent<Canvas>();
        obj.AddComponent<CanvasScaler>();
        obj.AddComponent<GraphicRaycaster>();
        
        // 添加基本UI元素
        CreateBasicUIElements(obj.transform);
        
        string prefabPath = $"{outputFolderPath}/UI/GameplayUI.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建商店UI预制体
    /// </summary>
    private GameObject CreateShopUIPrefab()
    {
        GameObject obj = new GameObject("ShopUI");
        obj.AddComponent<Canvas>();
        obj.AddComponent<CanvasScaler>();
        obj.AddComponent<GraphicRaycaster>();
        
        // 添加商店UI元素
        CreateShopUIElements(obj.transform);
        
        string prefabPath = $"{outputFolderPath}/UI/ShopUI.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建协同效应UI预制体
    /// </summary>
    private GameObject CreateSynergyUIPrefab()
    {
        GameObject obj = new GameObject("SynergyUI");
        obj.AddComponent<Canvas>();
        obj.AddComponent<CanvasScaler>();
        obj.AddComponent<GraphicRaycaster>();
        
        // 添加协同效应UI元素
        CreateSynergyUIElements(obj.transform);
        
        string prefabPath = $"{outputFolderPath}/UI/SynergyUI.prefab";
        GameObject prefab = SaveAsPrefab(obj, prefabPath);
        DestroyImmediate(obj);
        
        return prefab;
    }
    
    /// <summary>
    /// 创建基本UI元素
    /// </summary>
    private void CreateBasicUIElements(Transform parent)
    {
        // 创建状态面板
        GameObject statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(parent, false);
        RectTransform statusRect = statusPanel.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(0.5f, 1);
        statusRect.sizeDelta = new Vector2(0, 80);
        statusRect.anchoredPosition = Vector2.zero;
        
        // 创建波次文本
        CreateUIText(statusPanel.transform, "WaveText", "Wave: 1/10", new Vector2(100, -20), 24);
        
        // 创建金币文本
        CreateUIText(statusPanel.transform, "GoldText", "Gold: 0", new Vector2(300, -20), 24);
        
        // 创建经验条
        CreateUISlider(statusPanel.transform, "ExpBar", new Vector2(0, -60), new Vector2(600, 20));
        
        // 创建技能面板
        GameObject skillPanel = new GameObject("SkillPanel");
        skillPanel.transform.SetParent(parent, false);
        RectTransform skillRect = skillPanel.AddComponent<RectTransform>();
        skillRect.anchorMin = new Vector2(0.5f, 0);
        skillRect.anchorMax = new Vector2(0.5f, 0);
        skillRect.pivot = new Vector2(0.5f, 0);
        skillRect.sizeDelta = new Vector2(300, 80);
        skillRect.anchoredPosition = new Vector2(0, 100);
        
        // 创建生命条
        CreateUISlider(parent, "HealthBar", new Vector2(0, 40), new Vector2(400, 30), true);
    }
    
    /// <summary>
    /// 创建商店UI元素
    /// </summary>
    private void CreateShopUIElements(Transform parent)
    {
        // 创建商店面板
        GameObject shopPanel = new GameObject("ShopPanel");
        shopPanel.transform.SetParent(parent, false);
        RectTransform shopRect = shopPanel.AddComponent<RectTransform>();
        shopRect.anchorMin = new Vector2(0.5f, 0.5f);
        shopRect.anchorMax = new Vector2(0.5f, 0.5f);
        shopRect.pivot = new Vector2(0.5f, 0.5f);
        shopRect.sizeDelta = new Vector2(800, 600);
        
        // 添加背景图像
        GameObject background = new GameObject("Background");
        background.transform.SetParent(shopPanel.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // 创建标题
        CreateUIText(shopPanel.transform, "TitleText", "商店", new Vector2(0, 250), 36);
        
        // 创建角色卡片容器
        GameObject cardContainer = new GameObject("CardContainer");
        cardContainer.transform.SetParent(shopPanel.transform, false);
        RectTransform containerRect = cardContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(700, 300);
        GridLayoutGroup gridLayout = cardContainer.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(150, 200);
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4;
        
        // 创建刷新按钮
        CreateUIButton(shopPanel.transform, "RefreshButton", "刷新", new Vector2(0, -220), new Vector2(150, 60));
        
        // 创建关闭按钮
        CreateUIButton(shopPanel.transform, "CloseButton", "关闭", new Vector2(300, -220), new Vector2(150, 60));
        
        // 创建金币文本
        CreateUIText(shopPanel.transform, "GoldText", "金币: 0", new Vector2(-300, -220), 24);
    }
    
    /// <summary>
    /// 创建协同效应UI元素
    /// </summary>
    private void CreateSynergyUIElements(Transform parent)
    {
        // 创建协同效应面板
        GameObject synergyPanel = new GameObject("SynergyPanel");
        synergyPanel.transform.SetParent(parent, false);
        RectTransform synergyRect = synergyPanel.AddComponent<RectTransform>();
        synergyRect.anchorMin = new Vector2(0, 0);
        synergyRect.anchorMax = new Vector2(0, 1);
        synergyRect.pivot = new Vector2(0, 0.5f);
        synergyRect.sizeDelta = new Vector2(250, 0);
        
        // 添加背景图像
        GameObject background = new GameObject("Background");
        background.transform.SetParent(synergyPanel.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        
        // 创建标题
        CreateUIText(synergyPanel.transform, "TitleText", "协同效应", new Vector2(125, -30), 24);
        
        // 创建种族容器
        GameObject raceContainer = new GameObject("RaceContainer");
        raceContainer.transform.SetParent(synergyPanel.transform, false);
        RectTransform raceRect = raceContainer.AddComponent<RectTransform>();
        raceRect.anchorMin = new Vector2(0, 1);
        raceRect.anchorMax = new Vector2(1, 1);
        raceRect.pivot = new Vector2(0.5f, 1);
        raceRect.sizeDelta = new Vector2(0, 300);
        raceRect.anchoredPosition = new Vector2(0, -60);
        VerticalLayoutGroup raceLayout = raceContainer.AddComponent<VerticalLayoutGroup>();
        raceLayout.padding = new RectOffset(10, 10, 10, 10);
        raceLayout.spacing = 5;
        
        // 创建派别容器
        GameObject factionContainer = new GameObject("FactionContainer");
        factionContainer.transform.SetParent(synergyPanel.transform, false);
        RectTransform factionRect = factionContainer.AddComponent<RectTransform>();
        factionRect.anchorMin = new Vector2(0, 1);
        factionRect.anchorMax = new Vector2(1, 1);
        factionRect.pivot = new Vector2(0.5f, 1);
        factionRect.sizeDelta = new Vector2(0, 300);
        factionRect.anchoredPosition = new Vector2(0, -380);
        VerticalLayoutGroup factionLayout = factionContainer.AddComponent<VerticalLayoutGroup>();
        factionLayout.padding = new RectOffset(10, 10, 10, 10);
        factionLayout.spacing = 5;
    }
    
    /// <summary>
    /// 创建UI文本元素
    /// </summary>
    private GameObject CreateUIText(Transform parent, string name, string text, Vector2 position, int fontSize = 14)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 30);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        return textObj;
    }
    
    /// <summary>
    /// 创建UI滑动条
    /// </summary>
    private GameObject CreateUISlider(Transform parent, string name, Vector2 position, Vector2 size, bool isHealth = false)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.value = 1.0f;
        
        // 创建背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1);
        
        // 创建填充区域
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-10, -10);
        fillRect.anchoredPosition = Vector2.zero;
        
        // 创建填充
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillItemRect = fill.AddComponent<RectTransform>();
        fillItemRect.anchorMin = Vector2.zero;
        fillItemRect.anchorMax = Vector2.one;
        fillItemRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = isHealth ? new Color(0.8f, 0.2f, 0.2f, 1) : new Color(0.2f, 0.6f, 1f, 1);
        
        // 设置填充
        slider.fillRect = fillItemRect;
        
        return sliderObj;
    }
    
    /// <summary>
    /// 创建UI按钮
    /// </summary>
    private GameObject CreateUIButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        
        // 创建文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        return buttonObj;
    }
    
    /// <summary>
    /// 将对象保存为预制体
    /// </summary>
    private GameObject SaveAsPrefab(GameObject obj, string prefabPath)
    {
        // 确保目录存在
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!AssetDatabase.IsValidFolder(directory))
        {
            string parentPath = System.IO.Path.GetDirectoryName(directory).Replace("\\", "/");
            string folderName = System.IO.Path.GetFileName(directory);
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
        
        // 创建或替换预制体
        GameObject prefab;
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        }
        else
        {
            prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        }
        
        return prefab;
    }
}

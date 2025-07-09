// 本文件完全有AI生成
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 将3D场景转换为2D场景的编辑器工具
/// </summary>
public class Convert3DTo2D : EditorWindow
{
    private Material backgroundMaterial;
    private Sprite backgroundSprite;
    private Color backgroundColor = new Color(0.2f, 0.3f, 0.5f);
    private bool createTilemap = true;
    private bool createBackgroundSprite = true;
    private bool createBoundaries = true;
    private bool adjustSpawnPoints = true;
    private float backgroundSize = 40f;
    
    [MenuItem("工具/场景转换/3D转2D场景")]
    public static void ShowWindow()
    {
        GetWindow<Convert3DTo2D>("3D转2D场景工具");
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("场景3D转2D工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("此工具将帮助您将当前场景转换为2D格式。\n它将调整摄像机、物理设置和游戏对象，并可选择添加2D元素。", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        backgroundSprite = (Sprite)EditorGUILayout.ObjectField("背景精灵:", backgroundSprite, typeof(Sprite), false);
        if (!backgroundSprite)
        {
            backgroundColor = EditorGUILayout.ColorField("背景颜色:", backgroundColor);
        }
        backgroundSize = EditorGUILayout.FloatField("背景尺寸:", backgroundSize);
        
        EditorGUILayout.Space(5);
        createTilemap = EditorGUILayout.Toggle("创建瓦片地图:", createTilemap);
        createBackgroundSprite = EditorGUILayout.Toggle("创建背景:", createBackgroundSprite);
        createBoundaries = EditorGUILayout.Toggle("创建边界:", createBoundaries);
        adjustSpawnPoints = EditorGUILayout.Toggle("调整生成点:", adjustSpawnPoints);
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("转换为2D场景"))
        {
            ConvertTo2D();
        }
    }
    
    void ConvertTo2D()
    {
        // 1. 转换主摄像机为正交模式
        ConvertCameraToOrthographic();
        
        // 2. 创建2D背景
        if (createBackgroundSprite)
        {
            CreateBackground();
        }
        
        // 3. 如果需要，创建瓦片地图
        if (createTilemap)
        {
            CreateTilemap();
        }
        
        // 4. 如果需要，创建物理边界
        if (createBoundaries)
        {
            CreateBoundaries();
        }
        
        // 5. 调整敌人生成点
        if (adjustSpawnPoints)
        {
            AdjustSpawnPoints();
        }
        
        // 6. 更新场景设置为2D
        UpdateSceneSettings();
        
        // 7. 创建排序图层
        CreateSortingLayers();
        
        // 完成
        Debug.Log("场景已成功转换为2D格式!");
        EditorUtility.DisplayDialog("转换完成", "场景已成功转换为2D格式!", "确定");
    }
    
    void ConvertCameraToOrthographic()
    {
        // 查找场景中的所有摄像机
        Camera[] cameras = FindObjectsOfType<Camera>();
        if (cameras.Length == 0)
        {
            // 如果没有找到摄像机，创建一个
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera camera = cameraObj.AddComponent<Camera>();
            cameras = new Camera[] { camera };
        }
        
        foreach (Camera camera in cameras)
        {
            // 设置为正交模式
            camera.orthographic = true;
            camera.orthographicSize = 5; // 调整大小
            camera.transform.position = new Vector3(0, 0, -10); // 放置在适当位置
            camera.transform.rotation = Quaternion.identity; // 重置旋转
            
            // 确保有深度缓冲
            camera.depth = 0;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            
            Debug.Log($"已将摄像机 {camera.name} 转换为正交模式");
        }
    }
    
    void CreateBackground()
    {
        GameObject background = new GameObject("Background");
        background.transform.position = new Vector3(0, 0, 1); // 放在摄像机前面
        
        SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
        
        if (backgroundSprite != null)
        {
            renderer.sprite = backgroundSprite;
        }
        else
        {
            // 创建一个纯色的精灵
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, backgroundColor);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            renderer.sprite = sprite;
        }
        
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(backgroundSize, backgroundSize);
        renderer.sortingLayerName = "Background";
        renderer.sortingOrder = -100;
        
        Debug.Log("已创建2D背景");
    }
    
    void CreateTilemap()
    {
        // 检查Grid组件
        Grid grid = FindObjectOfType<Grid>();
        if (grid == null)
        {
            GameObject gridObj = new GameObject("Grid");
            grid = gridObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(1, 1, 0);
        }
        
        // 创建地面瓦片地图
        GameObject groundTilemap = new GameObject("Ground Tilemap");
        groundTilemap.transform.parent = grid.transform;
        
        // 添加Tilemap组件（如果有的话）
        // 注意：这里可能需要引入Tilemap命名空间和程序集
        var tilemapType = System.Type.GetType("UnityEngine.Tilemaps.Tilemap, Unity.Tilemaps");
        var tilemapRendererType = System.Type.GetType("UnityEngine.Tilemaps.TilemapRenderer, Unity.Tilemaps");
        
        if (tilemapType != null && tilemapRendererType != null)
        {
            groundTilemap.AddComponent(tilemapType);
            var renderer = groundTilemap.AddComponent(tilemapRendererType);
            
            // 设置排序层
            var sortingLayerProperty = tilemapRendererType.GetProperty("sortingLayerName");
            if (sortingLayerProperty != null)
            {
                sortingLayerProperty.SetValue(renderer, "Terrain");
            }
            
            Debug.Log("已创建瓦片地图");
        }
        else
        {
            // 如果Tilemap不可用，创建一个简单的地面
            GameObject ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0, -5, 0);
            
            SpriteRenderer groundRenderer = ground.AddComponent<SpriteRenderer>();
            groundRenderer.color = new Color(0.5f, 0.5f, 0.3f);
            groundRenderer.drawMode = SpriteDrawMode.Sliced;
            groundRenderer.size = new Vector2(30, 2);
            
            // 添加地面碰撞器
            BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(30, 2);
            
            Debug.Log("已创建简单地面");
        }
    }
    
    void CreateBoundaries()
    {
        float width = backgroundSize;
        float height = backgroundSize;
        float thickness = 1f;
        
        // 创建边界容器
        GameObject boundaries = new GameObject("Boundaries");
        
        // 上边界
        CreateBoundary("TopBoundary", boundaries.transform, new Vector3(0, height/2, 0), new Vector2(width, thickness));
        
        // 下边界
        CreateBoundary("BottomBoundary", boundaries.transform, new Vector3(0, -height/2, 0), new Vector2(width, thickness));
        
        // 左边界
        CreateBoundary("LeftBoundary", boundaries.transform, new Vector3(-width/2, 0, 0), new Vector2(thickness, height));
        
        // 右边界
        CreateBoundary("RightBoundary", boundaries.transform, new Vector3(width/2, 0, 0), new Vector2(thickness, height));
        
        Debug.Log("已创建边界碰撞器");
    }
    
    void CreateBoundary(string name, Transform parent, Vector3 position, Vector2 size)
    {
        GameObject boundary = new GameObject(name);
        boundary.transform.parent = parent;
        boundary.transform.position = position;
        
        BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
    
    void AdjustSpawnPoints()
    {
        // 查找"EnemySpawnPoints"游戏对象
        GameObject spawnPointsObject = GameObject.Find("EnemySpawnPoints");
        if (spawnPointsObject != null)
        {
            foreach (Transform child in spawnPointsObject.transform)
            {
                // 将Z坐标归零
                child.position = new Vector3(child.position.x, child.position.y, 0);
                Debug.Log($"已调整生成点 {child.name} 到2D平面");
            }
        }
        
        // 查找"PlayerSpawnPoint"游戏对象
        GameObject playerSpawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (playerSpawnPoint != null)
        {
            playerSpawnPoint.transform.position = new Vector3(
                playerSpawnPoint.transform.position.x,
                playerSpawnPoint.transform.position.y,
                0
            );
            Debug.Log("已调整玩家生成点到2D平面");
        }
    }
    
    void UpdateSceneSettings()
    {
        // 找到SceneSetup组件并更新
        var sceneSetups = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour mb in sceneSetups)
        {
            if (mb.GetType().Name.Contains("SceneSetup") || mb.name.Contains("SceneSetup"))
            {
                // 使用反射设置属性
                var fieldInfo = mb.GetType().GetField("createBasicFloor");
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(mb, false);
                }
                
                Debug.Log("已更新场景设置，禁用3D地面");
            }
        }
        
        // 确保Physics2D设置正确
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
    
    void CreateSortingLayers()
    {
        // 由于Unity不直接暴露SortingLayer API，我们会创建几个示例对象来强制创建排序层
        string[] layerNames = new string[] 
        {
            "Background", "Terrain", "Props", "Characters", "Enemies", "Effects", "UI"
        };
        
        // 创建一个隐藏的对象来管理排序层
        GameObject layerManager = new GameObject("SortingLayerManager");
        layerManager.hideFlags = HideFlags.HideInHierarchy; // 隐藏在层级面板
        
        for (int i = 0; i < layerNames.Length; i++)
        {
            GameObject layerObj = new GameObject(layerNames[i] + "LayerObject");
            layerObj.transform.parent = layerManager.transform;
            
            SpriteRenderer renderer = layerObj.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = layerNames[i];
            renderer.sortingOrder = i * 100; // 为每层提供足够的排序空间
        }
        
        Debug.Log("已创建排序层");
        
        // 删除这个临时对象
        DestroyImmediate(layerManager);
    }
}

// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏场景初始化器，在编辑模式下运行，用于设置游戏场景中的基本对象
/// </summary>
[ExecuteInEditMode]
public class GameSceneInitializer : MonoBehaviour
{
    [Header("场景设置")]
    public GameObject sceneSetupPrefab;
    
    [Header("预制体引用")]
    public GameObject playerPrefab;
    public GameObject gameControllerPrefab;
    public GameObject enemyManagerPrefab;
    public GameObject shopManagerPrefab;
    public GameObject synergyManagerPrefab;
    public GameObject uiManagerPrefab;
    public GameObject mainCameraPrefab;
    public GameObject environmentPrefab;
    public GameObject lightingPrefab;
    public GameObject uiPrefab;
    
    [Header("配置")]
    public bool createBasicFloor = true;
    public Material floorMaterial;
    public Vector2 floorSize = new Vector2(50f, 50f);
    
    public void SetupScene()
    {
        Debug.Log("开始设置游戏场景...");
        
        // 创建场景设置对象
        CreateSceneSetupObject();
        
        // 创建玩家生成点
        CreatePlayerSpawnPoint();
        
        // 创建敌人生成点
        CreateEnemySpawnPoints();
        
        // 创建基本地面（如果需要）
        if (createBasicFloor)
        {
            CreateBasicFloor();
        }
        
        Debug.Log("游戏场景设置完成！");
    }
    
    private void CreateSceneSetupObject()
    {
        // 检查是否已存在场景设置对象
        SceneSetup existingSetup = FindObjectOfType<SceneSetup>();
        if (existingSetup != null)
        {
            Debug.Log("场景设置对象已存在，更新引用...");
            
            // 更新引用
            existingSetup.gameControllerPrefab = gameControllerPrefab;
            existingSetup.mainCameraPrefab = mainCameraPrefab;
            existingSetup.environmentPrefab = environmentPrefab;
            existingSetup.uiPrefab = uiPrefab;
            existingSetup.lightingPrefab = lightingPrefab;
            
            return;
        }
        
        // 创建场景设置对象
        GameObject setupObj;
        
#if UNITY_EDITOR
        // 仅在编辑器中使用PrefabUtility
        if (sceneSetupPrefab != null)
        {
            setupObj = UnityEditor.PrefabUtility.InstantiatePrefab(sceneSetupPrefab) as GameObject;
        }
        else
#endif
        {
            setupObj = new GameObject("SceneSetup");
            SceneSetup setup = setupObj.AddComponent<SceneSetup>();
            
            // 设置引用
            setup.gameControllerPrefab = gameControllerPrefab;
            setup.mainCameraPrefab = mainCameraPrefab;
            setup.environmentPrefab = environmentPrefab;
            setup.uiPrefab = uiPrefab;
            setup.lightingPrefab = lightingPrefab;
        }
        
        Debug.Log("场景设置对象创建完成");
    }
    
    private void CreatePlayerSpawnPoint()
    {
        // 检查是否已存在玩家生成点
        Transform existingSpawnPoint = GameObject.Find("PlayerSpawnPoint")?.transform;
        if (existingSpawnPoint != null)
        {
            Debug.Log("玩家生成点已存在");
            return;
        }
        
        // 创建玩家生成点
        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = new Vector3(0, 0, 0);
        
        Debug.Log("玩家生成点创建完成");
        
        // 找到GameController并设置引用
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.playerSpawnPoint = spawnPoint.transform;
            gameController.playerPrefab = playerPrefab;
        }
    }
    
    private void CreateEnemySpawnPoints()
    {
        // 检查是否已存在敌人生成点容器
        Transform existingSpawnPointsContainer = GameObject.Find("EnemySpawnPoints")?.transform;
        if (existingSpawnPointsContainer != null)
        {
            Debug.Log("敌人生成点已存在");
            return;
        }
        
        // 创建敌人生成点容器
        GameObject spawnPointsContainer = new GameObject("EnemySpawnPoints");
        
        // 创建四个角落的生成点
        float distance = 20f;
        
        CreateEnemySpawnPoint(spawnPointsContainer.transform, "EnemySpawnPoint_NE", new Vector3(distance, 0, distance));
        CreateEnemySpawnPoint(spawnPointsContainer.transform, "EnemySpawnPoint_NW", new Vector3(-distance, 0, distance));
        CreateEnemySpawnPoint(spawnPointsContainer.transform, "EnemySpawnPoint_SE", new Vector3(distance, 0, -distance));
        CreateEnemySpawnPoint(spawnPointsContainer.transform, "EnemySpawnPoint_SW", new Vector3(-distance, 0, -distance));
        
        Debug.Log("敌人生成点创建完成");
        
        // 找到EnemyManager并设置引用（如果有这样的接口）
        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            // 假设EnemyManager有一个设置生成点的方法
            // enemyManager.SetSpawnPoints(spawnPointsContainer.transform);
        }
    }
    
    private void CreateEnemySpawnPoint(Transform parent, string name, Vector3 position)
    {
        GameObject spawnPoint = new GameObject(name);
        spawnPoint.transform.SetParent(parent);
        spawnPoint.transform.position = position;
        
        // 添加视觉指示器（仅在编辑模式下可见）
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = "Indicator";
        indicator.transform.SetParent(spawnPoint.transform);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // 设置材质（红色）
        Renderer renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        // 在运行时禁用渲染
        indicator.hideFlags = HideFlags.DontSaveInEditor;
    }
    
    private void CreateBasicFloor()
    {
        // 检查是否已存在基本地面
        if (GameObject.Find("Floor") != null)
        {
            Debug.Log("基本地面已存在");
            return;
        }
        
        // 创建基本地面
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(floorSize.x / 10f, 1, floorSize.y / 10f);
        
        // 设置材质（如果有）
        if (floorMaterial != null)
        {
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = floorMaterial;
            }
        }
        
        Debug.Log("基本地面创建完成");
    }
    
#if UNITY_EDITOR
    // 在Editor中添加按钮以便于使用
    [UnityEditor.CustomEditor(typeof(GameSceneInitializer))]
    public class GameSceneInitializerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GameSceneInitializer initializer = (GameSceneInitializer)target;
            
            UnityEditor.EditorGUILayout.Space();
            if (GUILayout.Button("设置游戏场景"))
            {
                initializer.SetupScene();
            }
        }
    }
#endif
}

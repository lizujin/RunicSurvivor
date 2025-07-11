// 本文件完全有AI生成
using System.Collections;
using UnityEngine;

/// <summary>
/// 场景设置，用于配置游戏场景和启动游戏
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [Header("主场景对象")]
    public GameObject gameControllerPrefab;
    public GameObject mainCameraPrefab;
    public GameObject environmentPrefab;
    public GameObject uiPrefab;
    public GameObject lightingPrefab;
    
    [Header("初始设置")]
    public bool startGameImmediately = false;
    
    private GameController gameController;
    
    private void Awake()
    {
        Debug.Log("初始化场景对象...");
        
        // 启动场景初始化协程，确保各组件按顺序创建
        StartCoroutine(InitializeSceneSequence());
    }
    
    /// <summary>
    /// 按顺序初始化场景的协程
    /// </summary>
    private IEnumerator InitializeSceneSequence()
    {
        // 1. 首先创建基础环境
        GameObject environment = CreateEnvironment();
        
        // 等待一帧，确保环境完全创建
        yield return null;
        
        // 2. 创建灯光
        CreateLighting();
        
        // 3. 创建相机
        CreateCamera();
        
        // 确保环境已经完全加载
        if (environment != null)
        {
            // 等待环境中可能的初始化过程完成
            // 这里可以检查环境中的特定组件是否已准备好
            yield return new WaitForSeconds(0.1f); // 给予短暂延迟确保环境加载
        }
        
        Debug.Log("环境创建完成，准备创建玩家和敌人出生点");
        
        // 4. 设置游戏场景（包括玩家和敌人出生点）
        SetupGameScene();
        
        // 5. 初始化核心管理器（必须在创建环境和出生点之后）
        CreateGameController();
        
        // 6. 创建UI
        CreateUI();
        
        Debug.Log("场景初始化完成");
    }
    
    /// <summary>
    /// 创建环境
    /// </summary>
    private GameObject CreateEnvironment()
    {
        GameObject environmentObj = null;
        
        // 设置环境
        if (environmentPrefab != null)
        {
            environmentObj = Instantiate(environmentPrefab);
            environmentObj.name = "Environment";
            Debug.Log("创建环境: Environment");
        }
        else
        {
            Debug.LogWarning("未设置环境预制体，将使用空环境");
            environmentObj = new GameObject("Environment");
        }
        
        return environmentObj;
    }
    
    /// <summary>
    /// 创建UI
    /// </summary>
    private void CreateUI()
    {
        if (uiPrefab != null)
        {
            var uiRoot = GameObject.FindGameObjectWithTag("uiRoot");
            GameObject ui = Instantiate(uiPrefab, uiRoot.transform);
            ui.name = "UI";
            Debug.Log("创建UI: UI");
        }
    }
    
    /// <summary>
    /// 创建游戏控制器
    /// </summary>
    private void CreateGameController()
    {
        // 初始化核心管理器
        if (gameControllerPrefab != null)
        {
            GameObject controller = Instantiate(gameControllerPrefab);
            controller.name = "GameController";
            gameController = controller.GetComponent<GameController>();
            Debug.Log("创建游戏控制器: GameController");
        }
        else
        {
            GameObject controller = new GameObject("GameController");
            gameController = controller.AddComponent<GameController>();
            Debug.Log("创建默认游戏控制器: GameController");
        }
    }
    
    /// <summary>
    /// 创建相机
    /// </summary>
    private void CreateCamera()
    {
        // 设置主相机
        if (mainCameraPrefab != null)
        {
            GameObject camera = Instantiate(mainCameraPrefab);
            camera.name = "Main Camera";
            Debug.Log("创建相机: Main Camera");
        }
        else if (Camera.main == null)
        {
            // 如果没有预制体，至少创建一个基本相机
            GameObject cameraObj = new GameObject("Main Camera");
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            
            // 设置为俯视图相机
            cameraObj.transform.position = new Vector3(0, 10, -10);
            cameraObj.transform.rotation = Quaternion.Euler(45, 0, 0);
            
            // 添加跟踪脚本
            cameraObj.AddComponent<CameraFollow>();
            Debug.Log("创建默认相机: Main Camera");
        }
    }
    
    /// <summary>
    /// 创建灯光
    /// </summary>
    private void CreateLighting()
    {
        // 设置光照
        if (lightingPrefab != null)
        {
            GameObject lighting = Instantiate(lightingPrefab);
            lighting.name = "Lighting";
            Debug.Log("创建灯光: Lighting");
        }
        else
        {
            // 如果没有光照预制体，创建默认光照
            CreateDefaultLighting();
        }
    }
    
    private void CreateDefaultLighting()
    {
        // 创建方向光
        GameObject directionalLight = new GameObject("Directional Light");
        Light light = directionalLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.color = Color.white;
        directionalLight.transform.rotation = Quaternion.Euler(50, 30, 0);
        Debug.Log("创建默认灯光: Directional Light");
    }
    
    private void Start()
    {
        // 如果设置为立即启动，则启动游戏
        if (startGameImmediately && gameController != null)
        {
            gameController.StartGame();
        }
    }
    
    /// <summary>
    /// 设置游戏场景，创建必要的游戏对象
    /// </summary>
    private void SetupGameScene()
    {
        Debug.Log("设置游戏场景...");
        
        // 检查是否已存在玩家出生点
        GameObject playerSpawnObj = GameObject.Find("PlayerSpawnPoint");
        if (playerSpawnObj == null)
        {
            // 创建玩家出生点
            playerSpawnObj = new GameObject("PlayerSpawnPoint");
            playerSpawnObj.transform.position = new Vector3(0, 0, 0);
            playerSpawnObj.tag = "PlayerSpawnPoint"; // 添加标签便于查找
            Debug.Log("创建玩家出生点: PlayerSpawnPoint");
        }
        
        // 检查是否已存在敌人出生点
        GameObject enemySpawnPointsObj = GameObject.Find("EnemySpawnPoints");
        if (enemySpawnPointsObj == null)
        {
            // 创建敌人出生点容器
            enemySpawnPointsObj = new GameObject("EnemySpawnPoints");
            
            // 创建4个敌人出生点
            for (int i = 0; i < 4; i++)
            {
                GameObject spawnPoint = new GameObject($"EnemySpawnPoint_{i+1}");
                float angle = i * 90f * Mathf.Deg2Rad;
                float radius = 15f; // 出生点到中心的距离
                
                // 在2D平面上以玩家为中心创建出生点
                spawnPoint.transform.position = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );
                
                // 设置出生点的父对象
                spawnPoint.transform.SetParent(enemySpawnPointsObj.transform);
            }
            Debug.Log("创建敌人出生点: EnemySpawnPoints");
        }
    }
}


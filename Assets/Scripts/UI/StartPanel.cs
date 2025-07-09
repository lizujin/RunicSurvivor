// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 处理游戏开始界面
/// </summary>
public class StartPanel : MonoBehaviour
{
    [Header("UI组件")]
    public Button startGameButton;      // 开始游戏按钮
    
    // 引用
    private GameManager gameManager;
    private UIManager uiManager;
    private EnemyManager enemyManager;
    
    private void Awake()
    {
        // 获取管理器引用
        gameManager = GameManager.Instance;
        uiManager = FindObjectOfType<UIManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        
        // 暂停游戏，防止游戏在未点击开始按钮前开始
        if (gameManager != null)
        {
            Time.timeScale = 0f;
        }
        
        // 检查按钮引用
        if (startGameButton == null)
        {
            // 尝试从子对象中寻找开始按钮
            startGameButton = GetComponentInChildren<Button>();
            if (startGameButton == null)
            {
                Debug.LogError("[StartPanel] 未设置StartGameButton且无法在子对象中找到按钮组件");
            }
            else
            {
                Debug.Log("[StartPanel] 在子对象中找到了Button组件作为StartGameButton");
            }
        }
    }
    
    private void Start()
    {
        // 设置按钮点击事件
        SetupButtonEvents();
        
        // 确保游戏在开始前停止
        StopEnemySpawning();
    }
    
    /// <summary>
    /// 停止敌人生成
    /// </summary>
    private void StopEnemySpawning()
    {
        if (enemyManager != null)
        {
            // 如果有自定义方法可以停止敌人生成，调用它
            // 这里假设EnemyManager有这样的方法，如果没有，可能需要添加
            // enemyManager.StopSpawning();
            Debug.Log("[StartPanel] 阻止敌人生成，等待开始按钮点击");
        }
    }
    
    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtonEvents()
    {
        if (startGameButton != null)
        {
            // 清除可能存在的旧监听器
            startGameButton.onClick.RemoveAllListeners();
            
            // 添加新的点击监听器
            startGameButton.onClick.AddListener(OnStartGameClick);
            Debug.Log("[StartPanel] 设置开始游戏按钮点击事件");
        }
    }
    
    /// <summary>
    /// 开始游戏按钮点击处理
    /// </summary>
    public void OnStartGameClick()
    {
        Debug.Log("[StartPanel] 开始游戏按钮点击");
        
        // 恢复游戏时间流动
        Time.timeScale = 1f;
        
        // 隐藏开始面板
        gameObject.SetActive(false);
        
        // 初始化游戏
        if (gameManager != null)
        {
            // 重新初始化游戏，确保状态正确
            gameManager.InitGame();
            
            // 在初始化游戏后，启动第一波
            gameManager.StartNextWave();
            Debug.Log("[StartPanel] 初始化游戏并开始第一波");
        }
        else
        {
            Debug.LogError("[StartPanel] GameManager引用为空，无法初始化游戏");
        }
        
        // 显示游戏界面
        if (uiManager != null)
        {
            uiManager.InitializeUI();
            Debug.Log("[StartPanel] 初始化UI界面");
        }
        else
        {
            Debug.LogError("[StartPanel] UIManager引用为空，无法初始化UI");
        }
    }
}

using UnityEngine;

/// <summary>
/// 敌人重叠调试器，用于可视化防重叠系统
/// </summary>
public class EnemyOverlapDebugger : MonoBehaviour
{
    [Header("调试设置")]
    public bool showGrid = true;
    public bool showAvoidanceForces = true;
    public bool showDensity = false;
    public Color gridColor = Color.yellow;
    public Color avoidanceColor = Color.red;
    public Color densityColor = Color.blue;
    
    private EnemyManager enemyManager;
    private Camera mainCamera;
    
    private void Start()
    {
        enemyManager = FindObjectOfType<EnemyManager>();
        mainCamera = Camera.main;
    }
    
    private void OnDrawGizmos()
    {
        if (enemyManager == null) return;
        
        // 绘制网格
        if (showGrid)
        {
            DrawSpatialGrid();
        }
        
        // 绘制避让力
        if (showAvoidanceForces)
        {
            DrawAvoidanceForces();
        }
        
        // 绘制密度
        if (showDensity)
        {
            DrawDensity();
        }
    }
    
    /// <summary>
    /// 绘制空间网格
    /// </summary>
    private void DrawSpatialGrid()
    {
        Gizmos.color = gridColor;
        
        // 获取网格统计信息
        int totalGrids, occupiedGrids, totalEnemies;
        enemyManager.GetGridStats(out totalGrids, out occupiedGrids, out totalEnemies);
        
        // 绘制网格边界
        Vector2 worldBoundsMin = new Vector2(-50f, -50f);
        Vector2 worldBoundsMax = new Vector2(50f, 50f);
        float gridSize = enemyManager.gridSize;
        
        int gridCountX = Mathf.CeilToInt((worldBoundsMax.x - worldBoundsMin.x) / gridSize);
        int gridCountY = Mathf.CeilToInt((worldBoundsMax.y - worldBoundsMin.y) / gridSize);
        
        for (int x = 0; x < gridCountX; x++)
        {
            for (int y = 0; y < gridCountY; y++)
            {
                Vector3 gridCenter = new Vector3(
                    worldBoundsMin.x + (x + 0.5f) * gridSize,
                    worldBoundsMin.y + (y + 0.5f) * gridSize,
                    0
                );
                
                // 绘制网格边界
                Vector3 size = new Vector3(gridSize, gridSize, 0.1f);
                Gizmos.DrawWireCube(gridCenter, size);
            }
        }
    }
    
    /// <summary>
    /// 绘制避让力
    /// </summary>
    private void DrawAvoidanceForces()
    {
        Gizmos.color = avoidanceColor;
        
        var activeEnemies = enemyManager.GetActiveEnemies();
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            
            Vector2 avoidanceForce = enemyManager.GetAvoidanceForce(enemy);
            if (avoidanceForce.magnitude > 0.1f)
            {
                Vector3 start = enemy.transform.position;
                Vector3 end = start + new Vector3(avoidanceForce.x, avoidanceForce.y, 0);
                
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(end, 0.2f);
            }
        }
    }
    
    /// <summary>
    /// 绘制密度
    /// </summary>
    private void DrawDensity()
    {
        if (mainCamera == null) return;
        
        Gizmos.color = densityColor;
        
        // 在屏幕中心绘制密度
        Vector3 screenCenter = mainCamera.transform.position;
        float density = enemyManager.GetEnemyDensity(screenCenter, 5f);
        
        // 根据密度调整颜色透明度
        Color densityColorWithAlpha = densityColor;
        densityColorWithAlpha.a = Mathf.Clamp01(density * 0.5f);
        Gizmos.color = densityColorWithAlpha;
        
        Gizmos.DrawWireSphere(screenCenter, 5f);
    }
    
    private void OnGUI()
    {
        if (enemyManager == null) return;
        
        // 显示统计信息
        int totalGrids, occupiedGrids, totalEnemies;
        enemyManager.GetGridStats(out totalGrids, out occupiedGrids, out totalEnemies);
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("敌人重叠调试信息", GUI.skin.box);
        GUILayout.Label($"总网格数: {totalGrids}");
        GUILayout.Label($"占用网格: {occupiedGrids}");
        GUILayout.Label($"活跃敌人: {totalEnemies}");
        GUILayout.Label($"网格利用率: {(float)occupiedGrids / totalGrids * 100f:F1}%");
        
        if (mainCamera != null)
        {
            Vector3 screenCenter = mainCamera.transform.position;
            float density = enemyManager.GetEnemyDensity(screenCenter, 5f);
            GUILayout.Label($"中心密度: {density:F2}");
        }
        
        GUILayout.EndArea();
    }
} 
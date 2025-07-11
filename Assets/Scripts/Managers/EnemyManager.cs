// 本文件完全有AI生成

using System;
using System.Collections;
using System.Collections.Generic;
using SimpleSystem;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 敌人管理器，处理敌人的生成和管理
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("敌人预制体")]
    public GameObject[] normalEnemyPrefabs;    // 普通敌人预制体
    public GameObject[] eliteEnemyPrefabs;     // 精英敌人预制体
    public GameObject[] bossEnemyPrefabs;      // Boss敌人预制体
    
    [Header("生成配置")]
    public float baseSpawnInterval = 1.5f;     // 基础敌人生成间隔(秒)
    public float minSpawnInterval = 0.3f;      // 最小生成间隔(秒)
    public float spawnIntervalDecreaseRate = 0.1f; // 每波次生成间隔递减率
    public int baseEnemiesPerWave = 10;        // 基本每波敌人数 (第1波)
    public int enemiesIncreasePerWave = 5;     // 每波敌人递增数量
    public Transform[] spawnPoints;            // 敌人生成点
    
    [Header("波次配置")]
    public int bossWaveInterval = 5;           // Boss波次间隔
    public float eliteChance = 0.15f;          // 精英敌人几率
    [Range(0f, 1f)]
    public float bossHpMultiplier = 3f;        // Boss生命值倍率
    [Range(0f, 1f)]
    public float bossDamageMultiplier = 2f;    // Boss伤害倍率
    
    [Header("波次间隔")]
    public float timeBetweenWaves = 3f;        // 波次之间的间隔时间
    
    // 波次状态
    private int currentWave = 0;
    private int enemiesRemaining = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private bool isPaused = false; // 是否暂停敌人生成
    
    // 玩家引用，用于获取当前攻击力和攻击间隔
    private PlayerController playerController;
    
    // 缓存的敌人列表
    private List<Enemy> activeEnemies = new List<Enemy>();
    
    // 波次配置
    private WaveConfig currentWaveConfig;
    
    // 波次开始/结束事件
    public delegate void WaveSpawnCompleteHandler(int waveNumber);
    public event WaveSpawnCompleteHandler OnWaveSpawnComplete;
    
    public delegate void WaveCompleteHandler(int waveNumber);
    public event WaveCompleteHandler OnWaveComplete;
    
    /// <summary>
    /// 波次配置
    /// </summary>
    [System.Serializable]
    public class WaveConfig
    {
        public int waveNumber;                  // 波次编号
        public int totalEnemies;                // 敌人总数
        public float normalEnemyChance;         // 普通敌人几率
        public float eliteEnemyChance;          // 精英敌人几率
        public float bossEnemyChance;           // Boss敌人几率
        public float healthMultiplier = 1f;     // 生命值倍率
        public float damageMultiplier = 1f;     // 伤害倍率
        public bool isBossWave = false;         // 是否Boss波次
    }
    
    // 敌人生成类型枚举
    private enum EnemySpawnType
    {
        Normal,
        Elite,
        Boss
    }
    
    private void Awake()
    {
        // 查找场景中的玩家控制器
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("未找到PlayerController，将使用默认敌人分配");
        }
    }
    
    private void Start()
    {
        // 确保有生成点
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // 首先尝试在场景中查找EnemySpawnPoints对象
            GameObject enemySpawnPointsObj = GameObject.Find("EnemySpawnPoints");
            
            if (enemySpawnPointsObj != null)
            {
                Debug.Log("找到场景中的EnemySpawnPoints对象");
                
                // 查找所有子对象作为敌人生成点
                Transform[] foundSpawnPoints = enemySpawnPointsObj.GetComponentsInChildren<Transform>();
                
                // 排除父对象本身
                List<Transform> validSpawnPoints = new List<Transform>();
                foreach (Transform t in foundSpawnPoints)
                {
                    if (t != enemySpawnPointsObj.transform)
                    {
                        validSpawnPoints.Add(t);
                    }
                }
                
                if (validSpawnPoints.Count > 0)
                {
                    spawnPoints = validSpawnPoints.ToArray();
                    Debug.Log($"使用场景中找到的 {spawnPoints.Length} 个敌人生成点");
                }
                else
                {
                    Debug.LogWarning("EnemySpawnPoints对象下没有子对象，将使用默认生成点");
                    CreateDefaultSpawnPoints();
                }
            }
            else
            {
                Debug.LogWarning("没有找到EnemySpawnPoints对象，将使用默认生成点");
                CreateDefaultSpawnPoints();
            }
        }
        
        // 游戏开始时自动生成第一波敌人
        StartFirstWave();
    }
    
    /// <summary>
    /// 开始第一波敌人
    /// </summary>
    public void StartFirstWave()
    {
        // 延迟一小段时间后开始第一波，给玩家一点准备时间
        StartCoroutine(StartFirstWaveDelayed(1f));
    }
    
    /// <summary>
    /// 延迟开始第一波
    /// </summary>
    private IEnumerator StartFirstWaveDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 如果GameManager存在，调用其StartNextWave方法
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.StartNextWave();
        }
        else
        {
            // 如果GameManager不存在，直接生成第一波敌人
            SpawnWave(1);
        }
    }
    
    /// <summary>
    /// 开始下一波敌人 - 立即开始，无需延迟
    /// </summary>
    private IEnumerator StartNextWaveAfterDelay(int nextWave, float delay)
    {
        // 通知UI显示下一波信息
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowWaveStartMessage(nextWave);
        }
        
        // 立即开始下一波，无需等待
        yield return new WaitForSeconds(0.5f); // 只等待很短的时间，让UI有时间更新
        
        // 如果GameManager存在，调用其StartNextWave方法
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.StartNextWave();
        }
        else
        {
            // 如果GameManager不存在，直接生成下一波敌人
            SpawnWave(nextWave);
        }
        
        Debug.Log($"场上没有敌人，立即开始第{nextWave}波");
    }
    
    /// <summary>
    /// 生成波次敌人
    /// </summary>
    public void SpawnWave(int waveNumber)
    {
        if (isSpawning) return;
        
        currentWave = waveNumber;
        
        // 清除之前的敌人（如果有的话）
        CleanupDeadEnemies();
        
        // 生成波次配置
        currentWaveConfig = GenerateWaveConfig(waveNumber);
        
        // 开始生成敌人
        StartCoroutine(SpawnEnemiesCoroutine(currentWaveConfig));
    }
    
    /// <summary>
    /// 生成波次配置
    /// </summary>
    private WaveConfig GenerateWaveConfig(int waveNumber)
    {
        WaveConfig config = new WaveConfig();
        config.waveNumber = waveNumber;
        
        // 计算敌人总数 - 按照 (波数-1)*20 + 5 的公式递增
        config.totalEnemies = GameManager.Instance.GetEnemyNumberPerWave(waveNumber);
        
        // 检查是否Boss波次
        config.isBossWave = (waveNumber % bossWaveInterval == 0);
        
        // 设置敌人几率
        if (config.isBossWave)
        {
            // Boss波次
            config.normalEnemyChance = 0.7f;
            config.eliteEnemyChance = 0.25f;
            config.bossEnemyChance = 0.05f;
            
            // 增加属性倍率
            config.healthMultiplier = 1f + (waveNumber * 0.1f);
            config.damageMultiplier = 1f + (waveNumber * 0.05f);
        }
        else
        {
            // 普通波次
            config.normalEnemyChance = 1f - eliteChance;
            config.eliteEnemyChance = eliteChance;
            config.bossEnemyChance = 0f;
            
            // 设置敌人属性
            // 计算出精确的属性倍率，使敌人的属性正好是：
            // 血量为 20 + 等级 * 10
            // 攻击力为 2 + 等级 * 1.1
            
            // 对于普通敌人，原始maxHealth是50，attackDamage是5
            float targetHealth = 20f + waveNumber * 10f;
            float targetDamage = 2f + waveNumber * 1.1f;
            
            // 计算需要的倍率，使最终值为目标值
            config.healthMultiplier = targetHealth / 50f; // 默认Enemy.maxHealth是50
            config.damageMultiplier = targetDamage / 5f;  // 默认Enemy.attackDamage是5
        }
        
        return config;
    }
    
    /// <summary>
    /// 暂停敌人生成
    /// </summary>
    public void PauseSpawning()
    {
        isPaused = true;
        Debug.Log("敌人生成已暂停");
    }
    
    /// <summary>
    /// 恢复敌人生成
    /// </summary>
    public void ResumeSpawning()
    {
        isPaused = false;
        Debug.Log("敌人生成已恢复");
    }
    
    /// <summary>
    /// 计算当前波次的敌人生成间隔
    /// </summary>
    private float CalculateSpawnInterval(int waveNumber)
    {
        // 根据波次计算生成间隔，波次越高，间隔越短
        float interval = baseSpawnInterval - ((waveNumber - 1) * spawnIntervalDecreaseRate);
        
        // 确保不小于最小生成间隔
        return Mathf.Max(interval, minSpawnInterval);
    }

    /// <summary>
    /// 获取玩家属性，用于调整敌人分配
    /// </summary>
    private void GetPlayerStats(out float attackPower, out float attackInterval)
    {
        attackPower = 10f;    // 默认攻击力
        attackInterval = 1.0f; // 默认攻击间隔
        
        // 如果找到了玩家控制器，基于队伍成员估算攻击力
        if (playerController != null)
        {
            // 获取玩家队伍中的角色数量作为攻击力的估算
            List<Character> teamMembers = playerController.GetTeamCharacters();
            int teamSize = teamMembers.Count;
            
            // 根据队伍大小和玩家生命值状态估算战斗力
            float healthRatio = playerController.currentHealth / playerController.maxHealth;
            
            // 估算攻击力 - 基于队伍大小
            attackPower = 5f + (teamSize * 2f);
            
            // 估算攻击间隔 - 队伍越大，攻击越频繁
            attackInterval = Mathf.Max(0.2f, 1.0f - (teamSize * 0.05f));
            
            // 如果血量低，考虑处于劣势
            if (healthRatio < 0.3f)
            {
                attackPower *= 0.7f;
                attackInterval *= 1.5f;
            }
            
            // 确保值在合理范围内
            attackPower = Mathf.Max(1f, attackPower);
            attackInterval = Mathf.Max(0.1f, attackInterval);
        }
    }
    
    /// <summary>
    /// 根据玩家状态计算敌人分配比例
    /// </summary>
    private float[] CalculateEnemyDistribution()
    {
        // 获取玩家属性
        float attackPower, attackInterval;
        GetPlayerStats(out attackPower, out attackInterval);
        
        // 计算玩家战斗力指数 (高攻击力和低攻击间隔表示更强)
        float playerPowerIndex = attackPower / attackInterval;
        
        // 分配比例 - 总和为1.0
        float[] distribution = new float[5];
        
        // 基于玩家战斗力调整敌人分配
        if (playerPowerIndex > 20f)
        {
            // 强力玩家 - 前期更多敌人
            distribution[0] = 0.30f; // 30% 在第一轮
            distribution[1] = 0.25f; // 25% 在第二轮
            distribution[2] = 0.20f; // 20% 在第三轮
            distribution[3] = 0.15f; // 15% 在第四轮
            distribution[4] = 0.10f; // 10% 在第五轮
        }
        else if (playerPowerIndex > 10f)
        {
            // 中等玩家 - 相对均衡，略微前倾
            distribution[0] = 0.25f; // 25% 在第一轮
            distribution[1] = 0.20f; // 20% 在第二轮
            distribution[2] = 0.20f; // 20% 在第三轮
            distribution[3] = 0.20f; // 20% 在第四轮
            distribution[4] = 0.15f; // 15% 在第五轮
        }
        else
        {
            // 弱势玩家 - 敌人更均匀分布
            distribution[0] = 0.15f; // 15% 在第一轮
            distribution[1] = 0.20f; // 20% 在第二轮
            distribution[2] = 0.20f; // 20% 在第三轮
            distribution[3] = 0.20f; // 20% 在第四轮
            distribution[4] = 0.25f; // 25% 在第五轮
        }
        
        return distribution;
    }

    /// <summary>
    /// 生成敌人协程 - 将敌人分5轮次生成
    /// </summary>
    private IEnumerator SpawnEnemiesCoroutine(WaveConfig config)
    {
        isSpawning = true;
        enemiesRemaining = config.totalEnemies;
        enemiesAlive = 0;
        
        // 计算当前波次的敌人生成间隔
        float currentSpawnInterval = CalculateSpawnInterval(config.waveNumber);
        
        // 获取基于玩家状态的敌人分配比例
        float[] distribution = CalculateEnemyDistribution();
        
        // 计算轮次和每轮次的敌人数量
        int totalRounds = 5;
        int[] enemiesPerRound = new int[totalRounds];
        
        // 根据分配比例计算每轮次的敌人数量
        int totalAssigned = 0;
        for (int i = 0; i < totalRounds - 1; i++) // 前4轮
        {
            enemiesPerRound[i] = Mathf.RoundToInt(config.totalEnemies * distribution[i]);
            totalAssigned += enemiesPerRound[i];
        }
        // 最后一轮分配剩余所有敌人，确保总数正确
        enemiesPerRound[totalRounds - 1] = config.totalEnemies - totalAssigned;
        
        Debug.Log($"开始生成第{config.waveNumber}波敌人，总数：{config.totalEnemies}，分{totalRounds}轮次生成");
        
        // 如果是Boss波次，确保至少生成一个Boss
        bool bossSpawned = false;
        
        // 分轮次生成敌人
        for (int round = 0; round < totalRounds; round++)
        {
            // 如果暂停了敌人生成，则等待直到恢复
            while (isPaused)
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            // 计算当前轮次应该生成的敌人数量
            int enemiesInThisRound = Mathf.Min(enemiesPerRound[round], enemiesRemaining);
            
            Debug.Log($"生成第{round + 1}轮次敌人，数量：{enemiesInThisRound}，剩余敌人：{enemiesRemaining}");
            
            // 在当前轮次中生成敌人
            for (int i = 0; i < enemiesInThisRound; i++)
            {
                // 如果是Boss波次且还没有生成Boss，确保生成一个Boss
                if (config.isBossWave && !bossSpawned && bossEnemyPrefabs.Length > 0 && i == 0 && round == 0)
                {
                    SpawnEnemy(EnemySpawnType.Boss, config);
                    bossSpawned = true;
                }
                else
                {
                    // 确定敌人类型
                    EnemySpawnType spawnType = DetermineEnemyType(config);
                    
                    // 生成敌人
                    SpawnEnemy(spawnType, config);
                }
                
                // 减少剩余敌人
                enemiesRemaining--;
                
                // 在同一轮次内，敌人之间的生成间隔较短
                yield return new WaitForSeconds(0.2f);
            }
            
            // 轮次之间的等待时间：当没有敌人时立即开始下一轮，否则最多等待3秒
            if (round < totalRounds - 1)
            {
                float maxRoundInterval = 3f; // 最大轮次间隔为3秒
                float waitTime = 0f;
                bool startedNextRound = false;
                
                // 等待敌人被清除或达到最大等待时间
                while (waitTime < maxRoundInterval && !startedNextRound)
                {
                    // 更新敌人计数
                    CleanupDeadEnemies();
                    
                    // 如果没有活跃敌人，立即开始下一轮
                    if (enemiesAlive <= 0)
                    {
                        Debug.Log($"场上没有敌人，立即开始第{round + 2}轮次");
                        startedNextRound = true;
                        break;
                    }
                    
                    // 等待一小段时间再检查
                    yield return new WaitForSeconds(0.5f);
                    waitTime += 0.5f;
                }
                
                if (!startedNextRound)
                {
                    Debug.Log($"达到最大等待时间({maxRoundInterval}秒)，开始下一轮次");
                }
            }
        }
        
        isSpawning = false;
        OnWaveSpawnComplete?.Invoke(config.waveNumber);
        
        Debug.Log($"第{config.waveNumber}波敌人生成完成");
        
        // 等待所有敌人被击败
        while (enemiesAlive > 0)
        {
            CleanupDeadEnemies();
            yield return new WaitForSeconds(1f);
        }
        
        // 波次完成
        OnWaveComplete?.Invoke(config.waveNumber);
        
        // 通知GameManager波次结束
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.EndWave(true);
            
            // 自动开始下一波敌人
            int nextWave = config.waveNumber + 1;
            StartCoroutine(StartNextWaveAfterDelay(nextWave, timeBetweenWaves));
        }
        
        Debug.Log($"第{config.waveNumber}波敌人全部击败");
    }
    
    /// <summary>
    /// 确定敌人类型
    /// </summary>
    private EnemySpawnType DetermineEnemyType(WaveConfig config)
    {
        float random = Random.value;
        
        if (random < config.normalEnemyChance)
        {
            return EnemySpawnType.Normal;
        }
        else if (random < config.normalEnemyChance + config.eliteEnemyChance)
        {
            return EnemySpawnType.Elite;
        }
        else
        {
            return EnemySpawnType.Boss;
        }
    }
    
    /// <summary>
    /// 生成敌人
    /// </summary>
    private void SpawnEnemy(EnemySpawnType spawnType, WaveConfig config)
    {
        // 选择生成点
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;
        
        // 获取对应类型的敌人预制体
        GameObject enemyPrefab = GetEnemyPrefab(spawnType);
        if (enemyPrefab == null) return;
        
        // 实例化敌人
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            // 根据波次和类型调整敌人属性
            float healthMultiplier = config.healthMultiplier;
            float damageMultiplier = config.damageMultiplier;
            
            // Boss有额外倍率
            if (spawnType == EnemySpawnType.Boss)
            {
                healthMultiplier *= bossHpMultiplier;
                damageMultiplier *= bossDamageMultiplier;
            }
            // 精英敌人有较小的额外倍率
            else if (spawnType == EnemySpawnType.Elite)
            {
                healthMultiplier *= 1.5f;
                damageMultiplier *= 1.3f;
            }
            
            // 设置敌人属性
            enemy.SetStats(healthMultiplier, damageMultiplier);
            
            // 添加到活跃敌人列表
            activeEnemies.Add(enemy);
            enemiesAlive++;
            
            // 监听敌人死亡
            StartCoroutine(MonitorEnemyDeath(enemy));
        }
    }
    
    /// <summary>
    /// 获取随机生成点
    /// </summary>
    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }
        
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
    
    /// <summary>
    /// 获取敌人预制体
    /// </summary>
    private GameObject GetEnemyPrefab(EnemySpawnType spawnType)
    {
        switch (spawnType)
        {
            case EnemySpawnType.Normal:
                if (normalEnemyPrefabs == null || normalEnemyPrefabs.Length == 0)
                {
                    Debug.LogWarning("没有设置普通敌人预制体");
                    return null;
                }
                return normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
                
            case EnemySpawnType.Elite:
                if (eliteEnemyPrefabs == null || eliteEnemyPrefabs.Length == 0)
                {
                    // 如果没有精英敌人预制体，使用普通敌人代替
                    if (normalEnemyPrefabs != null && normalEnemyPrefabs.Length > 0)
                    {
                        return normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
                    }
                    return null;
                }
                return eliteEnemyPrefabs[Random.Range(0, eliteEnemyPrefabs.Length)];
                
            case EnemySpawnType.Boss:
                if (bossEnemyPrefabs == null || bossEnemyPrefabs.Length == 0)
                {
                    // 如果没有Boss敌人预制体，使用精英敌人代替
                    if (eliteEnemyPrefabs != null && eliteEnemyPrefabs.Length > 0)
                    {
                        return eliteEnemyPrefabs[Random.Range(0, eliteEnemyPrefabs.Length)];
                    }
                    // 如果没有精英敌人预制体，使用普通敌人代替
                    else if (normalEnemyPrefabs != null && normalEnemyPrefabs.Length > 0)
                    {
                        return normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
                    }
                    return null;
                }
                return bossEnemyPrefabs[Random.Range(0, bossEnemyPrefabs.Length)];
                
            default:
                return null;
        }
    }
    
    /// <summary>
    /// 监控敌人死亡
    /// </summary>
    private IEnumerator MonitorEnemyDeath(Enemy enemy)
    {
        // 等待敌人死亡或被销毁
        while (enemy != null && enemy.IsAlive)
        {
            yield return null;
        }
        
        // 敌人死亡后从列表中移除
        if (enemy != null && activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            enemiesAlive--;
        }
    }
    
    /// <summary>
    /// 清理已死亡的敌人
    /// </summary>
    private void CleanupDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = activeEnemies[i];
            if (enemy == null || !enemy.IsAlive)
            {
                activeEnemies.RemoveAt(i);
                enemiesAlive--;
            }
        }
    }
    
    /// <summary>
    /// 获取当前活跃敌人列表
    /// </summary>
    public List<Enemy> GetActiveEnemies()
    {
        // 清理已死亡的敌人，确保返回的列表中只包含活跃敌人
        CleanupDeadEnemies();
        // 返回活跃敌人列表的副本
        return new List<Enemy>(activeEnemies);
    }
    
    /// <summary>
    /// 获取距离玩家最近的敌人
    /// </summary>
    public Enemy GetNearestEnemy(Vector3 position, float maxDistance = float.MaxValue)
    {
        CleanupDeadEnemies();
        
        Enemy nearestEnemy = null;
        float nearestDistance = maxDistance;
        
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }
        
        return nearestEnemy;
    }

    /// <summary>
    /// 获取距离玩家最近的敌人
    /// </summary>
    public bool GetNearestEnemies(Vector3 position, ref List<BaseEntity> nearestEnemies, float maxDistance = float.MaxValue,int count = 1)
    {
        CleanupDeadEnemies();
        if (nearestEnemies == null)
        {
            return false;
        }

        float[] nearestDistance = new float[count];
        int foundIndex = 0;
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);

                if (distance > maxDistance)
                {
                    continue;
                }

                if (nearestDistance[foundIndex] == 0 && foundIndex < (count - 1))
                {
                    nearestDistance[foundIndex] = distance;
                    nearestEnemies.Add(enemy);
                    foundIndex++;
                }

                var max = Math.Min(foundIndex, count - 1);
                for (int i = max; i > 0; i--)
                {
                    if (distance < nearestDistance[i])
                    {
                        if (i == foundIndex)
                        {
                            nearestDistance[i] = distance;
                            nearestEnemies[i] = enemy;
                        } else{
                            var tempDistance = nearestDistance[i];
                            nearestDistance[i] = distance;
                            nearestDistance[i+1] = tempDistance;

                            var tempEnemy = nearestEnemies[i];
                            nearestEnemies[i] = enemy;
                            nearestEnemies[i+1] = tempEnemy;
                        }
                    }else{
                        break;
                    }

                }
            }
        }
        return true;
    }

    
    /// <summary>
    /// 获取指定范围内的所有敌人
    /// </summary>
    public List<Enemy> GetEnemiesInRange(Vector3 position, float range)
    {
        CleanupDeadEnemies();
        
        List<Enemy> enemiesInRange = new List<Enemy>();
        float rangeSqr = range * range;
        
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                float distanceSqr = (enemy.transform.position - position).sqrMagnitude;
                if (distanceSqr <= rangeSqr)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }
        
        return enemiesInRange;
    }
    
    /// <summary>
    /// 创建默认的敌人生成点
    /// </summary>
    private void CreateDefaultSpawnPoints()
    {
        Debug.Log("创建默认敌人生成点");
        float r = 5f;
        // 创建默认生成点
        int count = 20;
        spawnPoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            GameObject spawnPointObj = new GameObject($"SpawnPoint_{i}");
            float angle = i * 90f * Mathf.Deg2Rad;
            spawnPointObj.transform.position = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);
            spawnPoints[i] = spawnPointObj.transform;
            spawnPointObj.transform.parent = transform;
        }
    }
}

// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using SimpleSystem;
using UnityEngine;

/// <summary>
/// 玩家控制器，管理玩家输入和队伍
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("队伍设置")]
    public int maxTeamSize = 40;
    public Transform characterContainer;
    public float characterSpacing = 0.8f;
    public float reformationDelay = 0.5f;
    public float followThreshold = 0.5f;
    
    [Header("状态")]
    public bool invulnerable = false;
    public float invulnerabilityTime = 1f;
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    
    // 内部变量
    private List<Character> teamMembers = new List<Character>();
    private float invulnerabilityTimer = 0f;
    private float lastMovementTime = 0f;
    private Character currentCenterCharacter;
    private float targetSearchInterval = 0.5f; // 目标搜索间隔
    private float targetSearchTimer = 0f;      // 目标搜索计时器
    
    // 输入控制
    private Vector3 moveDirection = Vector3.zero;
    
    // 属性
    public int TeamSize => teamMembers.Count;
    public IReadOnlyList<Character> TeamMembers => teamMembers.AsReadOnly();
    public Character CurrentCenterCharacter => currentCenterCharacter;
    public bool IsMoving => moveDirection.magnitude > 0.1f;
    
    // 事件
    public delegate void CharacterAddedHandler(Character character);
    public event CharacterAddedHandler OnCharacterAdded;
    
    public delegate void CharacterRemovedHandler(Character character);
    public event CharacterRemovedHandler OnCharacterRemoved;
    
    public delegate void PlayerDamageHandler(float amount, float currentHealth);
    public event PlayerDamageHandler OnPlayerDamage;
    
    public delegate void CenterChangeHandler(Character oldCenter, Character newCenter);
    public event CenterChangeHandler OnCenterChanged;
    
    public delegate void TeamChangedHandler(List<Character> teamMembers);
    public event TeamChangedHandler OnTeamChanged;
    
    [Header("渲染设置")]
    public int playerSortingOrder = 10;       // 玩家渲染排序
    public string playerSortingLayerName = "Characters"; // 玩家排序层
    
    private void Awake()
    {
        // 确保有角色容器
        if (characterContainer == null)
        {
            GameObject container = new GameObject("CharacterContainer");
            container.transform.parent = transform;
            characterContainer = container.transform;
        }
        
        // 设置初始状态
        currentHealth = maxHealth;
        
        // 确保玩家角色始终在环境之上
        EnsurePlayerVisibility();
    }
    
    /// <summary>
    /// 确保玩家可见性，不被环境遮挡
    /// </summary>
    private void EnsurePlayerVisibility()
    {
        // 1. 调整Z坐标确保在前面
        Vector3 pos = transform.position;
        pos.z = -1; // 在2D游戏中，更小的z值会显示在前面
        transform.position = pos;
        
        // 2. 设置玩家的排序层级
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = playerSortingOrder;
            
            // 如果指定了排序层名称，也设置它
            if (!string.IsNullOrEmpty(playerSortingLayerName))
            {
                renderer.sortingLayerName = playerSortingLayerName;
            }
        }
    }
    
    private void Start()
    {
        // 不再自动创建角色，由GameController负责创建玩家预制体
        // PlayerController由GameController创建并挂载到玩家对象上
    }
    
    /// <summary>
    /// 由GameController调用，初始化第一个角色
    /// </summary>
    public void InitializeFirstCharacter(GameObject characterPrefab)
    {
        if (characterPrefab != null)
        {
            // 在玩家位置实例化角色预制体
            GameObject characterObj = Instantiate(characterPrefab, transform.position, Quaternion.identity);
            
            // 获取角色组件
            Character character = characterObj.GetComponent<Character>();
            if (character != null)
            {
                // 确保角色在环境前面显示
                SetCharacterVisibility(characterObj);
                
                // 添加到队伍
                AddCharacter(character);
                
                // 设置为中心角色
                if (currentCenterCharacter == null)
                {
                    SetCenterCharacter(character);
                }
            }
        }
    }
    
    /// <summary>
    /// 设置角色可见性，不被环境遮挡
    /// </summary>
    private void SetCharacterVisibility(GameObject characterObj)
    {
        if (characterObj == null) return;
        
        // 调整角色Z轴位置
        Vector3 pos = characterObj.transform.position;
        pos.z = -1; // 在2D游戏中，更小的z值会显示在前面
        characterObj.transform.position = pos;
        
        // 设置角色的所有渲染器
        SpriteRenderer[] renderers = characterObj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = playerSortingOrder;
            
            // 如果指定了排序层名称，也设置它
            if (!string.IsNullOrEmpty(playerSortingLayerName))
            {
                renderer.sortingLayerName = playerSortingLayerName;
            }
        }
    }
    
    private void Update()
    {
        // 处理输入
        HandleInput();
        
        // 更新无敌时间
        if (invulnerable && invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0)
            {
                invulnerable = false;
            }
        }
        
        // 定期搜索敌人
        targetSearchTimer -= Time.deltaTime;
        if (targetSearchTimer <= 0)
        {
            SearchForTargets();
            targetSearchTimer = targetSearchInterval;
        }
    }
    
    /// <summary>
    /// 为队伍中的所有角色搜索并设置攻击目标
    /// </summary>
    private void SearchForTargets()
    {
        if (teamMembers.Count == 0) return;
        
        // 获取敌人管理器
        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager == null) return;
        
        // 为每个队伍成员寻找最近的敌人
        foreach (Character character in teamMembers)
        {
            try
            {
                // 获取角色攻击范围内的最近敌人
                float searchRange = character.attackRange * 1.2f;
                Enemy nearestEnemy = null;
                
                // 获取所有活跃敌人
                List<Enemy> activeEnemies = enemyManager.GetActiveEnemies();
                float nearestDistance = searchRange;
                
                // 手动查找最近的敌人
                foreach (Enemy enemy in activeEnemies)
                {
                    if (enemy != null && enemy.IsAlive)
                    {
                        float distance = Vector3.Distance(character.transform.position, enemy.transform.position);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestEnemy = enemy;
                        }
                    }
                }
                
                if (nearestEnemy != null)
                {
                    // 设置目标
                    character.SetTarget(nearestEnemy.transform);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"搜索目标时出错: {e.Message}");
            }
        }
    }
    
    private void FixedUpdate()
    {
        // 移动玩家
        MovePlayer();
        
        // 如果玩家移动，则更新最后移动时间
        if (moveDirection.magnitude > 0.1f)
        {
            lastMovementTime = Time.time;
        }
        
        // 延迟重组队形（在停止移动一段时间后）
        if (Time.time - lastMovementTime > reformationDelay)
        {
            ArrangeFormation();
        }
    }
    
    // 已删除CreatePlayerCharacter方法，现在由GameController负责创建角色
    // 请使用InitializeFirstCharacter方法替代
    
    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 水平移动输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // 计算移动方向 (为2D游戏调整方向)
        moveDirection = new Vector3(horizontal, vertical, 0).normalized;
        
        // 切换C位角色
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleCenter();
        }
        
        // 开发测试功能：添加测试角色
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CreateTestCharacter();
        }
    }
    
    /// <summary>
    /// 移动玩家
    /// </summary>
    private void MovePlayer()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            // 计算移动速度 (2D平面上的移动)
            Vector3 movement = new Vector3(moveDirection.x, moveDirection.y, 0) * moveSpeed * Time.fixedDeltaTime;
            
            // 计算新位置
            Vector3 newPosition = transform.position + movement;
            
            // 获取屏幕边界（摄像机视口的世界坐标）
            Vector3 minScreenBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 maxScreenBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
            
            // 添加一些偏移以避免角色部分超出屏幕
            float offsetX = 0.5f; // 水平偏移（根据角色宽度调整）
            float offsetY = 0.5f; // 垂直偏移（根据角色高度调整）
            
            // 限制位置在屏幕边界内
            newPosition.x = Mathf.Clamp(newPosition.x, minScreenBounds.x + offsetX, maxScreenBounds.x - offsetX);
            newPosition.y = Mathf.Clamp(newPosition.y, minScreenBounds.y + offsetY, maxScreenBounds.y - offsetY);
            
            // 应用修正后的位置
            transform.position = newPosition;
            
            // 2D游戏中设置角色朝向（左/右）
            if (moveDirection.x != 0)
            {
                // 设置缩放来翻转精灵
                Vector3 characterScale = transform.localScale;
                characterScale.x = (moveDirection.x > 0) ? Mathf.Abs(characterScale.x) : -Mathf.Abs(characterScale.x);
                transform.localScale = characterScale;
            }
        }
    }
    
    /// <summary>
    /// 添加角色到队伍
    /// </summary>
    public bool AddCharacter(Character character)
    {
        if (character == null || teamMembers.Count >= maxTeamSize)
        {
            return false;
        }
        
        // 如果已在队伍中，则返回
        if (teamMembers.Contains(character))
        {
            return false;
        }
        
        // 确保角色在环境前面显示
        if (character.gameObject != null)
        {
            SetCharacterVisibility(character.gameObject);
        }
        
        // 添加到队伍
        teamMembers.Add(character);
        
        // 重新组织队形
        ArrangeFormation();
        
        // 设置父物体
        character.transform.SetParent(characterContainer);
        
        // 如果是第一个角色，设为中心角色
        if (teamMembers.Count == 1)
        {
            SetCenterCharacter(character);
        }
        
        // 订阅死亡事件
        character.OnCharacterDeath += HandleCharacterDeath;
        
        // 触发添加事件
        OnCharacterAdded?.Invoke(character);
        
        // 通知协同效应管理器
        SynergyManager synergyManager = SynergyManager.Instance;
        if (synergyManager != null)
        {
            synergyManager.UpdateTeamSynergies(teamMembers);
        }
        
        // 触发队伍改变事件
        OnTeamChanged?.Invoke(GetTeamCharacters());
        World.GetInstance().GetSystem<SkillSystem>().CastEntitySkill(character);
        return true;
    }
    
    /// <summary>
    /// 从队伍中移除角色
    /// </summary>
    public void RemoveCharacter(Character character)
    {
        if (character == null || !teamMembers.Contains(character))
        {
            return;
        }
        
        // 取消订阅事件
        character.OnCharacterDeath -= HandleCharacterDeath;
        
        // 如果是中心角色，选择新的中心角色
        if (character == currentCenterCharacter)
        {
            int index = teamMembers.IndexOf(character);
            Character newCenter = null;
            
            // 尝试选择下一个角色为中心，如果没有下一个则选上一个
            if (index < teamMembers.Count - 1)
            {
                newCenter = teamMembers[index + 1];
            }
            else if (teamMembers.Count > 1)
            {
                newCenter = teamMembers[0];
            }
            
            if (newCenter != null)
            {
                SetCenterCharacter(newCenter);
            }
            else
            {
                currentCenterCharacter = null;
            }
        }
        
        // 从队伍中移除
        teamMembers.Remove(character);
        
        // 重新组织队形
        ArrangeFormation();
        
        // 触发移除事件
        OnCharacterRemoved?.Invoke(character);
        
        // 通知协同效应管理器
        SynergyManager synergyManager = SynergyManager.Instance;
        if (synergyManager != null)
        {
            synergyManager.UpdateTeamSynergies(teamMembers);
        }
        
        // 触发队伍改变事件
        OnTeamChanged?.Invoke(GetTeamCharacters());
    }
    
    /// <summary>
    /// 处理角色死亡事件
    /// </summary>
    private void HandleCharacterDeath(Character character)
    {
        RemoveCharacter(character);
    }
    
    /// <summary>
    /// 设置中心角色
    /// </summary>
    public void SetCenterCharacter(Character character)
    {
        if (!teamMembers.Contains(character))
        {
            return;
        }
        
        Character oldCenter = currentCenterCharacter;
        
        // 取消旧的中心角色
        if (currentCenterCharacter != null)
        {
            // 使用Character中的SetAsCenter方法设置
            currentCenterCharacter.SetAsCenter(false);
        }
        
        // 设置新的中心角色
        currentCenterCharacter = character;
        // 使用Character中的SetAsCenter方法设置
        if (currentCenterCharacter != null)
        {
            currentCenterCharacter.SetAsCenter(true);
        }
        
        // 重新组织队形
        ArrangeFormation();
        
        // 触发事件
        OnCenterChanged?.Invoke(oldCenter, currentCenterCharacter);
    }
    
    /// <summary>
    /// 循环切换中心角色
    /// </summary>
    public void CycleCenter()
    {
        if (teamMembers.Count <= 1)
        {
            return;
        }
        
        // 找到当前中心角色的索引
        int currentIndex = teamMembers.IndexOf(currentCenterCharacter);
        
        // 计算下一个角色的索引
        int nextIndex = (currentIndex + 1) % teamMembers.Count;
        
        // 设置新的中心角色
        SetCenterCharacter(teamMembers[nextIndex]);
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float amount)
    {
        // 如果处于无敌状态，则不受伤害
        if (invulnerable) 
        {
            Debug.Log($"玩家处于无敌状态，忽略伤害: {amount}");
            return;
        }
        
        float oldHealth = currentHealth;
        
        // 应用伤害
        currentHealth -= amount;
        
        Debug.Log($"玩家受到伤害: {amount}, 生命值从 {oldHealth} 变为 {currentHealth}");
        
        // 触发事件
        OnPlayerDamage?.Invoke(amount, currentHealth);
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("玩家生命值为零，触发死亡");
            Die();
        }
        else
        {
            // 短暂无敌
            invulnerable = true;
            invulnerabilityTimer = invulnerabilityTime;
            Debug.Log($"玩家进入无敌状态，持续 {invulnerabilityTime} 秒");
        }
    }
    
    /// <summary>
    /// 治疗生命值
    /// </summary>
    public void Heal(float amount)
    {
        if (amount <= 0) return;
        
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        Debug.Log($"玩家被治疗: {amount}, 生命值从 {oldHealth} 变为 {currentHealth}");
        
        // 触发治疗事件 (使用负数表示治疗而非伤害)
        OnPlayerDamage?.Invoke(-amount, currentHealth);
    }
    
    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        // 游戏结束
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.EndGame(false);
        }
    }
    
    /// <summary>
    /// 组织队伍队形：中心一个角色，剩余角色按照每次十个依次向外扩展
    /// 当外圈不满10人时，确保均衡站位
    /// </summary>
    private void ArrangeFormation()
    {
        // 如果没有队员或没有角色容器，则不用排布
        if (teamMembers.Count == 0 || characterContainer == null)
        {
            return;
        }
        
        // 如果只有一个角色，直接放在中心
        if (teamMembers.Count == 1)
        {
            StartCoroutine(MoveToPosition(teamMembers[0], Vector3.zero));
            return;
        }
        
        // 首先确保中心角色在中间位置
        if (currentCenterCharacter != null)
        {
            StartCoroutine(MoveToPosition(currentCenterCharacter, Vector3.zero));
        }
        
        // 创建一个不包含中心角色的列表
        List<Character> nonCenterCharacters = new List<Character>();
        foreach (Character character in teamMembers)
        {
            if (character != currentCenterCharacter)
            {
                nonCenterCharacters.Add(character);
            }
        }
        
        // 计算每个环上的角色数量
        const int maxCharactersPerRing = 10;
        int totalCharacters = nonCenterCharacters.Count;
        
        // 计算需要多少层环形阵列
        int totalRings = Mathf.CeilToInt((float)totalCharacters / maxCharactersPerRing);
        
        // 计算每个环上的精确角色数量
        int[] charactersInRing = new int[totalRings];
        int remainingCharacters = totalCharacters;
        
        for (int ring = 0; ring < totalRings; ring++)
        {
            // 为当前环分配角色（最多10个）
            charactersInRing[ring] = Mathf.Min(remainingCharacters, maxCharactersPerRing);
            remainingCharacters -= charactersInRing[ring];
        }
        
        // 为每层角色计算位置并移动
        int characterIndex = 0;
        
        for (int ring = 0; ring < totalRings; ring++)
        {
            int charactersThisRing = charactersInRing[ring];
            float radius = (ring + 1) * characterSpacing;
            
            // 根据当前环中的实际角色数量均匀分布角度
            for (int i = 0; i < charactersThisRing; i++)
            {
                // 根据当前环中的实际角色数量计算角度间隔
                float angleIncrement = 360f / charactersThisRing;
                float angle = angleIncrement * i;
                
                // 计算位置（在2D平面上）
                Vector3 position = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0
                );
                
                // 根据圈数计算缩放比例 (圈数越大，缩放越小)
                float scale = 1f - ring * 0.1f;
                scale = Mathf.Max(0.5f, scale); // 确保缩放不会太小，最小为0.5
                
                // 移动角色到队形位置并设置缩放
                StartCoroutine(MoveToPositionAndScale(nonCenterCharacters[characterIndex], position, scale));
                characterIndex++;
            }
        }
    }
    
    /// <summary>
    /// 平滑移动角色到指定位置
    /// </summary>
    private IEnumerator MoveToPosition(Character character, Vector3 targetLocalPosition)
    {
        if (character == null) yield break;
        
        Transform charTransform = character.transform;
        
        // 计算世界空间中的目标位置
        Vector3 targetWorldPosition = characterContainer.TransformPoint(targetLocalPosition);
        
        // 如果角色足够接近目标，不需要移动
        if (Vector3.Distance(charTransform.position, targetWorldPosition) < followThreshold)
        {
            yield break;
        }
        
        // 设置角色的世界位置
        charTransform.position = targetWorldPosition;
        
        // 让角色面向移动方向（2D中只需要翻转X轴的缩放）
        if (moveDirection.x != 0)
        {
            Vector3 scale = charTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * (moveDirection.x > 0 ? 1 : -1);
            charTransform.localScale = scale;
        }
    }
    
    /// <summary>
    /// 移动角色到指定位置并设置缩放
    /// </summary>
    private IEnumerator MoveToPositionAndScale(Character character, Vector3 targetLocalPosition, float scale)
    {
        if (character == null) yield break;
        
        Transform charTransform = character.transform;
        
        // 计算世界空间中的目标位置
        Vector3 targetWorldPosition = characterContainer.TransformPoint(targetLocalPosition);
        
        // 如果角色足够接近目标，不需要移动
        if (Vector3.Distance(charTransform.position, targetWorldPosition) < followThreshold)
        {
            // 即使不移动也要设置缩放
            Vector3 scaleVector = new Vector3(
                Mathf.Abs(charTransform.localScale.x) * (moveDirection.x > 0 ? scale : -scale),
                scale,
                scale
            );
            charTransform.localScale = scaleVector;
            
            yield break;
        }
        
        // 设置角色的世界位置
        charTransform.position = targetWorldPosition;
        
        // 应用缩放并保持面向（保存X轴的方向）
        float xDirection = moveDirection.x != 0 ? 
            (moveDirection.x > 0 ? 1 : -1) : 
            (charTransform.localScale.x > 0 ? 1 : -1);
            
        Vector3 finalScale = new Vector3(
            Mathf.Abs(scale) * xDirection,
            scale,
            scale
        );
        
        charTransform.localScale = finalScale;
    }
    
    /// <summary>
    /// 创建测试角色（仅用于开发测试）
    /// </summary>
    private void CreateTestCharacter()
    {
        // 创建一个基本的胶囊体作为角色
        GameObject characterObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        characterObj.name = "TestCharacter_" + teamMembers.Count;
        
        // 随机设置位置（2D平面上）
        characterObj.transform.position = transform.position + new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(-3f, 3f),
            0f
        );
        
        // 添加Character组件
        Character character = characterObj.AddComponent<Character>();
        character.characterName = "测试角色" + teamMembers.Count;
        
        // 随机设置种族和派别
        character.race = (RaceType)Random.Range(0, System.Enum.GetValues(typeof(RaceType)).Length);
        character.faction = (FactionType)Random.Range(0, System.Enum.GetValues(typeof(FactionType)).Length);
        
        // 随机设置颜色以区分不同角色
        Renderer renderer = characterObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(
                Random.Range(0.2f, 1f),
                Random.Range(0.2f, 1f),
                Random.Range(0.2f, 1f)
            );
        }
        
        // 添加到队伍
        AddCharacter(character);
    }
    
    /// <summary>
    /// 从商店添加角色到队伍
    /// </summary>
    public bool AddCharacterToTeam(GameObject characterPrefab)
    {
        if (characterPrefab == null || !HasSpaceForNewCharacter())
        {
            return false;
        }
        
        // 实例化角色（2D平面上）
        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        GameObject characterObj = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        
        // 获取角色组件
        Character character = characterObj.GetComponent<Character>();
        if (character != null)
        {
            // 添加到队伍
            return AddCharacter(character);
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取队伍中的所有角色
    /// </summary>
    public List<Character> GetTeamCharacters()
    {
        return new List<Character>(teamMembers);
    }
    
    /// <summary>
    /// 检查是否有足够的空间添加新角色（用于商店购买检查）
    /// </summary>
    public bool HasSpaceForNewCharacter()
    {
        return teamMembers.Count < maxTeamSize;
    }
    
    /// <summary>
    /// 玩家升级时调用，增加移动速度
    /// </summary>
    public void OnLevelUp()
    {
        // 每次升级增加移动速度0.1
        moveSpeed += 0.1f;
        Debug.Log($"玩家升级，移动速度增加为: {moveSpeed}");
    }
}

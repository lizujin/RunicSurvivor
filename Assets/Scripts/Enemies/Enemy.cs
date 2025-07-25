// 本文件完全有AI生成
using System.Collections;
using SimpleSystem;
using UnityEngine;

/// <summary>
/// 敌人基类，控制AI行为和攻击逻辑 (2D版本，无刚体)
/// </summary>
public class Enemy : BaseEntity
{
    [Header("基础属性")]
    public string enemyName = "未命名敌人";
    public float maxHealth = 50f;
    public float health = 50f;
    public float attackDamage = 5f;
    public float attackSpeed = 1f;
    public float attackRange = 1.5f;
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    
    [Header("掉落物")]
    public int expReward = 1;
    public int goldReward = 0; // 击杀敌人不再获得金币
    [Range(0f, 1f)] public float dropChance = 0.1f;
    public GameObject[] possibleDrops;
    
    [Header("视觉效果")]
    public GameObject attackEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject hitEffectPrefab;
    
    [Header("AI设置")]
    public float attackCooldown = 2f;
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;
    public bool useAvoidance = true;           // 是否使用避让系统
    public float avoidanceWeight = 0.3f;       // 避让力权重
    
    // 状态
    protected enum EnemyState
    {
        Idle,
        Wandering,
        Chasing,
        Attacking,
        Stunned,
        Dead
    }
    protected EnemyState currentState = EnemyState.Idle;
    
    // 内部变量
    protected Transform playerTransform;
    protected float currentAttackCooldown = 0f;
    protected float currentWanderTimer;
    protected Vector2 lastTargetPosition;
    protected Vector2 moveDirection;
    protected Vector2 wanderTarget;
    protected bool isPathfinding = false;
    
    // 避让系统
    protected EnemyManager enemyManager;
    protected Vector2 avoidanceForce = Vector2.zero;
    
    // 动画相关
    protected Animator animator;
    protected static readonly int AttackTrigger = Animator.StringToHash("Attack");
    protected static readonly int HitTrigger = Animator.StringToHash("Hit");
    protected static readonly int DeathTrigger = Animator.StringToHash("Death");
    protected static readonly int SpeedParam = Animator.StringToHash("Speed");
    
    // 事件委托
    public delegate void EnemyDeathHandler(Enemy enemy, Vector3 position);
    public static event EnemyDeathHandler OnEnemyDeath;
    
    // 状态效果
    protected float slowAmount = 0f;
    protected float slowDuration = 0f;
    protected float stunDuration = 0f;
    protected float knockbackTimeRemaining = 0f;
    protected Vector2 knockbackDirection = Vector2.zero;
    protected float knockbackForce = 0f;
    
    // 属性
    public bool IsAlive => health > 0;
    
    protected virtual void Awake()
    {
        // 初始化组件引用
        animator = GetComponentInChildren<Animator>();
        
        // 设置初始值
        health = maxHealth;
        currentWanderTimer = wanderTimer;
        moveSpeed += Random.Range(0, 0.5f);
    }
    
    protected virtual void Start()
    {
        // 查找玩家
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
        
        // 查找敌人管理器
        enemyManager = FindObjectOfType<EnemyManager>();
        
        // 进入默认状态
        ChangeState(EnemyState.Idle);
    }
    
    protected virtual void Update()
    {
        if (!IsAlive) return;
        
        // 更新状态效果
        UpdateStatusEffects();
        
        // 更新避让力
        if (useAvoidance && enemyManager != null)
        {
            avoidanceForce = enemyManager.GetAvoidanceForce(this);
        }
        
        // 执行当前状态的行为
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdleState();
                break;
            case EnemyState.Wandering:
                UpdateWanderState();
                break;
            case EnemyState.Chasing:
                UpdateChaseState();
                break;
            case EnemyState.Attacking:
                UpdateAttackState();
                break;
            case EnemyState.Stunned:
                UpdateStunnedState();
                break;
        }
        
        // 更新动画
        UpdateAnimation();
    }
    
    protected virtual void FixedUpdate()
    {
        if (!IsAlive) return;
        
        // 如果正在击退中
        if (knockbackTimeRemaining > 0)
        {
            // 应用击退力
            transform.position += (Vector3)(knockbackDirection * knockbackForce * Time.fixedDeltaTime);
            return;
        }
        
        // 如果被眩晕或正在攻击，不应用移动
        if (currentState == EnemyState.Stunned || currentState == EnemyState.Attacking) return;
        
        // 计算最终移动方向（包含避让力）
        Vector2 finalMoveDirection = CalculateFinalMoveDirection();
        
        // 应用移动
        if (finalMoveDirection.magnitude > 0.1f)
        {
            // 应用减速效果
            float currentMoveSpeed = moveSpeed * (1f - slowAmount);
            
            // 直接移动物体
            Vector3 movement = new Vector3(finalMoveDirection.x, finalMoveDirection.y, 0) * currentMoveSpeed * Time.fixedDeltaTime;
            transform.position += movement;
            
            // 如果有x方向的移动，设置面向
            if (Mathf.Abs(finalMoveDirection.x) > 0.1f)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = Mathf.Abs(newScale.x) * (finalMoveDirection.x > 0 ? 1 : -1);
                transform.localScale = newScale;
            }
        }
    }
    
    /// <summary>
    /// 计算最终移动方向（包含避让力）
    /// </summary>
    protected virtual Vector2 CalculateFinalMoveDirection()
    {
        Vector2 finalDirection = moveDirection;
        
        // 应用避让力
        if (useAvoidance && avoidanceForce.magnitude > 0.1f)
        {
            // 将避让力与原始移动方向结合
            finalDirection += avoidanceForce * avoidanceWeight;
            
            // 标准化方向向量
            if (finalDirection.magnitude > 0.1f)
            {
                finalDirection.Normalize();
            }
        }
        
        return finalDirection;
    }
    
    /// <summary>
    /// 更新状态效果
    /// </summary>
    private void UpdateStatusEffects()
    {
        // 更新减速效果
        if (slowDuration > 0)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0)
            {
                // 恢复速度
                slowAmount = 0f;
            }
        }
        
        // 更新眩晕效果
        if (stunDuration > 0)
        {
            stunDuration -= Time.deltaTime;
            if (stunDuration <= 0 && currentState == EnemyState.Stunned)
            {
                // 恢复活动能力
                ChangeState(EnemyState.Idle);
            }
        }
        
        // 更新击退效果
        if (knockbackTimeRemaining > 0)
        {
            knockbackTimeRemaining -= Time.deltaTime;
            if (knockbackTimeRemaining <= 0)
            {
                knockbackDirection = Vector2.zero;
                knockbackForce = 0f;
            }
        }
        
        // 更新攻击冷却
        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // 设置移动速度参数
        float currentSpeed = moveDirection.magnitude * moveSpeed;
        animator.SetFloat(SpeedParam, currentSpeed / moveSpeed);
    }
    
    /// <summary>
    /// 更新闲置状态
    /// </summary>
    protected virtual void UpdateIdleState()
    {
        // 重置移动方向
        moveDirection = Vector2.zero;
        
        // 检测玩家
        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance((Vector2)transform.position, (Vector2)playerTransform.position);
            if (distToPlayer < detectionRange)
            {
                ChangeState(EnemyState.Chasing);
                return;
            }
        }
        
        // 随机漫游
        currentWanderTimer -= Time.deltaTime;
        if (currentWanderTimer <= 0)
        {
            ChangeState(EnemyState.Wandering);
        }
    }
    
    /// <summary>
    /// 更新漫游状态
    /// </summary>
    protected virtual void UpdateWanderState()
    {
        // 检测玩家
        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance((Vector2)transform.position, (Vector2)playerTransform.position);
            if (distToPlayer < detectionRange)
            {
                ChangeState(EnemyState.Chasing);
                return;
            }
        }
        
        // 检查是否到达目的地
        float distToTarget = Vector2.Distance((Vector2)transform.position, wanderTarget);
        if (distToTarget < 0.5f)
        {
            // 到达目标位置，返回闲置状态
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // 向目标移动
        moveDirection = (wanderTarget - (Vector2)transform.position).normalized;
    }
    
    /// <summary>
    /// 更新追逐状态
    /// </summary>
    protected virtual void UpdateChaseState()
    {
        if (playerTransform == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // 获取到玩家的距离
        Vector2 playerPos = (Vector2)playerTransform.position;
        float distToPlayer = Vector2.Distance((Vector2)transform.position, playerPos);
        
        // 更新目标位置
        lastTargetPosition = playerPos;
        
        // 检查是否到达攻击范围
        if (distToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        // 如果太远则放弃追逐
        else if (distToPlayer > detectionRange * 1.5f)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // 向玩家移动
        moveDirection = (playerPos - (Vector2)transform.position).normalized;
    }
    
    /// <summary>
    /// 更新攻击状态
    /// </summary>
    protected virtual void UpdateAttackState()
    {
        if (playerTransform == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // 停止移动
        moveDirection = Vector2.zero;
        
        // 确保目标在攻击范围内
        float distToPlayer = Vector2.Distance((Vector2)transform.position, (Vector2)playerTransform.position);
        if (distToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // 面向玩家
        Vector2 direction = (Vector2)playerTransform.position - (Vector2)transform.position;
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * (direction.x > 0 ? 1 : -1);
            transform.localScale = newScale;
        }
        
        // 攻击逻辑
        if (currentAttackCooldown <= 0)
        {
            PerformAttack();
            currentAttackCooldown = 1f / attackSpeed;
        }
    }
    
    /// <summary>
    /// 更新眩晕状态
    /// </summary>
    protected virtual void UpdateStunnedState()
    {
        // 眩晕期间什么都不做，在UpdateStatusEffects中处理眩晕时间
        moveDirection = Vector2.zero;
        
        if (stunDuration <= 0)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
    /// <summary>
    /// 执行攻击动作
    /// </summary>
    protected virtual void PerformAttack()
    {
        if (playerTransform == null) return;
        
        // 触发攻击动画
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }
        
        // 播放攻击缩放动画
        StartCoroutine(AttackScaleAnimation());
        
        // 攻击特效
        if (attackEffectPrefab != null)
        {
            Instantiate(attackEffectPrefab, playerTransform.position, Quaternion.identity);
        }
        
        // 对玩家造成伤害
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
        }
    }
    
    /// <summary>
    /// 攻击缩放动画 (1.0 -> 1.1 -> 1.0)
    /// </summary>
    protected IEnumerator AttackScaleAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.1f; // 放大到1.1倍
        
        // 保持X的正负符号，保证不会因为放大而改变朝向
        targetScale.x = originalScale.x > 0 ? Mathf.Abs(targetScale.x) : -Mathf.Abs(targetScale.x);
        
        // 放大阶段 (1.0 -> 1.1)
        float duration = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 使用插值计算当前缩放值
            Vector3 currentScale = Vector3.Lerp(originalScale, targetScale, t);
            transform.localScale = currentScale;
            
            yield return null;
        }
        
        // 确保达到最大缩放
        transform.localScale = targetScale;
        
        // 缩小阶段 (1.1 -> 1.0)
        elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 使用插值计算当前缩放值
            Vector3 currentScale = Vector3.Lerp(targetScale, originalScale, t);
            transform.localScale = currentScale;
            
            yield return null;
        }
        
        // 确保恢复到原始缩放
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// 受击缩放动画 (1.0 -> 0.9 -> 1.0)
    /// </summary>
    protected IEnumerator HitScaleAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 0.8f; // 缩小到0.8倍
        
        // 保持X的正负符号，保证不会因为缩放而改变朝向
        targetScale.x = originalScale.x > 0 ? Mathf.Abs(targetScale.x) : -Mathf.Abs(targetScale.x);
        
        // 缩小阶段 (1.0 -> 0.8)
        float duration = 0.05f; // 更快的动画速度
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 使用插值计算当前缩放值
            Vector3 currentScale = Vector3.Lerp(originalScale, targetScale, t);
            transform.localScale = currentScale;
            
            yield return null;
        }
        
        // 确保达到最小缩放
        transform.localScale = targetScale;
        
        // 恢复阶段 (0.8 -> 1.0)
        elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 使用插值计算当前缩放值
            Vector3 currentScale = Vector3.Lerp(targetScale, originalScale, t);
            transform.localScale = currentScale;
            
            yield return null;
        }
        
        // 确保恢复到原始缩放
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// 改变状态
    /// </summary>
    protected virtual void ChangeState(EnemyState newState)
    {
        // 退出当前状态
        switch (currentState)
        {
            case EnemyState.Wandering:
                break;
            case EnemyState.Chasing:
                break;
            case EnemyState.Attacking:
                break;
            case EnemyState.Stunned:
                break;
        }
        
        // 设置新状态
        currentState = newState;
        
        // 进入新状态
        switch (newState)
        {
            case EnemyState.Idle:
                moveDirection = Vector2.zero;
                currentWanderTimer = Random.Range(wanderTimer * 0.5f, wanderTimer * 1.5f);
                break;
            case EnemyState.Wandering:
                FindWanderDestination();
                break;
            case EnemyState.Chasing:
                if (playerTransform != null)
                {
                    lastTargetPosition = (Vector2)playerTransform.position;
                }
                break;
            case EnemyState.Attacking:
                moveDirection = Vector2.zero;
                break;
            case EnemyState.Stunned:
                moveDirection = Vector2.zero;
                break;
        }
    }
    
    /// <summary>
    /// 寻找漫游的目的地
    /// </summary>
    protected virtual void FindWanderDestination()
    {
        // 在周围找一个随机点
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        wanderTarget = (Vector2)transform.position + randomDirection;
        
        // 检查随机位置是否有效 (这里可以添加射线检测避免障碍物)
        // 简化版本中我们不做这个检测，假设敌人可以自由移动到任何地方
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        
        // 应用伤害
        float oldHealth = health;
        health -= amount;
        
        // 显示伤害数字（如果有UI管理器）
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowDamageNumber(amount, transform.position);
        }
        
        // 播放受击效果
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
        
        // 播放受击动画
        if (animator != null)
        {
            animator.SetTrigger(HitTrigger);
        }
        
        // 播放受击音效
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.enabled)
        {
            audioSource.Play();
        }
        
        // 播放受击缩放动画 (1.0 -> 0.9 -> 1.0)
        StartCoroutine(HitScaleAnimation());
        
        // 检查是否死亡
        if (health <= 0)
        {
            health = 0;
            Die();
        }
        else
        {
            // 如果受到大量伤害（超过最大生命值的25%），短暂眩晕
            if (amount > maxHealth * 0.25f)
            {
                ApplyStun(0.5f);
            }
            
            // 如果不是在追逐或攻击状态，则进入追逐状态
            if (currentState != EnemyState.Chasing && currentState != EnemyState.Attacking)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
    }
    
    /// <summary>
    /// 应用减速效果
    /// </summary>
    public virtual void ApplySlow(float amount, float duration)
    {
        if (!IsAlive) return;
        
        // 应用更强的减速效果
        if (amount > slowAmount || (amount == slowAmount && duration > slowDuration))
        {
            slowAmount = amount;
            slowDuration = duration;
        }
    }
    
    /// <summary>
    /// 应用眩晕效果
    /// </summary>
    public virtual void ApplyStun(float duration)
    {
        if (!IsAlive) return;
        
        // 应用更长的眩晕时间
        if (duration > stunDuration)
        {
            stunDuration = duration;
            ChangeState(EnemyState.Stunned);
        }
    }
    
    /// <summary>
    /// 应用击退效果
    /// </summary>
    public virtual void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (!IsAlive) return;
        
        // 设置击退参数
        knockbackDirection = direction.normalized;
        knockbackForce = force;
        knockbackTimeRemaining = duration;
        
        // 暂时进入眩晕状态
        ChangeState(EnemyState.Stunned);
        StartCoroutine(KnockbackCoroutine(duration));
    }
    
    protected IEnumerator KnockbackCoroutine(float duration)
    {
        // 记录当前状态
        EnemyState previousState = currentState;
        
        // 等待击退时间
        yield return new WaitForSeconds(duration);
        
        // 恢复AI
        if (currentState == EnemyState.Stunned)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
    /// <summary>
    /// 死亡处理
    /// </summary>
    protected virtual void Die()
    {
        // 如果已经死亡，防止重复调用
        if (currentState == EnemyState.Dead)
            return;
            
        // 更改状态
        currentState = EnemyState.Dead;
        
        // 停止所有移动
        moveDirection = Vector2.zero;
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger(DeathTrigger);
        }
        
        // 创建死亡粒子效果
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            
            // 设置粒子系统颜色
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    var main = ps.main;
                    main.startColor = sr.color;
                }
            }
            
            // 10秒后自动销毁特效
            Destroy(effect, 1f);
        }
        
        // 掉落物品
        DropRewards();
        
        // 禁用碰撞
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // 触发死亡事件
        OnEnemyDeath?.Invoke(this, transform.position);
        
        // 更新游戏统计
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.RegisterEnemyKill(enemyName);
        }
        
        // 死亡动画结束后淡出
        StartCoroutine(DeathFadeOut());
    }
    
    /// <summary>
    /// 死亡后的淡出效果
    /// </summary>
    private IEnumerator DeathFadeOut()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        
        // 等待一段时间让死亡动画播放
        yield return new WaitForSeconds(.2f);
        
        // 淡出效果
        float duration = .2f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.8f, 0, elapsed / duration);
            
            foreach (SpriteRenderer renderer in renderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
            
            yield return null;
        }
        
        // 销毁对象
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 掉落奖励物品
    /// </summary>
    protected virtual void DropRewards()
    {
        // 给予经验奖励，不再给予金币奖励
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.AddExperience(expReward);
            
            // 显示经验获取提示
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                if (expReward > 0) 
                    uiManager.ShowFloatingText("+" + expReward + " 经验", transform.position, Color.green);
            }
        }
        
        // 随机掉落物品
        if (possibleDrops != null && possibleDrops.Length > 0)
        {
            // 计算是否掉落物品
            if (Random.value <= dropChance)
            {
                // 选择一个随机物品
                GameObject selectedDrop = possibleDrops[Random.Range(0, possibleDrops.Length)];
                if (selectedDrop != null)
                {
                    // 创建掉落物
                    GameObject droppedItem = Instantiate(selectedDrop, transform.position + Vector3.up * 0.2f, Quaternion.identity);
                    
                    // 添加简单的弹跳效果
                    StartCoroutine(AnimateItemDrop(droppedItem));
                }
            }
        }
    }
    
    /// <summary>
    /// 让掉落物有个小弹跳效果
    /// </summary>
    private IEnumerator AnimateItemDrop(GameObject item)
    {
        if (item == null) yield break;
        
        // 初始位置
        Vector3 startPos = item.transform.position;
        
        // 随机弹跳方向
        Vector2 randomDir = Random.insideUnitCircle.normalized * 0.5f;
        Vector3 targetPos = startPos + new Vector3(randomDir.x, randomDir.y + 0.5f, 0);
        
        // 上升阶段
        float elapsed = 0f;
        float duration = 0.3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            item.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        // 下落阶段
        elapsed = 0f;
        Vector3 peakPos = item.transform.position;
        Vector3 groundPos = new Vector3(peakPos.x, startPos.y, startPos.z);
        
        while (elapsed < duration * 1.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 1.5f);
            item.transform.position = Vector3.Lerp(peakPos, groundPos, t);
            yield return null;
        }
        
        // 确保物品在地面上
        item.transform.position = groundPos;
    }
    
    /// <summary>
    /// 设置敌人基础属性
    /// </summary>
    public virtual void SetStats(float healthMultiplier, float damageMultiplier)
    {
        maxHealth *= healthMultiplier;
        health = maxHealth;
        attackDamage *= damageMultiplier;
    }

    public void AddDamage(float value, BaseEntity source, SkillConfig skillConfig)
    {
        TakeDamage(value);
    }
}


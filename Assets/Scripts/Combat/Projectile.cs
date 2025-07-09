// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹类，处理投射物的移动、碰撞检测和伤害计算 (2D版本，无刚体)
/// </summary>
public class Projectile : MonoBehaviour
{
    // 基本属性
    public float speed = 15f;
    public float damage = 10f;
    public float lifeTime = 3f;
    public float radius = 0.2f;
    public int pierceCount = 0;
    public bool homing = false;
    public float homingStrength = 5f;
    
    // 视觉效果
    public GameObject hitEffectPrefab;
    public GameObject trailEffectPrefab;
    
    // 引用
    private Character owner;
    private Transform target;
    
    // 状态
    private int currentPierceCount = 0;
    private List<GameObject> hitObjects = new List<GameObject>();
    private bool isInitialized = false;
    private Vector2 moveDirection;
    private float currentSpeed;
    
    // 碰撞检测
    [SerializeField] private float collisionCheckRadius = 0.2f;
    [SerializeField] private LayerMask collisionLayers;
    
    private void Awake()
    {
        // 创建拖尾效果
        if (trailEffectPrefab != null)
        {
            Instantiate(trailEffectPrefab, transform);
        }
        
        // 设置生命周期
        Destroy(gameObject, lifeTime);
        
        // 强制设置碰撞图层以确保包含敌人层
        collisionLayers = LayerMask.GetMask("Enemy", "Player", "Obstacle");
        Debug.Log($"Projectile collision layers: {collisionLayers.value} - 包含敌人层: {LayerMask.NameToLayer("Enemy")}");
    }
    
    /// <summary>
    /// 初始化子弹
    /// </summary>
    public void Initialize(Character owner, float damage, Transform target = null)
    {
        this.owner = owner;
        this.damage = damage;
        this.target = target;
        
        // 初始移动方向为投射物的右方向
        moveDirection = transform.right;
        currentSpeed = speed;
        
        isInitialized = true;
        
        // 如果指定了目标，且允许追踪，启用追踪功能
        if (target != null && homing)
        {
            StartCoroutine(HomingUpdate());
        }
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // 移动投射物
        MoveProjectile();
        
        // 碰撞检测
        CheckCollisions();
    }
    
    /// <summary>
    /// 移动投射物
    /// </summary>
    private void MoveProjectile()
    {
        // 应用移动
        transform.position += (Vector3)(moveDirection * currentSpeed * Time.deltaTime);
        
        // 更新旋转以匹配移动方向
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    /// <summary>
    /// 使用物理检测碰撞
    /// </summary>
    private void CheckCollisions()
    {
        // 使用Physics2D.OverlapCircle来检测碰撞
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collisionCheckRadius, collisionLayers);
        if (hits.Length == 0)
        {
            // 没有碰撞体，直接返回
            return;
        }
        Debug.Log($"检测到 {hits.Length} 个碰撞体");
        
        foreach (Collider2D hit in hits)
        {
            // 忽略已经命中的对象
            if (hitObjects.Contains(hit.gameObject))
                continue;
            
            // 输出碰撞对象信息
            Debug.Log($"碰撞对象: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");
            
            // 检查是否是敌人
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 输出调试信息
                Debug.Log($"子弹命中敌人 {enemy.name}，应用伤害: {damage}，敌人当前生命值: {enemy.health}");
                
                // 确保敌人从当前生命值减去伤害值
                float oldHealth = enemy.health;
                enemy.TakeDamage(damage);
                
                // 再次检查敌人状态
                Debug.Log($"伤害应用后，敌人生命值: {enemy.health}，存活状态: {enemy.IsAlive}");
                
                // 如果生命值没有变化，强制执行伤害
                if (Mathf.Approximately(oldHealth, enemy.health) && enemy.health > 0)
                {
                    Debug.LogWarning("伤害没有被正确应用，尝试强制伤害");
                    // 设置生命值为非常小的值，然后再次调用TakeDamage以触发死亡逻辑
                    enemy.health = 0.1f; // 设置为接近0但不为0的值
                    enemy.TakeDamage(1f); // 再次调用TakeDamage以触发死亡逻辑
                }
                
                // 创建命中特效
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }
                
                // 记录已命中的对象
                hitObjects.Add(hit.gameObject);
                
                // 处理穿透逻辑
                currentPierceCount++;
                if (currentPierceCount > pierceCount)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// 追踪目标的协程
    /// </summary>
    private IEnumerator HomingUpdate()
    {
        while (target != null && gameObject != null && gameObject.activeSelf)
        {
            Vector2 targetDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            
            // 逐渐向目标方向转向
            moveDirection = Vector2.Lerp(moveDirection.normalized, targetDirection, Time.deltaTime * homingStrength);
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 设置子弹的属性
    /// </summary>
    public void SetProjectileProperties(ProjectileProperties properties)
    {
        if (properties == null) return;
        
        speed = properties.speed;
        currentSpeed = speed;
        damage = properties.damage;
        lifeTime = properties.lifeTime;
        pierceCount = properties.pierceCount;
        homing = properties.homing;
        homingStrength = properties.homingStrength;
    }
    
    /// <summary>
    /// 用于在编辑器中可视化碰撞检测范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
    }
}

/// <summary>
/// 子弹属性数据类，用于配置不同类型的子弹
/// </summary>
[System.Serializable]
public class ProjectileProperties
{
    public string name = "Default";
    public float speed = 15f;
    public float damage = 10f;
    public float lifeTime = 3f;
    public int pierceCount = 0;
    public bool homing = false;
    public float homingStrength = 5f;
}

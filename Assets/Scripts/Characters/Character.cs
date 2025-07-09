// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Skills;
using UnityEngine;
using SimpleSystem;

/// <summary>
/// 角色基类，所有可加入队伍的角色继承此类 (2D版本)
/// </summary>
public class Character : BaseEntity
{
    [Header("基本属性")]
    public string characterName = "未命名角色";
    public string description = "角色描述";
    public RaceType race = RaceType.Human;
    public FactionType faction = FactionType.Independent;
    public PositionType position = PositionType.Warrior;
    public int tier = 1; // 角色星级
    public int cost = 1;  // 角色成本
    
    [Header("战斗属性")]
    public float maxHealth = 100f;
    public float health = 100f;
    public float attack = 10f;
    public float attackSpeed = 1f;
    public float attackRange = 2f;
    public float critChance = 0.05f;
    public float critDamage = 1.5f;
    public float moveSpeed = 3.5f;
    public float defense = 0f;
    
    [Header("能量系统")]
    public float maxMana = 100f;
    public float mana = 50f;
    public float manaRegen = 5f;
    
    [Header("技能设置")]
    public List<Skill> skills = new List<Skill>();
    
    [Header("视觉设置")]
    public Sprite characterIcon;
    public GameObject attackEffectPrefab;
    public GameObject hitEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject levelUpEffectPrefab;

    [Header("子弹设置")]
    public GameObject projectilePrefab; // 子弹预制体

    SpriteRenderer[] renderers;

    [System.Serializable]
    public class ProjectileSettings
    {
        public string name = "Default";
        public float speed = 15f;
        public float damage = 10f;
        public float lifeTime = 3f;
        public int pierceCount = 0;
        public bool homing = false;
        public float homingStrength = 5f;
    }
    public ProjectileSettings projectileSettings; // 子弹属性配置
    public Transform projectileSpawnPoint; // 子弹发射点
    
    // 状态标识
    public bool isCenter = false;
    private bool isAlive = true;
    
    // 公共访问器（已有IsAlive和IsCenter，但添加设置中心角色的方法）
    
    // 内部状态
    private float attackCooldown = 0f;
    public Transform targetEnemy;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // 2D精灵渲染器
    
    // Buff系统
    private List<BuffEffect> activeBuffs = new List<BuffEffect>();
    
    // 属性访问器
    public bool IsAlive => isAlive;
    public bool IsCenter => isCenter;
    
    // 事件系统
    public delegate void CharacterDeathHandler(Character character);
    public event CharacterDeathHandler OnCharacterDeath;
    
    public delegate void CharacterLevelUpHandler(Character character, int newLevel);
    public event CharacterLevelUpHandler OnCharacterLevelUp;
    
    public delegate void CharacterAttackHandler(Character character, GameObject target, float damage);
    public event CharacterAttackHandler OnCharacterAttack;
    
    public delegate void CharacterDamageHandler(Character character, float amount);
    public event CharacterDamageHandler OnCharacterDamage;
    
    public delegate void CharacterHealHandler(Character character, float amount);
    public event CharacterHealHandler OnCharacterHeal;
    private HeroSkillConfig _heroSkillConfigs;
    private SkillComponent _skillComponent;
    protected virtual void Awake()
    {
        // 获取组件
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // 初始化状态
        health = maxHealth;
        mana = maxMana / 2;
        isAlive = true;
        InitSkills();
    }
    
    protected virtual void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        // 初始化技能
        foreach (Skill skill in skills)
        {
            if (skill != null)
            {
                skill.Initialize();
            }
        }
    }
    
    protected virtual void Update()
    {
        if (!IsAlive) return;
        
        // 更新攻击冷却
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
        
        // 更新魔法值恢复
        if (mana < maxMana)
        {
            mana = Mathf.Min(mana + manaRegen * Time.deltaTime, maxMana);
        }
        
        // 更新Buff
        UpdateBuffs();
        
        // 无论是否是中心角色，只要有目标就自动攻击
        if (targetEnemy != null)
        {
            // 检查目标是否有效
            Enemy enemy = targetEnemy.GetComponent<Enemy>();
            if (enemy == null || !enemy.IsAlive)
            {
                targetEnemy = null;
                return;
            }
            
            // 检查是否在攻击范围内 (2D版本使用Vector2.Distance)
            float distance = Vector2.Distance((Vector2)transform.position, (Vector2)targetEnemy.position);
            if (distance <= attackRange)
            {
                if (attackCooldown <= 0)
                {
                    Attack(enemy.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// 攻击目标
    /// </summary>
    public virtual void Attack(GameObject target)
    {
        if (!IsAlive || target == null) return;
        
        // 重置攻击冷却
        attackCooldown = 1f / attackSpeed;
        
        // 获取敌人组件
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null || !enemy.IsAlive) return;
        
        // 计算伤害
        float damage = CalculateDamage();
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 播放攻击缩放动画
        StartCoroutine(AttackScaleAnimation());
        
        // 2D环境中使用翻转精灵来朝向目标，而不是旋转
        Vector2 direction = (Vector2)(target.transform.position - transform.position);
        FaceDirection(direction.x);
        
        // 生成并发射子弹
        if (projectilePrefab != null)
        {
            // 确定发射位置 (2D环境下)
            Vector3 spawnPosition = projectileSpawnPoint != null 
                ? projectileSpawnPoint.position 
                : transform.position + new Vector3(transform.localScale.x > 0 ? 0.5f : -0.5f, 0.3f, 0);
                
            // 创建子弹 (2D环境下计算朝向)
            Quaternion rotation = Quaternion.identity;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, rotation);
            
            // 设置子弹属性
            if (projectileSettings != null)
            {
                var projectileScript = projectileObj.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    projectileScript.Initialize(this, damage, target.transform);
                    projectileScript.SetProjectileProperties(new ProjectileProperties
                    {
                        speed = projectileSettings.speed,
                        damage = damage,
                        lifeTime = projectileSettings.lifeTime,
                        pierceCount = projectileSettings.pierceCount,
                        homing = projectileSettings.homing,
                        homingStrength = projectileSettings.homingStrength
                    });
                }
            }
        }
        // 如果没有子弹预制体，直接应用伤害（回退机制）
        else
        {
            // 创建攻击特效
            if (attackEffectPrefab != null)
            {
                GameObject effect = Instantiate(attackEffectPrefab, enemy.transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            
            // 应用伤害
            enemy.TakeDamage(damage);
        }
        
        // 应用攻击特殊效果
        ApplyAttackSpecialEffects(enemy);
        
        // 触发攻击事件
        OnCharacterAttack?.Invoke(this, target, damage);
    }
    
    /// <summary>
    /// 让角色朝向指定方向 (2D)
    /// </summary>
    protected virtual void FaceDirection(float directionX)
    {
        // 只有当方向值足够大时才改变朝向，避免抖动
        if (Mathf.Abs(directionX) > 0.1f)
        {
            // 通过设置X缩放来翻转精灵
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Abs(newScale.x) * (directionX > 0 ? 1 : -1);
            transform.localScale = newScale;
        }
    }
    
    /// <summary>
    /// 应用攻击的特殊效果
    /// </summary>
    protected virtual void ApplyAttackSpecialEffects(Enemy enemy)
    {
        // 基类不实现特殊效果
        // 由派生类或具体角色实现
        
        // 检查妖族协同效应
        if (race == RaceType.Demon)
        {
            SynergyManager synergyManager = SynergyManager.Instance;
            if (synergyManager != null)
            {
                int synergyLevel = 0;
                
                // 获取妖族协同效应等级
                try
                {
                    synergyLevel = synergyManager.GetRaceSynergyLevel(RaceType.Demon);
                }
                catch
                {
                    // 方法不存在或协同效应系统未完全实现，使用默认值0
                    synergyLevel = 0;
                }
                
                if (synergyLevel > 0)
                {
                    // 有几率造成减速效果
                    float slowChance = 0.1f * synergyLevel; // 10%/层
                    if (Random.value < slowChance)
                    {
                        float slowAmount = 0.2f + 0.1f * synergyLevel; // 30%-50%减速
                        float slowDuration = 2f + synergyLevel; // 3-5秒减速
                        enemy.ApplySlow(slowAmount, slowDuration);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 计算伤害
    /// </summary>
    protected virtual float CalculateDamage()
    {
        // 计算基础伤害
        float damage = attack;
        
        // 计算暴击
        bool isCrit = Random.value < critChance;
        if (isCrit)
        {
            damage *= critDamage;
        }
        
        // 应用伤害加成（从buff等获取）
        float damageBonus = GetBuffTotalValue(BuffType.Attack);
        if (damageBonus > 0)
        {
            damage *= (1 + damageBonus);
        }
        
        return damage;
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        
        // 应用伤害减免
        float damageReduction = GetBuffTotalValue(BuffType.Shield);
        float actualDamage = amount * (1 - Mathf.Clamp01(damageReduction));
        
        // 应用最终伤害
        health -= actualDamage;
        
        // 创建受击特效 (2D环境下调整位置)
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // 播放受击动画
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // 触发受伤事件
        OnCharacterDamage?.Invoke(this, actualDamage);
        
        // 检查是否死亡
        if (health <= 0)
        {
            health = 0;
            Die();
        }
        else
        {
            // 播放受伤颜色特效
            StartCoroutine(DamageEffect());
        }
    }
    
    /// <summary>
    /// 受伤颜色特效
    /// </summary>
    private IEnumerator DamageEffect()
    {
        // 淡出效果
        float duration = .2f;
        float halfDuration = duration / 2;
        float elapsed = 0;
        float delt = .5f;
        
        // 保存原始颜色
        Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
                originalColors[renderer] = renderer.color;
        }
        
        // 红色闪烁效果
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer != null)
                {
                    Color color = renderer.color;
                    if (elapsed < halfDuration) 
                    {
                        color.r = Mathf.Min(color.r + delt, 1.0f); // 增加红色
                    }
                    else
                    {
                        color.r = Mathf.Max(originalColors[renderer].r, color.r - delt); // 恢复红色
                    }
                    
                    renderer.color = color;
                }
            }
            
            yield return null;
        }
        
        // 恢复原始颜色
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && originalColors.ContainsKey(renderer))
            {
                renderer.color = originalColors[renderer];
            }
        }
    }
    
    /// <summary>
    /// 治疗生命值
    /// </summary>
    public virtual void Heal(float amount)
    {
        if (!IsAlive || amount <= 0) return;
        
        float oldHealth = health;
        health = Mathf.Min(health + amount, maxHealth);
        
        // 触发治疗事件
        OnCharacterHeal?.Invoke(this, health - oldHealth);
    }
    
    /// <summary>
    /// 消耗魔法值
    /// </summary>
    public virtual bool ConsumeMana(float amount)
    {
        if (!IsAlive || amount <= 0) return true;
        
        // 检查魔法值是否足够
        if (mana >= amount)
        {
            mana -= amount;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 恢复魔法值
    /// </summary>
    public virtual void RestoreMana(float amount)
    {
        if (!IsAlive || amount <= 0) return;
        
        mana = Mathf.Min(mana + amount, maxMana);
    }
    
    /// <summary>
    /// 升级角色
    /// </summary>
    public virtual void LevelUp()
    {
        // 增加角色星级
        tier++;
        
        // 提升基础属性
        maxHealth *= 1.3f;
        health = maxHealth;
        attack *= 1.25f;
        
        // 播放升级特效
        if (levelUpEffectPrefab != null)
        {
            GameObject effect = Instantiate(levelUpEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // 触发升级事件
        OnCharacterLevelUp?.Invoke(this, tier);
    }
    
    /// <summary>
    /// 死亡处理
    /// </summary>
    protected virtual void Die()
    {
        // 设置状态
        isAlive = false;
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // 播放死亡特效
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // 解除所有buff
        ClearAllBuffs();
        
        // 触发死亡事件
        OnCharacterDeath?.Invoke(this);
        
        // 2D环境下，可以添加淡出效果
        StartCoroutine(FadeOutOnDeath());
    }
    
    /// <summary>
    /// 死亡后淡出效果
    /// </summary>
    protected virtual IEnumerator FadeOutOnDeath()
    {
        yield return new WaitForSeconds(1.0f); // 等待死亡动画播放
        
        if (spriteRenderer != null)
        {
            float duration = 1.0f;
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }
    }
    
    /// <summary>
    /// 设置为中心角色
    /// </summary>
    public virtual void SetAsCenter(bool center)
    {
        isCenter = center;
        
        // 可以在这里添加中心角色的视觉效果
        // 例如添加一个特效或改变材质
    }
    
    /// <summary>
    /// 设置目标敌人
    /// </summary>
    public virtual void SetTarget(Transform target)
    {
        targetEnemy = target;
    }
    
    /// <summary>
    /// 获取攻击范围的平方
    /// </summary>
    public float GetAttackRangeSqr()
    {
        return attackRange * attackRange;
    }
    
    /// <summary>
    /// 获取到目标的距离（2D版本）
    /// </summary>
    public float GetDistanceToTarget(Transform target)
    {
        if (target == null) return float.MaxValue;
        return Vector2.Distance((Vector2)transform.position, (Vector2)target.position);
    }
    
    /// <summary>
    /// 获取目标方向（2D版本）
    /// </summary>
    public Vector2 GetDirectionToTarget(Transform target)
    {
        if (target == null) return Vector2.zero;
        return ((Vector2)target.position - (Vector2)transform.position).normalized;
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
    
    #region Buff系统
    
    /// <summary>
    /// 添加Buff
    /// </summary>
    public virtual void ApplyBuff(BuffEffect buff)
    {
        if (buff == null || !IsAlive) return;
        
        // 检查是否已有同类型Buff
        bool hasExistingBuff = false;
        foreach (BuffEffect existingBuff in activeBuffs)
        {
            if (existingBuff.name == buff.name)
            {
                // 如果Buff可叠加，增加层数
                if (existingBuff.isStackable)
                {
                    existingBuff.currentStack = Mathf.Min(existingBuff.currentStack + 1, existingBuff.maxStack);
                }
                
                // 刷新持续时间
                existingBuff.remainingTime = buff.duration;
                hasExistingBuff = true;
                break;
            }
        }
        
        // 如果是新Buff，添加到列表
        if (!hasExistingBuff)
        {
            // 创建新的Buff实例
            BuffEffect newBuff = new BuffEffect(
                buff.name,
                buff.description,
                buff.buffType,
                buff.value,
                buff.duration,
                buff.isStackable,
                buff.maxStack,
                buff.isDebuff
            );
            
            activeBuffs.Add(newBuff);
            newBuff.ApplyEffect(this);
        }
    }
    
    /// <summary>
    /// 移除Buff
    /// </summary>
    public virtual void RemoveBuff(BuffEffect buff)
    {
        if (buff == null) return;
        
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].effectId == buff.effectId)
            {
                activeBuffs[i].RemoveEffect(this);
                activeBuffs.RemoveAt(i);
                break;
            }
        }
    }
    
    /// <summary>
    /// 更新所有Buff
    /// </summary>
    protected virtual void UpdateBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            // 更新Buff
            activeBuffs[i].UpdateEffect(this, Time.deltaTime);
            
            // 移除过期的Buff (不再活跃)
            if (!activeBuffs[i].isActive)
            {
                activeBuffs.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// 清除所有Buff
    /// </summary>
    public virtual void ClearAllBuffs()
    {
        foreach (BuffEffect buff in activeBuffs)
        {
            buff.RemoveEffect(this);
        }
        
        activeBuffs.Clear();
    }
    
    /// <summary>
    /// 获取所有激活的Buff
    /// </summary>
    public List<BuffEffect> GetActiveBuffs()
    {
        return new List<BuffEffect>(activeBuffs);
    }
    
    /// <summary>
    /// 获取某类型Buff的总效果值
    /// </summary>
    public float GetBuffTotalValue(BuffType type)
    {
        float total = 0;
        
        foreach (BuffEffect buff in activeBuffs)
        {
            if (buff.buffType == type)
            {
                total += buff.value * buff.currentStack;
            }
        }
        
        return total;
    }
    
    /// <summary>
    /// 检查是否有某个Buff
    /// </summary>
    public bool HasBuff(string buffName)
    {
        foreach (BuffEffect buff in activeBuffs)
        {
            if (buff.name == buffName)
            {
                return true;
            }
        }
        
        return false;
    }
    
    #endregion
    
    /// <summary>
    /// 获取角色详细信息
    /// </summary>
    public virtual string GetCharacterInfo()
    {
        SynergyManager synergyManager = SynergyManager.Instance;
        string raceStr = race.ToString();
        string factionStr = faction.ToString();
        
        // 尝试从SynergyManager获取种族和派别名称翻译
        if (synergyManager != null)
        {
            // 获取相应种族的种族协同效应数据
            SynergyData.RaceSynergyLevel raceData = synergyManager.synergyData?.GetRaceSynergyData(race);
            if (raceData != null && !string.IsNullOrEmpty(raceData.raceName))
            {
                raceStr = raceData.raceName;
            }
            
            // 获取相应派别的派别协同效应数据
            SynergyData.FactionSynergyLevel factionData = synergyManager.synergyData?.GetFactionSynergyData(faction);
            if (factionData != null && !string.IsNullOrEmpty(factionData.factionName))
            {
                factionStr = factionData.factionName;
            }
        }
        
        string info = $"{characterName}\n";
        info += $"等级 {tier} - {position}\n";
        info += $"种族: {raceStr} | 派别: {factionStr}\n";
        info += $"生命: {health}/{maxHealth} | 攻击: {attack}\n";
        info += $"攻速: {attackSpeed} | 暴击: {critChance * 100}%\n";
        
        if (skills.Count > 0)
        {
            info += $"技能:\n";
            foreach (Skill skill in skills)
            {
                if (skill != null)
                {
                    info += $"- {skill.skillName}: {skill.description}\n";
                }
            }
        }
        
        return info;
    }

    private void InitSkills()
    {
        int maxSkillCount = 4;
        if (_skillComponent == null)
        {
            _skillComponent = EntityManager.GetInstance().AddComponent<SkillComponent>(this);
            _skillComponent.skillConfig = new SkillConfig[maxSkillCount];
            _skillComponent.skillContent = new SkillContent[maxSkillCount];
        }
        _heroSkillConfigs = SkillConfigTool.GetInstance().GetHeroSkillConfig(id);
        if (_heroSkillConfigs == null)
        {
            Debug.Log($"角色 {characterName} 的技能配置文件未找到");
            return;
        }

        for (int i = 0; i < _heroSkillConfigs.skillIds.Length; i++)
        {
            var curIndex = _skillComponent.curSkillCount;
            if (curIndex >= maxSkillCount)
            {
                break;
            }
            var skillId = _heroSkillConfigs.skillIds[i];
            var skillCfg = SkillConfigTool.GetInstance().GetSkillConfig(skillId);
            if (skillCfg != null)
            {
                _skillComponent.skillConfig[curIndex] = skillCfg;
                _skillComponent.skillContent[curIndex] = new SkillContent();
                _skillComponent.skillContent[curIndex].id = skillCfg.id;
                _skillComponent.curSkillCount++;
            }
            else
            {
                Debug.Log($"技能 {skillId} 的配置文件未找到");
            }
        }
    }
}

// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色技能系统
/// </summary>
public class Skill
{
    // 基本属性
    public string skillId;                  // 技能ID
    public string skillName;                // 技能名称
    public string description;              // 技能描述
    public Sprite icon;                     // 技能图标
    
    // 技能数值
    public float damage = 10f;              // 技能伤害
    public float cooldown = 5f;             // 冷却时间(秒)
    public float manaCost = 10f;            // 法力消耗
    public float range = 5f;                // 技能范围
    public float areaRadius = 2f;           // 区域效果半径
    
    // 状态
    private float currentCooldown = 0f;     // 当前冷却时间
    private bool isInitialized = false;     // 是否已初始化
    
    // 引用
    private Character owner;                // 技能拥有者

    /// <summary>
    /// 构造函数
    /// </summary>
    public Skill(string id, string name, string desc)
    {
        skillId = id;
        skillName = name;
        description = desc;
    }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public Skill() { }

    /// <summary>
    /// 初始化技能
    /// </summary>
    public virtual void Initialize()
    {
        if (isInitialized) return;
        
        // 查找所属角色
        if (owner == null)
        {
            owner = GameObject.FindObjectOfType<Character>();
        }
        
        currentCooldown = 0f;
        isInitialized = true;
    }

    /// <summary>
    /// 使用技能
    /// </summary>
    public virtual bool Use(Vector3 targetPosition, GameObject targetObject = null)
    {
        // 检查冷却
        if (currentCooldown > 0f) return false;
        
        // 检查法力值
        if (owner != null && !owner.ConsumeMana(manaCost)) return false;
        
        // 执行技能效果
        ApplySkillEffect(targetPosition, targetObject);
        
        // 设置冷却
        currentCooldown = cooldown;
        
        return true;
    }

    /// <summary>
    /// 应用技能效果
    /// </summary>
    protected virtual void ApplySkillEffect(Vector3 targetPosition, GameObject targetObject)
    {
        // 基类不实现具体效果，由子类实现
        Debug.Log($"使用技能: {skillName}");
    }

    /// <summary>
    /// 更新技能状态
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        // 更新冷却时间
        if (currentCooldown > 0f)
        {
            currentCooldown -= deltaTime;
            if (currentCooldown < 0f) currentCooldown = 0f;
        }
    }

    /// <summary>
    /// 设置技能拥有者
    /// </summary>
    public void SetOwner(Character character)
    {
        owner = character;
    }

    /// <summary>
    /// 获取冷却进度
    /// </summary>
    public float GetCooldownProgress()
    {
        if (cooldown <= 0f) return 1f;
        return 1f - (currentCooldown / cooldown);
    }

    /// <summary>
    /// 重置技能冷却
    /// </summary>
    public void ResetCooldown()
    {
        currentCooldown = 0f;
    }
}

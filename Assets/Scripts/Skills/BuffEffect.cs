// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Skills
{
    /// <summary>
    /// 效果类型
    /// </summary>
    public enum BuffType
    {
        None,           // 无效果
        Attack,         // 攻击加成
        Health,         // 生命加成
        Speed,          // 速度加成
        Shield,         // 护盾
        HealthRegen,    // 生命恢复
        Stun,           // 眩晕
        Slow,           // 减速
        Poison,         // 中毒
        Invincible,     // 无敌
        StatusImmunity, // 状态免疫
        Other           // 其他
    }
    
    /// <summary>
    /// Buff效果基类，由技能、道具、协同效应等系统应用到角色上
    /// </summary>
    public class BuffEffect
    {
        // 基本属性
        public string name;                  // 名称
        public string description;           // 描述
        public BuffType buffType;            // 效果类型
        public float value;                  // 效果值
        public float duration;               // 持续时间
        public bool isStackable;             // 是否可叠加
        public int maxStack;                 // 最大叠加层数
        public bool isDebuff;                // 是否为负面效果
        
        // 运行时属性
        public int currentStack = 1;         // 当前叠加层数
        public float remainingTime;          // 剩余时间
        public bool isActive = false;        // 是否激活
        
        // 效果标识符（用于判断相同效果）
        public string effectId;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffEffect()
        {
            name = "Buff";
            description = "Basic buff";
            buffType = BuffType.None;
            value = 0;
            duration = 5f;
            isStackable = false;
            maxStack = 1;
            isDebuff = false;
            effectId = System.Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        public BuffEffect(string name, string description, BuffType type, float value, float duration, bool isStackable, int maxStack, bool isDebuff)
        {
            this.name = name;
            this.description = description;
            this.buffType = type;
            this.value = value;
            this.duration = duration;
            this.remainingTime = duration;
            this.isStackable = isStackable;
            this.maxStack = maxStack;
            this.isDebuff = isDebuff;
            this.effectId = System.Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// 应用效果
        /// </summary>
        public virtual void ApplyEffect(Character target)
        {
            isActive = true;
            remainingTime = duration;
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        public virtual void RemoveEffect(Character target)
        {
            isActive = false;
        }
        
        /// <summary>
        /// 更新效果（每帧调用）
        /// </summary>
        public virtual void UpdateEffect(Character target, float deltaTime)
        {
            if (!isActive) return;
            
            // 有限时效果，更新时间
            if (duration > 0 && !Mathf.Approximately(duration, float.MaxValue))
            {
                remainingTime -= deltaTime;
                
                // 效果结束
                if (remainingTime <= 0)
                {
                    RemoveEffect(target);
                }
            }
        }
        
        /// <summary>
        /// 叠加效果
        /// </summary>
        public virtual void StackEffect(Character target, BuffEffect newEffect)
        {
            if (!isStackable) return;
            
            // 增加层数，不超过最大层数
            currentStack = Mathf.Min(currentStack + 1, maxStack);
            
            // 对于有持续时间的效果，重置时间
            if (duration > 0)
            {
                remainingTime = duration;
            }
        }
        
        /// <summary>
        /// 获取效果描述（包含当前层数和效果值）
        /// </summary>
        public virtual string GetEffectDescription()
        {
            string desc = description;
            
            if (isStackable && currentStack > 1)
            {
                desc += $" (x{currentStack})";
            }
            
            if (remainingTime > 0 && !Mathf.Approximately(duration, float.MaxValue))
            {
                desc += $" ({remainingTime:F1}s)";
            }
            
            return desc;
        }
    }
}

// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Skills
{
    /// <summary>
    /// 专门用于协同效应系统的Buff效果
    /// </summary>
    public class SynergyBuffEffect : BuffEffect
    {
        // 属性修饰符
        public float attackDamageModifier = 0f;      // 攻击力加成(百分比)
        public float maxHealthModifier = 0f;         // 最大生命值加成(百分比)
        public float attackSpeedModifier = 0f;       // 攻击速度加成(百分比)
        public float moveSpeedModifier = 0f;         // 移动速度加成(百分比)
        public float defenseModifier = 0f;           // 防御力加成(百分比)
        public float experienceModifier = 0f;        // 经验获取加成(百分比)
        public float manaRegenModifier = 0f;         // 魔法回复加成(百分比)
        public float cooldownReductionModifier = 0f; // 冷却缩减(百分比)
        public float dodgeChanceModifier = 0f;       // 闪避几率(百分比)
        public float lifestealModifier = 0f;         // 生命偷取(百分比)

        /// <summary>
        /// 构造函数
        /// </summary>
        public SynergyBuffEffect() : base("协同效应", "由队伍协同效应提供的增益", BuffType.Other, 0, float.MaxValue, false, 1, true)
        {
            // 协同效应Buff是永久性的，直到队伍组成变化
        }

        /// <summary>
        /// 构造函数(带名称和描述)
        /// </summary>
        public SynergyBuffEffect(string name, string description) : base(name, description, BuffType.Other, 0, float.MaxValue, false, 1, true)
        {
            // 协同效应Buff是永久性的，直到队伍组成变化
        }

        /// <summary>
        /// 应用效果(重写)
        /// </summary>
        public override void ApplyEffect(Character target)
        {
            base.ApplyEffect(target);
            
            // 应用各种属性加成
            if (attackDamageModifier != 0)
            {
                target.attack *= (1 + attackDamageModifier);
            }
            
            if (maxHealthModifier != 0)
            {
                float oldMaxHealth = target.maxHealth;
                target.maxHealth *= (1 + maxHealthModifier);
                
                // 按比例增加当前生命值
                if (oldMaxHealth > 0)
                {
                    target.health *= (target.maxHealth / oldMaxHealth);
                }
            }
            
            if (attackSpeedModifier != 0)
            {
                target.attackSpeed *= (1 + attackSpeedModifier);
            }
            
            if (moveSpeedModifier != 0)
            {
                target.moveSpeed *= (1 + moveSpeedModifier);
            }
            
            if (defenseModifier != 0 && target.GetType().GetProperty("defense") != null)
            {
                // 使用反射获取和设置防御值
                System.Reflection.PropertyInfo propInfo = target.GetType().GetProperty("defense");
                if (propInfo != null)
                {
                    float currentDefense = (float)propInfo.GetValue(target);
                    propInfo.SetValue(target, currentDefense * (1 + defenseModifier));
                }
            }
            
            // 其他修饰符在相应的系统中应用
            // 例如：经验修饰符在角色获得经验时应用
            // 冷却缩减在技能使用时应用
        }
        
        /// <summary>
        /// 移除效果(重写)
        /// </summary>
        public override void RemoveEffect(Character target)
        {
            // 反向应用各种属性加成
            if (attackDamageModifier != 0)
            {
                target.attack /= (1 + attackDamageModifier);
            }
            
            if (maxHealthModifier != 0)
            {
                float oldMaxHealth = target.maxHealth;
                target.maxHealth /= (1 + maxHealthModifier);
                
                // 按比例减少当前生命值，但不低于1
                if (oldMaxHealth > 0)
                {
                    target.health = Mathf.Max(1f, target.health * (target.maxHealth / oldMaxHealth));
                }
            }
            
            if (attackSpeedModifier != 0)
            {
                target.attackSpeed /= (1 + attackSpeedModifier);
            }
            
            if (moveSpeedModifier != 0)
            {
                target.moveSpeed /= (1 + moveSpeedModifier);
            }
            
            if (defenseModifier != 0 && target.GetType().GetProperty("defense") != null)
            {
                // 使用反射获取和设置防御值
                System.Reflection.PropertyInfo propInfo = target.GetType().GetProperty("defense");
                if (propInfo != null)
                {
                    float currentDefense = (float)propInfo.GetValue(target);
                    propInfo.SetValue(target, currentDefense / (1 + defenseModifier));
                }
            }
            
            // 调用基类的移除方法
            base.RemoveEffect(target);
        }
        
        /// <summary>
        /// 创建种族协同效应Buff
        /// </summary>
        public static SynergyBuffEffect CreateRaceSynergyBuff(RaceType race, int level)
        {
            string raceName = GetRaceTypeName(race);
            SynergyBuffEffect buff = new SynergyBuffEffect($"{raceName}协同效应", $"{raceName}单位之间的协同效应，等级{level}");
            
            // 根据种族类型和等级设置buff效果
            switch (race)
            {
                case RaceType.God:
                    // 神族: 生命值加成、魔法回复、冷却缩减
                    switch (level)
                    {
                        case 1: buff.maxHealthModifier = 0.1f; break;
                        case 2: buff.maxHealthModifier = 0.1f; buff.manaRegenModifier = 0.2f; break;
                        case 3: buff.maxHealthModifier = 0.1f; buff.manaRegenModifier = 0.2f; buff.cooldownReductionModifier = 0.3f; break;
                    }
                    break;
                    
                case RaceType.Human:
                    // 人族: 经验加成、攻击力加成
                    switch (level)
                    {
                        case 1: buff.experienceModifier = 0.15f; break;
                        case 2: buff.experienceModifier = 0.15f; buff.attackDamageModifier = 0.15f; break;
                        case 3: buff.experienceModifier = 0.15f; buff.attackDamageModifier = 0.25f; break;
                    }
                    break;
                    
                case RaceType.Monster:
                    // 妖族: 攻击速度、移动速度
                    switch (level)
                    {
                        case 1: buff.attackSpeedModifier = 0.15f; break;
                        case 2: buff.attackSpeedModifier = 0.15f; buff.moveSpeedModifier = 0.25f; break;
                        case 3: buff.attackSpeedModifier = 0.25f; buff.moveSpeedModifier = 0.25f; break;
                    }
                    break;
                    
                case RaceType.Ghost:
                    // 鬼族: 闪避、生命偷取
                    switch (level)
                    {
                        case 1: buff.dodgeChanceModifier = 0.15f; break;
                        case 2: buff.dodgeChanceModifier = 0.15f; buff.lifestealModifier = 0.1f; break;
                        case 3: buff.dodgeChanceModifier = 0.25f; buff.lifestealModifier = 0.2f; break;
                    }
                    break;
                    
                case RaceType.Demon:
                    // 怪族: 攻击力
                    switch (level)
                    {
                        case 1: buff.attackDamageModifier = 0.2f; break;
                        case 2: buff.attackDamageModifier = 0.3f; break;
                        case 3: buff.attackDamageModifier = 0.4f; break;
                    }
                    break;
            }
            
            return buff;
        }
        
        /// <summary>
        /// 创建派别协同效应Buff
        /// </summary>
        public static SynergyBuffEffect CreateFactionSynergyBuff(FactionType faction, int level)
        {
            string factionName = GetFactionTypeName(faction);
            SynergyBuffEffect buff = new SynergyBuffEffect($"{factionName}协同效应", $"{factionName}派别之间的协同效应，等级{level}");
            
            // 根据派别类型和等级设置buff效果
            switch (faction)
            {
                case FactionType.Taoist:
                    // 道教: 冷却缩减
                    switch (level)
                    {
                        case 1: buff.cooldownReductionModifier = 0.15f; break;
                        case 2: buff.cooldownReductionModifier = 0.25f; break;
                        case 3: buff.cooldownReductionModifier = 0.35f; break;
                    }
                    break;
                    
                case FactionType.Buddhist:
                    // 佛教: 生命值
                    switch (level)
                    {
                        case 1: buff.maxHealthModifier = 0.2f; break;
                        case 2: buff.maxHealthModifier = 0.3f; break;
                        case 3: buff.maxHealthModifier = 0.4f; break;
                    }
                    break;
                    
                case FactionType.Heaven:
                    // 天庭: 防御力
                    switch (level)
                    {
                        case 1: buff.defenseModifier = 0.15f; break;
                        case 2: buff.defenseModifier = 0.25f; break;
                        case 3: buff.defenseModifier = 0.35f; break;
                    }
                    break;
                    
                case FactionType.Hell:
                    // 地府: 攻击力
                    switch (level)
                    {
                        case 1: buff.attackDamageModifier = 0.15f; break;
                        case 2: buff.attackDamageModifier = 0.25f; break;
                        case 3: buff.attackDamageModifier = 0.35f; break;
                    }
                    break;
                    
                case FactionType.Independent:
                    // 散修: 全属性加成
                    float bonus = 0f;
                    switch (level)
                    {
                        case 1: bonus = 0.1f; break;
                        case 2: bonus = 0.2f; break;
                        case 3: bonus = 0.3f; break;
                    }
                    
                    buff.attackDamageModifier = bonus;
                    buff.maxHealthModifier = bonus;
                    buff.attackSpeedModifier = bonus;
                    buff.moveSpeedModifier = bonus;
                    buff.defenseModifier = bonus;
                    break;
            }
            
            return buff;
        }
        
        /// <summary>
        /// 获取种族名称
        /// </summary>
        public static string GetRaceTypeName(RaceType race)
        {
            switch (race)
            {
                case RaceType.God: return "神族";
                case RaceType.Human: return "人族";
                case RaceType.Monster: return "妖族";
                case RaceType.Ghost: return "鬼族";
                case RaceType.Demon: return "怪族";
                default: return "未知";
            }
        }
        
        /// <summary>
        /// 获取派别名称
        /// </summary>
        public static string GetFactionTypeName(FactionType faction)
        {
            switch (faction)
            {
                case FactionType.Taoist: return "道教";
                case FactionType.Buddhist: return "佛教";
                case FactionType.Heaven: return "天庭";
                case FactionType.Hell: return "地府";
                case FactionType.Independent: return "散修";
                default: return "无派别";
            }
        }
    }
}

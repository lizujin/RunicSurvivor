// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色数据，定义西游记中的角色
/// </summary>
[CreateAssetMenu(fileName = "CharacterData", menuName = "游戏/角色数据")]
public class CharacterData : ScriptableObject
{
    [Header("基本信息")]
    public string characterID;
    public string characterName;
    [TextArea(2, 5)]
    public string description;
    public Sprite characterIcon;
    public GameObject characterPrefab;
    public CharacterRarity rarity = CharacterRarity.Common;
    public int cost = 1; // 商店购买成本

    [Header("角色定位")]
    public RaceType race;
    public FactionType faction;
    public PositionType position;
    
    [Header("基础属性")]
    public float maxHealth = 100f;
    public float attack = 10f;
    public float attackSpeed = 1f;
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
    public float critChance = 0.05f;
    public float critDamage = 1.5f;
    
    [Header("能量系统")]
    public float maxMana = 100f;
    public float manaRegen = 5f;
    
    [Header("视觉效果")]
    public GameObject attackEffectPrefab;
    public GameObject hitEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject levelUpEffectPrefab;
    
    [Header("子弹设置")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float projectileLifetime = 3f;
    public int projectilePierceCount = 0;
    public bool projectileHoming = false;
    public float projectileHomingStrength = 5f;
    
    [Header("技能")]
    public SkillData[] skills;
    
    [Header("升级加成")]
    public float healthBonusPerLevel = 1.3f; // 每级生命值增加倍率
    public float attackBonusPerLevel = 1.25f; // 每级攻击力增加倍率
    
    [Header("特殊效果")]
    [TextArea(2, 5)]
    public string specialEffect; // 特殊效果描述
    
    /// <summary>
    /// 创建一个角色实例
    /// </summary>
    public Character CreateInstance(Vector3 spawnPosition)
    {
        if (characterPrefab == null)
        {
            Debug.LogError($"角色预制体为空: {characterName}");
            return null;
        }
        
        // 实例化角色
        GameObject instance = Instantiate(characterPrefab, spawnPosition, Quaternion.identity);
        Character character = instance.GetComponent<Character>();
        
        if (character == null)
        {
            Debug.LogError($"预制体上没有Character组件: {characterName}");
            Destroy(instance);
            return null;
        }
        
        // 设置基本信息
        character.characterName = characterName;
        character.description = description;
        character.race = race;
        character.faction = faction;
        character.position = position;
        character.tier = (int)rarity;
        character.cost = cost;
        
        // 设置基础属性
        character.maxHealth = maxHealth;
        character.health = maxHealth;
        character.attack = attack;
        character.attackSpeed = attackSpeed;
        character.attackRange = attackRange;
        character.moveSpeed = moveSpeed;
        character.critChance = critChance;
        character.critDamage = critDamage;
        
        // 设置能量系统
        character.maxMana = maxMana;
        character.mana = maxMana / 2;
        character.manaRegen = manaRegen;
        
        // 设置视觉效果
        character.characterIcon = characterIcon;
        character.attackEffectPrefab = attackEffectPrefab;
        character.hitEffectPrefab = hitEffectPrefab;
        character.deathEffectPrefab = deathEffectPrefab;
        character.levelUpEffectPrefab = levelUpEffectPrefab;
        
        // 设置子弹
        character.projectilePrefab = projectilePrefab;
        character.projectileSettings = new Character.ProjectileSettings
        {
            speed = projectileSpeed,
            damage = attack,
            lifeTime = projectileLifetime,
            pierceCount = projectilePierceCount,
            homing = projectileHoming,
            homingStrength = projectileHomingStrength
        };
        
        // 添加技能
        if (skills != null && skills.Length > 0)
        {
            character.skills.Clear();
            foreach (var skillData in skills)
            {
                if (skillData != null)
                {
                    Skill skill = skillData.CreateInstance();
                    if (skill != null)
                    {
                        character.skills.Add(skill);
                    }
                }
            }
        }
        
        return character;
    }
}

/// <summary>
/// 技能数据
/// </summary>
[System.Serializable]
public class SkillData
{
    public string skillID;
    public string skillName;
    [TextArea(2, 5)]
    public string description;
    public float manaCost = 50f;
    public float cooldown = 10f;
    public GameObject effectPrefab;
    
    public Skill CreateInstance()
    {
        Skill skill = new Skill();
        skill.skillId = skillID;
        skill.skillName = skillName;
        skill.description = description;
        skill.manaCost = manaCost;
        skill.cooldown = cooldown;
        // effectPrefab不是Skill类的属性，所以不设置
        return skill;
    }
}

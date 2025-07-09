// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色位置类型
/// </summary>
public enum PositionType
{
    Warrior,    // 战士（近战）
    Ranger,     // 游侠（远程）
    Mage,       // 法师（法术）
    Support,    // 辅助（增益）
    Tank        // 坦克（防御）
}

/// <summary>
/// 种族类型
/// </summary>
public enum RaceType
{
    None,       // 无种族
    God,        // 神族(天界)
    Human,      // 人族(人类)
    Monster,    // 妖族(妖怪)
    Ghost,      // 鬼族(幽灵)
    Demon       // 怪族(恶魔)
}

/// <summary>
/// 派别类型
/// </summary>
public enum FactionType
{
    None,           // 无派别
    Taoist,         // 道教
    Buddhist,       // 佛教
    Heaven,         // 天庭
    Hell,           // 地府
    Independent     // 散修
}

/// <summary>
/// 角色稀有度
/// </summary>
public enum CharacterRarity
{
    Common = 1,     // 普通
    Uncommon = 2,   // 少见
    Rare = 3,       // 稀有
    Epic = 4,       // 史诗
    Legendary = 5   // 传说
}

/// <summary>
/// 敌人类型
/// </summary>
public enum EnemyType
{
    Normal,     // 普通敌人
    Elite,      // 精英敌人
    Boss        // Boss敌人
}

/// <summary>
/// 敌人生成类型
/// </summary>
public enum EnemySpawnType
{
    Normal,     // 普通
    Elite,      // 精英
    Boss        // Boss
}

/// <summary>
/// 游戏难度
/// </summary>
public enum GameDifficulty
{
    Easy,       // 简单
    Normal,     // 普通
    Hard,       // 困难
    Expert      // 专家
}

/// <summary>
/// 项目类型
/// </summary>
public enum ItemType
{
    None,       // 无类型
    Character,  // 角色
    Equipment,  // 装备
    Consumable, // 消耗品
    Currency,   // 货币
    Special     // 特殊
}

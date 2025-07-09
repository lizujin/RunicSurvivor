// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hero数据结构，用于从Heros.json加载英雄数据
/// </summary>
[System.Serializable]
public class Hero
{
    public string name;
    public int cost;
    public string icon;
    public string prefab;
    public string race;    // 种族特性：暗夜、星辰、光明
    public string classType;   // 流派特性：生长、狂野、治愈、极速、凶残、善良
}

[System.Serializable]
public class HeroDataList
{
    public List<Hero> heros;
}

// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 定义各种种族和派别的协同效应数据
/// </summary>
[CreateAssetMenu(fileName = "SynergyData", menuName = "游戏/协同效应数据")]
public class SynergyData : ScriptableObject
{
    [System.Serializable]
    public class RaceSynergyLevel
    {
        public RaceType raceType;
        public string raceName;
        [TextArea(2, 5)]
        public string description;
        public int[] activationThresholds = new int[3]; // 激活该层级效果所需的单位数量
        [TextArea(2, 5)]
        public string[] effectDescriptions = new string[3]; // 各层级效果描述
    }
    
    [System.Serializable]
    public class FactionSynergyLevel
    {
        public FactionType factionType;
        public string factionName;
        [TextArea(2, 5)]
        public string description;
        public int[] activationThresholds = new int[3]; // 激活该层级效果所需的单位数量
        [TextArea(2, 5)]
        public string[] effectDescriptions = new string[3]; // 各层级效果描述
    }
    
    [Header("种族协同效应")]
    public RaceSynergyLevel[] raceSynergies;
    
    [Header("派别协同效应")]
    public FactionSynergyLevel[] factionSynergies;
    
    /// <summary>
    /// 获取种族协同效应数据
    /// </summary>
    public RaceSynergyLevel GetRaceSynergyData(RaceType raceType)
    {
        foreach (var synergy in raceSynergies)
        {
            if (synergy.raceType == raceType)
            {
                return synergy;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 获取派别协同效应数据
    /// </summary>
    public FactionSynergyLevel GetFactionSynergyData(FactionType factionType)
    {
        foreach (var synergy in factionSynergies)
        {
            if (synergy.factionType == factionType)
            {
                return synergy;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 获取种族协同效应激活等级 (0 = 未激活, 1-3 = 激活等级)
    /// </summary>
    public int GetRaceSynergyLevel(RaceType raceType, int count)
    {
        RaceSynergyLevel data = GetRaceSynergyData(raceType);
        if (data == null) return 0;
        
        for (int i = data.activationThresholds.Length - 1; i >= 0; i--)
        {
            if (count >= data.activationThresholds[i])
            {
                return i + 1;
            }
        }
        
        return 0;
    }
    
    /// <summary>
    /// 获取派别协同效应激活等级 (0 = 未激活, 1-3 = 激活等级)
    /// </summary>
    public int GetFactionSynergyLevel(FactionType factionType, int count)
    {
        FactionSynergyLevel data = GetFactionSynergyData(factionType);
        if (data == null) return 0;
        
        for (int i = data.activationThresholds.Length - 1; i >= 0; i--)
        {
            if (count >= data.activationThresholds[i])
            {
                return i + 1;
            }
        }
        
        return 0;
    }
}

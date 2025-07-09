// 本文件完全有AI生成
using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.Scripts.Skills;

// Adapt the API to use the correct ApplyBuff and RemoveBuff methods from the Character class
// This ensures compatibility with the Character implementation

/// <summary>
/// 管理角色的协同效应系统，处理种族和派别的组合效果
/// </summary>
public class SynergyManager : MonoBehaviour
{
    [Header("协同效应配置")]
    public SynergyData synergyData;
    public bool debugMode = false;
    
    // 单例实例
    private static SynergyManager _instance;
    public static SynergyManager Instance => _instance;
    
    // 当前队伍的种族和派别统计
    private Dictionary<RaceType, int> raceCount = new Dictionary<RaceType, int>();
    private Dictionary<FactionType, int> factionCount = new Dictionary<FactionType, int>();
    
    // 活跃的协同效应
    private Dictionary<RaceType, int> activeRaceSynergies = new Dictionary<RaceType, int>();
    private Dictionary<FactionType, int> activeFactionSynergies = new Dictionary<FactionType, int>();
    
    // 当前应用的增益效果
    private List<SynergyBuffEffect> appliedBuffs = new List<SynergyBuffEffect>();
    
    // 委托和事件
    public delegate void SynergyChangedHandler(Dictionary<RaceType, int> raceLevel, Dictionary<FactionType, int> factionLevel);
    public event SynergyChangedHandler OnSynergyChanged;

    private void Awake()
    {
        // 设置单例
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // 确保不会被销毁
        DontDestroyOnLoad(gameObject);
        
        // 初始化字典
        InitializeCounters();
        
        // 加载协同效应数据
        LoadSynergyData();
    }
    
    private void Start()
    {
        // 订阅玩家团队变化事件
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            PlayerController playerController = gameManager.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.OnTeamChanged += UpdateTeamSynergies;
            }
        }
    }

    /// <summary>
    /// 初始化计数器
    /// </summary>
    private void InitializeCounters()
    {
        // 初始化种族计数
        foreach (RaceType race in System.Enum.GetValues(typeof(RaceType)))
        {
            raceCount[race] = 0;
            activeRaceSynergies[race] = 0;
        }
        
        // 初始化派别计数
        foreach (FactionType faction in System.Enum.GetValues(typeof(FactionType)))
        {
            factionCount[faction] = 0;
            activeFactionSynergies[faction] = 0;
        }
    }
    
    /// <summary>
    /// 加载协同效应数据
    /// </summary>
    private void LoadSynergyData()
    {
        if (synergyData == null)
        {
            synergyData = Resources.Load<SynergyData>("SynergyData/DefaultSynergyData");
            if (synergyData == null)
            {
                Debug.LogWarning("未找到协同效应数据，将创建基础协同效应数据。");
                synergyData = CreateDefaultSynergyData();
            }
        }
    }
    
    /// <summary>
    /// 创建默认的协同效应数据（仅在找不到时使用）
    /// </summary>
    private SynergyData CreateDefaultSynergyData()
    {
        SynergyData data = ScriptableObject.CreateInstance<SynergyData>();
        
        // 创建基础的种族协同效应
        List<SynergyData.RaceSynergyLevel> raceSynergies = new List<SynergyData.RaceSynergyLevel>();
        
        // 神族
        SynergyData.RaceSynergyLevel godSynergy = new SynergyData.RaceSynergyLevel
        {
            raceType = RaceType.God,
            raceName = "神族",
            description = "天界之神，拥有强大的灵力和神通。",
            activationThresholds = new int[] { 2, 4, 6 },
            effectDescriptions = new string[] 
            {
                "所有神族单位获得10%额外生命值",
                "所有神族单位获得20%额外魔法值回复",
                "所有神族单位获得30%冷却缩减"
            }
        };
        raceSynergies.Add(godSynergy);
        
        // 人族
        SynergyData.RaceSynergyLevel humanSynergy = new SynergyData.RaceSynergyLevel
        {
            raceType = RaceType.Human,
            raceName = "人族",
            description = "凡界之人，虽然没有天赋神通，但有极大的潜力。",
            activationThresholds = new int[] { 3, 6, 9 },
            effectDescriptions = new string[] 
            {
                "所有人族单位获得15%经验加成",
                "所有人族单位获得15%攻击力加成",
                "所有人族单位获得额外的技能伤害"
            }
        };
        raceSynergies.Add(humanSynergy);
        
        // 妖族
        SynergyData.RaceSynergyLevel monsterSynergy = new SynergyData.RaceSynergyLevel
        {
            raceType = RaceType.Monster,
            raceName = "妖族",
            description = "修炼成精的动物，拥有变化之术。",
            activationThresholds = new int[] { 2, 4, 6 },
            effectDescriptions = new string[] 
            {
                "所有妖族单位获得15%额外攻击速度",
                "所有妖族单位获得25%移动速度加成",
                "所有妖族单位攻击有20%几率复制技能效果"
            }
        };
        raceSynergies.Add(monsterSynergy);
        
        // 鬼族
        SynergyData.RaceSynergyLevel ghostSynergy = new SynergyData.RaceSynergyLevel
        {
            raceType = RaceType.Ghost,
            raceName = "鬼族",
            description = "阴间的鬼魂，拥有神出鬼没的能力。",
            activationThresholds = new int[] { 2, 4, 6 },
            effectDescriptions = new string[] 
            {
                "所有鬼族单位获得15%几率躲避伤害",
                "所有鬼族单位获得生命偷取能力",
                "死亡时有30%几率短暂复活，持续5秒"
            }
        };
        raceSynergies.Add(ghostSynergy);
        
        // 怪族
        SynergyData.RaceSynergyLevel demonSynergy = new SynergyData.RaceSynergyLevel
        {
            raceType = RaceType.Demon,
            raceName = "怪族",
            description = "魔界的恶魔，拥有强大的破坏力。",
            activationThresholds = new int[] { 2, 3, 5 },
            effectDescriptions = new string[] 
            {
                "所有怪族单位获得20%额外攻击力",
                "所有怪族单位获得20%几率造成暴击",
                "所有怪族单位暴击伤害增加至300%"
            }
        };
        raceSynergies.Add(demonSynergy);
        
        // 设置种族协同效应
        data.raceSynergies = raceSynergies.ToArray();
        
        // 创建基础的派别协同效应
        List<SynergyData.FactionSynergyLevel> factionSynergies = new List<SynergyData.FactionSynergyLevel>();
        
        // 道教
        SynergyData.FactionSynergyLevel taoistSynergy = new SynergyData.FactionSynergyLevel
        {
            factionType = FactionType.Taoist,
            factionName = "道教",
            description = "修真之士，精通符咒、法术和丹药。",
            activationThresholds = new int[] { 2, 4, 6 },
            effectDescriptions = new string[] 
            {
                "所有道教单位获得15%冷却缩减",
                "所有道教单位获得额外的法术穿透",
                "每10秒生成一个保护罩，吸收下一次伤害"
            }
        };
        factionSynergies.Add(taoistSynergy);
        
        // 佛教
        SynergyData.FactionSynergyLevel buddhistSynergy = new SynergyData.FactionSynergyLevel
        {
            factionType = FactionType.Buddhist,
            factionName = "佛教",
            description = "佛门弟子，精通佛法、咒语和禅定。",
            activationThresholds = new int[] { 2, 4, 6 },
            effectDescriptions = new string[] 
            {
                "所有佛教单位获得20%额外生命值",
                "所有佛教单位每3秒恢复1%最大生命值",
                "受到致命伤害时，有50%几率保留1点生命值"
            }
        };
        factionSynergies.Add(buddhistSynergy);
        
        // 天庭
        SynergyData.FactionSynergyLevel heavenSynergy = new SynergyData.FactionSynergyLevel
        {
            factionType = FactionType.Heaven,
            factionName = "天庭",
            description = "玉皇大帝的天兵天将，维护天庭秩序。",
            activationThresholds = new int[] { 2, 4, 6 },
            effectDescriptions = new string[] 
            {
                "所有天庭单位获得15%额外防御力",
                "所有天庭单位攻击有20%几率眩晕目标1秒",
                "每60秒召唤一次天雷，对周围敌人造成大量伤害"
            }
        };
        factionSynergies.Add(heavenSynergy);
        
        // 地府
        SynergyData.FactionSynergyLevel hellSynergy = new SynergyData.FactionSynergyLevel
        {
            factionType = FactionType.Hell,
            factionName = "地府",
            description = "阎罗王的阴差使者，负责勾魂索命。",
            activationThresholds = new int[] { 2, 3, 5 },
            effectDescriptions = new string[] 
            {
                "所有地府单位获得15%额外攻击力",
                "所有地府单位攻击有25%几率造成恐惧效果",
                "敌人死亡时有30%几率转化为己方阴兵，持续10秒"
            }
        };
        factionSynergies.Add(hellSynergy);
        
        // 散修
        SynergyData.FactionSynergyLevel independentSynergy = new SynergyData.FactionSynergyLevel
        {
            factionType = FactionType.Independent,
            factionName = "散修",
            description = "不属于任何门派的独行侠。",
            activationThresholds = new int[] { 1, 3, 5 },
            effectDescriptions = new string[] 
            {
                "所有散修单位获得10%全属性加成",
                "所有散修单位获得25%冷却缩减",
                "战斗开始时获得30%攻击力加成，持续15秒"
            }
        };
        factionSynergies.Add(independentSynergy);
        
        // 设置派别协同效应
        data.factionSynergies = factionSynergies.ToArray();
        
        return data;
    }
    
    /// <summary>
    /// 更新队伍协同效应
    /// </summary>
    public void UpdateTeamSynergies(List<Character> characters)
    {
        // 重置计数
        InitializeCounters();
        
        // 移除所有现有buff
        RemoveAllSynergyBuffs();
        
        // 统计每个种族和派别的数量
        foreach (Character character in characters)
        {
            if (character != null)
            {
                // 计算种族数量
                if (!raceCount.ContainsKey(character.race))
                {
                    raceCount[character.race] = 0;
                }
                raceCount[character.race]++;
                
                // 计算派别数量
                if (!factionCount.ContainsKey(character.faction))
                {
                    factionCount[character.faction] = 0;
                }
                factionCount[character.faction]++;
            }
        }
        
        // 确定哪些协同效应被激活
        CalculateActiveSynergies();
        
        // 应用协同效应buff
        ApplySynergyBuffs(characters);
        
        // 触发事件
        OnSynergyChanged?.Invoke(activeRaceSynergies, activeFactionSynergies);
        
        // 调试输出
        if (debugMode)
        {
            LogActiveSynergies();
        }
    }
    
    /// <summary>
    /// 计算当前激活的协同效应
    /// </summary>
    private void CalculateActiveSynergies()
    {
        if (synergyData == null) return;
        
        // 计算种族协同效应激活等级
        foreach (RaceType race in System.Enum.GetValues(typeof(RaceType)))
        {
            if (race == RaceType.None) continue;
            
            // 获取当前数量
            int count = raceCount.ContainsKey(race) ? raceCount[race] : 0;
            
            // 计算激活等级
            int level = synergyData.GetRaceSynergyLevel(race, count);
            activeRaceSynergies[race] = level;
        }
        
        // 计算派别协同效应激活等级
        foreach (FactionType faction in System.Enum.GetValues(typeof(FactionType)))
        {
            if (faction == FactionType.None) continue;
            
            // 获取当前数量
            int count = factionCount.ContainsKey(faction) ? factionCount[faction] : 0;
            
            // 计算激活等级
            int level = synergyData.GetFactionSynergyLevel(faction, count);
            activeFactionSynergies[faction] = level;
        }
    }
    
    /// <summary>
    /// 应用协同效应buff
    /// </summary>
    private void ApplySynergyBuffs(List<Character> characters)
    {
        // 这里将根据激活的协同效应应用buff
        // 在实际游戏中，这里会根据每个协同效应的具体效果来实现
        // 现在只是创建示例，实际效果需要在Character类中实现
        
        foreach (var raceEntry in activeRaceSynergies)
        {
            RaceType race = raceEntry.Key;
            int level = raceEntry.Value;
            
            if (level > 0 && race != RaceType.None)
            {
                // 对应种族的所有角色应用buff
                foreach (Character character in characters)
                {
                    if (character != null && character.race == race)
                    {
                        // 根据等级应用buff
                        ApplyRaceBuff(character, race, level);
                    }
                }
            }
        }
        
        foreach (var factionEntry in activeFactionSynergies)
        {
            FactionType faction = factionEntry.Key;
            int level = factionEntry.Value;
            
            if (level > 0 && faction != FactionType.None)
            {
                // 对应派别的所有角色应用buff
                foreach (Character character in characters)
                {
                    if (character != null && character.faction == faction)
                    {
                        // 根据等级应用buff
                        ApplyFactionBuff(character, faction, level);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 应用种族buff
    /// </summary>
    private void ApplyRaceBuff(Character character, RaceType race, int level)
    {
        // 不同种族的不同等级buff效果
        switch (race)
        {
            case RaceType.God:
                // 神族buff
                ApplyGodBuff(character, level);
                break;
                
            case RaceType.Human:
                // 人族buff
                ApplyHumanBuff(character, level);
                break;
                
            case RaceType.Monster:
                // 妖族buff
                ApplyMonsterBuff(character, level);
                break;
                
            case RaceType.Ghost:
                // 鬼族buff
                ApplyGhostBuff(character, level);
                break;
                
            case RaceType.Demon:
                // 怪族buff
                ApplyDemonBuff(character, level);
                break;
        }
    }
    
    /// <summary>
    /// 应用派别buff
    /// </summary>
    private void ApplyFactionBuff(Character character, FactionType faction, int level)
    {
        // 不同派别的不同等级buff效果
        switch (faction)
        {
            case FactionType.Taoist:
                // 道教buff
                ApplyTaoistBuff(character, level);
                break;
                
            case FactionType.Buddhist:
                // 佛教buff
                ApplyBuddhistBuff(character, level);
                break;
                
            case FactionType.Heaven:
                // 天庭buff
                ApplyHeavenBuff(character, level);
                break;
                
            case FactionType.Hell:
                // 地府buff
                ApplyHellBuff(character, level);
                break;
                
            case FactionType.Independent:
                // 散修buff
                ApplyIndependentBuff(character, level);
                break;
        }
    }
    
    #region 种族Buff实现
    
    private void ApplyGodBuff(Character character, int level)
    {
        // 神族buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateRaceSynergyBuff(RaceType.God, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyHumanBuff(Character character, int level)
    {
        // 人族buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateRaceSynergyBuff(RaceType.Human, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyMonsterBuff(Character character, int level)
    {
        // 妖族buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateRaceSynergyBuff(RaceType.Monster, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyGhostBuff(Character character, int level)
    {
        // 鬼族buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateRaceSynergyBuff(RaceType.Ghost, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyDemonBuff(Character character, int level)
    {
        // 怪族buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateRaceSynergyBuff(RaceType.Demon, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    #endregion
    
    #region 派别Buff实现
    
    private void ApplyTaoistBuff(Character character, int level)
    {
        // 道教buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateFactionSynergyBuff(FactionType.Taoist, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyBuddhistBuff(Character character, int level)
    {
        // 佛教buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateFactionSynergyBuff(FactionType.Buddhist, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyHeavenBuff(Character character, int level)
    {
        // 天庭buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateFactionSynergyBuff(FactionType.Heaven, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyHellBuff(Character character, int level)
    {
        // 地府buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateFactionSynergyBuff(FactionType.Hell, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    private void ApplyIndependentBuff(Character character, int level)
    {
        // 散修buff效果
        SynergyBuffEffect buff = SynergyBuffEffect.CreateFactionSynergyBuff(FactionType.Independent, level);
        
        // 使用Buff转换为BuffEffect并应用
        BuffEffect buffEffect = new BuffEffect(
            buff.name, 
            buff.description, 
            buff.buffType, 
            buff.value,
            buff.duration,
            buff.isStackable,
            buff.maxStack,
            buff.isDebuff
        );
        
        // 应用buff
        character.ApplyBuff(buffEffect);
        appliedBuffs.Add(buff);
        
        // 其他效果省略，实际游戏中需要实现
    }
    
    #endregion
    
    /// <summary>
    /// 移除所有协同效应buff
    /// </summary>
    private void RemoveAllSynergyBuffs()
    {
        // 找到玩家控制器和队伍成员
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            PlayerController playerController = gameManager.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // 移除之前应用的所有buff
                foreach (var buff in appliedBuffs)
                {
                    // 对所有队伍成员移除该buff
                    foreach (var character in playerController.TeamMembers)
                    {
                        // 直接传递SynergyBuffEffect，因为它继承自BuffEffect
                        character.RemoveBuff(buff);
                    }
                }
            }
            
            appliedBuffs.Clear();
        }
    }
    
    /// <summary>
    /// 输出当前激活的协同效应信息（调试用）
    /// </summary>
    private void LogActiveSynergies()
    {
        Debug.Log("--- 当前激活的协同效应 ---");
        
        // 输出种族协同效应
        Debug.Log("种族协同效应:");
        foreach (var entry in activeRaceSynergies)
        {
            if (entry.Value > 0)
            {
                SynergyData.RaceSynergyLevel data = synergyData.GetRaceSynergyData(entry.Key);
                if (data != null)
                {
                    Debug.Log($"  {data.raceName}: 等级 {entry.Value} ({data.effectDescriptions[entry.Value-1]})");
                }
                else
                {
                    Debug.Log($"  {entry.Key}: 等级 {entry.Value}");
                }
            }
        }
        
        // 输出派别协同效应
        Debug.Log("派别协同效应:");
        foreach (var entry in activeFactionSynergies)
        {
            if (entry.Value > 0)
            {
                SynergyData.FactionSynergyLevel data = synergyData.GetFactionSynergyData(entry.Key);
                if (data != null)
                {
                    Debug.Log($"  {data.factionName}: 等级 {entry.Value} ({data.effectDescriptions[entry.Value-1]})");
                }
                else
                {
                    Debug.Log($"  {entry.Key}: 等级 {entry.Value}");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取所有当前队伍的种族协同效应等级
    /// </summary>
    public Dictionary<RaceType, int> GetRaceSynergyLevels()
    {
        return new Dictionary<RaceType, int>(activeRaceSynergies);
    }
    
    /// <summary>
    /// 获取所有当前队伍的派别协同效应等级
    /// </summary>
    public Dictionary<FactionType, int> GetFactionSynergyLevels()
    {
        return new Dictionary<FactionType, int>(activeFactionSynergies);
    }
    
    /// <summary>
    /// 获取特定种族的数量
    /// </summary>
    public int GetRaceCount(RaceType race)
    {
        return raceCount.ContainsKey(race) ? raceCount[race] : 0;
    }
    
    /// <summary>
    /// 获取特定派别的数量
    /// </summary>
    public int GetFactionCount(FactionType faction)
    {
        return factionCount.ContainsKey(faction) ? factionCount[faction] : 0;
    }
    
    /// <summary>
    /// 获取特定种族的协同效应等级
    /// </summary>
    public int GetRaceSynergyLevel(RaceType race)
    {
        return activeRaceSynergies.ContainsKey(race) ? activeRaceSynergies[race] : 0;
    }
    
    /// <summary>
    /// 获取特定派别的协同效应等级
    /// </summary>
    public int GetFactionSynergyLevel(FactionType faction)
    {
        return activeFactionSynergies.ContainsKey(faction) ? activeFactionSynergies[faction] : 0;
    }
    
    #region UI和图标相关方法

    /// <summary>
    /// 获取种族图标
    /// </summary>
    public Sprite GetRaceIcon(RaceType race)
    {
        // 从Resources加载种族图标
        string iconPath = $"Icons/Race/{race.ToString()}";
        Sprite icon = Resources.Load<Sprite>(iconPath);
        
        // 如果没有找到，尝试使用默认图标
        if (icon == null)
        {
            icon = Resources.Load<Sprite>("Icons/Race/Default");
        }
        
        return icon;
    }
    
    /// <summary>
    /// 获取派别图标
    /// </summary>
    public Sprite GetFactionIcon(FactionType faction)
    {
        // 从Resources加载派别图标
        string iconPath = $"Icons/Faction/{faction.ToString()}";
        Sprite icon = Resources.Load<Sprite>(iconPath);
        
        // 如果没有找到，尝试使用默认图标
        if (icon == null)
        {
            icon = Resources.Load<Sprite>("Icons/Faction/Default");
        }
        
        return icon;
    }
    
    /// <summary>
    /// 获取所有活跃的种族协同效应
    /// </summary>
    public Dictionary<RaceType, int> GetActiveRaceSynergies()
    {
        return new Dictionary<RaceType, int>(activeRaceSynergies);
    }
    
    /// <summary>
    /// 获取所有活跃的派别协同效应
    /// </summary>
    public Dictionary<FactionType, int> GetActiveFactionSynergies()
    {
        return new Dictionary<FactionType, int>(activeFactionSynergies);
    }
    
    /// <summary>
    /// 获取所有种族数量
    /// </summary>
    public Dictionary<RaceType, int> GetRaceCounts()
    {
        return new Dictionary<RaceType, int>(raceCount);
    }
    
    /// <summary>
    /// 获取所有派别数量
    /// </summary>
    public Dictionary<FactionType, int> GetFactionCounts()
    {
        return new Dictionary<FactionType, int>(factionCount);
    }
    
    /// <summary>
    /// 获取种族名称字符串
    /// </summary>
    public string GetRaceTypeString(RaceType race)
    {
        if (synergyData != null)
        {
            SynergyData.RaceSynergyLevel raceData = synergyData.GetRaceSynergyData(race);
            if (raceData != null && !string.IsNullOrEmpty(raceData.raceName))
            {
                return raceData.raceName;
            }
        }
        
        // 默认返回枚举名称
        return SynergyBuffEffect.GetRaceTypeName(race);
    }
    
    /// <summary>
    /// 获取种族描述
    /// </summary>
    public string GetRaceDescription(RaceType race)
    {
        if (synergyData != null)
        {
            SynergyData.RaceSynergyLevel raceData = synergyData.GetRaceSynergyData(race);
            if (raceData != null && !string.IsNullOrEmpty(raceData.description))
            {
                return raceData.description;
            }
        }
        
        // 默认返回空描述
        return "";
    }
    
    /// <summary>
    /// 获取种族协同效应描述
    /// </summary>
    public string GetRaceSynergyDescription(RaceType race, int level)
    {
        if (level <= 0) return "";
        
        if (synergyData != null)
        {
            SynergyData.RaceSynergyLevel raceData = synergyData.GetRaceSynergyData(race);
            if (raceData != null && raceData.effectDescriptions != null && level <= raceData.effectDescriptions.Length)
            {
                return raceData.effectDescriptions[level - 1];
            }
        }
        
        return "";
    }
    
    /// <summary>
    /// 获取派别名称字符串
    /// </summary>
    public string GetFactionTypeString(FactionType faction)
    {
        if (synergyData != null)
        {
            SynergyData.FactionSynergyLevel factionData = synergyData.GetFactionSynergyData(faction);
            if (factionData != null && !string.IsNullOrEmpty(factionData.factionName))
            {
                return factionData.factionName;
            }
        }
        
        // 默认返回枚举名称
        return SynergyBuffEffect.GetFactionTypeName(faction);
    }
    
    /// <summary>
    /// 获取派别描述
    /// </summary>
    public string GetFactionDescription(FactionType faction)
    {
        if (synergyData != null)
        {
            SynergyData.FactionSynergyLevel factionData = synergyData.GetFactionSynergyData(faction);
            if (factionData != null && !string.IsNullOrEmpty(factionData.description))
            {
                return factionData.description;
            }
        }
        
        // 默认返回空描述
        return "";
    }
    
    /// <summary>
    /// 获取派别协同效应描述
    /// </summary>
    public string GetFactionSynergyDescription(FactionType faction, int level)
    {
        if (level <= 0) return "";
        
        if (synergyData != null)
        {
            SynergyData.FactionSynergyLevel factionData = synergyData.GetFactionSynergyData(faction);
            if (factionData != null && factionData.effectDescriptions != null && level <= factionData.effectDescriptions.Length)
            {
                return factionData.effectDescriptions[level - 1];
            }
        }
        
        return "";
    }
    
    #endregion

    /// <summary>
    /// 从资源加载协同效应数据
    /// </summary>
    public void LoadSynergyDataFromResources(string path)
    {
        SynergyData loadedData = Resources.Load<SynergyData>(path);
        if (loadedData != null)
        {
            synergyData = loadedData;
            Debug.Log($"已加载协同效应数据: {path}");
            
            // 如果已经有角色，立即更新协同效应
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                PlayerController playerController = gameManager.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    UpdateTeamSynergies(playerController.GetTeamCharacters());
                }
            }
        }
        else
        {
            Debug.LogWarning($"未能加载协同效应数据: {path}");
        }
    }
}

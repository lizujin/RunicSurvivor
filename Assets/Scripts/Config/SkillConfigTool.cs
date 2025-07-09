using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleSystem;
using UnityEngine;

public class SkillConfigTool
{
    private static SkillConfigTool Instance;
    private SkillConfigTool()
    {
    }
    public static SkillConfigTool GetInstance()
    {
        if (Instance == null)
        {
            Instance = new SkillConfigTool();
        }
        return Instance;
    }

    private Dictionary<int, SkillConfig> _configs = new ();
    private Dictionary<int, HeroSkillConfig> _heroSkillConfigs = new ();
    public void InitSkillConfigs()
    {
        var skillConfigs = GameConfigs.GetInstance().GetConfig<SkillsJsonType>("Skill/Skills");
        if (skillConfigs == null)
        {
            Debug.LogError("技能配置文件加载失败");
            return;
        }
        foreach (var config in skillConfigs.skills)
        {
            _configs[config.id] = config;
        }
        var heroSkillConfigs = GameConfigs.GetInstance().GetConfig<HeroSkillJsonType>("Skill/HeroSkill");
        if (heroSkillConfigs == null)
        {
            Debug.LogError("英雄技能配置文件加载失败");
            return;
        }
        foreach (var config in heroSkillConfigs.heros)
        {
            _heroSkillConfigs[config.id] = config;
        }
    }

    public SkillConfig GetSkillConfig(int id)
    {
        if (_configs.TryGetValue(id, out var ret))
        {
            return ret;
        }
        return null;
    }

    public HeroSkillConfig GetHeroSkillConfig(int id)
    {
        _heroSkillConfigs.TryGetValue(id, out var ret);
        return ret;
    }
    
}
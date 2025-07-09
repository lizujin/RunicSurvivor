using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleSystem;
using UnityEngine;

public class GameConfigs
{
    private static GameConfigs Instance;
    private GameConfigs()
    {
    }
    public static GameConfigs GetInstance()
    {
        if (Instance == null)
        {
            Instance = new GameConfigs();
        }
        return Instance;
    }

    private Dictionary<string, object> _configs = new ();

    public void InitGameConfigs()
    {
        LoadDefaultConfig();
        SkillConfigTool.GetInstance().InitSkillConfigs();
    }

    public void LoadDefaultConfig()
    {
        
        Dictionary<string, object> DefaultConfigs = new Dictionary<string, object>(){
            {"Skill/Skills", typeof(SkillsJsonType)},
            {"Skill/HeroSkill", typeof(HeroSkillJsonType)}
        };
        
        foreach (var config in DefaultConfigs)
        {
            var configPath = $"Config/{config.Key}";
            var configAsset = Resources.Load<TextAsset>(configPath);
            if (configAsset == null)
            {
                Debug.LogError($"配置文件 {configPath} 未找到");
                continue;
            }
            if (config.Value == null)
            {
                Debug.LogError($"配置文件 {configPath} 类型未找到");
                continue;
            }

            string classStr = "SimpleSystem." + GetConfigNameByPath(configPath) + "JsonType";
            Type type = Type.GetType(classStr);
            if (type == null)
            {
                Debug.LogError($"配置文件 {classStr} 类型未找到");
                continue;
            }

            var configData = JsonUtility.FromJson(configAsset.text, type);
            if (configData == null)
            {
                Debug.LogError($"配置文件 {configPath} 解析失败");
                continue;
            }
            _configs[config.Key] = configData;
            Debug.Log($"配置文件 {config} 加载成功");
        }
    }
    
    // 获得路径中的最后一个/后的名字
    public string GetConfigNameByPath(string path){
        var configName = path.Split('/').Last();
        return configName;
    }

    public T GetConfig<T>(string configName)
    {
        var result = _configs[configName];
        if (result == null)
        {
            // 加载/Resources/Config/下的json文件
            var configPath = $"Config/{configName}";
            var configAsset = Resources.Load<TextAsset>(configPath);
            if (configAsset == null)
            {
                Debug.LogError($"配置文件1 {configPath} 未找到");
                return default(T);
            }
            result = JsonUtility.FromJson<T>(configAsset.text);
            _configs[configName] = result;
        }
        return (T)result;
    }
}
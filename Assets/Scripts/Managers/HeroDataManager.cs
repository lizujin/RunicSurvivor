// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 负责从Heros.json中加载英雄数据并提供访问接口
/// </summary>
public class HeroDataManager : MonoBehaviour
{
    private static HeroDataManager _instance;
    public static HeroDataManager Instance => _instance;
    
    private HeroDataList heroDataList;
    private Dictionary<string, Hero> heroDict = new Dictionary<string, Hero>();
    
    private void Awake()
    {
        // 单例设置
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 加载英雄数据
        LoadHeroData();
    }
    
    /// <summary>
    /// 从Resources加载英雄数据
    /// </summary>
    private void LoadHeroData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Heros");
        
        if (jsonFile == null)
        {
            Debug.LogError("无法加载Heros.json文件！");
            return;
        }
        
        // 解析JSON数据
        heroDataList = JsonUtility.FromJson<HeroDataList>(jsonFile.text);
        
        if (heroDataList == null || heroDataList.heros == null)
        {
            Debug.LogError("Heros.json解析失败或格式不正确！");
            return;
        }
        
        // 将英雄数据添加到字典中，方便按名称查找
        foreach (Hero hero in heroDataList.heros)
        {
            heroDict[hero.name] = hero;
        }
        
        Debug.Log($"成功加载了 {heroDict.Count} 个英雄数据");
    }
    
    /// <summary>
    /// 获取所有英雄数据
    /// </summary>
    public List<Hero> GetAllHeroes()
    {
        return heroDataList?.heros ?? new List<Hero>();
    }
    
    /// <summary>
    /// 通过名称获取英雄数据
    /// </summary>
    public Hero GetHeroByName(string name)
    {
        if (heroDict.TryGetValue(name, out Hero hero))
        {
            return hero;
        }
        return null;
    }
    
    /// <summary>
    /// 获取指定cost值的英雄列表
    /// </summary>
    public List<Hero> GetHeroesByCost(int cost)
    {
        return heroDataList?.heros.Where(h => h.cost == cost).ToList() ?? new List<Hero>();
    }
    
    /// <summary>
    /// 按cost值对英雄进行分组
    /// </summary>
    public Dictionary<int, List<Hero>> GetHeroesGroupedByCost()
    {
        var result = new Dictionary<int, List<Hero>>();
        
        if (heroDataList?.heros == null)
            return result;
            
        foreach (Hero hero in heroDataList.heros)
        {
            if (!result.ContainsKey(hero.cost))
            {
                result[hero.cost] = new List<Hero>();
            }
            result[hero.cost].Add(hero);
        }
        
        return result;
    }
    
    /// <summary>
    /// 随机获取指定cost的英雄
    /// </summary>
    public Hero GetRandomHeroByCost(int cost)
    {
        var heroes = GetHeroesByCost(cost);
        if (heroes.Count == 0)
            return null;
            
        int randomIndex = Random.Range(0, heroes.Count);
        return heroes[randomIndex];
    }
    
    /// <summary>
    /// 根据几率分布随机获取英雄cost值
    /// </summary>
    public int GetRandomCost(float[] costChances)
    {
        // costChances应该是按cost顺序的概率分布，如1星到5星对应cost 1-5
        float random = Random.value;
        float cumulativeChance = 0;
        
        for (int i = 0; i < costChances.Length; i++)
        {
            cumulativeChance += costChances[i];
            if (random < cumulativeChance)
            {
                return i + 1; // cost是1-5，而索引是0-4
            }
        }
        
        return 1; // 默认返回最低cost
    }
    
    #region 种族和流派特性相关方法
    
    /// <summary>
    /// 获取所有种族类型
    /// </summary>
    public List<string> GetAllRaceTypes()
    {
        return heroDataList?.heros
            .Select(h => h.race)
            .Distinct()
            .Where(race => !string.IsNullOrEmpty(race))
            .ToList() ?? new List<string>();
    }
    
    /// <summary>
    /// 获取所有流派类型
    /// </summary>
    public List<string> GetAllClassTypes()
    {
        return heroDataList?.heros
            .Select(h => h.classType)
            .Distinct()
            .Where(classType => !string.IsNullOrEmpty(classType))
            .ToList() ?? new List<string>();
    }
    
    /// <summary>
    /// 获取指定种族的所有英雄
    /// </summary>
    public List<Hero> GetHeroesByRace(string race)
    {
        return heroDataList?.heros
            .Where(h => h.race == race)
            .ToList() ?? new List<Hero>();
    }
    
    /// <summary>
    /// 获取指定流派的所有英雄
    /// </summary>
    public List<Hero> GetHeroesByClassType(string classType)
    {
        return heroDataList?.heros
            .Where(h => h.classType == classType)
            .ToList() ?? new List<Hero>();
    }
    
    /// <summary>
    /// 按种族对英雄进行分组
    /// </summary>
    public Dictionary<string, List<Hero>> GetHeroesGroupedByRace()
    {
        var result = new Dictionary<string, List<Hero>>();
        
        if (heroDataList?.heros == null)
            return result;
            
        foreach (Hero hero in heroDataList.heros)
        {
            if (string.IsNullOrEmpty(hero.race))
                continue;
                
            if (!result.ContainsKey(hero.race))
            {
                result[hero.race] = new List<Hero>();
            }
            result[hero.race].Add(hero);
        }
        
        return result;
    }
    
    /// <summary>
    /// 按流派对英雄进行分组
    /// </summary>
    public Dictionary<string, List<Hero>> GetHeroesGroupedByClassType()
    {
        var result = new Dictionary<string, List<Hero>>();
        
        if (heroDataList?.heros == null)
            return result;
            
        foreach (Hero hero in heroDataList.heros)
        {
            if (string.IsNullOrEmpty(hero.classType))
                continue;
                
            if (!result.ContainsKey(hero.classType))
            {
                result[hero.classType] = new List<Hero>();
            }
            result[hero.classType].Add(hero);
        }
        
        return result;
    }
    
    #endregion
}

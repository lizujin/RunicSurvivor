// 本文件完全有AI生成
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Skills;
using System.Linq;

namespace Assets.Scripts.Managers
{
    /// <summary>
    /// 符文系统管理器：根据英雄的classType提供不同的队伍增益
    /// </summary>
    public class RunesManager : MonoBehaviour
    {
        private static RunesManager _instance;
        public static RunesManager Instance => _instance;
        
        // 当前应用的符文效果
        private Dictionary<string, BuffEffect> appliedBuffs = new Dictionary<string, BuffEffect>();
        
        // 每种符文类型的英雄计数
        private Dictionary<string, int> classTypeCounts = new Dictionary<string, int>();
        
        // 类型列表
        private readonly string[] CLASS_TYPES = new string[] 
        {
            "凶残", "善良", "狂野", "极速", "治愈", "生长"
        };
        
        // 自动刷新时间间隔（用于"生长"和"治愈"型的持续效果）
        private float autoEffectInterval = 1.0f;
        private float timeCounter = 0f;
        
        // 生长计数器（每5秒触发一次）
        private float growthCounter = 0f;
        
        private void Awake()
        {
            // 单例设置
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            // 确保不被销毁
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            // 初始化字典
            InitializeCounters();
            
            // 订阅玩家团队变化事件
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                PlayerController playerController = gameManager.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.OnTeamChanged += UpdateTeamClassSynergies;
                }
            }
        }
        
        private void Update()
        {
            // 处理需要每帧更新的效果
            timeCounter += Time.deltaTime;
            
            // 每秒处理效果（治愈）
            if (timeCounter >= autoEffectInterval)
            {
                ApplyAutoEffects();
                timeCounter = 0f;
            }
        }
        
        /// <summary>
        /// 初始化计数器
        /// </summary>
        private void InitializeCounters()
        {
            foreach (string classType in CLASS_TYPES)
            {
                classTypeCounts[classType] = 0;
            }
        }
        
        /// <summary>
        /// 更新队伍的符文增益效果
        /// </summary>
        public void UpdateTeamClassSynergies(List<Character> characters)
        {
            // 重置计数
            InitializeCounters();
            
            // 移除所有现有buff
            RemoveAllClassBuffs();
            
            // 统计每种classType的数量
            foreach (Character character in characters)
            {
                if (character != null)
                {
                    HeroDataManager heroDataManager = HeroDataManager.Instance;
                    if (heroDataManager != null)
                    {
                        // 查找对应的Hero数据
                        Hero heroData = heroDataManager.GetAllHeroes()
                            .FirstOrDefault(h => h.name == character.characterName);
                            
                        if (heroData != null && !string.IsNullOrEmpty(heroData.classType))
                        {
                            // 计数对应的符文类型
                            if (!classTypeCounts.ContainsKey(heroData.classType))
                            {
                                classTypeCounts[heroData.classType] = 0;
                            }
                            classTypeCounts[heroData.classType]++;
                        }
                    }
                }
            }
            
            // 应用符文增益效果
            ApplyClassBuffs(characters);
        }
        
        /// <summary>
        /// 根据英雄符文类型应用增益效果
        /// </summary>
        private void ApplyClassBuffs(List<Character> characters)
        {
            // 应用凶残效果 - 攻击+5
            if (classTypeCounts.ContainsKey("凶残") && classTypeCounts["凶残"] > 0)
            {
                foreach (Character character in characters)
                {
                    // 创建增益效果
                    BuffEffect buff = new BuffEffect(
                        "凶残增益", 
                        "来自凶残型角色的增益：攻击力+5", 
                        BuffType.Attack, 
                        5f, // 固定值为5
                        float.MaxValue, // 永久性
                        false, // 不可堆叠
                        1, 
                        false // 不是debuff
                    );
                    
                    // 应用buff
                    character.ApplyBuff(buff);
                    
                    // 记录已应用的buff
                    appliedBuffs["凶残"] = buff;
                }
            }
            
            // 应用善良效果 - 血量+10
            if (classTypeCounts.ContainsKey("善良") && classTypeCounts["善良"] > 0)
            {
                foreach (Character character in characters)
                {
                    // 直接增加最大生命值和当前生命值
                    character.maxHealth += 10f;
                    character.health += 10f;
                    
                    // 创建增益效果（用于记录和移除）
                    BuffEffect buff = new BuffEffect(
                        "善良增益", 
                        "来自善良型角色的增益：最大生命值+10", 
                        BuffType.Health, 
                        10f,
                        float.MaxValue,
                        false,
                        1,
                        false
                    );
                    
                    // 记录应用的buff
                    appliedBuffs["善良"] = buff;
                }
            }
            
            // 应用狂野效果 - 攻击速度+1
            if (classTypeCounts.ContainsKey("狂野") && classTypeCounts["狂野"] > 0)
            {
                foreach (Character character in characters)
                {
                    // 直接增加攻击速度
                    character.attackSpeed += 1f;
                    
                    // 创建增益效果
                    BuffEffect buff = new BuffEffect(
                        "狂野增益",
                        "来自狂野型角色的增益：攻击速度+1",
                        BuffType.Speed,
                        1f,
                        float.MaxValue,
                        false,
                        1,
                        false
                    );
                    
                    // 记录应用的buff
                    appliedBuffs["狂野"] = buff;
                }
            }
            
            // 应用极速效果 - 移动速度+1
            if (classTypeCounts.ContainsKey("极速") && classTypeCounts["极速"] > 0)
            {
                foreach (Character character in characters)
                {
                    // 直接增加移动速度
                    character.moveSpeed += 1f;
                    
                    // 创建增益效果
                    BuffEffect buff = new BuffEffect(
                        "极速增益",
                        "来自极速型角色的增益：移动速度+1",
                        BuffType.Speed,
                        1f,
                        float.MaxValue,
                        false,
                        1,
                        false
                    );
                    
                    // 记录应用的buff
                    appliedBuffs["极速"] = buff;
                }
            }
            
            // 治愈和生长效果由每帧更新来处理
        }
        
    /// <summary>
    /// 应用符文效果到特定角色
    /// </summary>
    public void ApplyRuneEffects(Character character, int level)
    {
        if (character == null) return;
        
        // 查找对应的Hero数据以获取classType
        HeroDataManager heroDataManager = HeroDataManager.Instance;
        if (heroDataManager != null)
        {
            // 查找对应的Hero数据
            Hero heroData = heroDataManager.GetAllHeroes()
                .FirstOrDefault(h => h.name == character.characterName);
                
            if (heroData != null && !string.IsNullOrEmpty(heroData.classType))
            {
                string classType = heroData.classType;
                
                // 应用不同符文类型的效果
                switch (classType)
                {
                    case "凶残":
                        // 攻击+5
                        BuffEffect attackBuff = new BuffEffect(
                            "凶残增益", 
                            "来自凶残型角色的增益：攻击力+5", 
                            BuffType.Attack, 
                            5f, // 固定值为5
                            float.MaxValue, // 永久性
                            false, // 不可堆叠
                            1, 
                            false // 不是debuff
                        );
                        character.ApplyBuff(attackBuff);
                        break;
                        
                    case "善良":
                        // 血量+10
                        character.maxHealth += 10f;
                        character.health += 10f;
                        break;
                        
                    case "狂野":
                        // 攻击速度+1
                        character.attackSpeed += 1f;
                        break;
                        
                    case "极速":
                        // 移动速度+1
                        character.moveSpeed += 1f;
                        break;
                        
                    // 治愈和生长效果由每秒更新处理，所以这里不需要单独处理
                }
                
                Debug.Log($"已应用{classType}符文效果到 {character.characterName} (等级 {level})");
            }
            else
            {
                Debug.LogWarning($"未找到角色 {character.characterName} 的符文数据");
            }
        }
    }
    
    /// <summary>
    /// 应用需要自动刷新的效果（治愈和生长）
    /// </summary>
    private void ApplyAutoEffects()
    {
            // 获取玩家控制器和队伍成员
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null) return;
            
            PlayerController playerController = gameManager.GetComponent<PlayerController>();
            if (playerController == null) return;
            
            List<Character> characters = playerController.GetTeamCharacters();
            
            // 应用治愈效果 - 每秒恢复1点血量
            if (classTypeCounts.ContainsKey("治愈") && classTypeCounts["治愈"] > 0)
            {
                foreach (Character character in characters)
                {
                    // 每秒恢复1点生命值
                    character.Heal(1f);
                }
            }
            
            // 应用生长效果 - 每5秒增加最大血量1
            if (classTypeCounts.ContainsKey("生长") && classTypeCounts["生长"] > 0)
            {
                growthCounter += autoEffectInterval;
                
                // 每5秒触发一次
                if (growthCounter >= 5f)
                {
                    foreach (Character character in characters)
                    {
                        // 增加最大生命值和当前生命值
                        character.maxHealth += 1f;
                        character.health += 1f;
                    }
                    
                    // 重置计数器
                    growthCounter = 0f;
                }
            }
        }
        
        /// <summary>
        /// 移除所有应用的符文增益效果
        /// </summary>
        private void RemoveAllClassBuffs()
        {
            // 找到玩家控制器和队伍成员
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null) return;
            
            PlayerController playerController = gameManager.GetComponent<PlayerController>();
            if (playerController == null) return;
            
            foreach (var buffEntry in appliedBuffs)
            {
                string classType = buffEntry.Key;
                BuffEffect buff = buffEntry.Value;
                
                // 对所有队伍成员移除该buff
                foreach (var character in playerController.TeamMembers)
                {
                    switch (classType)
                    {
                        case "凶残":
                            character.RemoveBuff(buff);
                            break;
                            
                        case "善良":
                            // 恢复原始生命值
                            character.maxHealth -= 10f;
                            character.health = Mathf.Min(character.health, character.maxHealth);
                            break;
                            
                        case "狂野":
                            // 恢复原始攻击速度
                            character.attackSpeed -= 1f;
                            break;
                            
                        case "极速":
                            // 恢复原始移动速度
                            character.moveSpeed -= 1f;
                            break;
                    }
                }
            }
            
            // 清空已应用的buff列表
            appliedBuffs.Clear();
        }
        
        /// <summary>
        /// 获取特定符文类型的数量
        /// </summary>
        public int GetClassTypeCount(string classType)
        {
            if (classTypeCounts.ContainsKey(classType))
            {
                return classTypeCounts[classType];
            }
            return 0;
        }
        
        /// <summary>
        /// 获取所有符文类型及其数量
        /// </summary>
        public Dictionary<string, int> GetAllClassTypeCounts()
        {
            return new Dictionary<string, int>(classTypeCounts);
        }
    }
}

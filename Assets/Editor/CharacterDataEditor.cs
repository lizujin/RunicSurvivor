// 本文件完全有AI生成
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 角色数据编辑器工具
/// </summary>
public class CharacterDataEditor : EditorWindow
{
    private string characterID = "xingjun";
    private string characterName = "孙悟空";
    private string description = "西游记的主角，拥有强大的战斗力。";
    private Sprite icon;
    private GameObject prefab;
    private CharacterRarity rarity = CharacterRarity.Rare;
    private int cost = 3;
    
    private RaceType race = RaceType.God;
    private FactionType faction = FactionType.Buddhist;
    private PositionType position = PositionType.Warrior;
    
    private float maxHealth = 150f;
    private float attack = 15f;
    private float attackSpeed = 1.2f;
    private float attackRange = 2.5f;
    private float moveSpeed = 4.0f;
    private float critChance = 0.1f;
    private float critDamage = 2.0f;
    
    private float maxMana = 100f;
    private float manaRegen = 5f;
    
    private GameObject attackEffect;
    private GameObject hitEffect;
    private GameObject deathEffect;
    private GameObject levelUpEffect;
    
    private GameObject projectilePrefab;
    private float projectileSpeed = 20f;
    private float projectileLifetime = 2f;
    private int projectilePierceCount = 1;
    private bool projectileHoming = false;
    private float projectileHomingStrength = 0f;
    
    private string skillID = "jinzhubang";
    private string skillName = "金箍棒";
    private string skillDescription = "使用金箍棒进行范围攻击，对周围敌人造成伤害。";
    private float skillManaCost = 30f;
    private float skillCooldown = 8f;
    private GameObject skillEffectPrefab;
    
    private Vector2 scrollPosition;
    private string savePath = "Assets/Resources/CharacterData/";
    private string filename = "SunWukong";
    
    [MenuItem("工具/角色数据编辑器")]
    public static void ShowWindow()
    {
        GetWindow<CharacterDataEditor>("角色数据编辑器");
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("角色数据创建工具", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);
        characterID = EditorGUILayout.TextField("角色ID:", characterID);
        characterName = EditorGUILayout.TextField("角色名称:", characterName);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
        icon = (Sprite)EditorGUILayout.ObjectField("角色图标:", icon, typeof(Sprite), false);
        prefab = (GameObject)EditorGUILayout.ObjectField("角色预制体:", prefab, typeof(GameObject), false);
        rarity = (CharacterRarity)EditorGUILayout.EnumPopup("稀有度:", rarity);
        cost = EditorGUILayout.IntField("购买成本:", cost);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("角色定位", EditorStyles.boldLabel);
        race = (RaceType)EditorGUILayout.EnumPopup("种族:", race);
        faction = (FactionType)EditorGUILayout.EnumPopup("派别:", faction);
        position = (PositionType)EditorGUILayout.EnumPopup("位置:", position);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("基础属性", EditorStyles.boldLabel);
        maxHealth = EditorGUILayout.FloatField("最大生命值:", maxHealth);
        attack = EditorGUILayout.FloatField("攻击力:", attack);
        attackSpeed = EditorGUILayout.FloatField("攻击速度:", attackSpeed);
        attackRange = EditorGUILayout.FloatField("攻击范围:", attackRange);
        moveSpeed = EditorGUILayout.FloatField("移动速度:", moveSpeed);
        critChance = EditorGUILayout.Slider("暴击几率:", critChance, 0f, 1f);
        critDamage = EditorGUILayout.FloatField("暴击伤害:", critDamage);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("能量系统", EditorStyles.boldLabel);
        maxMana = EditorGUILayout.FloatField("最大法力值:", maxMana);
        manaRegen = EditorGUILayout.FloatField("法力回复:", manaRegen);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("视觉效果", EditorStyles.boldLabel);
        attackEffect = (GameObject)EditorGUILayout.ObjectField("攻击特效:", attackEffect, typeof(GameObject), false);
        hitEffect = (GameObject)EditorGUILayout.ObjectField("受击特效:", hitEffect, typeof(GameObject), false);
        deathEffect = (GameObject)EditorGUILayout.ObjectField("死亡特效:", deathEffect, typeof(GameObject), false);
        levelUpEffect = (GameObject)EditorGUILayout.ObjectField("升级特效:", levelUpEffect, typeof(GameObject), false);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("子弹设置", EditorStyles.boldLabel);
        projectilePrefab = (GameObject)EditorGUILayout.ObjectField("子弹预制体:", projectilePrefab, typeof(GameObject), false);
        projectileSpeed = EditorGUILayout.FloatField("子弹速度:", projectileSpeed);
        projectileLifetime = EditorGUILayout.FloatField("子弹存活时间:", projectileLifetime);
        projectilePierceCount = EditorGUILayout.IntField("穿透数量:", projectilePierceCount);
        projectileHoming = EditorGUILayout.Toggle("追踪效果:", projectileHoming);
        if (projectileHoming)
        {
            EditorGUI.indentLevel++;
            projectileHomingStrength = EditorGUILayout.FloatField("追踪强度:", projectileHomingStrength);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("技能", EditorStyles.boldLabel);
        skillID = EditorGUILayout.TextField("技能ID:", skillID);
        skillName = EditorGUILayout.TextField("技能名称:", skillName);
        skillDescription = EditorGUILayout.TextArea(skillDescription, GUILayout.Height(60));
        skillManaCost = EditorGUILayout.FloatField("法力消耗:", skillManaCost);
        skillCooldown = EditorGUILayout.FloatField("冷却时间:", skillCooldown);
        skillEffectPrefab = (GameObject)EditorGUILayout.ObjectField("技能特效:", skillEffectPrefab, typeof(GameObject), false);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("保存设置", EditorStyles.boldLabel);
        savePath = EditorGUILayout.TextField("保存路径:", savePath);
        filename = EditorGUILayout.TextField("文件名:", filename);
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("创建角色数据"))
        {
            CreateCharacterData();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CreateCharacterData()
    {
        // 确保目录存在
        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }
        
        // 创建角色数据对象
        CharacterData characterData = ScriptableObject.CreateInstance<CharacterData>();
        
        // 设置基本信息
        characterData.characterID = characterID;
        characterData.characterName = characterName;
        characterData.description = description;
        characterData.characterIcon = icon;
        characterData.characterPrefab = prefab;
        characterData.rarity = rarity;
        characterData.cost = cost;
        
        // 设置角色定位
        characterData.race = race;
        characterData.faction = faction;
        characterData.position = position;
        
        // 设置基础属性
        characterData.maxHealth = maxHealth;
        characterData.attack = attack;
        characterData.attackSpeed = attackSpeed;
        characterData.attackRange = attackRange;
        characterData.moveSpeed = moveSpeed;
        characterData.critChance = critChance;
        characterData.critDamage = critDamage;
        
        // 设置能量系统
        characterData.maxMana = maxMana;
        characterData.manaRegen = manaRegen;
        
        // 设置视觉效果
        characterData.attackEffectPrefab = attackEffect;
        characterData.hitEffectPrefab = hitEffect;
        characterData.deathEffectPrefab = deathEffect;
        characterData.levelUpEffectPrefab = levelUpEffect;
        
        // 设置子弹
        characterData.projectilePrefab = projectilePrefab;
        characterData.projectileSpeed = projectileSpeed;
        characterData.projectileLifetime = projectileLifetime;
        characterData.projectilePierceCount = projectilePierceCount;
        characterData.projectileHoming = projectileHoming;
        characterData.projectileHomingStrength = projectileHomingStrength;
        
        // 添加技能
        SkillData skill = new SkillData();
        skill.skillID = skillID;
        skill.skillName = skillName;
        skill.description = skillDescription;
        skill.manaCost = skillManaCost;
        skill.cooldown = skillCooldown;
        skill.effectPrefab = skillEffectPrefab;
        characterData.skills = new SkillData[] { skill };
        
        // 保存到资源文件
        string fullPath = savePath + filename + ".asset";
        AssetDatabase.CreateAsset(characterData, fullPath);
        AssetDatabase.SaveAssets();
        
        // 刷新资产数据库
        AssetDatabase.Refresh();
        
        // 选中新创建的资源
        Selection.activeObject = characterData;
        
        Debug.Log($"角色数据已创建: {fullPath}");
    }
}

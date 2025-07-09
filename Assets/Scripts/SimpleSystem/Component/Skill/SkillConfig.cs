using Unity.VisualScripting;
using UnityEngine.SocialPlatforms;

namespace SimpleSystem{
    [System.Serializable]
    public class SkillConfig
    {
        public int id;
        public string name;
        public string description;

        // 技能形状
        public ESkillShape shape;
        public float shapeRadius;
        public float shapeWidth;
        public float shapeHeight;
        public ESkillShapeTrigger shapeTrigger;

        // 技能作用
        public string targetProperty; // 作用的属性 血量 护盾 攻击力 防御力 速度 等
        public string enhanceProperty; // 增强的属性 血量 护盾 攻击力 防御力 速度 等

        // 技能持续时间
        public ESkillDurationType durationType;

        public float durationValue;

        public float intervalValue;

        // 技能伤害
        public float forceValue;

        // 技能CD
        public float CD;

        // 技能计算类型
        public ESkillCaleType caleType;

        // 技能值类型
        public ESkillValueType valueType;

        public float value;

        public float valueMax;

        // 技能值最大类型
        public ESkillValueMaxType valueMaxType;

        // 技能目标选择类型
        public ESkillTargetSelectType targetSelectType;

        // 技能目标数量
        public int targetNum;

        // 技能移动目标类型
        public ESkillMoveTargetType moveTargetType;

        // 技能移动速度
        public float moveSpeed;

        // 技能移动距离
        public float moveDistance;

        // 技能效果ID
        public int effectId;

        public ESkillEffectGroupType effectType;

        // 技能前延迟
        public float preDelay;

        // 技能后延迟
        public float postDelay;

        // 技能开始动画
        public string startAnimation;

        // 技能结束动画
        public string endAnimation;
        
        //
    }

    [System.Serializable]
    public class SkillsJsonType
    {
        public SkillConfig[] skills;
    }
    
    [System.Serializable]
    public class HeroSkillConfig
    {
        public int id;
        public int[] skillIds;
    }
    
    [System.Serializable]
    public class HeroSkillJsonType
    {
        public HeroSkillConfig[] heros;
    }
}
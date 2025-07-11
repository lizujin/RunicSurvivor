using System.Collections;
using System.Collections.Generic;
using SimpleSystem.Utils;
using UnityEngine;

namespace SimpleSystem {
    // 技能系统
    public class SkillSystem : BaseSystem
    {
        private List<SkillComponent> _skillComponents = new List<SkillComponent>();

        private BaseSystemMono _mono;
        // 技能初始化
        override public void Init(){
        }

        override public void Start()
        {
            _mono = GameManager.Instance.EffectRoot.gameObject.AddComponent<BaseSystemMono>();
        }

        // 技能更新
        override public void Update(float delta){
            foreach (var skillComponent in _skillComponents){
                for (int i = 0; i < skillComponent.skillConfig.Length; i++){
                    var skillConfig = skillComponent.skillConfig[i];
                    var skillContent = skillComponent.skillContent[i];
                    if (skillConfig == null || skillContent == null)
                    {
                        break;
                    }
                    if (skillContent.state == ESkillState.Cooling){
                        skillContent.CDLeftTime -= delta;
                        if (skillContent.CDLeftTime <= 0){
                            Debug.Log("SkillSystem: 技能冷却结束，重新释放技能" + skillConfig.name);
                            skillContent.state = ESkillState.None;
                            _mono.StartCoroutine(CastSkill(skillContent.source, skillConfig, skillContent, skillContent.position, skillContent.target));
                        }
                    } else if (skillContent.state == ESkillState.Casting){
                        if (skillConfig.durationType == ESkillDurationType.Time)
                        {
                            skillContent.CastTime = skillContent.CastTime + delta;
                            if (skillConfig.durationType == ESkillDurationType.Time && skillConfig.durationValue <= skillContent.CastTime){
                                skillContent.state = ESkillState.Cooling;
                                skillContent.CDLeftTime = skillConfig.CD;
                                Debug.Log("SkillSystem: 技能持续时间到， 进入冷却" + skillConfig.name);
                            }
                        } else if (skillConfig.durationType == ESkillDurationType.Times)
                        {
                            CheckSkillCast(skillContent.source, skillConfig, skillContent, skillContent.position, skillContent.target);
                        }
                    }
                }
            }
        }
        
        public void Stop(){
        }

        public void Destroy(){
        }

        public void CastEntitySkill(BaseEntity source, Vector3 position = default, BaseEntity target = null){
            var skillComponent = EntityManager.GetInstance().GetComponent<SkillComponent>(source);
            if (skillComponent == null){
                return;
            }
            _skillComponents.Add(skillComponent);
            Debug.Log("SkillSystem: 实体释放技能 " + source.name);
            for (int i = 0; i < skillComponent.skillConfig.Length; i++){
                var skillConfig = skillComponent.skillConfig[i];
                var skillContent = skillComponent.skillContent[i];
                if (skillContent == null || skillConfig == null){
                    continue;
                }
                skillContent.position = position;
                skillContent.target = target;
                skillContent.source = source;
                if (skillConfig.id != 0 && skillContent.state == ESkillState.None){
                    _mono.StartCoroutine(CastSkill(source, skillConfig, skillContent, position, target));
                }
            }
        }
        
        public IEnumerator CastSkill(BaseEntity source, SkillConfig skillConfig, SkillContent skillContent, Vector3 position = default, BaseEntity target = null){
            if (skillContent.state == ESkillState.None)
            {
                Debug.Log("SkillSystem: 开始释放技能" + skillConfig.name);
                PlaySkillAnimation(source, skillConfig.startAnimation);
                yield return new WaitForSeconds(skillConfig.preDelay);
                CreateSkillShape(source, skillConfig);
                skillContent.state = ESkillState.Casting;
            } else if (skillContent.state != ESkillState.Casting)
            {
                Debug.Log("SkillSystem: 当前状态不是None状态" + skillContent.state);
                yield break;
            }
            CheckSkillCast(source, skillConfig, skillContent, position, target);
        }

        private bool CheckSkillCast(BaseEntity source, SkillConfig skillConfig, SkillContent skillContent,
            Vector3 position = default, BaseEntity target = null)
        {
            var targets = new List<BaseEntity>();
            if (target != null){
                targets.Add(source);
            }
            else{
                targets = GetTargets(source, skillConfig, target);
            }
            if (targets.Count == 0){
                return false;
            }
            _mono.StartCoroutine(DoCastSkill(source, skillConfig, skillContent, targets));
            return true;
        }

        private IEnumerator DoCastSkill(BaseEntity source, SkillConfig skillConfig, SkillContent skillContent,
             List<BaseEntity> targets)
        {
            Debug.Log("SkillSystem: 释放技能 " + skillConfig.name + "目标数量：" + targets.Count);
            AfterCastSkill(source, skillConfig, targets, skillContent);
            foreach (var t in targets){
                var effectValue = GetEffectValue(source, skillConfig, t);
                ApplyEffectValue(source, skillConfig, t, effectValue);
                var pos = t.transform.position;
                CreateSkillEffect(source, skillConfig.effectId, pos);
                // yield return new WaitForSeconds(skillConfig.intervalValue);
            }
            yield break;
        }

        private bool CreateSkillShape(BaseEntity source, SkillConfig skillConfig){
            return true;
        }

        private void PlaySkillAnimation(BaseEntity source, string anim){
            // 播放技能动画
        }

        private void CreateSkillEffect(BaseEntity source, int effectId, Vector3 pos)
        {
            Debug.Log("SkillSystem: 创建技能特效 " + effectId);
            var path = "Prefabs/Skill/Effect/" + effectId;
            var effect = ObjectPool.Instance.GetPrefabInstance(path);
            var root = GameManager.Instance.EffectRoot;
            effect.transform.SetParent(root);
            effect.transform.position = pos;
            effect.transform.localScale = Vector3.one;
        }

        private void AfterCastSkill(BaseEntity source, SkillConfig skillConfig, List<BaseEntity> targets, SkillContent skillContent){
            if (skillContent == null){
                return;
            }
            if(skillConfig.durationType == ESkillDurationType.Times){
                // 技能次数
                skillContent.CastCount = skillContent.CastCount + 1;
                if(skillContent.CastCount >= skillConfig.durationValue){
                    skillContent.state = ESkillState.Cooling;
                    skillContent.CDLeftTime = skillConfig.CD;
                    Debug.Log("SkillSystem: 技能结束 进入冷却" + skillConfig.name);
                }
            }
        }

        private (SkillConfig, SkillContent) SkillCheck(BaseEntity source, int skillId, BaseEntity target){
            var skillComponent = EntityManager.GetInstance().GetComponent<SkillComponent>(source);
            if (skillComponent == null){
                return (null, null);
            }

            SkillConfig skillConfig = null;
            int index = 0;
            for (int i = 0; i < skillComponent.skillConfig.Length; i++)
            {
                var s = skillComponent.skillConfig[i];
                if (s.id == skillId)
                {
                    skillConfig = s;
                    index = i;
                }
            }
            if (skillConfig == null){
                return (null, null); // 技能配置不存在
            }
            
            SkillContent skillContent = null;
            for (int i = 0; i < skillComponent.skillContent.Length; i++)
            {
                if (skillComponent.skillContent[i].id == skillId)
                {
                    skillContent = skillComponent.skillContent[i];
                }
            }
            if (skillContent == null){
                skillContent = new SkillContent();
                skillContent.id = skillId;
                skillComponent.skillContent[index] = skillContent;
            }
            return (skillConfig, skillContent);
        }

        private List<BaseEntity> GetTargets(BaseEntity source, SkillConfig skillConfig, BaseEntity target){
            var ret = new List<BaseEntity>();
            if (skillConfig.effectType == ESkillEffectGroupType.Self)
            {
                ret.Add(source);
                return ret;
            }
            switch(skillConfig.targetSelectType){
                case ESkillTargetSelectType.Nearest:
                    GetNearestTargets(source, skillConfig, ref ret);
                    break;
                case ESkillTargetSelectType.Random:
                    ret = GetRandomTargets(source, skillConfig);
                    break;
                case ESkillTargetSelectType.All:
                    ret = GetAllTargets(source, skillConfig);
                    break;
                case ESkillTargetSelectType.AllRandom:
                    ret = GetAllRandomTargets(source, skillConfig);
                    break;
                case ESkillTargetSelectType.Shape:
                    ret = GetShapeTargets(source, skillConfig);
                    break;
                case ESkillTargetSelectType.ShapeRandom:
                    ret = GetShapeRandomTargets(source, skillConfig);
                    break;
            }
            return ret;
        }

        private bool GetNearestTargets(BaseEntity source, SkillConfig skillConfig, ref List<BaseEntity> enemies)
        {
            return World.GetInstance().GetNearestEnemies(source, ref enemies, skillConfig.shapeRadius, skillConfig.targetNum);
        }
        
        private List<BaseEntity> GetRandomTargets(BaseEntity source, SkillConfig skillConfig){
            var ret = new List<BaseEntity>();
            return ret;
        }

        private List<BaseEntity> GetAllTargets(BaseEntity source, SkillConfig skillConfig){
            var ret = new List<BaseEntity>();
            return ret;
        }

        private List<BaseEntity> GetAllRandomTargets(BaseEntity source, SkillConfig skillConfig){
            var ret = new List<BaseEntity>();
            return ret;
        }
        
        private List<BaseEntity> GetShapeTargets(BaseEntity source, SkillConfig skillConfig){
            var ret = new List<BaseEntity>();
            return ret;
        }

        private List<BaseEntity> GetShapeRandomTargets(BaseEntity source, SkillConfig skillConfig){
            var ret = new List<BaseEntity>();
            return ret;
        }
        
        private float GetEffectValue(BaseEntity source, SkillConfig skillConfig, BaseEntity target){
            var ret = 0f;
            switch(skillConfig.valueType){
                case ESkillValueType.Fixed:
                    ret = skillConfig.value;
                    break;
                case ESkillValueType.Random:
                    ret = UnityEngine.Random.Range(skillConfig.value, skillConfig.valueMax);
                    break;
                case ESkillValueType.Percent:
                    ret = GetEntityPropertyValue(source, skillConfig.targetProperty) * skillConfig.value;
                    break;
                default:
                    Debug.LogError("GetEffectValue: " + skillConfig.valueType + " not found");
                    ret = 0f;
                    break;
            }
            return ret;
        }

        private float GetEntityPropertyValue(BaseEntity entity, string property){
            var type = entity.GetType();
            var method = type.GetMethod("Get" + property);
            if (method == null){
                Debug.LogError("GetEntityPropertyValue: " + property + " not found");
                return 0f;
            }
            return (float)method.Invoke(entity, null);
        }

        private void ApplyEffectValue(BaseEntity source, SkillConfig skillConfig, BaseEntity target, float effectValue){
            var type = target.GetType();
            if (skillConfig.caleType == ESkillCaleType.Add){
                var method = type.GetMethod("Add" + skillConfig.targetProperty);
                if (method == null){
                    Debug.LogError("ApplyEffectValue: " + skillConfig.targetProperty + " not found");
                    return;
                }
                method.Invoke(target, new object[] { effectValue, source, skillConfig });
            }
            else if (skillConfig.caleType == ESkillCaleType.Mul){
                var method = type.GetMethod("Mul" + skillConfig.valueMaxType);
                if (method == null){
                    Debug.LogError("ApplyEffectValue: " + skillConfig.valueMaxType + " not found");
                    return;
                }
                method.Invoke(target, new object[] { effectValue , source, skillConfig});
            }
            else{
                Debug.LogError("ApplyEffectValue: " + skillConfig.caleType + " not found");
            }
        }
    }
}

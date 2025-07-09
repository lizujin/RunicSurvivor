
using UnityEngine;

namespace SimpleSystem{
    public class SkillContent
    {
        public int id;
        public ESkillState state;
        public float CDLeftTime; // 技能CD剩余时间
        public float CastTime; // 技能释放时间持续时间
        public int CastCount; // 技能释放次数
        public float CastIntervalLeftTime; // 技能释放间隔时间
        public Vector3 position;
        public BaseEntity target;
        public BaseEntity source;
    }
}
namespace SimpleSystem{
    public enum ESkillShape{
        None = 0,
        Circle = 1,
        Line = 2,
        Point = 3,
        Rect = 4,
        Sector = 5,
        All = 6,
    }

    public enum ESkillShapeTrigger{
        Hit = 1,
        Enter = 2,
        Exit = 3,
        In = 4,
        Out = 5,
    }

    // 技能持续类型
    public enum ESkillDurationType{
        Time = 1,
        Times = 2,
    }

    public enum ESkillCaleType{
        Add = 1,
        Mul = 2,
    }

    public enum ESkillValueType{
        Fixed = 1,
        Random = 2,
        Percent = 3,
    }

    public enum ESkillValueMaxType{
        Current,
        Max,
    }

    public enum ESkillEffectGroupType{
        Self = 1,
        Friend = 2,
        Enemy = 3,
    }

    public enum ESkillTargetSelectType{
        Nearest = 1,
        Shape = 2,
        ShapeRandom = 3,
        Random = 4,
        All = 5,
        AllRandom = 6,
    }

    public enum ESkillMoveTargetType{
        None = 0,
        LineCircle = 3,
    }

    public enum ESkillState{
        None = 0,
        Casting = 1,
        Pause = 2,
        Cancel = 3,
        Finish = 4,
        Cooling = 5,
    }
}
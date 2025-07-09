using System.Collections.Generic;

namespace SimpleSystem{
    public class SkillComponent : BaseComponent
    {
        public SkillConfig[] skillConfig;
        public SkillContent[] skillContent;
        public int curSkillCount = 0;
    }
}

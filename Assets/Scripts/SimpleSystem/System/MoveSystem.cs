using System.Collections.Generic;
namespace SimpleSystem {
    // 移动系统
    public class MoveSystem<T> : BaseSystem where T : BaseEntity
    {
        private List<T> _entities = new List<T>();
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
            var entities = GetEntities();
            
        }
        
        public void Stop(){
        }

        public void Destroy(){
        }

        public void AddMoveEntity(T obj){
            _entities.Add(obj);
        }

        public void RemoveMoveEntity(T entity){
            _entities.Remove(entity);
        }

        private List<T> GetEntities(){
            return _entities;
        }
    }
}

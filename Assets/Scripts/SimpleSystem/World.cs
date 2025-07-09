using System.Collections.Generic;

namespace SimpleSystem
{
    public class World
    {
        private static World instance;
        private List<BaseSystem> systems = new List<BaseSystem>();
        private World()
        {
        }

        public static World GetInstance()
        {
            if (instance == null)
            {
                instance = new World();
            }
            return instance;
        }

        public void Start(){
            foreach (var system in systems){
                system.Start();
            }
        }

        public void Init(){
            AddSystem(new SkillSystem());
        }

        public void Destroy(){
            foreach (var system in systems){
                system.Destroy();
            }
        }

        public List<Enemy> GetNearestEnemies(BaseEntity character, float radius, int count)
        {
            var mgr = GameManager.Instance.GetEnemyManager();
            if (mgr)
            {
                return mgr.GetNearestEnemies(character.transform.position, radius, count);
            }
            return null;
        }

        public void AddSystem(BaseSystem system){
            system.Init();
            systems.Add(system);
        }

        public void RemoveSystem(BaseSystem system){
            systems.Remove(system);
        }

        public T GetSystem<T>()
        {
            foreach (var system in systems)
            {
                if (system is T tSys)
                {
                    return tSys;
                }
            }
            return default;
        }

        public void Update(float deltaTime){
            foreach (var system in systems){
                system.Update(deltaTime);
            }
        }
    }
}
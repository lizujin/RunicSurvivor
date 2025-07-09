using System.Collections.Generic;
using System;
using System.Numerics;
using UnityEngine;

namespace SimpleSystem
{
    public class EntityManager
    {
        private static EntityManager instance;
        private EntityManager(){
        }

        public static EntityManager GetInstance(){
            if (instance == null){
                instance = new EntityManager();
            }
            return instance;
        }

        private Dictionary<int, BaseEntity> entityMap = new Dictionary<int, BaseEntity>();
        private Dictionary<int, List<BaseComponent>> componentMap = new Dictionary<int, List<BaseComponent>>();

        private int entityId = 0;
        public void Start(){
        }

        public BaseEntity CreateEntity(){
            var id = entityId++;
            if (id == int.MaxValue){
                throw new Exception("Entity id overflow");
            }
            BaseEntity entity = new BaseEntity();
            entity.id = id;
            entityMap.Add(entity.id, entity);
            return entity;
        }

        public bool RemoveEntity(BaseEntity entity){
            entityMap.Remove(entity.id);
            return true;
        }
        
        public BaseEntity GetEntity(int id)
        {
            entityMap.TryGetValue(id, out BaseEntity ret);
            return ret;
        }

        public T AddComponent<T>(BaseEntity entity) where T : BaseComponent, new(){
             componentMap.TryGetValue(entity.id, out var components);
            if (components == null){
                components = new List<BaseComponent>();
                componentMap.Add(entity.id, components);
            }

            var ret = components.Find(c => c is T);
            if (ret != null){
                return (T)ret;
            }
            var component = new T();
            components.Add(component);
            return component;
        }

        public bool RemoveComponent(BaseEntity entity, BaseComponent component){
            componentMap.TryGetValue(entity.id, out var components);
            if (components == null){
                return false;
            }
            components.Remove(component);
            return true;
        }

        public T GetComponent<T>(BaseEntity entity){
            componentMap.TryGetValue(entity.id, out var components);
            if (components == null)
            {
                return default;
            }
            foreach (var component in components){
                if (component is T tComponent){
                    return tComponent;
                }
            }
            return default;
        }
    }
}
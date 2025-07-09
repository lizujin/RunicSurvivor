// 对象池   

using System.Collections.Generic;
using UnityEngine;

namespace SimpleSystem.Utils
{
    public class ObjectPool
    {
        private static ObjectPool _instance;
        public static ObjectPool Instance
        {
            get
            {
                return _instance ?? (_instance = new ObjectPool());
            }
        }

        private Dictionary<string, Queue<object>> _pool;
        private ObjectPool()
        {
            _pool = new Dictionary<string, Queue<object>>();
        }

        public T Get<T>(string key) where T : class, new()
        {
            if (!_pool.ContainsKey(key))
            {
                _pool[key] = new Queue<object>();
            }

            if (_pool[key].Count > 0)
            {
                return _pool[key].Dequeue() as T;
            }

            return new T();
        }

        public void Recycle(string key, object obj)
        {
            if (!_pool.ContainsKey(key))
            {
                _pool[key] = new Queue<object>();
            }

            _pool[key].Enqueue(obj);
        }

        public void Destroy()
        {
            _pool.Clear();
        }

        public GameObject GetPrefabInstance(string path)
        {
            var ret = Get<GameObject>(path);
            if (ret == null)
            {
                var prefab = Resources.Load<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab not found: {path}");
                    return null;
                }
                ret = GameObject.Instantiate(prefab);
            }
            return ret;
        }

        public void RecyclePrefabInstance(string key, GameObject obj)
        {
            Recycle(key, obj);
        }
    }
}
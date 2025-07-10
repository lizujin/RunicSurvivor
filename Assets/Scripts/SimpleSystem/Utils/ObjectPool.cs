// 对象池   

using System.Collections.Generic;
using UnityEngine;

namespace SimpleSystem.Utils
{
    public class ObjectPool
    {
        private static ObjectPool _instance;
        private Transform poolRoot;
        public static ObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ObjectPool();
                    _instance.poolRoot = GameObject.Find("PoolRoot").transform;
                }

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

            return default;
        }

        public void Recycle(string key, GameObject obj)
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
            if (ret == default || ret == null)
            {
                var prefab = Resources.Load<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab not found: {path}");
                    return null;
                }
                ret = GameObject.Instantiate(prefab);
            }
            ret.SetActive(true);
            return ret;
        }

        public void RecyclePrefabInstance(string key, GameObject obj)
        {
            obj.transform.SetParent(poolRoot);
            obj.SetActive(false);
            obj.transform.localPosition = Vector3.zero;
            Recycle(key, obj);
        }
    }
}
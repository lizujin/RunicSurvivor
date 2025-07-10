using System;
using System.Collections;
using SimpleSystem.Utils;
using UnityEngine;

namespace Utils
{
    public class AutoCollectComponent : MonoBehaviour
    {
        public float CollectTime = 1;
        public string CollectKey;
        private float _time = 0;
        private ObjectPool objectPool;
        public void Start()
        {
            objectPool = ObjectPool.Instance;
            StartCoroutine(CollectObj());
        }

        IEnumerator CollectObj()
        {
            yield return new WaitForSeconds(CollectTime);
            objectPool.RecyclePrefabInstance(CollectKey, gameObject);
            Debug.Log("回收"+CollectKey);
        }
    }
}
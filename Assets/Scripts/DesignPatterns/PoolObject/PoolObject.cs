using System;
using UnityEngine;
using UnityEngine.Pool;

namespace FastPolygons
{
    public class PoolObject : MonoBehaviour
    {
        private readonly GameObject m_prefab;
        private readonly int m_size;
        public readonly IObjectPool<GameObject> m_objectPool;

        public PoolObject(GameObject prefab, int size)
        {
            m_size = size;
            m_prefab = prefab;

            m_objectPool = new UnityEngine.Pool.ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool,
                OnReturnedToPool, OnDestroyPoolObject, true, Size);
        }

        public int Size { get => m_size; }
        public int CountInactive => m_objectPool.CountInactive;

        private void OnReturnedToPool(GameObject _Object)
        {
            _Object.SetActive(false);
        }
        private void OnTakeFromPool(GameObject _Object)
        {
            _Object.SetActive(true);
        }
        private void OnDestroyPoolObject(GameObject _Object)
        {
            Destroy(_Object);
        }
        private GameObject CreatePooledItem()
        {
            return Instantiate(m_prefab);
        }

        public GameObject Get()
        {
            return m_objectPool.Get();
        }
        public PooledObject<GameObject> Get(out GameObject v)
        {
            return m_objectPool.Get(out v);
        }
        public void Release(GameObject element)
        {
            m_objectPool.Release(element);
        }
        public void Clear()
        {
            m_objectPool.Clear();
        }
    }
}

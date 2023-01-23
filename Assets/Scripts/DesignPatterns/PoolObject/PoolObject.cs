using System;
using UnityEngine;
using UnityEngine.Pool;

namespace FastPolygons
{
    public class PoolObject : MonoBehaviour
    {
        private GameObject m_prefab;
        private int m_size;
        private IObjectPool<GameObject> m_objectPool;

        public PoolObject(GameObject prefab, int size)
        {
            this.m_size = size;
            this.m_prefab = prefab;
        }

        public int PoolSize { get => m_size; }
        public IObjectPool<GameObject> Items
        {
            get
            {
                if (m_objectPool != null) return m_objectPool;

                m_objectPool = new UnityEngine.Pool.ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool,
                    OnReturnedToPool, OnDestroyPoolObject, true, PoolSize);

                return m_objectPool;
            }
        }

        /* These functions are for the object pool pattern */
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
    }
}

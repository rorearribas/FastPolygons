using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FastPolygons
{
    public class PersistentSingleton<T> : MonoBehaviour where T:Component
    {
        private static T m_instance;
        public static T Instance
        {
            get
            {
                if(m_instance == null)
                {
                    m_instance = FindObjectOfType<T>();

                    if(m_instance == null)
                    {
                        GameObject obj = new();
                        m_instance = obj.AddComponent<T>();
                    }

                }

                return m_instance;
            }
        }
        public virtual void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            if (m_instance == null)
                m_instance = this as T;
            else
                Destroy(gameObject);
        }
    }
}

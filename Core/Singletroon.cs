using UnityEngine;

public class Singletroon<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_Instance;

    public static T Instance
    {
        get
        {
            m_Instance = (T)FindFirstObjectByType(typeof(T));
            if (m_Instance != null)
            {
                return m_Instance;
            } 
            else 
            {
                var singletonObject = new GameObject();
                m_Instance = singletonObject.AddComponent<T>();
                singletonObject.name = typeof(T).ToString() + " (Singleton)";
                return m_Instance;
            }
        }
    }

    public static bool InstanceExists
    {
        get
        {
            if (m_Instance != null)
                return true;

            m_Instance = (T)FindFirstObjectByType(typeof(T));
            return m_Instance != null;
        }
    }
}


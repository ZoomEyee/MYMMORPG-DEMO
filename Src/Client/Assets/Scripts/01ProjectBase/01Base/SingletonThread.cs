using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonThread<T> where T : new()
{
    private class SingletonHolder
    {
        public static T instance = new T();
    }
    public static T Instance
    {
        get
        {            
            return SingletonHolder.instance;
        }
    }
}

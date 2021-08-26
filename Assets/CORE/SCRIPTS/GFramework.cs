﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.DataManager;
namespace Framework
{
    public class GFramework
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Main()
        {
            Debug.Log("GFramework启动");
            GameObject go = new GameObject("MemPoolManager");
            go.AddComponent(typeof(MemPool<>));
        }
    }
}
namespace MF
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new();

        public static void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        void Update()
        {
            while (_executionQueue.Count > 0)
            {
                Action action;
                lock (_executionQueue)
                {
                    action = _executionQueue.Dequeue();
                }
                action.Invoke();
            }
        }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            if (FindObjectOfType<UnityMainThreadDispatcher>() == null)
            {
                var go = new GameObject("MainThreadDispatcher");
                DontDestroyOnLoad(go);
                go.AddComponent<UnityMainThreadDispatcher>();
            }
        }
    }
}
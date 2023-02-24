using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace septim.core.threading
{
    [Obsolete("Need to overhaul into thread only version")]
    /// <summary>
    /// We arragne threads here, each threads should contains a coroutine dictionary.
    /// We should limit maximum threads by Environment.ProcessorCount.
    /// We only pending threads into thread master when there is no activate threads exist
    /// </summary>
    public class ThreadManager
    {
        /*
        private static ThreadManager instance;

        public static ThreadManager GetInstance()
        {
            if (ThreadManager.instance == null)
            {
                ThreadManager.instance = new ThreadManager();
            }
            return ThreadManager.instance;
        }

        public ThreadManager()
        {
            activeThreads = new HashSet<ThreadBody>();
            maxThreads = Environment.ProcessorCount;
            threadPool = new List<IEnumerator>();
        }

        HashSet<ThreadBody> activeThreads;
        public int maxThreads;

        List<IEnumerator> threadPool;
        bool onThreading = false;

        private void StartNextThreads()
        {
            if(threadPool.Count != 0)
            {
                GameManager.instance.StartCoroutine(threadPool[0]);
                threadPool.RemoveAt(0);
            }
        }

        public void AssignThreadsRegistration(List<ThreadBody> input, int pendingSecond, string requestName, string className)
        {
            threadPool.Add(ThreadRegistration(input, pendingSecond, requestName, className));
            if(activeThreads.Count == 0)
            {
                StartNextThreads();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">package all tasks into threadBodies before pending</param>
        /// <param name="pendingSecond">time out second to prevent thread jamming</param>
        /// <param name="requestName">pass function name when call this method</param>
        /// <param name="className">pass class name when call this method</param>
        /// <returns></returns>
        private IEnumerator ThreadRegistration(List<ThreadBody> input, int pendingSecond, string requestName, string className)
        {
            int pendingTime = 0;
            bool response = true;
            while(activeThreads.Count != 0)
            {
                pendingTime++;
                if(pendingTime > pendingSecond)
                {
                    response = false;
                }
                yield return new WaitForSeconds(1);
            }
            if (response)
            {
                Debug.LogError("408 Request Time Out On :" + requestName + " , in :" + className);
            }
            else
            {
                foreach(ThreadBody var in input)
                {
                    activeThreads.Add(var);
                    var.Run();
                }
            }
        }

        public void ThreadDeregistration(ThreadBody input)
        {
            if (activeThreads.Contains(input))
            {
                activeThreads.Remove(input);
            }
            if(activeThreads.Count == 0)
            {
                StartNextThreads();
            }
        }
        */
    }
}


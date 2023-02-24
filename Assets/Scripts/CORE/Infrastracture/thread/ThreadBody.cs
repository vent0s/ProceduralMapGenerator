using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace septim.core.threading
{
    [Obsolete("Need to overhaul into thread only version")]
    public class ThreadBody : MonoBehaviour
    {
        /*
        //threadManager

        //thread itself
        Thread thread;

        //coroutine dictionary
        Dictionary<CoroutineBody, IEnumerator> coroutineDict;

        public int coroutineCount = 0;

        public void Run()
        {
            thread = new Thread(RunCoroutines);
            thread.Start();
            //ThreadManager.GetInstance()；
        }

        public void AssignCoroutine(CoroutineBody input)
        {
            coroutineDict.Add(input, input.coroutine);
        }

        public void OnDeregistrateCoroutine(CoroutineBody input)
        {
            if (coroutineDict.ContainsKey(input))
            {
                StopCoroutine(coroutineDict[input]);
                coroutineDict.Remove(input);
                coroutineCount--;
            }
            if (coroutineCount == 0)
            {
                StartCoroutine(OnDestroy());
            }
        }

        IEnumerator OnDestroy()
        {
            while (thread.IsAlive)
            {
                yield return new WaitForSeconds(1);
            }
            ThreadManager.GetInstance().ThreadDeregistration(this);
            Destroy(this);
        }

        //coroutineFunction
        private void RunCoroutines()
        {
            if(coroutineDict.Count != 0)
            {
                foreach(KeyValuePair<CoroutineBody,IEnumerator> var in coroutineDict)
                {
                    StartCoroutine(var.Value);
                }
            }
        }
        */
    }

}

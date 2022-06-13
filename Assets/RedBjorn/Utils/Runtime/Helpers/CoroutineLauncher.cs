using RedBjorn.Utils;
using System;
using System.Collections;
using UnityEngine;

namespace RedBjorn.Utils
{
    public class CoroutineLauncher : MonoBehaviour
    {

        static CoroutineLauncher CachedInstance;
        static CoroutineLauncher Instance
        {
            get
            {
                if (CachedInstance == null)
                {
                    var go = new GameObject("CoroutineLauncher");
                    CachedInstance = go.AddComponent<CoroutineLauncher>();
                }
                return CachedInstance;
            }
        }

        public static Coroutine Launch(IEnumerator ienum, Action onCompleted = null)
        {
            return Instance.Play(ienum, onCompleted);
        }

        public static void Finish(Coroutine coroutine)
        {
            Instance.Stop(coroutine);
        }

        public Coroutine Play(IEnumerator ienum, Action onCompleted = null)
        {
            return StartCoroutine(WithOnCompleted(ienum, onCompleted));
        }

        public void Stop(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        public IEnumerator WithOnCompleted(IEnumerator ienum, Action onCompleted)
        {
            while (true)
            {
                object current;
                if (!ienum.MoveNext())
                {
                    break;
                }
                current = ienum.Current;
                yield return current;
            }
            onCompleted.SafeInvoke();
        }
    }
}
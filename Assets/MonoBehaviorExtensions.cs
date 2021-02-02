using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public static class MonoBehaviourExtensions
{
    private class TaskEnumerator<T> : IEnumerator
    {
        private readonly IEnumerator routine;
        private readonly TaskCompletionSource<T> taskCompletionSource;

        public TaskEnumerator(IEnumerator routine)
        {
            this.routine = routine;
            this.taskCompletionSource = new TaskCompletionSource<T>();
        }

        public Task<T> Task
        {
            get
            {
                return taskCompletionSource.Task;
            }
        }

        public object Current
        {
            get
            {
                return routine.Current;
            }
        }

        public bool MoveNext()
        {
            bool result = routine.MoveNext();

            if (result == false)
            {
                taskCompletionSource.TrySetResult((T)routine.Current);
            }

            return result;
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }

    public static Task<T> StartCoroutine<T>(this MonoBehaviour owner, IEnumerator routine)
    {
        var e = new TaskEnumerator<T>(routine);
        owner.StartCoroutine(e);
        return e.Task;
    }
}

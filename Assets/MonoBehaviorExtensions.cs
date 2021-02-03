using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class MonoBehaviourExtensions
{
    private class Interrupt
    {
        public bool IsInterrupted { get; private set; }
        public void SetInterrupted()
        {
            IsInterrupted = true;
        }
    }

    private static IEnumerator Run<T>(int level, IEnumerator routine, Interrupt interrupt, TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken)
    {
        while (interrupt.IsInterrupted == false)
        {
            object current;

            try
            {
                if (routine.MoveNext() == false)
                {
                    if (level == 0 && interrupt.IsInterrupted == false)
                    {
                        interrupt.SetInterrupted();
                        taskCompletionSource.TrySetResult((T)routine.Current);
                    }

                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    interrupt.SetInterrupted();
                    taskCompletionSource.TrySetCanceled(cancellationToken);
                    break;
                }

                current = routine.Current;

                if (current is IEnumerator)
                {
                    current = Run(level + 1, (IEnumerator)current, interrupt, taskCompletionSource, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                interrupt.SetInterrupted();
                taskCompletionSource.TrySetException(ex);
                break;
            }

            yield return current;
        }
    }

    public static Task<T> StartCoroutine<T>(this MonoBehaviour owner, IEnumerator routine)
    {
        return StartCoroutine<T>(owner, routine, CancellationToken.None);
    }

    public static Task<T> StartCoroutine<T>(this MonoBehaviour owner, IEnumerator routine, CancellationToken cancellationToken)
    {
        try
        {
            var interrupt = new Interrupt();
            var taskCompletionSource = new TaskCompletionSource<T>();

            var e = Run<T>(0, routine, interrupt, taskCompletionSource, cancellationToken);
            owner.StartCoroutine(e);

            return taskCompletionSource.Task;
        }
        catch (Exception ex)
        {
            return Task.FromException<T>(ex);
        }
    }
}

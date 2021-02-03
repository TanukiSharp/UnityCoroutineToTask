using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class SampleBehavior : MonoBehaviour
{
    private IEnumerator Func4(bool throws)
    {
        yield return new WaitForSeconds(1.0f);

        if (throws)
        {
            throw new Exception("Something went horribly wrong!");
        }

        var www = new UnityWebRequest("google.com");
        yield return www.SendWebRequest();
        Debug.LogFormat("HTTP request result: {0}", www.responseCode);
    }

    private IEnumerator Func3(bool throws)
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log("3");
        yield return Func4(throws);
        Debug.Log("4");
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Func2(bool throws)
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log("2");
        yield return Func3(throws);
        Debug.Log("5");
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Func1(bool earlyBreak, bool throws)
    {
        Debug.Log("1");
        yield return Func2(throws);
        if (earlyBreak)
        {
            yield return 32;
            yield break;
        }
        Debug.Log("6");
        yield return new WaitForSeconds(1.0f);
        yield return 51;
    }

    async void Start()
    {
        Debug.Log("--- Before the task await");
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();

            // Uncomment the line below to test cancellation.
            //cancellationTokenSource.CancelAfter(1500);

            // Uncomment the line below to test early breaking a coroutine.
            bool testEarlyBreak = false;

            // Uncomment the line below to test throwing in a nested sub coroutine.
            bool testThrowException = false;

            int result = await this.StartCoroutine<int>(Func1(testEarlyBreak, testThrowException), cancellationTokenSource.Token);

            Debug.LogFormat("--- After the task await, result is {0}", result);
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("--- Exception: {0}", ex);
        }
        finally
        {
            Debug.Log("--- After the task await");
        }
    }
}

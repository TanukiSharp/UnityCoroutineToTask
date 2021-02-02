using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class SampleBehavior : MonoBehaviour
{
    private IEnumerator Func4()
    {
        yield return new WaitForSeconds(1.0f);
        var www = new UnityWebRequest("google.com");
        yield return www.SendWebRequest();
        Debug.LogFormat("HTTP request result: {0}", www.responseCode);
    }

    private IEnumerator Func3()
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log("3");
        yield return Func4();
        Debug.Log("4");
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Func2()
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log("2");
        yield return Func3();
        Debug.Log("5");
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Func1(bool earlyBreak)
    {
        Debug.Log("1");
        yield return Func2();
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
        int result = await this.StartCoroutine<int>(Func1(false));
        Debug.LogFormat("--- After the task await, result is {0}", result);
    }
}

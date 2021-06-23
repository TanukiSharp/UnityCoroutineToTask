# Overview

This is a sample project to demonstrate a way to convert an `UnityEngine.Coroutine` (or more precisely a `System.Collections.IEnumerator`) to a `System.Threading.Tasks.Task<T>`.

Actually, here is the definition of the `Coroutine` type:

```cs
[RequiredByNativeCodeAttribute]
public sealed class Coroutine : YieldInstruction
{
    ~Coroutine();
}
```

Not much we can do with that...<br/>
And here is the `YieldInstruction` for the curious:

```cs
[UsedByNativeCodeAttribute]
public class YieldInstruction
{
    public YieldInstruction();
}
```

Thank you Unity for the millions of possibilities in term of extension points.

## The code

The code you are interested in is located in the file `Assets/MonoBehaviorExtensions.cs`. You need to take the whole `MonoBehaviourExtensions` static class, as the `StartCoroutine<T>` methods requires the `Interrupt` private class and the `Run<T>` private static method.

Feel free to move it into an appropriate namespace.

## How it works

The class `MonoBehaviourExtensions` adds two `StartCoroutine<T>` extension methods on the `MonoBehaviour` class to start a coroutine, but instead of returning a `Coroutine`, they return a `Task<T>` that you can await on.

Here is the prototype of the methods:

```cs
public static Task<T> StartCoroutine<T>(this MonoBehaviour owner, IEnumerator routine);
public static Task<T> StartCoroutine<T>(this MonoBehaviour owner, IEnumerator routine, CancellationToken cancellationToken);
```

The type `T` is the type of value returned by the last `yield return` statement that is executed by the iterator method, represented by `routine`.

It must be provided (the type `T`) to the `StartCoroutine<T>` method because this is a return value, therefore the type cannot be inferred by the compiler.

If your iterator method is defined as follow:

```cs
private IEnumerator MyFunc()
{
    yield return "Hello!";
    yield return new WaitForSeconds(1.0f);
    yield return 51;
}
```

then because the last `yield return` statement executed yields the value `51`, your method is considered returning an `int`.

## How to use

If you are using a too old version of the C# language and the extension methods are not supported, simply remove the `this` keyword in front of `MonoBehavior owner`, then you will call them differently (explained later).

To call them, first get any instance of `MonoBehaviour`, and now there should be two new methods named `StartCoroutine<T>` that take an `IEnumerator` like the Unity one, but that return a `Task<T>`.

Call them like this:

```cs
// Your legacy iterator method.
private IEnumerator MyFunc()
{
    yield return 51;
}

async Task SomeAsyncMethod()
{
    int result = await someMonoBehavior.StartCoroutine<int>(MyFunc());

    // or

    int result = await someMonoBehavior.StartCoroutine<int>(MyFunc(), existingCancellationToken);
}
```

If you needed to remove the `this` keyword because you are using a too old C# language version, call them like this:

```cs
int result = await MonoBehaviourExtensions.StartCoroutine<int>(someMonoBehavior, MyFunc());

// or

int result = await MonoBehaviourExtensions.StartCoroutine<int>(someMonoBehavior, MyFunc(), existingCancellationToken);
```

## Important notes

### Not sure of the return type

In case you are not sure what type your iterator method returns, or if it can return many different types, or if you do not care, you can simply use `object` as `T`, in this case you will never encounter cast exception.

```cs
var didntReadLol = await someMonoBehavior.StartCoroutine<object>(MyFunc());
// or even simply
await someMonoBehavior.StartCoroutine<object>(AnotherFunc());
```

### Early break

In case you need to early break in your method, in regular code, you would do this:

```cs
int MyFunc(bool earlyBreak)
{
    DoSomething();

    if (earlyBreak)
    {
        return 32;
    }

    DoSomethingElse();

    return 51;
}
```

In an iterator method, you early break with a `yield break` statement, but beware of that, since the `StartCoroutine<T>` method will return a task containing the last yielded value, therefore in the following code:

```cs
IEnumerator MyFunc(bool earlyBreak)
{
    DoSomething();

    yield return new WaitForSeconds(1.0f);

    if (earlyBreak)
    {
        yield break; // This is how you interrupt an iterator method.
    }

    DoSomethingElse();

    yield return 51;
}
```

the last yielded value is of type `WaitForSeconds`, and so calling `MyFunc` with `StartCoroutine<int>` will end up in a cast exception (cannot cast `WaitForSeconds` to `int`).

To fix this issue, here is what you have to do then:

```cs
IEnumerator MyFunc(bool earlyBreak)
{
    DoSomething();

    yield return new WaitForSeconds(1.0f);

    if (earlyBreak)
    {
        yield return 32; // Here we explicitly yield 32 before interrupting the iterator method.
        yield break;
    }

    DoSomethingElse();

    yield return 51;
}
```

## Disclaimer

This is probably not the only way to achieve this goal (hiding the old `yield` behind the new `async`/`await`), and there is no pretension in being the best way either.

Here, each call to `StartCoroutine<T>` will incur an allocation of an `Interrupt` class and a `TaskCompletionSource<T>` instances, as well as the `Task<T>` that comes along.

This way however, exceptions thrown in nested sub coroutine is correctly handled, and support for cancellation via `CancellationToken` can be a great win in some situations.

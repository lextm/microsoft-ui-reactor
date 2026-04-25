using System.Diagnostics;

namespace Microsoft.UI.Reactor.Hooks;

/// <summary>
/// Shared loading-state ref-count consumed by the <c>Pending</c> element and populated by
/// <c>UseResource</c> / <c>UseInfiniteResource</c> hooks inside the scope. When the scope
/// observes <b>any</b> registered resource in the <c>Loading</c> state (not <c>Reloading</c>),
/// the owning <c>Pending</c> element renders its fallback instead of the child subtree.
/// </summary>
/// <remarks>
/// <para><b>Semantics.</b> Only <c>Loading</c> triggers the fallback — spec §10.1. A
/// <c>Reloading(previous)</c> is "we already have something to show" and the subtree
/// continues to render normally.</para>
/// <para><b>Threading.</b> UI-thread-affined. The scope captures its owner thread on the
/// first method call (lazy, so the static <c>AppContexts.PendingScope</c> default isn't
/// pinned to whichever thread happened to load the type) and then asserts (DEBUG only)
/// that every subsequent <see cref="Register"/> / <see cref="SetLoading"/> /
/// <see cref="Unregister"/> / <see cref="AnyLoading"/> / <see cref="Count"/> access comes
/// from that same thread. Production callers (hook constructors during render, hook
/// continuations marshalled through <c>IHookDispatcher</c>, hook <c>Dispose</c> during
/// cleanup) all run on the UI thread, so the affinity is a natural fit. Background-thread
/// callers must marshal through the dispatcher first; the no-dispatcher edge case
/// (headless hosts where <c>UseResource</c> applies completions inline on the Task
/// completion thread) violates this and is caught in DEBUG.</para>
/// <para><b>Scope nesting.</b> Each <c>Pending</c> provides a fresh scope to its subtree,
/// so nested <c>Pending</c>s are independent. A hook registers only with its nearest
/// ancestor scope.</para>
/// </remarks>
public sealed class PendingScope
{
    private readonly Dictionary<object, bool> _loadingByToken = new(capacity: 4);
    // Captured on first method call rather than in the constructor. The default
    // PendingScope is created during render (UI thread), but the affinity assertion
    // must tolerate test setups that construct the scope on one thread and use it
    // from another — the *first user* defines the affinity.
    private int _ownerThreadId;

    [Conditional("DEBUG")]
    private void AssertOwnerThread()
    {
        int current = Environment.CurrentManagedThreadId;
        int captured = Interlocked.CompareExchange(ref _ownerThreadId, current, 0);
        if (captured != 0 && captured != current)
            throw new InvalidOperationException(
                $"PendingScope accessed from thread {current}, " +
                $"but it is affined to thread {captured}. Hooks must marshal through " +
                $"IHookDispatcher.Post before touching the scope.");
    }

    /// <summary>Fires when a resource joins, leaves, or changes its loading state.</summary>
    public event Action? Changed;

    /// <summary>
    /// Start tracking <paramref name="token"/> with the given initial <paramref name="isLoading"/>
    /// state. A hook typically uses its own <c>this</c>-equivalent as the token.
    /// </summary>
    public void Register(object token, bool isLoading)
    {
        AssertOwnerThread();
        _loadingByToken[token] = isLoading;
        Changed?.Invoke();
    }

    /// <summary>
    /// Update <paramref name="token"/>'s loading state. Silently ignored if the token
    /// was never registered (defensive — avoids forcing the caller to track whether they
    /// registered).
    /// </summary>
    public void SetLoading(object token, bool isLoading)
    {
        AssertOwnerThread();
        if (!_loadingByToken.TryGetValue(token, out var prev)) return;
        if (prev == isLoading) return;
        _loadingByToken[token] = isLoading;
        Changed?.Invoke();
    }

    /// <summary>Stop tracking <paramref name="token"/>. Idempotent.</summary>
    public void Unregister(object token)
    {
        AssertOwnerThread();
        if (_loadingByToken.Remove(token))
            Changed?.Invoke();
    }

    /// <summary>True iff any tracked token is currently <c>Loading</c>.</summary>
    public bool AnyLoading
    {
        get
        {
            AssertOwnerThread();
            foreach (var v in _loadingByToken.Values) if (v) return true;
            return false;
        }
    }

    /// <summary>Snapshot the number of registered tokens (loading or not). Diagnostic only.</summary>
    public int Count
    {
        get
        {
            AssertOwnerThread();
            return _loadingByToken.Count;
        }
    }
}

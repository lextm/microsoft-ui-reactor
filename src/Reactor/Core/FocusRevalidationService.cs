using System.Diagnostics;

namespace Microsoft.UI.Reactor.Core;

/// <summary>
/// Central service that invalidates stale <see cref="QueryCache"/> entries when the
/// app window is reactivated or resumed, so consumers that opted in via
/// <c>ResourceOptions.RefetchOnWindowFocus = true</c> see fresh data after an
/// Alt-Tab or resume. Disabled by default — see
/// <see cref="ReactorFeatureFlags.FocusRevalidation"/>.
/// </summary>
/// <remarks>
/// <para><b>Per-resource enrollment.</b> Hooks that opt in call <see cref="Enroll"/>
/// with their cache key; the service tracks enrolled keys as a set and invalidates
/// only those whose entry is past <c>StaleTime</c> at activation time. Non-enrolled
/// keys are untouched.</para>
/// <para><b>Throttling.</b> A default 30-second window between activation-driven
/// revalidation sweeps prevents Alt-Tab thrashing from refetching on every
/// transient focus event. Adjustable via <see cref="ThrottleWindow"/>.</para>
/// <para><b>Threading.</b> UI-thread-affined. The service captures the thread it was
/// constructed on and asserts (DEBUG only) that every public method comes from that
/// thread. Production callers (hook lifecycle on render/cleanup, WinUI activation
/// callbacks on the UI thread) satisfy that. Background-thread callers must marshal
/// through the dispatcher first. Invalidation in turn fires
/// <c>QueryCache.EntryChanged</c>, which the <c>UseResource</c> hook listens to and
/// re-renders from.</para>
/// </remarks>
public sealed class FocusRevalidationService
{
    private readonly QueryCache _cache;
    private readonly HashSet<string> _enrolled = new();
    // Captured on first method call rather than in the constructor — the static
    // AppContexts.FocusRevalidation default is constructed at type-init time on whichever
    // thread first touches the type, which is not necessarily the production UI thread.
    private int _ownerThreadId;
    private DateTime _lastSweepUtc = DateTime.MinValue;

    /// <summary>
    /// Minimum time between activation-driven revalidation sweeps. Defaults to 30s.
    /// </summary>
    public TimeSpan ThrottleWindow { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Clock override for deterministic tests.</summary>
    public Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    public FocusRevalidationService(QueryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    [Conditional("DEBUG")]
    private void AssertOwnerThread()
    {
        int current = Environment.CurrentManagedThreadId;
        int captured = Interlocked.CompareExchange(ref _ownerThreadId, current, 0);
        if (captured != 0 && captured != current)
            throw new InvalidOperationException(
                $"FocusRevalidationService accessed from thread {current}, " +
                $"but it is affined to thread {captured}. Marshal through IHookDispatcher.Post first.");
    }

    /// <summary>Diagnostic: current number of keys enrolled for focus revalidation.</summary>
    public int EnrolledCount
    {
        get
        {
            AssertOwnerThread();
            return _enrolled.Count;
        }
    }

    /// <summary>
    /// Enroll <paramref name="key"/> in focus revalidation. Hooks call this when
    /// <c>ResourceOptions.RefetchOnWindowFocus = true</c>. Idempotent — re-enrolling
    /// the same key is a no-op.
    /// </summary>
    public void Enroll(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        AssertOwnerThread();
        _enrolled.Add(key);
    }

    /// <summary>
    /// Remove <paramref name="key"/> from the focus-revalidation set. Hooks call this
    /// on unmount or when their cache key changes.
    /// </summary>
    public void Unenroll(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        AssertOwnerThread();
        _enrolled.Remove(key);
    }

    /// <summary>
    /// Revalidate enrolled entries. Invalidates every enrolled key whose entry is
    /// past its <c>StaleTime</c>. Returns the keys that were invalidated.
    /// </summary>
    /// <remarks>
    /// Short-circuits if the last call fired within <see cref="ThrottleWindow"/> —
    /// returns an empty list without touching the cache.
    /// </remarks>
    public IReadOnlyList<string> RevalidateNow()
    {
        AssertOwnerThread();
        var now = UtcNow();
        if (now - _lastSweepUtc < ThrottleWindow)
            return Array.Empty<string>();
        _lastSweepUtc = now;

        // Snapshot the enrolled set before iteration: Invalidate fires EntryChanged,
        // whose handlers can re-enter the service (e.g. unenroll on unmount during a
        // refetch storm). Mutating _enrolled while iterating it would throw.
        var snapshot = _enrolled.ToArray();

        var invalidated = new List<string>();
        foreach (var key in snapshot)
        {
            if (IsStale(key, now))
            {
                _cache.Invalidate(key);
                invalidated.Add(key);
            }
        }
        return invalidated;
    }

    /// <summary>
    /// Forces a revalidation sweep, bypassing the throttle window. Diagnostic /
    /// test-only — production code paths should go through <see cref="RevalidateNow"/>.
    /// </summary>
    public IReadOnlyList<string> RevalidateNowForce()
    {
        AssertOwnerThread();
        _lastSweepUtc = DateTime.MinValue;
        return RevalidateNow();
    }

    private bool IsStale(string key, DateTime now)
    {
        // We don't have a typed TryGet here. Walk the cache's slot-level snapshot
        // via the non-generic tryGetUnchecked API.
        if (!_cache.TryGetFetchedAt(key, out var fetchedAt, out var staleTime))
            return false;
        return now - fetchedAt >= staleTime;
    }
}

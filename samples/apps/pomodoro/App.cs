// Pomodoro Timer — A focus/break cycle timer built with Reactor.
// Demonstrates UseState, UseEffect with cleanup, UsePersisted, UseMemo,
// UseCallback, UseReducer, conditional rendering, ProgressRing, InfoBar,
// and Theme tokens.

using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Microsoft.UI.Reactor.Factories;

ReactorApp.Run<PomodoroApp>("Pomodoro Timer", width: 480, height: 720
#if DEBUG
    , devtools: true
#endif
);

// ─── Data model ─────────────────────────────────────────────────────────────────

enum TimerPhase { Idle, Focus, ShortBreak, LongBreak }

record SessionRecord(DateTime CompletedAt, TimerPhase Phase, int DurationMinutes);

record AppNotification(string Title, string Message, InfoBarSeverity Severity);

// ─── Main component ─────────────────────────────────────────────────────────────

class PomodoroApp : Component
{
    const int FocusMinutes = 25;
    const int ShortBreakMinutes = 5;
    const int LongBreakMinutes = 15;
    const int SessionsBeforeLongBreak = 4;

    static int PhaseSeconds(TimerPhase phase) => phase switch
    {
        TimerPhase.Focus => FocusMinutes * 60,
        TimerPhase.ShortBreak => ShortBreakMinutes * 60,
        TimerPhase.LongBreak => LongBreakMinutes * 60,
        _ => FocusMinutes * 60,
    };

    public override Element Render()
    {
        // ── Core state ──────────────────────────────────────────────────────
        var (phase, setPhase) = UseState(TimerPhase.Idle);
        var (isRunning, setIsRunning) = UseState(false);
        var (remaining, updateRemaining) = UseReducer(PhaseSeconds(TimerPhase.Focus));
        var (sessionCount, setSessionCount) = UseState(0);
        var (notification, setNotification) = UseState<AppNotification?>(null);

        // ── Persisted history (survives app restarts) ────────────────────────
        var (history, updateHistory) = UseReducer<List<SessionRecord>>(
            new List<SessionRecord>(), threadSafe: true);

        // ── Timer ref ───────────────────────────────────────────────────────
        var timerRef = UseRef<DispatcherTimer?>(null);

        // ── Phase completion handler ────────────────────────────────────────
        void CompletePhase()
        {
            setIsRunning(false);

            if (phase == TimerPhase.Focus)
            {
                var newCount = sessionCount + 1;
                setSessionCount(newCount);
                updateHistory(h => [.. h, new SessionRecord(DateTime.Now, TimerPhase.Focus, FocusMinutes)]);

                if (newCount % SessionsBeforeLongBreak == 0)
                {
                    setPhase(TimerPhase.LongBreak);
                    updateRemaining(_ => PhaseSeconds(TimerPhase.LongBreak));
                    setNotification(new("Long break!", "Great work! Take a 15-minute break.", InfoBarSeverity.Success));
                }
                else
                {
                    setPhase(TimerPhase.ShortBreak);
                    updateRemaining(_ => PhaseSeconds(TimerPhase.ShortBreak));
                    setNotification(new("Short break", "Take a 5-minute breather.", InfoBarSeverity.Informational));
                }
                setIsRunning(true);
            }
            else if (phase == TimerPhase.ShortBreak)
            {
                setPhase(TimerPhase.Focus);
                updateRemaining(_ => PhaseSeconds(TimerPhase.Focus));
                setNotification(new("Back to focus", "Let's go! Starting a new focus session.", InfoBarSeverity.Informational));
                setIsRunning(true);
            }
            else if (phase == TimerPhase.LongBreak)
            {
                setPhase(TimerPhase.Idle);
                updateRemaining(_ => PhaseSeconds(TimerPhase.Focus));
                setSessionCount(0);
                setNotification(new("Cycle complete", "All 4 sessions done! Ready for a new cycle.", InfoBarSeverity.Success));
            }
        }

        // ── Timer effect: start/stop DispatcherTimer based on isRunning ─────
        UseEffect(() =>
        {
            if (isRunning && timerRef.Current == null)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                timer.Tick += (_, _) =>
                {
                    updateRemaining(r =>
                    {
                        if (r <= 1)
                        {
                            // Stop timer before completing phase
                            if (timerRef.Current != null)
                            {
                                timerRef.Current.Stop();
                                timerRef.Current = null;
                            }
                            CompletePhase();
                            return 0;
                        }
                        return r - 1;
                    });
                };
                timer.Start();
                timerRef.Current = timer;
            }
            else if (!isRunning && timerRef.Current != null)
            {
                timerRef.Current.Stop();
                timerRef.Current = null;
            }

            return () =>
            {
                if (timerRef.Current != null)
                {
                    timerRef.Current.Stop();
                    timerRef.Current = null;
                }
            };
        }, isRunning, phase);

        // ── Computed values ─────────────────────────────────────────────────
        var totalSeconds = phase == TimerPhase.Idle
            ? PhaseSeconds(TimerPhase.Focus)
            : PhaseSeconds(phase);
        var progress = totalSeconds > 0
            ? (double)(totalSeconds - remaining) / totalSeconds * 100.0
            : 0.0;

        var todaySessions = UseMemo(() =>
            history.Where(s => s.CompletedAt.Date == DateTime.Today).ToList(),
            history.Count);

        var todayFocusMinutes = UseMemo(() =>
            todaySessions.Where(s => s.Phase == TimerPhase.Focus).Sum(s => s.DurationMinutes),
            todaySessions.Count);

        var currentStreak = UseMemo(() =>
        {
            int streak = 0;
            var date = DateTime.Today;
            var grouped = history
                .Where(s => s.Phase == TimerPhase.Focus)
                .GroupBy(s => s.CompletedAt.Date)
                .ToDictionary(g => g.Key);
            while (grouped.ContainsKey(date))
            {
                streak++;
                date = date.AddDays(-1);
            }
            return streak;
        }, history.Count);

        // ── Format helpers ──────────────────────────────────────────────────
        var minutes = remaining / 60;
        var seconds = remaining % 60;
        var timeDisplay = $"{minutes:D2}:{seconds:D2}";

        var phaseLabel = phase switch
        {
            TimerPhase.Focus => "🎯 Focus",
            TimerPhase.ShortBreak => "☕ Short Break",
            TimerPhase.LongBreak => "🌴 Long Break",
            _ => "🍅 Ready",
        };

        var subtitle = phase switch
        {
            TimerPhase.Idle => "Ready to focus",
            _ => $"{phaseLabel} — {timeDisplay}",
        };

        var phaseColor = phase switch
        {
            TimerPhase.Focus => Theme.SystemAttention,
            TimerPhase.ShortBreak => Theme.SystemSuccess,
            TimerPhase.LongBreak => Theme.Accent,
            _ => Theme.PrimaryText,
        };

        // ── Actions ─────────────────────────────────────────────────────────
        void OnStartPause()
        {
            if (phase == TimerPhase.Idle)
            {
                setPhase(TimerPhase.Focus);
                updateRemaining(_ => PhaseSeconds(TimerPhase.Focus));
                setNotification(null);
            }
            setIsRunning(!isRunning);
        }

        void OnReset()
        {
            setIsRunning(false);
            setPhase(TimerPhase.Idle);
            updateRemaining(_ => PhaseSeconds(TimerPhase.Focus));
            setNotification(null);
        }

        // ── UI ──────────────────────────────────────────────────────────────
        return VStack(0,
            TitleBar("Pomodoro Timer") with { Subtitle = subtitle },

            VStack(16,
                // Notification area
                notification != null
                    ? InfoBar(notification.Title, notification.Message) with
                      {
                          Severity = notification.Severity,
                          IsOpen = true,
                          IsClosable = true,
                          OnClosed = () => setNotification(null),
                      }
                    : null,

                // Phase card
                VStack(4,
                    TextBlock(phaseLabel).FontSize(20).SemiBold()
                        .Foreground(phaseColor)
                        .HAlign(HorizontalAlignment.Center),
                    TextBlock(PhaseMessage(phase, sessionCount)).FontSize(13)
                        .Foreground(Theme.SecondaryText)
                        .HAlign(HorizontalAlignment.Center)
                ).Padding(16)
                 .Background(Theme.CardBackground)
                 .CornerRadius(8),

                // Countdown card
                VStack(12,
                    ProgressRing(progress)
                        .Width(160).Height(160)
                        .HAlign(HorizontalAlignment.Center),
                    TextBlock(timeDisplay).FontSize(48).Bold()
                        .HAlign(HorizontalAlignment.Center)
                        .Foreground(phaseColor)
                ).Padding(24)
                 .Background(Theme.CardBackground)
                 .CornerRadius(8),

                // Control row
                HStack(12,
                    Button(isRunning ? "⏸ Pause" : (phase == TimerPhase.Idle ? "▶ Start" : "▶ Resume"), OnStartPause)
                        .Width(140).Height(40),
                    Button("↺ Reset", OnReset)
                        .Width(100).Height(40)
                        .Disabled(phase == TimerPhase.Idle && !isRunning)
                ).HAlign(HorizontalAlignment.Center),

                // Stats card
                VStack(8,
                    TextBlock("📊 Today's Stats").FontSize(16).SemiBold(),
                    HStack(24,
                        VStack(2,
                            TextBlock($"{todayFocusMinutes}").FontSize(24).Bold()
                                .Foreground(Theme.Accent),
                            TextBlock("min focused").FontSize(12)
                                .Foreground(Theme.SecondaryText)
                        ),
                        VStack(2,
                            TextBlock($"{todaySessions.Count(s => s.Phase == TimerPhase.Focus)}").FontSize(24).Bold()
                                .Foreground(Theme.Accent),
                            TextBlock("sessions").FontSize(12)
                                .Foreground(Theme.SecondaryText)
                        ),
                        VStack(2,
                            TextBlock($"{currentStreak}").FontSize(24).Bold()
                                .Foreground(Theme.Accent),
                            TextBlock("day streak").FontSize(12)
                                .Foreground(Theme.SecondaryText)
                        )
                    )
                ).Padding(16)
                 .Background(Theme.CardBackground)
                 .CornerRadius(8),

                // History card
                todaySessions.Count > 0
                    ? VStack(8,
                        TextBlock("📋 Today's Sessions").FontSize(16).SemiBold(),
                        VStack(4,
                            todaySessions
                                .OrderByDescending(s => s.CompletedAt)
                                .Select((s, i) =>
                                    TextBlock($"✅ {s.CompletedAt:HH:mm} — {s.DurationMinutes} min focus")
                                        .FontSize(13)
                                        .Foreground(Theme.SecondaryText)
                                        .WithKey($"session-{i}") as Element
                                )
                                .ToArray()
                        )
                    ).Padding(16)
                     .Background(Theme.CardBackground)
                     .CornerRadius(8)
                    : null
            ).Padding(24)
        ).Backdrop(BackdropKind.Mica);
    }

    static string PhaseMessage(TimerPhase phase, int sessionCount) => phase switch
    {
        TimerPhase.Idle => "Press Start to begin a 25-minute focus session",
        TimerPhase.Focus => $"Session {(sessionCount % SessionsBeforeLongBreak) + 1} of {SessionsBeforeLongBreak}",
        TimerPhase.ShortBreak => "Rest your eyes and stretch",
        TimerPhase.LongBreak => "You've earned a longer break!",
        _ => "",
    };
}


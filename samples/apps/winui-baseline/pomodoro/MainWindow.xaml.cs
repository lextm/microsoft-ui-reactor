using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Dispatching;

namespace Pomodoro;

public enum TimerPhase
{
    Work,
    ShortBreak,
    LongBreak
}

public sealed partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private TimeSpan _remaining;
    private TimeSpan _totalDuration;
    private bool _isRunning;
    private TimerPhase _currentPhase = TimerPhase.Work;
    private int _completedPomodoros;
    private int _workSessionsBeforeLongBreak;

    // Durations in minutes
    private int _workMinutes = 25;
    private int _shortBreakMinutes = 5;
    private int _longBreakMinutes = 15;

    // Notification flash state
    private DispatcherTimer? _flashTimer;
    private int _flashCount;

    public MainWindow()
    {
        this.InitializeComponent();
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(450, 700));

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;

        ResetToPhase(_currentPhase);
    }

    private void Timer_Tick(object? sender, object e)
    {
        _remaining -= TimeSpan.FromSeconds(1);

        if (_remaining <= TimeSpan.Zero)
        {
            _remaining = TimeSpan.Zero;
            _timer.Stop();
            _isRunning = false;
            StartPauseButton.Content = "Start";
            OnPhaseCompleted();
        }

        UpdateDisplay();
    }

    private void OnPhaseCompleted()
    {
        // Flash notification
        FlashNotification();

        if (_currentPhase == TimerPhase.Work)
        {
            _completedPomodoros++;
            _workSessionsBeforeLongBreak++;

            if (_workSessionsBeforeLongBreak >= 4)
            {
                _workSessionsBeforeLongBreak = 0;
                SwitchToPhase(TimerPhase.LongBreak);
            }
            else
            {
                SwitchToPhase(TimerPhase.ShortBreak);
            }
        }
        else
        {
            SwitchToPhase(TimerPhase.Work);
        }

        if (AutoStartToggle.IsOn)
        {
            StartTimer();
        }
    }

    private void SwitchToPhase(TimerPhase phase)
    {
        _currentPhase = phase;
        ResetToPhase(phase);
    }

    private void ResetToPhase(TimerPhase phase)
    {
        int minutes = phase switch
        {
            TimerPhase.Work => _workMinutes,
            TimerPhase.ShortBreak => _shortBreakMinutes,
            TimerPhase.LongBreak => _longBreakMinutes,
            _ => _workMinutes
        };

        _totalDuration = TimeSpan.FromMinutes(minutes);
        _remaining = _totalDuration;
        UpdateDisplay();
        UpdatePhaseVisuals();
    }

    private void UpdateDisplay()
    {
        TimerText.Text = $"{(int)_remaining.TotalMinutes:D2}:{_remaining.Seconds:D2}";
        SessionText.Text = $"Pomodoros completed: {_completedPomodoros}";

        // Update progress ring (0-100)
        double elapsed = (_totalDuration - _remaining).TotalSeconds;
        double total = _totalDuration.TotalSeconds;
        ProgressRing.Value = total > 0 ? (elapsed / total) * 100.0 : 0;
    }

    private void UpdatePhaseVisuals()
    {
        var (text, color) = _currentPhase switch
        {
            TimerPhase.Work => ("Work", Colors.Tomato),
            TimerPhase.ShortBreak => ("Short Break", Colors.MediumSeaGreen),
            TimerPhase.LongBreak => ("Long Break", Colors.CornflowerBlue),
            _ => ("Work", Colors.Tomato)
        };

        PhaseText.Text = text;
        PhaseText.Foreground = new SolidColorBrush(color);
        ProgressRing.Foreground = new SolidColorBrush(color);
    }

    private void FlashNotification()
    {
        _flashCount = 0;
        _flashTimer = new DispatcherTimer();
        _flashTimer.Interval = TimeSpan.FromMilliseconds(300);
        _flashTimer.Tick += (s, e) =>
        {
            _flashCount++;
            if (_flashCount > 6)
            {
                _flashTimer.Stop();
                RootGrid.Background = new SolidColorBrush(Colors.Transparent);
                return;
            }

            var flashColor = _currentPhase switch
            {
                TimerPhase.Work => Colors.MediumSeaGreen,
                TimerPhase.ShortBreak => Colors.Tomato,
                TimerPhase.LongBreak => Colors.Tomato,
                _ => Colors.Yellow
            };

            RootGrid.Background = _flashCount % 2 == 0
                ? new SolidColorBrush(Colors.Transparent)
                : new SolidColorBrush(flashColor) { Opacity = 0.3 };
        };
        _flashTimer.Start();
    }

    private void StartTimer()
    {
        _isRunning = true;
        StartPauseButton.Content = "Pause";
        _timer.Start();
    }

    private void StartPause_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
            StartPauseButton.Content = "Start";
        }
        else
        {
            if (_remaining <= TimeSpan.Zero)
            {
                ResetToPhase(_currentPhase);
            }
            StartTimer();
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _isRunning = false;
        StartPauseButton.Content = "Start";
        _completedPomodoros = 0;
        _workSessionsBeforeLongBreak = 0;
        _currentPhase = TimerPhase.Work;
        ResetToPhase(TimerPhase.Work);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        SettingsPanel.Visibility = SettingsPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void WorkSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        _workMinutes = (int)e.NewValue;
        if (_currentPhase == TimerPhase.Work && !_isRunning)
        {
            ResetToPhase(TimerPhase.Work);
        }
    }

    private void ShortBreakSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        _shortBreakMinutes = (int)e.NewValue;
        if (_currentPhase == TimerPhase.ShortBreak && !_isRunning)
        {
            ResetToPhase(TimerPhase.ShortBreak);
        }
    }

    private void LongBreakSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        _longBreakMinutes = (int)e.NewValue;
        if (_currentPhase == TimerPhase.LongBreak && !_isRunning)
        {
            ResetToPhase(TimerPhase.LongBreak);
        }
    }
}

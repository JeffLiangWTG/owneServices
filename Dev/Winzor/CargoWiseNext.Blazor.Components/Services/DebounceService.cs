namespace CargoWiseNext.Blazor.Components;

public class DebounceService : IDisposable
{
	readonly Timer _timer;
	readonly int _delay;
	Action? _actionToDebounce;

	public DebounceService(int delay)
	{
		_delay = delay;
		_timer = new Timer(_ => _actionToDebounce?.Invoke(), null, Timeout.Infinite, Timeout.Infinite);
	}

	public void Debounce(Action action)
	{
		_actionToDebounce = action;
		_timer.Change(_delay, Timeout.Infinite);
	}

	public void Dispose()
	{
		_timer?.Dispose();
	}
}

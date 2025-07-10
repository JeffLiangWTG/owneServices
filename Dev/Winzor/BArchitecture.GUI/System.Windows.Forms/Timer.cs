using System.ComponentModel;
using WinzorFramework;
using ThreadingTimer = System.Threading.Timer;

namespace System.Windows.Forms;

public partial class Timer : IDisposable, IComponent
{
	public Timer()
	{
	}

	public Timer(IContainer container)
	{
		container.Add(this);
	}

	public int Interval
	{
		get => interval;
		set
		{
			if (interval != value)
			{
				interval = value;
				if (Enabled)
				{
					Stop();
					Start();
				}
			}
		}
	}
	int interval = 100;

	public virtual bool Enabled
	{
		get => threadingTimer != null;
		set
		{
			if (value && threadingTimer == null)
			{
				Start();
			}
			else if (!value && threadingTimer != null)
			{
				Stop();
			}
		}
	}

	public object? Tag { get; set; }

	public event EventHandler? Tick;

	public void Start()
	{
		dispatcher = WinzorDispatcher.Current;
		if (threadingTimer == null)
		{
			StartThreadingTimer();
		}
	}

	protected virtual void OnTick(EventArgs e) => Tick?.Invoke(this, e);

	void StartThreadingTimer()
	{
		threadingTimer = new ThreadingTimer(ThreadingTimerCallback, null, Interval, Timeout.Infinite);
	}

	void ThreadingTimerCallback(object? o)
	{
		var t = threadingTimer;
		if (t != null)
		{
			t.Dispose();
			dispatcher?.Queue(() =>
			{
				var context = new ServerInitiatedCallbackContext();
				using (dispatcher.WithContext(context))
				{
					try
					{
						OnTick(EventArgs.Empty);
					}
					catch (Exception ex)
					{
						Application.OnThreadException(ex);
					}
				}

				if (threadingTimer is not null)
				{
					StartThreadingTimer();
				}
			});
		}
	}

	public void Stop()
	{
		threadingTimer?.Dispose();
		threadingTimer = null;
	}

	public void Dispose()
	{
		Stop();
		Tick = null;
		Disposed?.Invoke(this, EventArgs.Empty);
	}

	public event EventHandler? Disposed;

	ISite? IComponent.Site { get; set; }

	ThreadingTimer? threadingTimer;
	WinzorDispatcher? dispatcher;
}

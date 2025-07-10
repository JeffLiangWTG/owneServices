using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace WinzorFramework;

public class WinzorDispatcher : IDisposable
{
	/// <summary>
	/// The Winzor dispatcher is a reimplementaiton of the WinForms event pump
	/// It maintains a queue of messages which it processes in the order they were added
	/// </summary>
	/// <param name="formOpener"></param>
	/// <param name="formInstanceRegister"></param>
	/// <param name="threadName"></param>
	/// <param name="isBackgroundThread"></param>
	/// <param name="setSynchronizationContext"></param>
	[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Waiting for initial creation of dispatcher thread")]
	public WinzorDispatcher(IFormOpener formOpener, IFormInstanceRegister formInstanceRegister, string threadName = "WinzorDispatcher", bool isBackgroundThread = false, bool setSynchronizationContext = true)
	{
		FormOpener = formOpener;
		FormInstanceRegister = formInstanceRegister;

		thread = new Thread(Run) { Name = threadName };
		thread.SetApartmentState(ApartmentState.STA);
		if (isBackgroundThread)
		{
			thread.IsBackground = true;
		}
		thread.Start();

		if (setSynchronizationContext)
		{
			InvokeAsync(InitializeSynchronizationContext).GetAwaiter().GetResult();
		}
	}

	void InitializeSynchronizationContext()
	{
		SynchronizationContext.SetSynchronizationContext(new WinzorDispatcherSynchronizationContext(this));
	}

	public Task InvokeAsync(Action action)
	{
		try
		{
			var message = new Message(action);
			messageQueue.Add(message);
			return message.Task;
		}
		catch (ObjectDisposedException)
		{
			// WinzorDispatcher has been disposed - we are unable to run any more actions at this point
			return Task.CompletedTask;
		}
	}

	public void Queue(Action action)
	{
		try
		{
			var message = new Message(action);
			messageQueue.Add(message);
		}
		catch (ObjectDisposedException)
		{
		}
	}

	void Run()
	{
		instances.Value = this;
		contextStack = new Stack<IWinzorDispatcherContext>();

		try
		{
			RunMessageLoop(topLevelCancellationTokenSource.Token, isTopLevel: true, null);
		}
		finally
		{
			instances.Value = null;
			contextStack = null;
		}
	}

	public void RunMessageLoop(CancellationTokenSource cancellationTokenSource, Form? contextForm = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
	{
		if (IsDisposed)
		{
			return;
		}

		if (HasCurrentContext)
		{
			CurrentContext.OnEnterMessageLoop();
		}

		innerMessageLoops.Push(new InnerMessageLoop(callerMemberName, callerFilePath, callerLineNumber, cancellationTokenSource));
		try
		{
			RunMessageLoop(cancellationTokenSource.Token, isTopLevel: false, contextForm);
		}
		finally
		{
			innerMessageLoops.Pop();
		}
	}

	void RunMessageLoop(CancellationToken cancellationToken, bool isTopLevel, Form? contextForm)
	{
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var message = messageQueue.Take(cancellationToken);
				Execute(message);
				contextForm?.CheckCloseDialog(false);
				if (messageQueue.Count == 0)
				{
					OnIdle();
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (ObjectDisposedException)
		{
		}

		if (isTopLevel)
		{
			Application.OnThreadExit();
		}
	}

	void Execute(Message message)
	{
		try
		{
			message.Action();
			message.TaskCompletionSource.SetResult();
		}
		catch (Exception ex)
		{
			message.TaskCompletionSource.SetException(ex);
		}
	}

	public void DoEvents()
	{
		if (!IsCurrent)
		{
			throw new InvalidOperationException("DoEvents can only be called on WinzorDispatcher thread");
		}

		while (messageQueue.Count > 0)
		{
			var message = messageQueue.Take();
			Execute(message);
		}
	}

	public IFormInstanceRegister FormInstanceRegister { get; }

	public IFormOpener FormOpener { get; }

	public static WinzorDispatcher Current
	{
		get
		{
			if (!instances.IsValueCreated || instances.Value == null)
			{
				throw new InvalidOperationException("The current thread is not a WinzorDispatcher");
			}
			return instances.Value;
		}
	}

	public static bool IsCurrent => instances.IsValueCreated && instances.Value != null;

	static readonly ThreadLocal<WinzorDispatcher?> instances = new();

	public int ManagedThreadId => thread.ManagedThreadId;

	internal event EventHandler? Idle;

	void OnIdle()
	{
		if (Idle != null)
		{
			var context = new ServerInitiatedCallbackContext();
			using (WithContext(context))
			{
				try
				{
					Idle.Invoke(null, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Application.OnThreadException(ex);
				}
			}
		}
	}

	public bool IsDisposed { get; private set; }

	public void RegisterDisposeAction(Action action) => disposeActions.Add(action);

	readonly List<Action> disposeActions = new();

	internal TimeSpan ShutdownTimeLimit { get; set; } = TimeSpan.FromMinutes(1);

	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}

		IsDisposed = true;

		if (innerMessageLoops.Count > 0)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			while (innerMessageLoops.Count > 0 && stopWatch.Elapsed.TotalSeconds < 5.0)
			{
				Thread.Sleep(10);
			}
			stopWatch.Stop();
		}

		var runningInnerMessageLoop = false;
		var callerMemberInfo = new StringBuilder();
		var loops = innerMessageLoops.ToList();
		foreach (var innerMessageLoop in loops)
		{
			try
			{
				if (!innerMessageLoop.CancellationTokenSource.IsCancellationRequested)
				{
					callerMemberInfo.AppendLine(innerMessageLoop.ToString());
					innerMessageLoop.CancellationTokenSource.Cancel();
					runningInnerMessageLoop = true;
				}
			}
			catch (ObjectDisposedException)
			{
			}
		}

		topLevelCancellationTokenSource.Cancel();
		var dispatcherDidShutdown = thread.Join(ShutdownTimeLimit);
		topLevelCancellationTokenSource.Dispose();
		messageQueue.Dispose();

		foreach (var action in disposeActions)
		{
			action();
		}

		if (!dispatcherDidShutdown)
		{
			throw new TimeoutException("The message loop did not shutdown within 1 minute. The Application.OnThreadExit event may not have been raised.");
		}
		if (runningInnerMessageLoop)
		{
			throw new InvalidOperationException($"Attempted to dispose a WinzorDispatcher running an inner message loop. [InnerMessageLoops]: {callerMemberInfo}");
		}
	}

	readonly Thread thread;
	readonly BlockingCollection<Message> messageQueue = new();
	readonly CancellationTokenSource topLevelCancellationTokenSource = new();
	readonly Stack<InnerMessageLoop> innerMessageLoops = new();

	class InnerMessageLoop
	{
		public InnerMessageLoop(string callerMemberName, string callerFilePath, int callerLineNumber, CancellationTokenSource cancellationTokenSource)
		{
			CallerMemberName = callerMemberName;
			CallerFilePath = callerFilePath;
			CallerLineNumber = callerLineNumber;
			CancellationTokenSource = cancellationTokenSource;
		}

		public string CallerMemberName { get; init; }

		public string CallerFilePath { get; init; }

		public int CallerLineNumber { get; init; }

		public CancellationTokenSource CancellationTokenSource { get; init; }

		public override string ToString() => $"{CallerFilePath}:{CallerLineNumber} in {CallerMemberName}";
	}

	class Message
	{
		public Message(Action action) => Action = action;

		public Action Action { get; }

		public TaskCompletionSource TaskCompletionSource { get; } = new TaskCompletionSource();

		public Task Task => TaskCompletionSource.Task;
	}

	#region ContextStack

	static Stack<IWinzorDispatcherContext> ContextStack
	{
		get
		{
			if (contextStack == null)
			{
				throw new InvalidOperationException("ContextStack is bound only to a WinzorDispatcher thread.");
			}
			return contextStack;
		}
	}

	[ThreadStatic]
	static Stack<IWinzorDispatcherContext>? contextStack;

	public IWinzorDispatcherContext CurrentContext
	{
		get
		{
			if (ContextStack.Count == 0)
			{
				throw new InvalidOperationException("There is no current control context registered on the WinzorDispatcher");
			}
			return ContextStack.Peek();
		}
	}

	public static bool HasCurrentContext => ContextStack.Count > 0;

	public IDisposable WithContext(IWinzorDispatcherContext context)
	{
		ContextStack.Push(context);
		return new WithContextChange();
	}

	class WithContextChange : IDisposable
	{
		public void Dispose()
		{
			ContextStack.Pop();
		}
	}

	#endregion

	//Used for UserIdleWorker/UserIdleDetecter (when mouse/keyboard inputs are detected).
	public void OnUserActivity() => UserActivity?.Invoke(null, EventArgs.Empty);
	public EventHandler? UserActivity;

	public bool IsSameAs(WinzorDispatcher? winzorDispatcher) => this.ManagedThreadId == winzorDispatcher?.ManagedThreadId;
}

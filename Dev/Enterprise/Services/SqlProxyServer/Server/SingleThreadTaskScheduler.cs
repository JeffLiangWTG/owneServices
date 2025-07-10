using System.Collections.Concurrent;

namespace CargoWise.Data.SqlProxyServer;

public sealed class SingleThreadTaskScheduler : TaskScheduler, IDisposable
{
	public SingleThreadTaskScheduler()
	{
		mainThread = new Thread(ExecuteTasks) { IsBackground = true };
		mainThread.Start();
	}

	void ExecuteTasks()
	{
		foreach (var task in tasks.GetConsumingEnumerable())
		{
			TryExecuteTask(task);
		}
	}

	protected override void QueueTask(Task task) => tasks.Add(task);

	protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		=> Thread.CurrentThread == mainThread && TryExecuteTask(task);

	protected override IEnumerable<Task> GetScheduledTasks() => tasks.ToArray();

	public void Dispose() => tasks.CompleteAdding();

	readonly BlockingCollection<Task> tasks = new();
	readonly Thread mainThread;
}

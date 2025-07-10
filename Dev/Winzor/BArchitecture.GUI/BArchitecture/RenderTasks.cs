namespace WinzorFramework;

public class RenderTasks : WrappedList<Task>
{
	public Task WaitAllAsync()
	{
		var tasks = GetSnapshot();
		Clear();
		return Task.WhenAll(tasks);
	}
}

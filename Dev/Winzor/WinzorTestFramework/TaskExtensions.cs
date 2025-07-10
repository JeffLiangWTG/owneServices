using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace WinzorTestFramework;

public static class TaskExtensions
{
	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks")]
	public static async Task<bool> WithTimeout(this Task task, TimeSpan timeout)
	{
		var result = await Task.WhenAny(task, Task.Delay(timeout)) == task;
		if (result)
		{
			await task;
		}
		return result;
	}
}

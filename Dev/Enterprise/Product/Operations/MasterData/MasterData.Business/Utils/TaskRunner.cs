using System;
using System.Threading.Tasks;

namespace Enterprise.MasterData.Business
{
	public static class TaskRunner
	{
		public static async Task<T> WithExceptionHandler<T>(this Task<T> task, T defaultResult, Action<Exception> exceptionHandler)
		{
			var completedTask = await task.ContinueWith(t => t, TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(continueOnCapturedContext: false);

			if (completedTask.Status is TaskStatus.Faulted)
			{
				exceptionHandler.Invoke(completedTask.Exception.InnerException);
			}

			return completedTask.Status is TaskStatus.RanToCompletion ? await completedTask : defaultResult;
		}
	}
}

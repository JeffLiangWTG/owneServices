using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.GUI
{
	public class LoadingFormModel<T> : ModelBase<LoadingFormModel<T>> where T : class
	{
		public event RunWorkerCompletedEventHandler RunWorkerCompleted;

		public LoadingFormModel(string title, Task<T> formTask)
		{
			Title = title;
			LoadingTask = formTask;
		}

		public string Title { get; set; }

		public string LoadingContent => ResourceStringHelper.LoadingContent;

		public LoadingWindowResult<T> LoadingResult { get; set; }

		Task<T> LoadingTask { get; }

		bool isRunning;

		public async Task StartRunning()
		{
			if (isRunning)
			{
				return;
			}

			isRunning = true;

			LoadingResult = new LoadingWindowResult<T>();

			try
			{
				LoadingTask.Start();
				LoadingResult.Result = await LoadingTask;
			}
			catch (Exception ex)
			{
				LoadingResult.Exception = ex;
			}

			RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(LoadingResult, LoadingResult.Exception, false));
		}
	}

	public class LoadingWindowResult<T>
	{
		public T Result { get; set; }
		public Exception Exception { get; set; }
	}
}

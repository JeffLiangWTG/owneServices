using System;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationTimeout<T>
	{
		public delegate T WorkDelegate(Func<T> action);

		public DeduplicationTimeout(TimeSpan timeOut)
		{
			Timeout = timeOut;
		}

		readonly TimeSpan Timeout;
		protected Task<T> AsyncResult;
		protected WorkDelegate WorkFunction;

		public T DoWork(Func<T> action)
		{
			T result;
			WorkFunction = new WorkDelegate(DoWorkHandler);
			AsyncResult = Task.Factory.StartNew(() => WorkFunction.Invoke(action), TaskCreationOptions.AttachedToParent);

#if DEBUG
			TaskRegistryForTest.RegisterTask(AsyncResult, (NoResString)"Duplication => FindDuplication");
#endif

			var completed = AsyncResult.Wait(Timeout);
			if (!completed)
			{
				throw new TimeoutException();
			}
			else
			{
				result = AsyncResult.Result;
			}

			return result;
		}

		T DoWorkHandler(Func<T> action)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return action();
			}
		}
	}
}

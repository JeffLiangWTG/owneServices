using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	public class TaskRunnerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestTaskRunner()
		{
			var task = Task.Factory.StartNew(() => 100);
			var completedTask = task.WithExceptionHandler(defaultResult: -1, ex => throw new NotImplementedException("Should not throw here!", ex));

			var taskResult = AsyncTaskSynchronizer.Run(async () => await completedTask);

			AssertEquals(100, taskResult);
		}

		[ExpectNoExceptions]
		public void TestTaskRunnerHandleFaulted()
		{
			var task = Task.Factory.StartNew(new Func<int>(() => throw new NotImplementedException()));

			Exception handledException = null;
			var completedTask = task.WithExceptionHandler(defaultResult: 50, ex => handledException = ex);

			var taskResult = AsyncTaskSynchronizer.Run(async () => await completedTask);

			AssertEquals(50, taskResult);
			AssertNotNull(handledException);
			AssertType<NotImplementedException>(handledException);
		}

		[ExpectNoExceptions]
		public void TestTaskRunnerReturnDefaultValue_TaskCancelled()
		{
			var task = Task.Factory.StartNew(new Func<int>(() => 1), new CancellationToken(canceled: true));
			var completedTask = task.WithExceptionHandler(defaultResult: 2, ex => throw new NotImplementedException("Should not throw here!", ex));

			var taskResult = AsyncTaskSynchronizer.Run(async () => await completedTask);

			AssertEquals(2, taskResult);
		}
	}
}

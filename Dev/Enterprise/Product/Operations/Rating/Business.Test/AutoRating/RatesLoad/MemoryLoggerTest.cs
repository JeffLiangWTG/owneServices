using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Rating.Business.RateSelector;

namespace Enterprise.Rating.Business.Test.AutoRating.RatesLoad
{
	public class MemoryLoggerTest : TestCaseWithFactory
	{
		public MemoryLogger MemoryLogger { get; set; }

		public void TestConcurrent_LogAndClear_ShouldMaintainConsistency()
		{
			var logType = LogType.Warning;
			var message = "Concurrent test log message";
			var tasks = new List<Task>();

			for (var i = 0; i < 10000; i++)
			{
				tasks.Add(Task.Run(() => MemoryLogger.Log(logType, $"{message} {i}")));
				tasks.Add(Task.Run(() => MemoryLogger.Clear()));
			}

			AssertNoExceptionThrown(() => Task.WaitAll(tasks.ToArray()));
		}

		protected override void SetUp()
		{
			MemoryLogger = new MemoryLogger();
			MemoryLogger.LogsChanged += Logger_LogsChanged;
		}

		void Logger_LogsChanged(object sender, MemoryLogger.LogEventArgs e)
		{
		}
	}
}

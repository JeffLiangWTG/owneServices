using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business.ServiceTasks
{
	public class DataConsistencyCheckRunner
	{
		public DataConsistencyCheckRunner(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;

			Checkers = new List<DataConsistencyChecker>();
			Checkers.Add(new OrphanWIPorACRChecker());
		}

		protected List<DataConsistencyChecker> Checkers;
		protected ILogger ServiceLogger;

#if DEBUG
		public void PerformChecks()
		{
			PerformChecks(CancellationToken.None);
		}
#endif

		public void PerformChecks(CancellationToken token)
		{
			foreach (var checker in Checkers)
			{
				token.ThrowIfCancellationRequested();
				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Data Consistency Check - {0} - started.", checker.Description));
				checker.Check(ServiceLogger);
				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Data Consistency Check - {0} - finished.", checker.Description));
			}
		}
	}
}

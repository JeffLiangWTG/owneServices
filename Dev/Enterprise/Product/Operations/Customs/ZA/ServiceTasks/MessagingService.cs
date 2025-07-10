using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.ZA.ServiceTasks
{
	abstract class MessagingService : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = "ZAC";

		protected LoggingInformation GetNewLogger()
		{
			LoggingInformation result = new LoggingInformation();
			result.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(Logger_OnLogInfoAdded);
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}
	}
}

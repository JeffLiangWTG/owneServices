using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2
{
	class ServiceTaskLoggingInformation : LoggingInformation
	{
		public ServiceTaskLoggingInformation(ILogger serviceTaskLogger)
			: base()
		{
			this.serviceTaskLogger = serviceTaskLogger;
			this.OnLogInfoAdded += new LogInfoAdded(TransferLog);
		}
		readonly ILogger serviceTaskLogger;

		void TransferLog(string log, LogType logType)
		{
			this.serviceTaskLogger.Log(logType, log);
		}
	}
}

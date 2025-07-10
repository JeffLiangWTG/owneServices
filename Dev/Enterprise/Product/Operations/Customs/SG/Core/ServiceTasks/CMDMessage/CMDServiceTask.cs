using Enterprise.Integration;

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	abstract class CMDServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected void Log(string log, LogType logType) => ServiceLogger.Log(logType, log);
	}
}

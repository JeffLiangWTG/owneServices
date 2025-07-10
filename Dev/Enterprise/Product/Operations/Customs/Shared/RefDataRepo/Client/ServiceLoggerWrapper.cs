using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class ServiceLoggerWrapper : RefDbRepo.Client.Common.ILogger
	{
		public ServiceLoggerWrapper(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		readonly ILogger serviceLogger;

		public void Write(string text)
		{
			serviceLogger.Log(LogType.Information, text);
		}

		public void WriteLine(string text)
		{
			serviceLogger.Log(LogType.Information, text);
		}

		public void WriteError(string errorMessage)
		{
			serviceLogger.Log(LogType.Error, errorMessage);
		}

		public void WriteError(string errorMessage, System.Exception exception)
		{
			serviceLogger.Log(LogType.Error, errorMessage, exception);
		}
	}
}

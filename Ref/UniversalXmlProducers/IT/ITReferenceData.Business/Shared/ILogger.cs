using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public interface ILogger
	{
		void Log(string logMessage);

		void Log(Exception exception, string logMessage);

		void Log(Exception exception);
	}
}

using System;
using Enterprise.Integration;

namespace Enterprise.Customs.Business.Logging
{
	public interface ICommonLogger
	{
		void Log(LogType logLevel, string message);
		void Log(LogType logLevel, string message, Exception ex);
		void LogFormat(LogType logLevel, string format, params object[] args);
		void BumpSectionProgress();
		void SetSectionProgressMax(int max);
	}
}

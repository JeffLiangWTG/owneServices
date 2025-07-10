using System;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.GUI
{
	public class RefDataFormLogger : ILogger
	{
		public Action<string> OnLog;

		public void Log(LogType type, string message)
		{
			OnLog(message);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			OnLog(message + "\r\n" + ex.Message);
		}
	}
}

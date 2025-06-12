using System;
using System.Diagnostics;

namespace CargoWise.eServices.eHub.Common
{
	[Serializable]
	public class LogHelper
	{
		public void LogInfo(string message)
		{
			EventLog.WriteEntry(eHubApplication, message, EventLogEntryType.Information);
		}

		public void LogError(string message)
		{
			EventLog.WriteEntry(eHubApplication, message, EventLogEntryType.Error);
		}

		public void LogWarning(string message)
		{
			EventLog.WriteEntry(eHubApplication, message, EventLogEntryType.Warning);
		}

		const string eHubApplication = "CargoWise eHub";
	}
}

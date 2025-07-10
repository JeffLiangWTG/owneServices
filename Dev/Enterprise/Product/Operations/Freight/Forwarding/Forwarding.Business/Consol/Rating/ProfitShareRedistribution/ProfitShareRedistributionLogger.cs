using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ProfitShareRedistributionLogger : IDisposableProfitShareRedistributionLogger
	{
		public void Dispose()
		{
			logs = null;
		}

		public void Log(LogType type, string message)
		{
			logs.Add((type, message));
			OnLogging?.Invoke(this, new ProfitShareRedistributeLoggingEventArgs(type, message));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			message += "|" + ex.ToString();
			Log(type, message);
		}

		protected List<(LogType Type, string Message)> logs = new List<(LogType Type, string Message)>();
		public event EventHandler<ProfitShareRedistributedEventArgs> IndividualCompleted;
		public event EventHandler<EventArgs> AllCompleted;
		public event EventHandler<ProfitShareRedistributeLoggingEventArgs> OnLogging;

		public void OnIndividualCompleted(ProfitShareRedistributedEventArgs eventArgs) => IndividualCompleted?.Invoke(this, eventArgs);
		public void OnAllCompleted(EventArgs eventArgs) => AllCompleted?.Invoke(this, eventArgs);

		public string DumpLogs()
		{
			return logs.Select(x => x.Message).ToStringWithNewLineBetweenStrings();
		}
	}

	public class ProfitShareRedistributeLoggingEventArgs : EventArgs
	{
		public ProfitShareRedistributeLoggingEventArgs(LogType type, string message)
		{
			Type = type;
			Message = message;
		}

		public LogType Type;
		public string Message;
	}
}

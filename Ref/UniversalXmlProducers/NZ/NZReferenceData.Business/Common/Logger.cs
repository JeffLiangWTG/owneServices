using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public interface ILogger
	{
		public IEnumerable<(DateTime Time, string Kind, string Mesage)> Logs { get; }

		void LogInfo(string message);

		void LogError(string message);
	}

	public class Logger : ILogger
	{
		List<(DateTime, string, string)> LogsList { get; } = [];

		const string LogKindInfo = "Info";

		const string LogKindError = "Error";

		void AddLog(string logKind, string message)
		{
			LogsList.Add((DateTime.Now, logKind, message));
		}

		#region Implementation

		public IEnumerable<(DateTime, string, string)> Logs => LogsList;

		public void LogInfo(string message)
		{
			Console.WriteLine(message);
			AddLog(LogKindInfo, message);
		}

		public void LogError(string message)
		{
			Console.Error.WriteLine(message);
			AddLog(LogKindError, message);
		}

		#endregion
	}
}

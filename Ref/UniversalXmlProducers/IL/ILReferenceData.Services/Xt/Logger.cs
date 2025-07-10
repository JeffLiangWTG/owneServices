using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public class Logger : ILogger
	{
		public List<(LogType Type, string Message)> AllLogs => loggedContents;
		public List<string> InfoLogs => loggedContents.Where(l => l.Type == LogType.Information).Select(l => l.Message).ToList();
		public List<string> DebugLogs => loggedContents.Where(l => l.Type == LogType.Debug).Select(l => l.Message).ToList();
		public List<string> ErrorLogs => loggedContents.Where(l => l.Type == LogType.Error).Select(l => l.Message).ToList();
		public List<string> WarningLogs => loggedContents.Where(l => l.Type == LogType.Warning).Select(l => l.Message).ToList();

		static string LogFormat => $"[{{0}} {DateTime.Now:yy-MM-dd HH:mm:ss}]:";

		public void Log(LogType type, string message)
		{
			string formattedMessage = string.Format(CultureInfo.InvariantCulture, LogFormat, type) + message;
			loggedContents.Add((type, formattedMessage));

			if (type == LogType.Error)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine(formattedMessage);
				Console.ResetColor();
			}
			else
			{
				Console.WriteLine(formattedMessage);
			}
		}

		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, $"{message}:{ex.Message}");
		}

		readonly List<(LogType Type, string Message)> loggedContents = new List<(LogType Type, string Message)>();
	}
}

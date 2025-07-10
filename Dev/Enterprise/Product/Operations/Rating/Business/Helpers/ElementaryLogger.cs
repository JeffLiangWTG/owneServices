using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Rating.Business
{
	public interface ILoggerExtended : ILogger
	{
		IEnumerable<string> GetAllLogs();
		IEnumerable<string> GetErrorsAndWarnings();
	}

	public class ElementaryLogger : ILoggerExtended
	{
		public void Log(LogType type, string message) => Log(type, message, null);

		public void Log(LogType type, string message, Exception ex) => LogCore(type, message, ex);

		protected virtual void LogCore(LogType type, string message, Exception ex)
		{
			switch (type)
			{
				case LogType.Error:
					Errors.Add(message);
					break;
				case LogType.Warning:
					Warnings.Add(message);
					break;
				case LogType.Information:
					Infos.Add(message);
					break;
				case LogType.Debug:
					Debugs.Add(message);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public void ClearLogs()
		{
			Debugs.Clear();
			Infos.Clear();
			Warnings.Clear();
			Errors.Clear();
		}

		public readonly List<string> Debugs = new List<string>();
		public readonly List<string> Infos = new List<string>();
		public readonly List<string> Warnings = new List<string>();
		public readonly List<string> Errors = new List<string>();

		public IEnumerable<string> GetAllLogs()
		{
			var result = new List<string>();

			result.AddRange(Errors);
			result.AddRange(Warnings);
			result.AddRange(Infos);
			result.AddRange(Debugs);

			return result;
		}

		public IEnumerable<string> GetErrorsAndWarnings()
		{
			var result = new List<string>();

			result.AddRange(Errors);
			result.AddRange(Warnings);

			return result;
		}
	}
}

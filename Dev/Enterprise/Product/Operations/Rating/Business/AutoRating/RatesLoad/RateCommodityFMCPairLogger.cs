using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateCommodityFMCPairLogger : ILogger, IDisposable
	{
		public RateCommodityFMCPairLogger()
		{
		}

		RateCommodityFMCPairLogger(RateCommodityFMCPairLogger parent, int indentLevel)
		{
			this.indentLevel = indentLevel;
			this.parent = parent;
		}

		public RateCommodityFMCPairLogger CreateIndentedLogger() => new RateCommodityFMCPairLogger(this, indentLevel + 1);

		public void Log(LogType logType, string message, Exception ex) => Log(logType, message);
		public void Log(LogType logType, string message)
		{
			if (!filterStringStartingWith.Any(x => message.StartsWith(x)))
			{
				logEntries.Add(new string('\t', indentLevel) + message);
			}
		}

		public override string ToString() => string.Join(System.Environment.NewLine, logEntries);

		public void Dispose()
		{
			if (parent != null && logEntries.Count > 0)
			{
				parent.logEntries.Add(ToString());
				logEntries.Clear();
			}
		}

		readonly int indentLevel;
		readonly RateCommodityFMCPairLogger parent;
		readonly List<string> logEntries = new List<string>();
		readonly NoResString[] filterStringStartingWith = new[] {
			(NoResString)"RateLine",
			(NoResString)"Chargeable was added for",
			(NoResString)"Company Tariff Level"
		};
	}
}

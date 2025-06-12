using System.IO;
using System.Collections.Generic;
using log4net.Core;
using log4net.Layout.Pattern;

namespace CargoWise.eServices.USCustoms.InboundService
{
	public class CustomLogLevelPatternConverter : PatternLayoutConverter
	{
		private static readonly Dictionary<string, string> LevelMappings = new Dictionary<string, string>
		{
			{ "INFO", "INF" },
			{ "DEBUG", "DBG" },
			{ "WARN", "WRN" },
			{ "ERROR", "ERR" },
			{ "FATAL", "FAL" },
			{ "TRACE", "TRC" }
		};

		internal void FormatLogLevel(TextWriter writer, LoggingEvent loggingEvent)
		{
			Convert(writer, loggingEvent);
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			var level = loggingEvent.Level?.DisplayName.ToUpper();

			if (level != null && LevelMappings.TryGetValue(level, out var shortLevel))
			{
				writer.Write(shortLevel);
			}
			else
			{
				writer.Write(level != null ? level.Substring(0, 3) : "UNK");
			}
		}
	}
}

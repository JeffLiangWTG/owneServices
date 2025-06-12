using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace eServices.LoggingEnhancement.Log4netImpl.Log4Net
{
	public class CompactLevelPatternConverter : PatternLayoutConverter
	{
		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			string logName;
			switch (loggingEvent.Level.Name)
			{
				case "TRACE":
					logName = "TRC";
					break;
				case "DEBUG":
					logName = "DBG";
					break;
				case "INFO":
					logName = "INF";
					break;
				case "WARN":
					logName = "WRN";
					break;
				case "ERROR":
					logName = "ERR";
					break;
				case "FATAL":
					logName = "FAL";
					break;
				default:
					logName = loggingEvent.Level.DisplayName.Substring(0, 3);
					break;
			}
			writer.Write(logName);
		}
	}
}
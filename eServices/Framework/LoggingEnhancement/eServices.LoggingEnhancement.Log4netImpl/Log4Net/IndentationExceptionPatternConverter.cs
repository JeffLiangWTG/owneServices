using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace eServices.LoggingEnhancement.Log4netImpl.Log4Net
{
	public class IndentationExceptionPatternConverter : PatternLayoutConverter
	{
		public IndentationExceptionPatternConverter()
		{
			// This converter handles the exception
			IgnoresException = false;
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (loggingEvent.ExceptionObject != null && Option != null && Option.Length > 0)
			{
				switch (Option.ToLower())
				{
					case "message":
						IndentationMessagePatternConverter.WriteWithIndentation(writer, loggingEvent.ExceptionObject.Message);
						break;
					case "source":
						IndentationMessagePatternConverter.WriteWithIndentation(writer, loggingEvent.ExceptionObject.Source);
						break;
					case "stacktrace":
						IndentationMessagePatternConverter.WriteWithIndentation(writer, loggingEvent.ExceptionObject.StackTrace);
						break;
					case "targetsite":
						IndentationMessagePatternConverter.WriteWithIndentation(writer, loggingEvent.ExceptionObject.TargetSite.ToString());
						break;
					case "helplink":
						IndentationMessagePatternConverter.WriteWithIndentation(writer, loggingEvent.ExceptionObject.HelpLink);
						break;
					default:
						// do not output SystemInfo.NotAvailableText
						break;
				}
			}
			else
			{
				string exceptionString = loggingEvent.GetExceptionString();
				if (exceptionString != null && exceptionString.Length > 0)
				{
					IndentationMessagePatternConverter.WriteWithIndentation(writer, exceptionString, writeLine: true);
				}
				else
				{
					// do not output SystemInfo.NotAvailableText
				}
			}
		}
	}
}

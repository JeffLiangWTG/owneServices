using System;
using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace eServices.LoggingEnhancement.Log4netImpl.Log4Net
{
	public class IndentationMessagePatternConverter : PatternLayoutConverter
	{
		static IndentationMessagePatternConverter()
		{
			log4net.Util.SystemInfo.NullText = string.Empty;
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			var msg = loggingEvent.RenderedMessage;
			WriteWithIndentation(writer, msg);
		}

		internal static void WriteWithIndentation(TextWriter writer, string msg, bool writeLine = false)
		{
			writer.Write("   ");
			if (msg == null)
			{
				Write(writer, log4net.Util.SystemInfo.NullText, writeLine);
				return;
			}

			var newLine = Environment.NewLine;
			var startIndex = 0;
			var lastPossibleNewLine = msg.Length - newLine.Length;
			while (startIndex < msg.Length)
			{
				int newLineIndex = msg.IndexOf(newLine, startIndex);
				if (newLineIndex == -1)
				{
					Write(writer, msg.Substring(startIndex), writeLine);
					break;
				}
				else
				{
					writer.WriteLine(msg.Substring(startIndex, newLineIndex - startIndex));
					if (newLineIndex != lastPossibleNewLine)
					{
						writer.Write("   ");
					}
					startIndex = newLineIndex + newLine.Length;
				}
			}
		}

		static void Write(TextWriter writer, string text, bool withNewLine)
		{
			if (withNewLine)
			{
				writer.WriteLine(text);
			}
			else
			{
				writer.Write(text);
			}
		}
	}
}

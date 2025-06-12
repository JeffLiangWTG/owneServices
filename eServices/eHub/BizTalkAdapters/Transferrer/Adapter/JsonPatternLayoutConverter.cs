using System;
using System.Collections.Generic;
using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;
using Newtonsoft.Json;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class JsonMessage
	{
		[JsonProperty(PropertyName = "@timestamp")]
		public string TimeStamp { get; set; }

		public string CategoryName { get; set; }

		public string Severity { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Exception { get; set; }

		public string Body { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Attributes Attributes { get; set; }
	}

	public class Attributes
	{
		public string OriginalMessage { get; set; }
	}

	public class JsonPatternLayoutConverter: PatternLayoutConverter
	{
		private const int TruncateLength = 1024;

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			try
			{
				var jsonMessage = new JsonMessage
				{
					TimeStamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
					CategoryName = loggingEvent.LoggerName,
					Severity = loggingEvent.Level.Name switch
					{
						"TRACE" => "Trace",
						"DEBUG" => "Debug",
						"WARN" => "Warn",
						"ERROR" => "Error",
						"CRITICAL" => "Fatal",
						_ => "Info"
					}
				};

				var bodyMessage = loggingEvent.MessageObject.ToString();

				if (loggingEvent.ExceptionObject is not null)
				{
					var exceptionMessage = loggingEvent.ExceptionObject.ToString();
					jsonMessage.Exception = exceptionMessage;

					bodyMessage = bodyMessage + Environment.NewLine + exceptionMessage;
				}

				if (bodyMessage.Length > TruncateLength)
				{
					var jsonAttributes = new Attributes { OriginalMessage = bodyMessage };
					jsonMessage.Attributes = jsonAttributes;
					bodyMessage = bodyMessage.Substring(0, TruncateLength);
				}
				jsonMessage.Body = bodyMessage;

				writer.Write(JsonConvert.SerializeObject(jsonMessage));
			}
			catch (Exception)
			{
				writer.Write(loggingEvent.MessageObject);
			}
		}
	}
}

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Hawking.CSI.Plugins.FTP
{
	internal class FtpTraceLogger : TraceListener
	{
		public ILogger Log { get; set; }

		public override void Write(string message)
		{
			WriteLine(message);
		}

		public override void WriteLine(string message)
		{
			try
			{
				string line;
				if (Log != null && message != null)
					if (Log.IsEnabled(LogLevel.Debug))
						using (var rdr = new StringReader(message))
							while ((line = rdr.ReadLine()) != null)
								Log.LogDebug(line);
			}
			catch (Exception) { }
		}
	}
}

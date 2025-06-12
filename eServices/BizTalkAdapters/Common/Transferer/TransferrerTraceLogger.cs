using System;
using System.Diagnostics;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public class TransferrerTraceLogger : TraceListener
	{
		public ILog Logger { get; set; }

		public override void Write(string message)
		{
			WriteLine(message);
		}

		public override void WriteLine(string message)
		{
			try
			{
				if (this.Logger.IsDebugEnabled)
				foreach (var line in message.TrimEnd('\r','\n').Split(new[] { Environment.NewLine }, StringSplitOptions.None))
					TransferrerHelpers.Log(this, this.Logger, LogLevel.Debug, line);
				}
			catch (Exception) { }
		}
	}
}

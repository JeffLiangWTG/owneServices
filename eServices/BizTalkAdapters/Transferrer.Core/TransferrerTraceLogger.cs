using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Core
{
	[ExcludeFromCodeCoverage]
	public class TransferrerTraceLogger : TraceListener
	{
		public ILog Log { get; set; }

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
					if (Log.IsDebugEnabled)
						using (var rdr = new StringReader(message))
							while ((line = rdr.ReadLine()) != null)
								Log.Debug(line);
			}
			catch (Exception) { }
		}
	}
}

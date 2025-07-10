using System.Collections.Generic;
using Enterprise.Freight.Forwarding.AWB.TNT;

namespace Enterprise.Tracking.Business
{
	public class WebImportLogger : IImportLogger
	{
		public WebImportLogger()
		{
			Infos = new List<string>();
			Errors = new List<string>();
			Warnings = new List<string>();
		}

		public void LogInfo(string message)
		{
			Infos.Add(message);
		}

		public void LogError(string message)
		{
			Errors.Add(message);
		}

		public void LogWarning(string message)
		{
			Warnings.Add(message);
		}

		public List<string> Infos { get; set; }
		public List<string> Errors { get; set; }
		public List<string> Warnings { get; set; }
	}
}

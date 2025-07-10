using System;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class AppMonitorLog : AppLog
	{
		public DateTime StartTime { get; set; }
		public double Duration { get; set; }
		public string Status { get; set; }
	}
}

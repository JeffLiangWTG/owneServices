using System;
using CargoWise.Types;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class TimeoutSettings
	{
		TimeoutSettings()
		{
		}

		public static TimeoutSettings Instance
		{
			get { return timeoutSettings ?? (timeoutSettings = new TimeoutSettings()); }
		}

		[ThreadStatic]
		static TimeoutSettings timeoutSettings;

		public int OnlineSailingSchedulesAutoInitiatedSuppressionLevel { get; set; }

		public ZDateTime OnlineSailingSchedulesAutoInitiatedSuppressUntilUtc { get; set; }

		public int OnlineSailingSchedulesUserInitiatedSuppressionLevel { get; set; }

		public ZDateTime OnlineSailingSchedulesUserInitiatedSuppressUntilUtc { get; set; }
	}
}

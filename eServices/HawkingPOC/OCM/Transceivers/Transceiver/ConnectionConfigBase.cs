using System;

namespace OcmPoc.Transceivers
{
	public class ConnectionConfigBase
	{
		public TimeSpan PollInterval => TimeSpan.FromSeconds(PollIntervalSeconds);

		public int PollIntervalSeconds { get; set; } = 60;
	}
}

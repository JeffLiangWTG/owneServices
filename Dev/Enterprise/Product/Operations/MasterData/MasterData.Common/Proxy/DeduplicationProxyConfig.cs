using System;

namespace Enterprise.MasterData.Common
{
	public class DeduplicationProxyConfig
	{
		public ExecutionMode Mode { get; set; } = ExecutionMode.User;
		public bool ShouldInvokeDeduplicationEvents { get; set; }
		public bool UseMaxRecords { get; set; } = true;
		public TimeSpan CompareTimeout { get; set; } = TimeSpan.FromSeconds(10);
		public TimeSpan DuplicationFinderTimeout { get; set; } = TimeSpan.FromSeconds(20);
	}
}

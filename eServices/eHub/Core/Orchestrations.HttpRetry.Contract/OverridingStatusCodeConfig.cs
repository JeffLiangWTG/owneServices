using System;

namespace CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract
{
	[Serializable]
	public class OverridingStatusCodeConfig
	{
		public ushort StatusCode { get; set; }

		public string Message { get; set; }

		public bool ShouldRetry { get; set; }
	}
}

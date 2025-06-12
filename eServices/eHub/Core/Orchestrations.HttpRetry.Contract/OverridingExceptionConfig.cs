using System;

namespace CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract
{
	[Serializable]
	public class OverridingExceptionConfig
	{
		public string ExceptionType { get; set; }

		public string ExceptionMessage { get; set; }

		public bool ShouldRetry { get; set; }
	}
}

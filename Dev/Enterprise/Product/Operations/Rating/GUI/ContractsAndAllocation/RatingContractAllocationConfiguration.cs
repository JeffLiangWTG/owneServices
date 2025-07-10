using Enterprise.Freight.Integration;

namespace Enterprise.Rating.GUI
{
	public sealed class RatingContractAllocationConfiguration : IContractSimulationFormConfiguration
	{
		public RatingContractAllocationConfiguration(IRatingContractSimulationFormActions formActions, IRatingContractSimulationFilterDefaults filterDefaults)
		{
			FormActions = formActions;
			FilterDefaults = filterDefaults;
		}

		public IRatingContractSimulationFormActions FormActions { get; }

		public IRatingContractSimulationFilterDefaults FilterDefaults { get; }

		public IRatingContractSimulationQuantityProvider QuantityProvider => null;

		public IRatingContractSimulationNotificationProvider NotificationProvider => null;
	}
}

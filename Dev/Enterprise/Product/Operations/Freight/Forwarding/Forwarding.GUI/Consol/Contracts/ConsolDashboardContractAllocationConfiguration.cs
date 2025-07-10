using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class ConsolDashboardContractAllocationConfiguration : IContractSimulationFormConfiguration
	{
		public ConsolDashboardContractAllocationConfiguration(ForwardingConsol consol)
		{
			FormActions = new ConsolDashboardContractAllocationFormActions(consol);
			FilterDefaults = new ConsolContractAndRouteAllocationFilterDefaults(consol);
			QuantityProvider = new ConsolAllocationSimulationQuantityProvider(consol);
			NotificationProvider = new ConsolDashboardContractAllocationNotificationProvider(consol);
		}

		public IRatingContractSimulationFormActions FormActions { get; }

		public IRatingContractSimulationFilterDefaults FilterDefaults { get; }

		public IRatingContractSimulationQuantityProvider QuantityProvider { get; }

		public IRatingContractSimulationNotificationProvider NotificationProvider { get; }
	}
}

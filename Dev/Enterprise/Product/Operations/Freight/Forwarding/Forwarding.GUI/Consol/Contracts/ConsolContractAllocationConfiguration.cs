using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class ConsolContractAllocationConfiguration : IContractSimulationFormConfiguration
	{
		public ConsolContractAllocationConfiguration(ForwardingConsol consol)
		{
			FormActions = new ConsolContractAndRouteAllocationFormActions(consol);
			FilterDefaults = new ConsolContractAndRouteAllocationFilterDefaults(consol);
			QuantityProvider = new ConsolAllocationSimulationQuantityProvider(consol);
			NotificationProvider = new ConsolContractAndRouteAllocationNotificationProvider(consol);
		}

		public ConsolContractAllocationConfiguration(ForwardingContainer container)
		{
			FormActions = new ConsolContractAndRouteAllocationFormActions(container);
			FilterDefaults = new ConsolContractAndRouteAllocationFilterDefaults(container);
			QuantityProvider = new ConsolAllocationSimulationQuantityProvider(container.Consol);
			NotificationProvider = new ConsolContainerAllocationNotificationProvider(container);
		}

		public IRatingContractSimulationFormActions FormActions { get; }

		public IRatingContractSimulationFilterDefaults FilterDefaults { get; }

		public IRatingContractSimulationQuantityProvider QuantityProvider { get; }

		public IRatingContractSimulationNotificationProvider NotificationProvider { get; }
	}
}

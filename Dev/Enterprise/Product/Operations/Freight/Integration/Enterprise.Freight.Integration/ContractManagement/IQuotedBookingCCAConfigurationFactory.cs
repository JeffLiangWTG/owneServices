using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Integration
{
	public interface IQuotedBookingCCAConfigurationFactory
	{
		IContractSimulationFormConfiguration CreateForContainer(IForwardingContainer container);
	}
}

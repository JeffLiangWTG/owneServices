using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public sealed class QuotedBookingCCAConfigurationFactory : IQuotedBookingCCAConfigurationFactory
	{
		public IContractSimulationFormConfiguration CreateForContainer(IForwardingContainer container)
		{
			return new QuotedBookingContractAllocationConfiguration(container as ForwardingContainer);
		}
	}

	sealed class QuotedBookingContractAllocationConfiguration : IContractSimulationFormConfiguration
	{
		public QuotedBookingContractAllocationConfiguration(QuotedBooking quotedBooking)
		{
			FormActions = new QuotedBookingContractAllocationFormActions(quotedBooking);
			FilterDefaults = new QuotedBookingContractAllocationFilterDefaults(quotedBooking);
			QuantityProvider = new QuotedBookingAllocationSimulationQuantityProvider(quotedBooking);
			NotificationProvider = new QuotedBookingContractAllocationNotificationProvider(quotedBooking);
		}

		public QuotedBookingContractAllocationConfiguration(ForwardingContainer container)
		{
			var quotedBooking = container.QuotedBooking as QuotedBooking;
			FormActions = new QuotedBookingContractAllocationFormActions(container);
			FilterDefaults = new QuotedBookingContractAllocationFilterDefaults(container);
			QuantityProvider = new QuotedBookingAllocationSimulationQuantityProvider(quotedBooking);
			NotificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);
		}

		public IRatingContractSimulationFormActions FormActions { get; }

		public IRatingContractSimulationFilterDefaults FilterDefaults { get; }

		public IRatingContractSimulationQuantityProvider QuantityProvider { get; }

		public IRatingContractSimulationNotificationProvider NotificationProvider { get; }
	}
}

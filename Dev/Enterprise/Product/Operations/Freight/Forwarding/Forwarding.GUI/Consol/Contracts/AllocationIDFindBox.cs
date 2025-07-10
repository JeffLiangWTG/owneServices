using CargoWise.Application;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Forwarding.GUI
{
	class AllocationIDFindBox : ZGridGuidFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			if (CurrentItem is ForwardingContainer container)
			{
				IContractSimulationFormConfiguration configuration;
				if (container.Consol is ForwardingConsol consol)
				{
					configuration = new ConsolContractAllocationConfiguration(container);
				}
				else
				{
					var quotedBookingConfigurationFactory = ObjectFactory.Get<IQuotedBookingCCAConfigurationFactory>();
					configuration = quotedBookingConfigurationFactory.CreateForContainer(container);
				}

				var popup = new ContractAllocationFindBoxPopup(configuration);
				popup.Closed += (s, e) => this.SetCodeDescription(container?.AllocationLine as RatingContractAllocationLine);

				return popup;
			}

			return null;
		}
	}
}

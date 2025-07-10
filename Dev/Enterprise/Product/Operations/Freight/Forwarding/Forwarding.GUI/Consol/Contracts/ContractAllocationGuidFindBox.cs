using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ContractManagement;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.Forwarding.GUI
{
	[FormBasherTestPopupExclude]
	public class ContractAllocationGuidFindBox : ZGuidFindBox, IContractAllocationGuidFindBox
	{
		protected override bool CanReferenceByDescription => false;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			IContractSimulationFormConfiguration formConfiguration;

			switch (CurrentItem)
			{
				case ForwardingConsol consol:
					formConfiguration = new ConsolContractAllocationConfiguration(consol);
					return new ContractAllocationFindBoxPopup(formConfiguration);

				case ForwardingContainer container:
					formConfiguration = new ConsolContractAllocationConfiguration(container);
					return new ContractAllocationFindBoxPopup(formConfiguration);

				default:
					throw new NotImplementedException();
			}
		}
	}
}

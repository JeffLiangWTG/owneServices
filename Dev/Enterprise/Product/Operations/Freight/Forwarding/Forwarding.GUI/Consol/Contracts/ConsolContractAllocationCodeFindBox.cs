using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.Forwarding.GUI
{
	[FormBasherTestPopupExclude]
	[SuppressCheckControlModuleId]
	class ConsolContractAllocationCodeFindBox : ZCodeFindBox
	{
		protected override bool CanReferenceByDescription => false;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			var currentConsolidation = CurrentItem as ForwardingConsol;
			if (currentConsolidation != null)
			{
				var configurationFactory = new ConsolContractAllocationConfiguration(currentConsolidation);

				return new ContractAllocationFindBoxPopup(configurationFactory);
			}

			throw new NotImplementedException();
		}
	}
}

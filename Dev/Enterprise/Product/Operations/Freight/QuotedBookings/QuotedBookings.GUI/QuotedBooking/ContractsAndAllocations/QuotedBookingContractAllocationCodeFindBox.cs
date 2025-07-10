using System;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	class QuotedBookingContractAllocationCodeFindBox : ZCodeFindBox
	{
		protected override bool CanReferenceByDescription => false;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			IContractSimulationFormConfiguration formConfiguration;

			if (CurrentItem is QuotedBooking quotedBooking)
			{
				formConfiguration = new QuotedBookingContractAllocationConfiguration(quotedBooking);
				return new ContractAllocationFindBoxPopup(formConfiguration);
			}

			throw new NotImplementedException();
		}
	}
}

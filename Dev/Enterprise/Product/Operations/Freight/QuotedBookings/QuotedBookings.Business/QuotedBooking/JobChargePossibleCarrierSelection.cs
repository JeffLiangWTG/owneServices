using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class JobChargePossibleCarrierSelection : NonPersistentBusinessObject
	{
		readonly RateOneOffCarrierCollection possibleCarriers;
		readonly ICharge charge;

		public JobChargePossibleCarrierSelection(ICharge charge, RateOneOffCarrierCollection possibleCarriers)
		{
			this.possibleCarriers = possibleCarriers;
			this.charge = charge;
		}

		public RateOneOffCarrierCollection PossibleCreditorsAndCarriers => possibleCarriers;

		public void SetPossibleCarrierSelection(ZGuid carrier, ZGuid creditor)
		{
			var selectedOrganisation =
				creditor == ZGuid.Empty
				? carrier
				: creditor;

			charge.JR_OH_CostAccount = selectedOrganisation;
			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.CreateNewCharge;
		}
	}
}

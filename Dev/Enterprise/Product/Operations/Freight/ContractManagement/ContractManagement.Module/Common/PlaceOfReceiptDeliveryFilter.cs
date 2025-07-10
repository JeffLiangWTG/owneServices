using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	sealed class PlaceOfReceiptDeliveryFilter : AllocationRouteCoveringLocationFilter
	{
		protected override bool IncludeLinkedSchedules => false;

		public PlaceOfReceiptDeliveryFilter(ZString description, IBusinessObjectCollection locationList)
			: base(description, locationList, RatingContractAllocationLineSchema.RCA_PlaceOfReceipt, RatingContractAllocationLineSchema.RCA_PlaceOfDelivery)
		{
		}
	}
}

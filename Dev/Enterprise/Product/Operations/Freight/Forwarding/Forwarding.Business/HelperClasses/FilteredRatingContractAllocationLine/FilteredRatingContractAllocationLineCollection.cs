using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;

namespace Enterprise.Freight.Forwarding.Business.HelperClasses.FilteredRatingContractAllocationLine
{
	public class FilteredRatingContractAllocationLineCollection : RatingContractAllocationLineCollection
	{
		readonly Func<ZGuid?> getCarrierContractPK;

		public FilteredRatingContractAllocationLineCollection(BusinessObjectFactory factory, Func<ZGuid?> getCarrierContractPK)
			: base(factory)
		{
			this.getCarrierContractPK = getCarrierContractPK;
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get => new RatingContractAllocationFindBoxListProvider(this, getCarrierContractPK);
		}
	}
}

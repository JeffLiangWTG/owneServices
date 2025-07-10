using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.HelperClasses.FilteredRatingContractAllocationLine
{
	class RatingContractAllocationFindBoxListProvider : FindBoxListProvider
	{
		readonly Func<ZGuid?> getCarrierContractPK;
		public RatingContractAllocationFindBoxListProvider(IBusinessObjectCollection collection,
			Func<ZGuid?> getCarrierContractPK) : base(collection)
		{
			this.getCarrierContractPK = getCarrierContractPK;
		}

		void FilterByCarrierContractPK(ZQuery query)
		{
			var carrierContractPK = getCarrierContractPK();
			if (carrierContractPK != null)
			{
				var contractFilter = new ZQuery(RatingContractAllocationLineSchema.RCA_RCT_RatingContract, carrierContractPK);
				query.AddToFilter(contractFilter, JoinCondition.And);
			}
		}

		protected override void AddCodeEqualsFilter(ZQuery query, string code)
		{
			base.AddCodeEqualsFilter(query, code);

			FilterByCarrierContractPK(query);
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			base.AddCodeStartsWithFilter(query, code);

			FilterByCarrierContractPK(query);
		}
	}
}

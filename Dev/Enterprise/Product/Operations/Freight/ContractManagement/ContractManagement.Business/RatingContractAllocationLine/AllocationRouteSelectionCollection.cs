using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ContractManagement.Business
{
	public sealed class AllocationRouteSelectionCollection : BusinessObjectCollection<RatingContractAllocationLine>
	{
		public AllocationRouteSelectionCollection(BusinessObjectFactory factory, IReadOnlyCollection<RatingContractAllocationLine> routes)
			: base(factory)
		{
			foreach (var route in routes)
			{
				Add(route);
			}
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}

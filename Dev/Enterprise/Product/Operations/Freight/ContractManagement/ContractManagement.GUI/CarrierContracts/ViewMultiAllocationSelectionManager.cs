using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ContractManagement.Business;

namespace Enterprise.ContractManagement.GUI
{
	public sealed class ViewMultiAllocationSelectionManager : NonPersistentBusinessObject
	{
		public ViewMultiAllocationSelectionManager(
			BusinessObjectFactory factory,
			IReadOnlyCollection<RatingContractAllocationLine> routes)
			: base(factory)
		{
			AllocationRoutesForSelection = new AllocationRouteSelectionCollection(Factory, routes);
		}

		public AllocationRouteSelectionCollection AllocationRoutesForSelection { get; }
	}
}

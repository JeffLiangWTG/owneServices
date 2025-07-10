using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(AllocationRouteCoveringLocationFilter))]
	sealed class CoveringLocationFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AllocationRouteCoveringLocationFilter(
				AllocationRouteFilterConstants.LoadDischargePort,
				new LocationCollection(Factory));
		}
	}
}

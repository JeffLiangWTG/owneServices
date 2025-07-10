using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class AdhocReceivedPackagesCollection : ActiveBusinessObjectCollection<WhsItemPackageState>
	{
		public AdhocReceivedPackagesCollection(TransitWarehousePackagePlanner planner)
		: base(planner.Factory, new AdhocCollectionRelationship(typeof(WhsItemPackageState)))
		{
		}
	}
}

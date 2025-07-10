using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class TransitWarehouseDispatchLoadListDataTest : TransitAssemblyDataTest<WhsItemDispatchLoadList, TransitWarehouseDispatchLoadListData>
	{
		protected override string ExpectedHumanReadableName => "Transit Dispatch Load List";

		protected override Type ExpectedBusinessObjectCollectionType => typeof(WhsItemDispatchLoadListCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemDispatchLoadList;
	}
}

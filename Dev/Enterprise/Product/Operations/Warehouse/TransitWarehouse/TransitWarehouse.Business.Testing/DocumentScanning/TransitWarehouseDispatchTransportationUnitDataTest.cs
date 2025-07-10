using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class TransitWarehouseDispatchTransportationUnitDataTest : TransitAssemblyDataTest<WhsItemDispatchTransportationUnit, TransitWarehouseDispatchTransportationUnitData>
	{
		protected override string ExpectedHumanReadableName => "Transit Dispatch Transportation Unit";

		protected override Type ExpectedBusinessObjectCollectionType => typeof(WhsItemDispatchTransportationUnitCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemDispatchTransportationUnit;
	}
}

using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class TransitWarehouseReceiveTransportationUnitDataTest : TransitAssemblyDataTest<WhsItemReceiveTransportationUnit, TransitWarehouseReceiveTransportationUnitData>
	{
		protected override string ExpectedHumanReadableName => "Transit Receive Transportation Unit";

		protected override Type ExpectedBusinessObjectCollectionType => typeof(WhsItemReceiveTransportationUnitCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemReceiveTransportationUnit;
	}
}

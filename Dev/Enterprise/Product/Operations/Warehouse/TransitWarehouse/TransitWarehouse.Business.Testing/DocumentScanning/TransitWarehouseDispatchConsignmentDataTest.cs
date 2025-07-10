using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class TransitWarehouseDispatchConsignmentDataTest : TransitAssemblyDataTest<WhsItemDispatchConsignment, TransitWarehouseDispatchConsignmentData>
	{
		protected override string ExpectedHumanReadableName => "Transit Dispatch Consignment";

		protected override Type ExpectedBusinessObjectCollectionType => typeof(WhsItemDispatchConsignmentCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsTransitDispatchConsignment;
	}
}

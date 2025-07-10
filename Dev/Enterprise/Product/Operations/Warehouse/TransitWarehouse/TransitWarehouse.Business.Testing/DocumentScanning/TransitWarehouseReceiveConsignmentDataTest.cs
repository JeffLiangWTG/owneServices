using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class TransitWarehouseReceiveConsignmentDataTest : TransitAssemblyDataTest<WhsItemReceiveConsignment, TransitWarehouseReceiveConsignmentData>
	{
		protected override string ExpectedHumanReadableName => "Transit Receive Consignment";

		protected override Type ExpectedBusinessObjectCollectionType => typeof(WhsItemReceiveConsignmentCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsTransitReceiveConsignment;
	}
}

using System;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class TransitWarehouseReceiveASNUnitDataTest : TransitAssemblyDataTest<WhsItemReceiveASN, TransitWarehouseReceiveASNData>
	{
		protected override string ExpectedHumanReadableName => "Transit Receive ASN";

		protected override Type ExpectedBusinessObjectCollectionType => typeof(WhsItemReceiveASNCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemReceiveASN;
	}
}

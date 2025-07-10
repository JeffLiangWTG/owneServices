using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemTransferHeaderController))]
	public class WhsItemTransferHeaderControllerTest : WhsTransitControllerTest<WhsItemTransferHeaderController, WhsItemTransferHeader>
	{
		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemTransferHeader;

		protected override ControllerID GetControllerID() => ControllerIDs.WhsItemTransferHeader;
	}
}

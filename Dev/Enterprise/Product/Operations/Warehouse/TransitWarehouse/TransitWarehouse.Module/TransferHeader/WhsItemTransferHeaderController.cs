using System;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemTransferHeaderController : WhsTransitController
	{
		public override ControllerID ID => ControllerIDs.WhsItemTransferHeader;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsItemTransferHeader;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsItemTransferHeader);
	}
}

using Enterprise.Environment;
using Enterprise.Packing.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsHandlingUnitController : HandlingUnitController
	{
		public override ControllerID ID => ControllerIDs.WhsHandlingUnit;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsHandlingUnit;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.Warehouse;
	}
}

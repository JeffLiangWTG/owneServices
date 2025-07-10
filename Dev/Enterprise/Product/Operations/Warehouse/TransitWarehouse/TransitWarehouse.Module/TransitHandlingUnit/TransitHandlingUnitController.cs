using Enterprise.Environment;
using Enterprise.Packing.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class TransitHandlingUnitController : HandlingUnitController
	{
		public override ControllerID ID => ControllerIDs.TransitHandlingUnit;

		public override ModuleIdentifier ModuleID => ModuleIDs.TransitHandlingUnit;

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.TransitWarehouse;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.TransitWarehouse;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.TransitWarehouse;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.TransitWarehouse;

		#endregion
	}
}

using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.Module.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsHandlingUnitController))]
	public class WhsHandlingUnitControllerTest : HandlingUnitControllerTest<WhsHandlingUnitController, PkgHandlingUnit>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsHandlingUnit;
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsHandlingUnit;

		protected override SecurityCheckpoint checkpoint => Env.Security.Warehouse;
	}
}

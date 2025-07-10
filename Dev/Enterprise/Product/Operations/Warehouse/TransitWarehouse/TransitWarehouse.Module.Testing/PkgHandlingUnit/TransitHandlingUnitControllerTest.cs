using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.Module.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(TransitHandlingUnitController))]
	public class TransitHandlingUnitControllerTest : HandlingUnitControllerTest<TransitHandlingUnitController, PkgHandlingUnit>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TransitHandlingUnit;
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.TransitHandlingUnit;

		protected override SecurityCheckpoint checkpoint => Env.Security.TransitWarehouse;
	}
}

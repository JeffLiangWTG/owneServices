using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.GUI.Test
{
	[TestedType(typeof(GteVehicleMovementModule))]
	public class GteVehicleMovementModuleTest : GlowOnlyModuleTest<GteVehicleMovementModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(GteVehicleMovementFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(GteVehicleMovementFilterControl);

		protected override Type ExpectedCollectionType => typeof(GteVehicleMovementCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.GateManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.GateManagement;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GteVehicleMovement;

		protected override bool ExpectedSupportsWorkflow => true;
	}
}

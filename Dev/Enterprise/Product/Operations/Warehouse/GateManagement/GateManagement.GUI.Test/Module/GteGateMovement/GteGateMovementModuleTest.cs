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
	[TestedType(typeof(GteGateMovementModule))]
	public class GteGateMovementModuleTest : GlowOnlyModuleTest<GteGateMovementModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(GteGateMovementFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(GteGateMovementFilterControl);

		protected override Type ExpectedCollectionType => typeof(GteGateMovementCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.GateManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.GateManagement;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GteGateMovement;

		protected override bool ExpectedSupportsWorkflow => true;
	}
}

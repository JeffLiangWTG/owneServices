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
	[TestedType(typeof(GteGateMovementBookingModule))]
	public class GteGateMovementBookingModuleTest : GlowOnlyModuleTest<GteGateMovementBookingModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(GteGateMovementBookingFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(GteGateMovementBookingFilterControl);

		protected override Type ExpectedCollectionType => typeof(GteGateMovementBookingCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.GateManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.GateManagement;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GteGateMovementBooking;

		protected override bool ExpectedSupportsWorkflow => true;
	}
}

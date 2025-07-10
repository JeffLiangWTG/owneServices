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
	[TestedType(typeof(GteBookingModule))]
	public class GteBookingModuleTest : GlowOnlyModuleTest<GteBookingModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(GteBookingFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(GteBookingFilterControl);

		protected override Type ExpectedCollectionType => typeof(GteBookingCollection);

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.GateManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.GateManagement;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.GteBooking;

		protected override bool ExpectedSupportsWorkflow => true;
	}
}

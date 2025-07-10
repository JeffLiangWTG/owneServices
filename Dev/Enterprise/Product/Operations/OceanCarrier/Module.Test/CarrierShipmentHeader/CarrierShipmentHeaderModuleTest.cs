using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.OceanCarrier.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Module.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderModule))]
	sealed class CarrierShipmentHeaderModuleTest : GlowOnlyModuleTest<CarrierShipmentHeaderModule>
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CarrierShipmentHeader;

		protected override Type ExpectedFilterBusinessObjectType => typeof(CarrierShipmentHeaderFilterStripBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(CarrierShipmentHeaderFilterControl);

		protected override Type ExpectedCollectionType => typeof(CarrierShipmentHeaderCollection);

		protected override bool ExpectedSupportsWorkflow => true;

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.ShippingManager;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.LinerAndAgency;
	}
}

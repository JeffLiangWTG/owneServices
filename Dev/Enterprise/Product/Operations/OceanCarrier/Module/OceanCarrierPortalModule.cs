using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.OceanCarrier.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Module
{
	public sealed class OceanCarrierPortalModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.OceanCarrierPortal;

		public override Uri Url => OceanCarrierGlowHelper.GetOceanCarrierPortalUrl();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.LinerAndAgency; // can be changed later

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ShippingManager; // can be changed later
	}
}

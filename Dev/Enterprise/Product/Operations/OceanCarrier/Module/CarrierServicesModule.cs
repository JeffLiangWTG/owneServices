using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Module
{
	public sealed class CarrierServicesModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CarrierServices;

		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/CarrierServices");

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.LinerAndAgency; // can be changed later

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ShippingManager; // can be changed later
	}
}

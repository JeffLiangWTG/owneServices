using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.OceanCarrier.Module
{
	public sealed class RouteSegmentsModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.RouteSegments;

		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/RouteSegments");

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.LinerAndAgency; // can be changed later

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ShippingManager; // can be changed later
	}
}

using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbConsignmentWebPortalModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/ConsignmentList");

		public override ModuleIdentifier ID => ModuleIDs.DtbConsignmentWebPortal;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.DtbConsignmentWebPortal;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LandTransport;
	}
}

using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbConsignmentRunSheetWebPortalModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/RunSheetList");

		public override ModuleIdentifier ID => ModuleIDs.DtbConsignmentRunSheetWebPortal;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.DtbConsignmentRunSheetWebPortal;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LandTransport;
	}
}

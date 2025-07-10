using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ExternalRequestsModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/Requests");

		public override ModuleIdentifier ID => ModuleIDs.ExternalRequests;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ExternalRequests;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;
	}
}

using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Module
{
	public class GlowHRMSModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("/HRM");

		public override ModuleIdentifier ID => ModuleIDs.GlowHRMS;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlowHRMS;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
	}
}

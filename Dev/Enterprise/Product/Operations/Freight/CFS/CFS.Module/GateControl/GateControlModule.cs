using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	public class GateControlModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/GateControl");

		public override ModuleIdentifier ID => ModuleIDs.GateControl;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
	}
}

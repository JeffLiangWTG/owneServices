using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ControlTowerModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/ControlTower");

		public override ModuleIdentifier ID => ModuleIDs.ControlTower;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ControlTowerPlus;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;
	}
}

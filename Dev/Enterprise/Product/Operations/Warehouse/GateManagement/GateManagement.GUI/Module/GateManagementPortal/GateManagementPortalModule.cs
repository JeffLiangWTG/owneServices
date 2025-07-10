using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GateManagementPortalModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("/GDM");

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GateManagementPortal; }
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.GateManager;
	}
}

using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class TransitWarehousePortalModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TransitWarehousePortal; }
		}

		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/TransitManagementPortal");

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.TransitWarehouse;
	}
}

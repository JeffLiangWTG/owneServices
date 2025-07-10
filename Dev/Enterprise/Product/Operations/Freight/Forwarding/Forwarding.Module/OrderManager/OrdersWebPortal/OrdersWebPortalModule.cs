using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class OrdersWebPortalModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/OrderList");

		public override ModuleIdentifier ID => ModuleIDs.OrdersWebPortal;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.OrdersWebPortal;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;
	}
}

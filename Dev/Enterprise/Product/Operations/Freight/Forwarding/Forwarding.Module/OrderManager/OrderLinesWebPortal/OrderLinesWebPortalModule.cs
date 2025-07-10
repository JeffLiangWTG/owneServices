using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class OrderLinesWebPortalModule : ZSimpleUrlLauncherModule
	{
		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/OrderLineList");

		public override ModuleIdentifier ID => ModuleIDs.OrderLinesWebPortal;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.OrderLinesWebPortal;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;
	}
}

using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class OnlineSailingSchedulesModule : ZPopupModule
	{
		public override ModuleIdentifier ID => ModuleIDs.OnlineSailingSchedules;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.OnlineSailingSchedules;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
		protected override ZPopupController GetNewController()
		{
			return new OnlineSailingSchedulesController();
		}
		public Uri Url
		{
			get
			{
				var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("GSS");
				return url;
			}
		}
	}
}

using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class GlowRateSelectorModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CarrierConnect;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WiseRatesSearch;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
		public override Uri Url => null;

		public override void Show()
		{
			var baseGlowUrl = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			if (string.IsNullOrEmpty(baseGlowUrl))
			{
				var result = Res.GetString("9990df67-4795-49f7-971d-cb763957d881", "{0} cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: {1}/{2}", "CargoWise CarrierConnect", GlowRegistry.Instance.GlowPortalsUri.Category, GlowRegistry.Instance.GlowPortalsUri.Caption);
				Globals.Message.ShowError(result);
				return;
			}

			var url = UrlBuilder.GenerateURL(new Uri(baseGlowUrl), "C3");
			WebUrlLauncher.Launch(url.ToString());
		}
	}
}

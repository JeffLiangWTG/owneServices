using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProductionRules.GUI
{
	public static class ProductionRulesEngineLinkManager
	{
		public static void HandleProductionRulesEngineLinkClick(INotifications notify, string endpoint)
		{
			var url = new GlowUrlProvider(notify).TryGenerateUrl(endpoint, ResString.GetMultilingualString("8734a335-a519-4dff-a727-65a95648130d", "Production Rules Engine portal"));
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}
	}
}

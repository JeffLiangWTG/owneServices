using System.Collections.Generic;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.GUI
{
	public static class TransitWarehouseGUIHelper
	{
		public static void OpenInBrowser(string relativePath, string jobDescription, IEnumerable<(string Name, string Value)> additionalQueryStrings = null)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl(relativePath, jobDescription, additionalQueryStrings);
			if (url is not null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}
	}
}

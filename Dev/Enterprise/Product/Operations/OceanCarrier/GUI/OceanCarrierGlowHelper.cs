using System;
using Enterprise.OceanCarrier.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.OceanCarrier.GUI
{
	public static class OceanCarrierGlowHelper
	{
		public static Uri GetOceanCarrierPortalUrl() => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("OCS");

		public static bool OpenInBrowser(CarrierShipmentHeader carrierShipmentHeader)
		{
			if (carrierShipmentHeader == null)
			{
				return false;
			}

			var urlProvider = new GlowUrlProvider(GlobalNotificationsWrapper.Instance);

			var url = urlProvider.TryGenerateUrl(
				endpoint: "goto/CarrierShipmentHeader",
				carrierShipmentHeader.HumanReadableName,
				additionalQueryStrings: new[] { ("entityPK", carrierShipmentHeader.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
				return true;
			}

			return true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class ViewShipmentTrackingHelper
	{
		public static void LaunchURL(ForwardingShipment shipment)
		{
			LaunchURL(new[] { shipment });
		}

		public static void LaunchURL(IEnumerable<ForwardingShipment> shipments)
		{
			var trackingQueryStrings = new List<string>();
			foreach (var shipment in shipments)
			{
				trackingQueryStrings.Add($"trackingNumber={Uri.EscapeDataString(shipment.JS_UniqueConsignRef)}");
			}

			var glowPortalsUri = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (string.IsNullOrWhiteSpace(glowPortalsUri))
			{
				var errorMessage = ResString.GetMultilingualString("16d28be6-9fd4-41a0-aeab-cb63909b00b6",
@"This shipment cannot be opened in a browser as GLOW has not been configured for this client.
Registry: {0}/{1}", GlowRegistry.Instance.GlowPortalsUri.Category, GlowRegistry.Instance.GlowPortalsUri.Caption);

				string errorCaption = Res.GetString("360E64F4-7A86-4EE9-932E-98BA68B1CDAA", "Error");
				Globals.Message.Show(errorMessage, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var basePath = new Uri(glowPortalsUri, UriKind.Absolute).ToString();
			if (!basePath.EndsWith("/"))
			{
				basePath += "/";
			}

			var relativePath = (NoResString)"NST/Desktop?noHeader=true#/tracker?";
			var token = ObjectFactory.Get<IGlowSingleSignOnTokenProvider>().CreateLimitedToken();
			var url = $"{basePath}{relativePath}{string.Join("&", trackingQueryStrings)}&sso_otp={token}";
			WebUrlLauncher.Launch(url);
		}
	}
}

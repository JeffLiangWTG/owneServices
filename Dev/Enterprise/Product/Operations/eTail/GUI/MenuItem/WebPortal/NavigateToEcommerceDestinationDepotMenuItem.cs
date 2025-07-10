using System;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public class NavigateToEcommerceDestinationDepotMenuItem : BaseHVLVMenuItem
	{
		public NavigateToEcommerceDestinationDepotMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("500E2684-E224-4CCC-9B4A-164BDA745601", "Ecommerce Destination Depot"), shipment)
		{
		}

		protected override Action MenuAction => () => {
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl(EcommercePortals.Codes.ETL);
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		};
	}
}

using System;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public class NavigateToEcommerceOriginDepotMenuItem : BaseHVLVMenuItem
	{
		public NavigateToEcommerceOriginDepotMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("10C1B521-8299-4AF6-BA97-C98F1C817F4F", "Ecommerce Origin Depot"), shipment)
		{
		}

		protected override Action MenuAction => () => {
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl(EcommercePortals.Codes.EOS);
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		};
	}
}

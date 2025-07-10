using System.Drawing;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class HouseBillLogo : IHouseBillLogo
	{
		public HouseBillLogo(ForwardingShipment shipment, bool isOriginal)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.isOriginal = isOriginal;
		}

		readonly ForwardingShipment shipment;
		readonly bool isOriginal;

		public Image Image => image ?? (image = GetImage());
		Image image;

		Image GetImage()
		{
			var settings = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value
				.OfType<HouseBillOfLadingType>()
				.FirstOrDefault(t => t.Code == shipment.JS_HouseBillOfLadingType);

			if (settings == null
				|| settings.PrePrinted
				|| isOriginal && settings.PrintLogoInFormBuilder == PrintLogoOptions.Codes.Copy
				|| !isOriginal && settings.PrintLogoInFormBuilder == PrintLogoOptions.Codes.Original)
			{
				return null;
			}

			return FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.Value
				.OfType<RegistryImage>()
				.FirstOrDefault(t => t.Code == settings.LogoCode)
				?.Image;
		}
	}
}

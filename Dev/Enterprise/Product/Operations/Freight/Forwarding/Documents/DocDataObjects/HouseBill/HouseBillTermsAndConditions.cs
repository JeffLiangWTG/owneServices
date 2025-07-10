using System.Drawing;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class HouseBillTermsAndConditions : IHouseBillTermsAndConditions
	{
		public HouseBillTermsAndConditions(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
		}

		readonly ForwardingShipment shipment;

		public Image Image => image ?? (image = GetImage());
		Image image;

		Image GetImage()
		{
			var settings = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value
				.OfType<HouseBillOfLadingType>()
				.FirstOrDefault(t => t.Code == shipment.JS_HouseBillOfLadingType);

			if (settings == null
				|| settings.PrePrinted)
			{
				return null;
			}

			return FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.Value
				.OfType<HouseBillOfLadingTermsAndConditions>()
				.FirstOrDefault(t => t.Code == settings.TermsAndConditionsCode)
				?.Image;
		}
	}
}

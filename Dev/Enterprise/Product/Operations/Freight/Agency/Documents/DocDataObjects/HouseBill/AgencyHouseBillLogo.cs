using System.Drawing;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class AgencyHouseBillLogo : IAgencyHouseBillLogo
	{
		public AgencyHouseBillLogo(BillOfLading billOfLading)
		{
			this.billOfLading = Argument.NotNull(billOfLading, nameof(billOfLading));
		}
		readonly BillOfLading billOfLading;

		public Image Image => image ?? (image = GetImage());
		Image image;

		Image GetImage()
		{
			var billOfLadingImage = AgencyRegistry.Instance.BillOfLadingLogosImages.Value.Cast<BillOfLadingImage>()
				.FirstOrDefault(x => x.Enabled && x.PrincipalPK == billOfLading.JS_OH_DeliveryAgent);

			return billOfLadingImage?.Image ?? DocumentsDataRegistry.Instance.PrincipalDocumentBrand.FindBrandingForPrincipal(billOfLading.JS_OH_DeliveryAgent)?.Image;
		}
	}

	public interface IAgencyHouseBillLogo
	{
		Image Image { get; }
	}
}

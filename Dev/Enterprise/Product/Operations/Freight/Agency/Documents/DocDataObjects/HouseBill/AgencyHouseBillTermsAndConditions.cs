using System.Drawing;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class AgencyHouseBillTermsAndConditions : IAgencyHouseBillTermsAndConditions
	{
		public AgencyHouseBillTermsAndConditions(BillOfLading billOfLading)
		{
			this.billOfLading = Argument.NotNull(billOfLading, nameof(billOfLading));
		}

		readonly BillOfLading billOfLading;

		public Image Image => image ?? (image = GetImage());
		Image image;

		Image GetImage()
		{
			var billOfLadingImage = AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.Value.Cast<BillOfLadingImage>()
			.FirstOrDefault(x => x.Enabled && x.PrincipalPK == billOfLading.JS_OH_DeliveryAgent);

			return billOfLadingImage?.Image;
		}
	}

	public interface IAgencyHouseBillTermsAndConditions
	{
		Image Image { get; }
	}
}

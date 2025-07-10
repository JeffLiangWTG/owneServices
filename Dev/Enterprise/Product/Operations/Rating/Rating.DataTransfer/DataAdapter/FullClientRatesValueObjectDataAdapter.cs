using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class FullClientRatesValueObjectDataAdapter : FullRatingValueObjectDataAdapter<ClientRate>
	{
		#region Properties

		protected override ZString ExpectedRateType
		{
			get { return RatingConstants.RatingHeaderTypes.ClientRate; }
		}

		protected override bool ShouldCheckIfOwnerIsSpecified
		{
			get { return true; }
		}

		#endregion

		#region Import

		protected override void ImportRatingHeader(ClientRate ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			base.ImportRatingHeader(ratingHeader, rateXSD, context);
			ImportClientDetails(ratingHeader, rateXSD, context);
		}

		#endregion

		#region Export

		protected override void ExportRatingHeader(ClientRate ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			base.ExportRatingHeader(ratingHeader, rateXSD, context);
			ExportClientDetails(ratingHeader, rateXSD, context);
		}

		#endregion
	}
}

